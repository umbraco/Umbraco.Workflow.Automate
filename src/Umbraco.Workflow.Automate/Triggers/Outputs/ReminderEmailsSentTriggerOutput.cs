namespace Umbraco.Workflow.Automate.Triggers.Outputs;

public sealed class ReminderEmailsSentTriggerOutput
{
    public required int InstanceCount { get; init; }
    public required int TaskCount { get; init; }
}
