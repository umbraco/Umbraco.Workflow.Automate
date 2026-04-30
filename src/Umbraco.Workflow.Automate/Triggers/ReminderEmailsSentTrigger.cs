using Umbraco.Automate.Core.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.Email.Notifications;

namespace Umbraco.Workflow.Automate.Triggers;

[Trigger("umbracoworkflow.reminderEmailsSent", "Reminder Emails Sent",
    Description = "Fires when workflow reminder emails are sent to pending approvers.",
    Group = "Workflow",
    Icon = "icon-alarm-clock")]
public sealed class ReminderEmailsSentTrigger
    : NotificationTriggerBase<object, ReminderEmailsSentTriggerOutput, WorkflowEmailRemindersSentNotification>
{
    public ReminderEmailsSentTrigger(TriggerInfrastructure infrastructure) : base(infrastructure) { }

    public override IEnumerable<TriggerEvent> MapEvent(WorkflowEmailRemindersSentNotification notification)
    {
        yield return new TriggerEvent<ReminderEmailsSentTriggerOutput>
        {
            TriggerAlias = Alias,
            InitiatorType = "system",
            Output = new ReminderEmailsSentTriggerOutput
            {
                InstanceCount = notification.SentEntities.Count(),
                TaskCount = notification.Tasks.Count,
            },
        };
    }
}
