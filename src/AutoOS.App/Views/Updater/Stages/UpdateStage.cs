using AutoOS.Core.Helpers.GPU;
using AutoOS.Core.Helpers.Registry;
using Microsoft.Win32;
namespace AutoOS.Views.Updater.Stages;

public static class UpdateStage
{
	public static List<(string Title, Func<Task> Action, Func<bool> Condition)> UpdateActions(UpdateDialog dialog)
	{
		var gpus = GpuHelper.GetGPUs().Where(gpu => gpu.NVIDIA);

		var actions = new List<(string Title, Func<Task> Action, Func<bool> Condition)>
		{

		};

		foreach (var gpu in gpus)
		{
			actions.Add(("Configuring Miscellaneous NVIDIA Settings", async () => RegistryHelper.SetValue(RegistryHelper.Identity.TrustedInstaller, gpu.RegistryPath, "RmPowerFeature", 1413829989, RegistryValueKind.DWord), null));
			actions.Add(("Configuring Miscellaneous NVIDIA Settings", async () => RegistryHelper.SetValue(RegistryHelper.Identity.TrustedInstaller, gpu.RegistryPath, "RmPowerFeature2", 89478485, RegistryValueKind.DWord), null));
			actions.Add(("Configuring Miscellaneous NVIDIA Settings", async () => RegistryHelper.SetValue(RegistryHelper.Identity.TrustedInstaller, gpu.RegistryPath, "RMElcg", 1431655764, RegistryValueKind.DWord), null));
		}

		return actions;
	}
}