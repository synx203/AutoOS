using AutoOS.Views.Settings.Power;

namespace AutoOS.Views.Updater.Stages;

public static class UpdateStage
{
	public static List<(string Title, Func<Task> Action, Func<bool> Condition)> UpdateActions(UpdateDialog dialog)
	{
		Guid guid = Guid.Empty;

		return
		[
            // update power plan
            ("Selecting AutoOS Power Plan", async () => guid = PowerApi.GetPlanGuidByName("AutoOS"), null),
			(@"Disabling ""Hetero containment policy.""", async () => PowerApi.WriteACValueIndex(guid, new Guid("54533251-82be-4824-96c1-47b60b740d00"), new Guid("60fbe21b-efd9-49f2-b066-8674d8e9f423"), 0), null),
			(@"Disabling ""Hetero containment policy.""", async () => PowerApi.WriteDCValueIndex(guid, new Guid("54533251-82be-4824-96c1-47b60b740d00"), new Guid("60fbe21b-efd9-49f2-b066-8674d8e9f423"), 0), null),
			("Applying Changes", async () =>  PowerApi.PowerSetActiveScheme(guid), null)
		];
	}
}