using LevelDB;
using System.Text;
using System.Text.Json.Nodes;
using System.Runtime.InteropServices;

namespace AutoOS.Core.Helpers.Database;

public static partial class DatabaseHelper
{
    public static JsonNode Read(string databasePath, string domain, string keyName)
    {
        if (string.IsNullOrEmpty(databasePath) || !Directory.Exists(databasePath))
            return null;

        byte[] prefixBytes = Encoding.UTF8.GetBytes(domain);
        byte[] separatorBytes = [0x00, 0x01];
        byte[] keyNameBytes = Encoding.UTF8.GetBytes(keyName);
        byte[] finalKeyBytes = new byte[prefixBytes.Length + separatorBytes.Length + keyNameBytes.Length];

        Buffer.BlockCopy(prefixBytes, 0, finalKeyBytes, 0, prefixBytes.Length);
        Buffer.BlockCopy(separatorBytes, 0, finalKeyBytes, prefixBytes.Length, separatorBytes.Length);
        Buffer.BlockCopy(keyNameBytes, 0, finalKeyBytes, prefixBytes.Length + separatorBytes.Length, keyNameBytes.Length);

        JsonNode result = null;
        try
        {
            result = ReadFromDatabase(databasePath, finalKeyBytes);
        }
        catch
        {
            string tempDatabasePath = databasePath + " - Copy";
            try
            {
                Directory.CreateDirectory(tempDatabasePath);

                foreach (var file in Directory.GetFiles(databasePath))
                {
                    File.Copy(file, Path.Combine(tempDatabasePath, Path.GetFileName(file)), true);
                }

                try
                {
                    result = ReadFromDatabase(tempDatabasePath, finalKeyBytes);
                }
                catch
                {
                    Repair(tempDatabasePath);
                    result = ReadFromDatabase(tempDatabasePath, finalKeyBytes);
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                if (Directory.Exists(tempDatabasePath))
                {
                    Directory.Delete(tempDatabasePath, true);
                }
            }
        }

        return result;
    }

    private static JsonNode ReadFromDatabase(string databasePath, byte[] finalKeyBytes)
    {
        var options = new Options { CreateIfMissing = false };
        using var database = new DB(options, databasePath);
        byte[] valueBytes = database.Get(finalKeyBytes);

        if (valueBytes != null)
        {
            string value = Encoding.UTF8.GetString(valueBytes);

            if (value.Length > 0 && value[0] == '\x01')
            {
                value = value.Substring(1);
            }

            return JsonNode.Parse(value);
        }

        return null;
    }

    public static bool Write(string databasePath, string domain, string keyName, JsonNode jsonContent)
    {
        byte[] prefixBytes = Encoding.UTF8.GetBytes(domain);
        byte[] separatorBytes = [0x00, 0x01];
        byte[] keyNameBytes = Encoding.UTF8.GetBytes(keyName);
        byte[] finalKeyBytes = new byte[prefixBytes.Length + separatorBytes.Length + keyNameBytes.Length];

        Buffer.BlockCopy(prefixBytes, 0, finalKeyBytes, 0, prefixBytes.Length);
        Buffer.BlockCopy(separatorBytes, 0, finalKeyBytes, prefixBytes.Length, separatorBytes.Length);
        Buffer.BlockCopy(keyNameBytes, 0, finalKeyBytes, prefixBytes.Length + separatorBytes.Length, keyNameBytes.Length);

        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonContent.ToJsonString());

        byte[] finalValueBytes = new byte[1 + jsonBytes.Length];
        finalValueBytes[0] = 0x01;
        Buffer.BlockCopy(jsonBytes, 0, finalValueBytes, 1, jsonBytes.Length);
        var options = new Options { CreateIfMissing = false };
        using var database = new DB(options, databasePath);
        database.Put(finalKeyBytes, finalValueBytes);
        return true;
    }

    public static bool Delete(string databasePath, string domain, string keyName)
    {
        byte[] prefixBytes = Encoding.UTF8.GetBytes(domain);
        byte[] separatorBytes = [0x00, 0x01];
        byte[] keyNameBytes = Encoding.UTF8.GetBytes(keyName);
        byte[] finalKeyBytes = new byte[prefixBytes.Length + separatorBytes.Length + keyNameBytes.Length];

        Buffer.BlockCopy(prefixBytes, 0, finalKeyBytes, 0, prefixBytes.Length);
        Buffer.BlockCopy(separatorBytes, 0, finalKeyBytes, prefixBytes.Length, separatorBytes.Length);
        Buffer.BlockCopy(keyNameBytes, 0, finalKeyBytes, prefixBytes.Length + separatorBytes.Length, keyNameBytes.Length);

        var options = new Options { CreateIfMissing = false };
        using var database = new DB(options, databasePath);
        database.Delete(finalKeyBytes);
        return true;
    }

    public static void Repair(string databasePath)
    {
        var options = new Options { CreateIfMissing = false };
		leveldb_repair_db(options.Handle, databasePath, out nint errptr);
		if (errptr != IntPtr.Zero)
        {
            leveldb_free(errptr);
        }
    }

    [LibraryImport("leveldb", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial void leveldb_repair_db(IntPtr options, string name, out IntPtr errptr);

    [LibraryImport("leveldb")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static partial void leveldb_free(IntPtr ptr);
}
