using Microsoft.Extensions.Logging;
using Umbraco.Automate.Core.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.Models.Pocos;
using Umbraco.Workflow.Core.Notifications;

namespace Umbraco.Workflow.Automate.Triggers;

[Trigger("umbracoWorkflow.taskAssigned", "Task Assigned",
    Description = "Fires when a new workflow approval task is created and assigned.",
    Group = "Workflow",
    Icon = "icon-user")]
public sealed class TaskAssignedTrigger
    : NotificationTriggerBase<object, TaskAssignedTriggerOutput, WorkflowTaskCreatedNotification>
{
    private readonly ILogger<TaskAssignedTrigger> _logger;

    public TaskAssignedTrigger(TriggerInfrastructure infrastructure, ILogger<TaskAssignedTrigger> logger)
        : base(infrastructure)
    {
        _logger = logger;
    }

    public override IEnumerable<TriggerEvent> MapEvent(WorkflowTaskCreatedNotification notification)
    {
        if (notification.CreatedEntity is not WorkflowTaskPoco task)
        {
            _logger.LogWarning(
                "{TriggerAlias}: expected {ExpectedType}, received {ActualType}; skipping.",
                Alias,
                nameof(WorkflowTaskPoco),
                notification.CreatedEntity?.GetType().FullName ?? "null");
            yield break;
        }

        yield return new TriggerEvent<TaskAssignedTriggerOutput>
        {
            TriggerAlias = Alias,
            InitiatorType = "system",
            Output = new TaskAssignedTriggerOutput
            {
                ApprovalStep = task.ApprovalStep,
                GroupId = task.GroupId,
                WorkflowInstanceGuid = task.WorkflowInstanceGuid,
                TaskType = task.TaskStatus?.ToString() ?? string.Empty,
            },
        };
    }
}
