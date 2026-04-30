using Umbraco.Automate.Core.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.Models.Pocos;
using Umbraco.Workflow.Core.Notifications;

namespace Umbraco.Workflow.Automate.Triggers;

[Trigger("umbracoWorkflow.cancelled", "Workflow Cancelled",
    Description = "Fires when a workflow instance is cancelled.",
    Group = "Workflow",
    Icon = "icon-block")]
public sealed class WorkflowCancelledTrigger
    : NotificationTriggerBase<object, WorkflowInstanceTriggerOutput, WorkflowInstanceCancelledNotification>
{
    public WorkflowCancelledTrigger(TriggerInfrastructure infrastructure) : base(infrastructure) { }

    public override IEnumerable<TriggerEvent> MapEvent(WorkflowInstanceCancelledNotification notification)
    {
        if (notification.Target is not WorkflowInstancePoco instance)
        {
            yield break;
        }

        yield return new TriggerEvent<WorkflowInstanceTriggerOutput>
        {
            TriggerAlias = Alias,
            InitiatorType = "system",
            Output = new WorkflowInstanceTriggerOutput
            {
                NodeId = instance.NodeId,
                EntityKey = instance.EntityKey,
                WorkflowType = instance.WorkflowType.ToString(),
                WorkflowStatus = instance.WorkflowStatus.ToString(),
                AuthorUserId = instance.AuthorUserId,
                AuthorComment = instance.AuthorComment ?? string.Empty,
                Culture = instance.Culture ?? string.Empty,
                TotalSteps = instance.TotalSteps,
                CreatedDate = instance.CreatedDate,
            },
        };
    }
}
