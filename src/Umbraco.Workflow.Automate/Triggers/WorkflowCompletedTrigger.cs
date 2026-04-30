using Umbraco.Automate.Core.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.Models.Pocos;
using Umbraco.Workflow.Core.Notifications;

namespace Umbraco.Workflow.Automate.Triggers;

[Trigger("umbracoWorkflow.completed", "Workflow Completed",
    Description = "Fires when a workflow instance completes successfully.",
    Group = "Workflow",
    Icon = "icon-check")]
public sealed class WorkflowCompletedTrigger
    : NotificationTriggerBase<object, WorkflowCompletedTriggerOutput, WorkflowInstanceCompletedNotification>
{
    public WorkflowCompletedTrigger(TriggerInfrastructure infrastructure) : base(infrastructure) { }

    public override IEnumerable<TriggerEvent> MapEvent(WorkflowInstanceCompletedNotification notification)
    {
        if (notification.CompletedInstance is not WorkflowInstancePoco instance)
        {
            yield break;
        }

        yield return new TriggerEvent<WorkflowCompletedTriggerOutput>
        {
            TriggerAlias = Alias,
            InitiatorType = "system",
            Output = new WorkflowCompletedTriggerOutput
            {
                NodeId = instance.NodeId,
                EntityKey = instance.EntityKey,
                WorkflowType = notification.WorkflowType.ToString(),
                AuthorUserId = instance.AuthorUserId,
                AuthorComment = instance.AuthorComment ?? string.Empty,
                Culture = instance.Culture ?? string.Empty,
                TotalSteps = instance.TotalSteps,
                CreatedDate = instance.CreatedDate,
                CompletedDate = instance.CompletedDate,
            },
        };
    }
}
