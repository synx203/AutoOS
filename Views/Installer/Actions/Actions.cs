using AutoOS.Helpers.GPU;
using AutoOS.Helpers.Monitor;
using AutoOS.Helpers.RAM;
using Downloader;
using Microsoft.UI.Xaml.Media;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using WinRT.Interop;
using AutoOS.Views.Installer.Stages;
using AutoOS.Helpers.Device;

namespace AutoOS.Views.Installer.Actions;

public static class ProcessActions
{
    public static IntPtr WindowHandle { get; private set; }

    public static async Task RunNsudo(string user, string command)
    {
        string arguments = user switch
        {
            "TrustedInstaller" => $"-U:T -P:E -Wait -ShowWindowMode:Hide {command}",
            "CurrentUser" => $"-U:P -P:E -Wait -ShowWindowMode:Hide {command}",
            _ => throw new ArgumentException("Invalid user specified.", nameof(user))
        };

        await Process.Start(new ProcessStartInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Applications", "NSudo", "NSudoLC.exe"), arguments) { CreateNoWindow = true }).WaitForExitAsync();
    }

    public static async Task RunPowerShell(string command)
    {
        await Process.Start(new ProcessStartInfo("powershell.exe", $"-Command \"{command}\"") { CreateNoWindow = true, UseShellExecute = false }).WaitForExitAsync();
    }

    public static async Task RunConnectionCheck()
    {
        WindowHandle = WindowNative.GetWindowHandle(App.MainWindow);
        InstallPage.Info.Severity = InfoBarSeverity.Warning;
        InstallPage.Progress.Foreground = (Brush)Application.Current.Resources["SystemFillColorCautionBrush"];
        Helpers.Taskbar.TaskbarHelper.SetProgressState(WindowHandle, Helpers.Taskbar.TaskbarStates.Paused);
        InstallPage.ProgressRingControl.Foreground = (Brush)Application.Current.Resources["SystemFillColorCautionBrush"];

        await Task.Delay(1000);

        using (var httpClient = new HttpClient())
        {
            while (true)
            {
                try
                {
                    var response = await httpClient.GetAsync("http://www.google.com");
                    if (response.IsSuccessStatusCode)
                    {
                        InstallPage.Info.Severity = InfoBarSeverity.Informational;
                        InstallPage.Progress.ClearValue(ProgressBar.ForegroundProperty);
                        Helpers.Taskbar.TaskbarHelper.SetProgressState(WindowHandle, Helpers.Taskbar.TaskbarStates.Normal);
                        InstallPage.ProgressRingControl.Foreground = null;
                        InstallPage.Info.Title = "Internet connection successfully established...";
                        await Task.Delay(500);
                        break;
                    }
                }
                catch
                {

                }
            }
        }
    }

    public static async Task RunPowerShellScript(string script, string arguments)
    {
        await Process.Start(new ProcessStartInfo("powershell.exe", $"-ExecutionPolicy Bypass -File \"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Scripts", script)}\" {arguments}") { CreateNoWindow = true, UseShellExecute = false })!.WaitForExitAsync();
    }

    public static async Task RunApplication(string folderName, string executable, string arguments)
    {
        await Process.Start(new ProcessStartInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Applications", folderName, executable), arguments) { CreateNoWindow = true })!.WaitForExitAsync();
    }

    public static async Task RunDownload(string url, string path, string file = null, ProgressButton progressButton = null)
    {
        var uiContext = SynchronizationContext.Current;

        string title = progressButton == null ? InstallPage.Info?.Title ?? string.Empty : string.Empty;

        DownloadBuilder downloadBuilder;
        DownloadConfiguration config;

        if (url.Contains("raw.githubusercontent.com", StringComparison.OrdinalIgnoreCase))
        {
            using var client = new HttpClient();
            string dest = string.IsNullOrWhiteSpace(file) ? path : Path.Combine(path, file);
            await File.WriteAllTextAsync(dest, await client.GetStringAsync(url), Encoding.UTF8);
            return;
        }

        if (url.Contains("www2.ati.com", StringComparison.OrdinalIgnoreCase))
        {
            config = new DownloadConfiguration
            {
                RequestConfiguration = new RequestConfiguration
                {
                    Headers = new WebHeaderCollection
                {
                    { "Referer", "http://support.amd.com" },
                    { "Accept", "*/*" },
                    { "User-Agent", "AMD Catalyst Install Manager/0.0" },
                    { "Cache-Control", "no-cache" },
                    { "Connection", "Keep-Alive" }
                }
                }
            };

            downloadBuilder = DownloadBuilder.New()
                .WithUrl(url)
                .WithDirectory(path)
                .WithFileName(file)
                .WithConfiguration(config);
        }
        else
        {
            downloadBuilder = DownloadBuilder.New()
                .WithUrl(url)
                .WithDirectory(path)
                .WithFileName(file)
                .WithConfiguration(new DownloadConfiguration());
        }

        var download = downloadBuilder.Build();

        double speedMB = 0.0;
        double receivedMB = 0.0;
        double totalMB = 0.0;
        double percentage = 0.0;
        DateTime lastLoggedTime = DateTime.MinValue;

        download.DownloadProgressChanged += (sender, e) =>
        {
            if ((DateTime.Now - lastLoggedTime).TotalMilliseconds < 50) return;
            lastLoggedTime = DateTime.Now;

            speedMB = e.BytesPerSecondSpeed / (1024.0 * 1024.0);
            receivedMB = e.ReceivedBytesSize / (1024.0 * 1024.0);
            totalMB = e.TotalBytesToReceive / (1024.0 * 1024.0);
            percentage = e.ProgressPercentage;

            uiContext?.Post(_ =>
            {
                if (progressButton != null)
                {
                    progressButton.IsIndeterminate = false;
                    progressButton.Progress = percentage;
                }
                else if (!string.IsNullOrEmpty(title))
                {
                    InstallPage.Info.Title = $"{title} ({speedMB:F1} MB/s - {receivedMB:F2} MB of {totalMB:F2} MB)";
                    InstallPage.ProgressRingControl.IsIndeterminate = false;
                    InstallPage.ProgressRingControl.Value = percentage;
                }
            }, null);
        };

        download.DownloadFileCompleted += (sender, e) =>
        {
            uiContext?.Post(_ =>
            {
                if (progressButton != null)
                {
                    progressButton.Progress = 100;
                    progressButton.IsIndeterminate = true;
                }
                else if (!string.IsNullOrEmpty(title))
                {
                    InstallPage.Info.Title = $"{title} ({speedMB:F1} MB/s - {totalMB:F2} MB of {totalMB:F2} MB)";
                    InstallPage.ProgressRingControl.Value = 100;
                    InstallPage.ProgressRingControl.IsIndeterminate = true;
                }
            }, null);
        };

        await download.StartAsync();
    }

    public static async Task RunExtract(string inputPath, string outputPath)
    {
        await Process.Start(new ProcessStartInfo { FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Applications", "7-Zip", "7z.exe"), Arguments = $"x \"{inputPath}\" -y -o\"{outputPath}\"", CreateNoWindow = true })!.WaitForExitAsync();
    }

    public static async Task<string> GetLatestObsStudioUrl()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("AutoOS");

        string json = await client.GetStringAsync("https://api.github.com/repos/obsproject/obs-studio/releases/latest");
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement
            .GetProperty("assets")
            .EnumerateArray()
            .First(a => a.GetProperty("name").GetString().Contains("Windows-x64-Installer.exe"))
            .GetProperty("browser_download_url")
            .GetString();
    }

    public static async Task Log(bool bios = false)
    {
        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        string installStart = localSettings.Values["Install_Start"]?.ToString() ?? "N/A";
        string installEnd = localSettings.Values["Install_End"]?.ToString() ?? "N/A";

        string cpuName = Registry.GetValue(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0", "ProcessorNameString", "")?.ToString() ?? "";

        string manufacturer = Registry.GetValue(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\BIOS", "BaseBoardManufacturer", "")?.ToString() ?? "";

        string product = Registry.GetValue(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\BIOS", "BaseBoardProduct", "")?.ToString() ?? "";

        string motherboard = $"{manufacturer} {product}".Trim();

        string ram = $"{(RamHelper.GetRam() is var r ? $"{r.CapacityGB:N1} GB {r.DDRVersion} @ {r.MaxSpeedMHz} MHz" : "")}";

        var gpuList = PreparingStage.GPUs.Count > 0 ? PreparingStage.GPUs : GpuHelper.GetGPUs();
        string gpus = string.Join(", ", gpuList.Select(g => $"{g.DeviceName} (DeviceId: {g.DeviceId}, Install: {g.Install}, {g.CurrentVersion})"));

        string monitors = string.Join(", ", MonitorHelper.GetMonitors().Select(m => $"{m.DeviceName} ({m.Resolution.Width}x{m.Resolution.Height} @ {m.RefreshRate} Hz)"));

        var nicsList = DeviceHelper.GetDevices(DeviceType.NIC);
        string nics = nicsList.Count > 0 ? string.Join("\n", nicsList.Select(n => $"{n.FriendlyName} (DeviceId: {n.DeviceId}, VendorId: {n.VendorId}, Current Version: {n.DriverType} {n.CurrentVersion}, Connected: {n.IsActive})")) : "N/A";

        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
        string build = key.GetValue("CurrentBuild")?.ToString() ?? "";
        string ubr = key.GetValue("UBR")?.ToString() ?? "";
        string osVersion = $"{build}.{ubr}";

        string discordId = "Failed to get Discord account id";
        string discordUsername = "Failed to get Discord username";

        string discordJsonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discord", "sentry", "scope_v3.json");
        if (File.Exists(discordJsonPath))
        {
            try
            {
                string jsonText = File.ReadAllText(discordJsonPath);
                using JsonDocument doc = JsonDocument.Parse(jsonText);

                if (doc.RootElement.TryGetProperty("scope", out var scope) &&
                    scope.TryGetProperty("user", out var user))
                {
                    discordId = user.GetProperty("id").GetString() ?? discordId;
                    discordUsername = user.GetProperty("username").GetString() ?? discordUsername;
                }
            }
            catch
            {

            }
        }

        using var client = new HttpClient();

        using var multipart = new MultipartFormDataContent
        {
            { new StringContent(
                $"<@{discordId}>\n" +
                $"{discordUsername}\n" +
                $"{motherboard}\n" +
                $"{cpuName}\n" +
                $"{ram}\n" +
                $"{gpus}\n" +
                $"{monitors}\n" +
                $"{nics}\n" +
                $"{osVersion}\n" +
                $"Install start: {installStart}\n" +
                $"Install end: {installEnd}\n" +
                $"{ProcessInfoHelper.Version}"
            ), "content" }
        };

        if (bios)
            multipart.Add(new ByteArrayContent(File.ReadAllBytes(Path.Combine(PathHelper.GetAppDataFolderPath(), "SCEWIN", "nvram.txt"))), "file", Path.GetFileName(Path.Combine(PathHelper.GetAppDataFolderPath(), "SCEWIN", "nvram.txt")));

        string webhook = bios ? "https://discord.com/api/webhooks/1444743392868172016/1kq532maWmIguJEO-rp-X4RHG1idpbjKFWHC7IYwxr6KLEZxjhrJhwftYeeRKfKDYB-a" : "https://discord.com/api/webhooks/1444743483486240860/V_myd24FjH7TNJPruYbNJcnuE9Xany7C-tAScpygDV_FOGnwmuamSuOgXdxlts1Q2MhM";

        await client.PostAsync(webhook, multipart);
    }

    public static async Task LogError(Exception ex)
    {
        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        string installStart = localSettings.Values["Install_Start"]?.ToString() ?? "N/A";
        string installEnd = localSettings.Values["Install_End"]?.ToString() ?? "N/A";

        string cpuName = Registry.GetValue(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0", "ProcessorNameString", "")?.ToString() ?? "";

        string manufacturer = Registry.GetValue(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\BIOS", "BaseBoardManufacturer", "")?.ToString() ?? "";

        string product = Registry.GetValue(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\BIOS", "BaseBoardProduct", "")?.ToString() ?? "";

        string motherboard = $"{manufacturer} {product}".Trim();

        string ram = $"{(RamHelper.GetRam() is var r ? $"{r.CapacityGB:N1} GB {r.DDRVersion} @ {r.MaxSpeedMHz} MHz" : "")}";

        var gpuList = PreparingStage.GPUs.Count > 0 ? PreparingStage.GPUs : GpuHelper.GetGPUs();
        string gpus = string.Join(", ", gpuList.Select(g => $"{g.DeviceName} (DeviceId: {g.DeviceId}, Install: {g.Install}, {g.CurrentVersion})"));

        string monitors = string.Join(", ", MonitorHelper.GetMonitors().Select(m => $"{m.DeviceName} ({m.Resolution.Width}x{m.Resolution.Height} @ {m.RefreshRate} Hz)"));

        var nicsList = DeviceHelper.GetDevices(DeviceType.NIC);
        string nics = nicsList.Count > 0 ? string.Join("\n", nicsList.Select(n => $"{n.FriendlyName} (DeviceId: {n.DeviceId}, VendorId: {n.VendorId}, Current Version: {n.DriverType} {n.CurrentVersion}, Connected: {n.IsActive})")) : "N/A";

        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
        string build = key.GetValue("CurrentBuild")?.ToString() ?? "";
        string ubr = key.GetValue("UBR")?.ToString() ?? "";
        string osVersion = $"{build}.{ubr}";

        string discordId = "Failed to get Discord account id";
        string discordUsername = "Failed to get Discord username";

        string discordJsonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discord", "sentry", "scope_v3.json");
        if (File.Exists(discordJsonPath))
        {
            try
            {
                string jsonText = File.ReadAllText(discordJsonPath);
                using JsonDocument doc = JsonDocument.Parse(jsonText);

                if (doc.RootElement.TryGetProperty("scope", out var scope) &&
                    scope.TryGetProperty("user", out var user))
                {
                    discordId = user.GetProperty("id").GetString() ?? discordId;
                    discordUsername = user.GetProperty("username").GetString() ?? discordUsername;
                }
            }
            catch
            {

            }
        }

        using var client = new HttpClient();

        using var multipart = new MultipartFormDataContent
        {
            { new StringContent(
                $"<@{discordId}>\n" +
                $"{discordUsername}\n" +
                $"{motherboard}\n" +
                $"{cpuName}\n" +
                $"{ram}\n" +
                $"{gpus}\n" +
                $"{monitors}\n" +
                $"{nics}\n" +
                $"{osVersion}\n" +
                $"Install start: {installStart}\n" +
                $"Install end: {installEnd}\n" +
                $"{ex.GetType().FullName}\n" +
                $"Message: {ex.Message}\n" +
                $"HResult: 0x{ex.HResult:X}\n" +
                $"Source: {ex.Source}\n" +
                $"StackTrace:\n{ex.StackTrace}\n" +
                (ex.InnerException != null ? $"\nInnerException:\n{ex.InnerException}" : "") +
                $"\n{ProcessInfoHelper.Version}"
            ), "content" }
        };

        await client.PostAsync("https://discord.com/api/webhooks/1474078669596131409/Ha9bZsk1MZQRwuTrGWYpw1nYsL7OiPsi21BrRAaVoNlgjlFUOTtb1g2xgoZEfj6IT-Lc", multipart);
    }

    public static async Task DisableScheduledTasks()
    {
        ProcessStartInfo processStartInfo = new ProcessStartInfo("powershell.exe", $"-ExecutionPolicy Bypass -File \"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Scripts", "disablescheduledtasks.ps1")}\" \"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Applications", "NSudo", "NSudoLC.exe")}\"")
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
        };

        using (Process process = Process.Start(processStartInfo))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (double.TryParse(line, out double progress))
                    {
                        InstallPage.ProgressRingControl.IsIndeterminate = false;
                        InstallPage.ProgressRingControl.Value = progress;
                    }
                }
            }

            await process.WaitForExitAsync();
            InstallPage.ProgressRingControl.IsIndeterminate = true;
            InstallPage.ProgressRingControl.Value = 0;
        }
    }
}