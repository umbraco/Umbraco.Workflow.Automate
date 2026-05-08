using Microsoft.Extensions.Logging;
using Umbraco.Automate.Core.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.Models.Pocos;
using Umbraco.Workflow.Core.Notifications;

namespace Umbraco.Workflow.Automate.Triggers;

[Trigger("umbracoWorkflow.resubmitted", "Workflow Resubmitted",
    Description = "Fires when a rejected workflow instance is resubmitted for approval.",
    Group = "Workflow",
    Icon = "icon-sync")]
public sealed class WorkflowResubmittedTrigger
    : NotificationTriggerBase<object, WorkflowInstanceTriggerOutput, WorkflowInstanceResubmittedNotification>
{
    private readonly ILogger<WorkflowResubmittedTrigger> _logger;

    public WorkflowResubmittedTrigger(TriggerInfrastructure infrastructure, ILogger<WorkflowResubmittedTrigger> logger)
        : base(infrastructure)
    {
        _logger = logger;
    }

    public override IEnumerable<TriggerEvent> MapEvent(WorkflowInstanceResubmittedNotification notification)
    {
        if (notification.UpdatedEntity is not WorkflowInstancePoco instance)
        {
            _logger.LogWarning(
                "{TriggerAlias}: expected {ExpectedType}, received {ActualType}; skipping.",
                Alias,
                nameof(WorkflowInstancePoco),
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
