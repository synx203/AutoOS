using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.System.Services;

namespace AutoOS.Helpers.Service
{
    public static class ServiceHelper
    {
        public static unsafe void KillServiceProcess(string serviceName)
        {
            var scmHandle = PInvoke.OpenSCManager(null, null, 0x0001);
            if (scmHandle.IsInvalid) return;

            try
            {
                var serviceHandle = PInvoke.OpenService(scmHandle, serviceName, 0x0004);
                if (serviceHandle.IsInvalid) return;

                try
                {
                    SERVICE_STATUS_PROCESS status = default;
                    uint bytesNeeded;

                    bool success = PInvoke.QueryServiceStatusEx(
                        (SC_HANDLE)serviceHandle.DangerousGetHandle(),
                        SC_STATUS_TYPE.SC_STATUS_PROCESS_INFO,
                        (byte*)&status,
                        (uint)sizeof(SERVICE_STATUS_PROCESS),
                        &bytesNeeded);

                    if (success && status.dwProcessId != 0)
                    {
                        try
                        {
                            var process = Process.GetProcessById((int)status.dwProcessId);
                            process.Kill();
                            process.WaitForExit();
                        }
                        catch { }
                    }
                }
                finally
                {
                    serviceHandle.Dispose();
                }
            }
            finally
            {
                scmHandle.Dispose();
            }
        }
    }
}