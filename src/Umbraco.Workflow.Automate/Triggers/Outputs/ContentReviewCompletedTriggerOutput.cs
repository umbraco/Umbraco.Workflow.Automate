namespace Umbraco.Workflow.Automate.Triggers.Outputs;

public sealed class ContentReviewCompletedTriggerOutput
{
    public required string DocumentKey { get; init; }
    public required string DocumentName { get; init; }
    public required DateTime DueOn { get; init; }
    public required DateTime ReviewedOn { get; init; }
}
