using AutoOS.Views.Installer.Actions;
using AutoOS.Helpers.Registry;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;
using System.Diagnostics;
using Microsoft.Win32;
using AutoOS.Helpers.Services;

namespace AutoOS.Views.Installer.Stages;

public static class ServicesStage
{
    public static IntPtr WindowHandle { get; private set; }
    public static async Task Run()
    {
        WindowHandle = WindowNative.GetWindowHandle(App.MainWindow);
        InstallPage.Status.Text = "Configuring Services and Drivers...";

        string previousTitle = string.Empty;
        int stagePercentage = 2;

        var actions = new List<(string Title, Func<Task> Action, Func<bool> Condition)>
        {
            // group services
            ("Grouping services", async () => ServicesHelper.GroupServices(), null),

            // disable failure actions
            ("Disabling failure actions", async () => RegistryHelper.SetValue(RegistryHelper.Identity.TrustedInstaller, @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SoftwareProtectionPlatform", "InactivityShutdownDelay", 4294967295, RegistryValueKind.DWord), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure AudioEndpointBuilder reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag AudioEndpointBuilder 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure Appinfo reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag Appinfo 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure AppXSvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag AppXSvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure CaptureService reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag CaptureService 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure cbdhsvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag cbdhsvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure ClipSvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag ClipSvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure CryptSvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag CryptSvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure DevicesFlowUserSvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag DevicesFlowUserSvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure Dhcp reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag Dhcp 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure DispBrokerDesktopSvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag DispBrokerDesktopSvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure Dnscache reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag Dnscache 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure DoSvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag DoSvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure DsmSvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag DsmSvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure gpsvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag gpsvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure InstallService reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag InstallService 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure KeyIso reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag KeyIso 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure lfsvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag lfsvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure msiserver reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag msiserver 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure NcbService reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag NcbService 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure netprofm reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag netprofm 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure NgcSvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag NgcSvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure NgcCtnrSvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag NgcCtnrSvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure nsi reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag nsi 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure ProfSvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag ProfSvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure sppsvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag sppsvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure StateRepository reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag StateRepository 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure TextInputManagementService reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag TextInputManagementService 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure TrustedInstaller reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag TrustedInstaller 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure UdkUserSvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag UdkUserSvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure WFDSConMgrSvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag WFDSConMgrSvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure WinHttpAutoProxySvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag WinHttpAutoProxySvc 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure Winmgmt reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag Winmgmt 0") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failure Wcmsvc reset=0 actions=//") { CreateNoWindow = true }), null),
            ("Disabling failure actions", async () => await RegistryHelper.RunAs(RegistryHelper.Identity.TrustedInstaller, new ProcessStartInfo("sc.exe", "failureflag Wcmsvc 0") { CreateNoWindow = true }), null)
        };

        var filteredActions = actions.Where(a => a.Condition == null || a.Condition.Invoke()).ToList();
        int groupedTitleCount = 0;

        List<Func<Task>> currentGroup = [];

        for (int i = 0; i < filteredActions.Count; i++)
        {
            if (i == 0 || filteredActions[i].Title != filteredActions[i - 1].Title)
            {
                groupedTitleCount++;
            }
        }

        double incrementPerTitle = groupedTitleCount > 0 ? stagePercentage / (double)groupedTitleCount : 0;

        foreach (var (title, action, condition) in filteredActions)
        {
            if (previousTitle != string.Empty && previousTitle != title && currentGroup.Count > 0)
            {
                foreach (var groupedAction in currentGroup)
                {
                    try
                    {
                        await groupedAction();
                    }
                    catch (Exception ex)
                    {
                        await ProcessActions.LogError(ex);

                        InstallPage.Info.Title = $"{previousTitle}: {ex.Message}";
                        InstallPage.Info.Severity = InfoBarSeverity.Error;
                        InstallPage.Progress.Foreground = (Brush)Application.Current.Resources["SystemFillColorCriticalBrush"];
                        Helpers.Taskbar.TaskbarHelper.SetProgressState(WindowHandle, Helpers.Taskbar.TaskbarStates.Error);
                        InstallPage.ProgressRingControl.Visibility = Visibility.Collapsed;
                        InstallPage.ResumeButton.Visibility = Visibility.Visible;

                        var tcs = new TaskCompletionSource<bool>();

                        RoutedEventHandler resumeHandler = null;
                        resumeHandler = (sender, e) =>
                        {
                            InstallPage.ResumeButton.Click -= resumeHandler;
                            InstallPage.Info.Severity = InfoBarSeverity.Informational;
                            InstallPage.Progress.ClearValue(ProgressBar.ForegroundProperty);
                            Helpers.Taskbar.TaskbarHelper.SetProgressState(WindowHandle, Helpers.Taskbar.TaskbarStates.Normal);
                            InstallPage.ProgressRingControl.Visibility = Visibility.Visible;
                            InstallPage.ResumeButton.Visibility = Visibility.Collapsed;

                            tcs.TrySetResult(true);
                        };

                        InstallPage.ResumeButton.Click += resumeHandler;
                        await tcs.Task;
                    }
                }

                InstallPage.Progress.Value += incrementPerTitle;
                Helpers.Taskbar.TaskbarHelper.SetProgressValue(WindowHandle, InstallPage.Progress.Value, 100);
                await Task.Delay(150);
                currentGroup.Clear();
            }

            InstallPage.Info.Title = title + "...";
            currentGroup.Add(action);
            previousTitle = title;
        }

        if (currentGroup.Count > 0)
        {
            foreach (var groupedAction in currentGroup)
            {
                try
                {
                    await groupedAction();
                }
                catch (Exception ex)
                {
                    InstallPage.Info.Title += ": " + ex.Message;
                    InstallPage.Info.Severity = InfoBarSeverity.Error;
                    InstallPage.Progress.Foreground = (Brush)Application.Current.Resources["SystemFillColorCriticalBrush"];
                    Helpers.Taskbar.TaskbarHelper.SetProgressState(WindowHandle, Helpers.Taskbar.TaskbarStates.Error);
                    InstallPage.ProgressRingControl.Foreground = (Brush)Application.Current.Resources["SystemFillColorCriticalBrush"];
                    InstallPage.ProgressRingControl.Visibility = Visibility.Collapsed;
                    InstallPage.ResumeButton.Visibility = Visibility.Visible;
                    await ProcessActions.LogError(ex);

                    var tcs = new TaskCompletionSource<bool>();

                    InstallPage.ResumeButton.Click += (sender, e) =>
                    {
                        tcs.TrySetResult(true);
                        InstallPage.Info.Severity = InfoBarSeverity.Informational;
                        InstallPage.Progress.ClearValue(ProgressBar.ForegroundProperty);
                        Helpers.Taskbar.TaskbarHelper.SetProgressState(WindowHandle, Helpers.Taskbar.TaskbarStates.Normal);
                        InstallPage.ProgressRingControl.Foreground = null;
                        InstallPage.ProgressRingControl.Visibility = Visibility.Visible;
                        InstallPage.ResumeButton.Visibility = Visibility.Collapsed;
                    };

                    await tcs.Task;
                }
            }

            InstallPage.Progress.Value += incrementPerTitle;
            Helpers.Taskbar.TaskbarHelper.SetProgressValue(WindowHandle, InstallPage.Progress.Value, 100);
        }
        if (filteredActions.Count == 0)
        {
            InstallPage.Progress.Value += stagePercentage;
            Helpers.Taskbar.TaskbarHelper.SetProgressValue(WindowHandle, InstallPage.Progress.Value, 100);
        }
    }
}