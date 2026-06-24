using Microsoft.Extensions.Logging;
using Umbraco.Automate.Core.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.ContentApprovals.Models;
using Umbraco.Workflow.Core.Notifications;

namespace Umbraco.Workflow.Automate.Triggers;

[Trigger("umbracoWorkflow.cancelled", "Workflow Cancelled",
    Description = "Fires when a workflow instance is cancelled.",
    Group = "Workflow",
    Icon = "icon-block",
    RequiredSections = [Constants.Sections.Workflow])]
public sealed class WorkflowCancelledTrigger
    : NotificationTriggerBase<object, WorkflowInstanceTriggerOutput, WorkflowInstanceCancelledNotification>
{
    private readonly ILogger<WorkflowCancelledTrigger> _logger;

    public WorkflowCancelledTrigger(TriggerInfrastructure infrastructure, ILogger<WorkflowCancelledTrigger> logger)
        : base(infrastructure)
    {
        _logger = logger;
    }

    public override IEnumerable<TriggerEvent> MapEvent(WorkflowInstanceCancelledNotification notification)
    {
        if (notification.UpdatedEntity is not WorkflowInstanceDto instance)
        {
            _logger.LogWarning(
                "{TriggerAlias}: expected {ExpectedType}, received {ActualType}; skipping.",
                Alias,
                nameof(WorkflowInstanceDto),
                notification.UpdatedEntity?.GetType().FullName ?? "null");
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
