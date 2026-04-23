using AutoOS.Helpers.Registry;
using Microsoft.Win32;
using AutoOS.Helpers.GPU;

namespace AutoOS.Views.Updater.Stages;

public static class UpdateStage
{
    public static List<(string Title, Func<Task> Action, Func<bool> Condition)> UpdateActions(UpdateDialog dialog)
    {
        var gpus = GpuHelper.GetGPUs().Where(gpu => gpu.NVIDIA);

        var actions = new List<(string Title, Func<Task> Action, Func<bool> Condition)>
        {
            // enable timer serialization
            ("Enabling Timer Serialization", async () => RegistryHelper.SetValue(RegistryHelper.Identity.TrustedInstaller, @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "SerializeTimerExpiration", 1, RegistryValueKind.DWord), null)
        };

        foreach (var gpu in gpus)
        {
            // enable "enablegpufirmware"
            actions.Add(("Enabling GSP Firmware", async () => RegistryHelper.SetValue(RegistryHelper.Identity.TrustedInstaller, gpu.RegistryPath, "EnableGpuFirmware", 1, RegistryValueKind.DWord), () => gpu.DeviceName.Contains("RTX")));
            actions.Add(("Enabling GSP Firmware", async () => RegistryHelper.SetValue(RegistryHelper.Identity.TrustedInstaller, gpu.RegistryPath, "EnableGpuFirmwareLogs", 0, RegistryValueKind.DWord), () => gpu.DeviceName.Contains("RTX")));

            // force "hardware composed: independent flip"
            actions.Add((@"Forcing ""Hardware Composed: Independent Flip""", async () => RegistryHelper.SetValue(RegistryHelper.Identity.TrustedInstaller, gpu.RegistryPath, "enableRS2FlipCollapse", 1, RegistryValueKind.DWord), null));
            actions.Add((@"Forcing ""Hardware Composed: Independent Flip""", async () => RegistryHelper.SetValue(RegistryHelper.Identity.TrustedInstaller, gpu.RegistryPath, "enableRS2ImmediateFlipCompletionReporting", 1, RegistryValueKind.DWord), null));

            // remove "enablemshybrid"
            actions.Add((@"Removing ""EnableMsHybrid""", async () => RegistryHelper.DeleteValue(RegistryHelper.Identity.TrustedInstaller, gpu.RegistryPath, "EnableMsHybrid"), null));
        }

        return actions;
    }
}