using Umbraco.Workflow.Automate.Dispatch;

namespace Umbraco.Workflow.Automate.Triggers.Outputs;

public sealed class ContentReviewCompletedTriggerOutput : IContentScopedWorkflowOutput
{
    public required string DocumentKey { get; init; }
    public required string DocumentName { get; init; }
    public required DateTime DueOn { get; init; }
    public required DateTime ReviewedOn { get; init; }

    // DocumentKey is sourced from review.Document.Unique.ToString() — always a Guid at
    // the source. Defensive parse: if a future change widens the type and produces an
    // unparsable value, fall back to "no key" rather than throw and silently drop dispatch.
    Guid? IContentScopedWorkflowOutput.GetContentKey()
        => Guid.TryParse(DocumentKey, out var key) ? key : null;
}
