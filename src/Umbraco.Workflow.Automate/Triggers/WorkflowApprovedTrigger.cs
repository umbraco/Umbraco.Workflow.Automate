using Umbraco.Automate.Core.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.Models.Pocos;
using Umbraco.Workflow.Core.Notifications;

namespace Umbraco.Workflow.Automate.Triggers;

[Trigger("umbracoworkflow.approved", "Workflow Approved",
    Description = "Fires when a workflow instance is approved.",
    Group = "Workflow",
    Icon = "icon-thumb-up")]
public sealed class WorkflowApprovedTrigger
    : NotificationTriggerBase<object, WorkflowInstanceTriggerOutput, WorkflowInstanceApprovedNotification>
{
    public WorkflowApprovedTrigger(TriggerInfrastructure infrastructure) : base(infrastructure) { }

    public override IEnumerable<TriggerEvent> MapEvent(WorkflowInstanceApprovedNotification notification)
    {
        var instance = notification.Target as WorkflowInstancePoco;
        yield return new TriggerEvent<WorkflowInstanceTriggerOutput>
        {
            TriggerAlias = Alias,
            InitiatorType = "system",
            Output = new WorkflowInstanceTriggerOutput
            {
                NodeId = instance?.NodeId ?? 0,
                EntityKey = instance?.EntityKey,
                WorkflowType = notification.Target.WorkflowType.ToString(),
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
