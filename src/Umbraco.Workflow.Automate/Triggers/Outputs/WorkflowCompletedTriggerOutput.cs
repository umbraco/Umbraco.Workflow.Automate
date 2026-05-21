using Umbraco.Workflow.Automate.Dispatch;

namespace Umbraco.Workflow.Automate.Triggers.Outputs;

public sealed class WorkflowCompletedTriggerOutput : IContentScopedWorkflowOutput
{
    public required int NodeId { get; init; }
    public required Guid? EntityKey { get; init; }
    public required string WorkflowType { get; init; }
    public required Guid AuthorUserId { get; init; }
    public required string AuthorComment { get; init; }
    public required string Culture { get; init; }
    public required int TotalSteps { get; init; }
    public required DateTime CreatedDate { get; init; }
    public required DateTime? CompletedDate { get; init; }

    Guid? IContentScopedWorkflowOutput.GetContentKey() => EntityKey;
}
