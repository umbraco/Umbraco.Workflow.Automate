using Umbraco.Automate.Core.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.Models.Pocos;
using Umbraco.Workflow.Core.Notifications;

namespace Umbraco.Workflow.Automate.Triggers;

[Trigger("umbracoworkflow.resubmitted", "Workflow Resubmitted",
    Description = "Fires when a rejected workflow instance is resubmitted for approval.",
    Group = "Workflow",
    Icon = "icon-sync")]
public sealed class WorkflowResubmittedTrigger
    : NotificationTriggerBase<object, WorkflowInstanceTriggerOutput, WorkflowInstanceResubmittedNotification>
{
    public WorkflowResubmittedTrigger(TriggerInfrastructure infrastructure) : base(infrastructure) { }

    public override IEnumerable<TriggerEvent> MapEvent(WorkflowInstanceResubmittedNotification notification)
    {
        var instance = notification.UpdatedEntity as WorkflowInstancePoco;
        yield return new TriggerEvent<WorkflowInstanceTriggerOutput>
        {
            TriggerAlias = Alias,
            InitiatorType = "system",
            Output = new WorkflowInstanceTriggerOutput
            {
                NodeId = instance?.NodeId ?? 0,
                EntityKey = instance?.EntityKey,
                WorkflowType = notification.UpdatedEntity.WorkflowType.ToString(),
                WorkflowStatus = instance?.WorkflowStatus.ToString() ?? string.Empty,
                AuthorUserId = instance?.AuthorUserId ?? Guid.Empty,
                AuthorComment = instance?.AuthorComment ?? string.Empty,
                Culture = instance?.Culture ?? string.Empty,
                TotalSteps = instance?.TotalSteps ?? 0,
                CreatedDate = instance?.CreatedDate ?? DateTime.UtcNow,
            },
        };
    }
}
