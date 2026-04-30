using Umbraco.Automate.Core.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.Email.Notifications;

namespace Umbraco.Workflow.Automate.Triggers;

[Trigger("umbracoworkflow.emailSent", "Workflow Email Sent",
    Description = "Fires when a workflow notification email is sent.",
    Group = "Workflow",
    Icon = "icon-bell")]
public sealed class EmailSentTrigger
    : NotificationTriggerBase<object, EmailSentTriggerOutput, WorkflowEmailNotificationsSentNotification>
{
    public EmailSentTrigger(TriggerInfrastructure infrastructure) : base(infrastructure) { }

    public override IEnumerable<TriggerEvent> MapEvent(WorkflowEmailNotificationsSentNotification notification)
    {
        var emails = notification.Recipients
            .Select(r => r.Email ?? string.Empty)
            .Where(e => !string.IsNullOrEmpty(e))
            .ToList();

        yield return new TriggerEvent<EmailSentTriggerOutput>
        {
            TriggerAlias = Alias,
            InitiatorType = "system",
            Output = new EmailSentTriggerOutput
            {
                EmailType = notification.EmailType.ToString(),
                RecipientCount = notification.Recipients.Count,
                RecipientEmails = emails,
            },
        };
    }
}
