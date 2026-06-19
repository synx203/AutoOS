using Microsoft.Win32;

namespace AutoOS.Views.Updater.Stages;

public static class UpdateStage
{
	public static List<(string Title, Func<Task> Action, Func<bool> Condition)> UpdateActions(UpdateDialog dialog)
	{
		var actions = new List<(string Title, Func<Task> Action, Func<bool> Condition)>
		{
			// disable legacy context menu
			("Disable Legacy Context Menu", async () => Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}", false), null)
		};

		return actions;
	}
}
