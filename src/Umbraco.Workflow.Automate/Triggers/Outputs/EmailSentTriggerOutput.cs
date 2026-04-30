namespace Umbraco.Workflow.Automate.Triggers.Outputs;

public sealed class EmailSentTriggerOutput
{
    public required string EmailType { get; init; }
    public required int RecipientCount { get; init; }
    public required IEnumerable<string> RecipientEmails { get; init; }
}
