namespace Umbraco.Workflow.Automate.Triggers.Outputs;

public sealed class TaskAssignedTriggerOutput
{
    public required int ApprovalStep { get; init; }
    public required Guid? GroupId { get; init; }
    public required Guid WorkflowInstanceGuid { get; init; }
    public required string TaskType { get; init; }
}
