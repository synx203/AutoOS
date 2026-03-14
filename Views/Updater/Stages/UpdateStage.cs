using AutoOS.Views.Installer.Actions;

namespace AutoOS.Views.Updater.Stages;

public static class UpdateStage
{
    public static List<(string Title, Func<Task> Action, Func<bool> Condition)> UpdateActions(UpdateDialog dialog)
    {
        return
        [
            // revert split audio services
            ("Reverting splitting audio services", async () => await ProcessActions.RunPowerShell(@"Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Services\Audiosrv' -Name 'ImagePath' -Value '%systemroot%\system32\svchost.exe -k LocalServiceNetworkRestricted -p' -Type ExpandString"), null),
            ("Reverting splitting audio services", async () => await ProcessActions.RunPowerShell(@"Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Services\AudioEndpointBuilder' -Name 'ImagePath' -Value '%systemroot%\system32\svchost.exe -k LocalSystemNetworkRestricted -p' -Type ExpandString"), null),
        ];
    }
}