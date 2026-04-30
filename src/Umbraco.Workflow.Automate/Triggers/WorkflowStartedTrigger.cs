using Umbraco.Automate.Core.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.Models.Pocos;
using Umbraco.Workflow.Core.Notifications;

namespace Umbraco.Workflow.Automate.Triggers;

[Trigger("umbracoworkflow.started", "Workflow Started",
    Description = "Fires when a new workflow instance is created.",
    Group = "Workflow",
    Icon = "icon-activity")]
public sealed class WorkflowStartedTrigger
    : NotificationTriggerBase<object, WorkflowInstanceTriggerOutput, WorkflowInstanceCreatedNotification>
{
    public WorkflowStartedTrigger(TriggerInfrastructure infrastructure) : base(infrastructure) { }

    public override IEnumerable<TriggerEvent> MapEvent(WorkflowInstanceCreatedNotification notification)
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
