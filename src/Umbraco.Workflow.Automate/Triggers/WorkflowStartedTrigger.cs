using Microsoft.Extensions.Logging;
using Umbraco.Automate.Core.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.ContentApprovals.Models;
using Umbraco.Workflow.Core.Notifications;

namespace Umbraco.Workflow.Automate.Triggers;

[Trigger("umbracoWorkflow.started", "Workflow Started",
    Description = "Fires when a new workflow instance is created.",
    Group = "Workflow",
    Icon = "icon-activity",
    RequiredSections = [Constants.Sections.Workflow])]
public sealed class WorkflowStartedTrigger
    : NotificationTriggerBase<object, WorkflowInstanceTriggerOutput, WorkflowInstanceCreatedNotification>
{
    private readonly ILogger<WorkflowStartedTrigger> _logger;

    public WorkflowStartedTrigger(TriggerInfrastructure infrastructure, ILogger<WorkflowStartedTrigger> logger)
        : base(infrastructure)
    {
        _logger = logger;
    }

    public override IEnumerable<TriggerEvent> MapEvent(WorkflowInstanceCreatedNotification notification)
    {
        if (notification.CreatedEntity is not WorkflowInstanceDto instance)
        {
            _logger.LogWarning(
                "{TriggerAlias}: expected {ExpectedType}, received {ActualType}; skipping.",
                Alias,
                nameof(WorkflowInstanceDto),
                notification.CreatedEntity?.GetType().FullName ?? "null");
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
