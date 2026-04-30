using Umbraco.Automate.Core.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.Models.Pocos;
using Umbraco.Workflow.Core.Notifications;

namespace Umbraco.Workflow.Automate.Triggers;

[Trigger("umbracoworkflow.taskAssigned", "Task Assigned",
    Description = "Fires when a new workflow approval task is created and assigned.",
    Group = "Workflow",
    Icon = "icon-user")]
public sealed class TaskAssignedTrigger
    : NotificationTriggerBase<object, TaskAssignedTriggerOutput, WorkflowTaskCreatedNotification>
{
    public TaskAssignedTrigger(TriggerInfrastructure infrastructure) : base(infrastructure) { }

    public override IEnumerable<TriggerEvent> MapEvent(WorkflowTaskCreatedNotification notification)
    {
        var task = notification.Target as WorkflowTaskPoco;
        yield return new TriggerEvent<TaskAssignedTriggerOutput>
        {
            TriggerAlias = Alias,
            InitiatorType = "system",
            Output = new TaskAssignedTriggerOutput
            {
                ApprovalStep = task?.ApprovalStep ?? 0,
                GroupId = task?.GroupId,
                WorkflowInstanceGuid = task?.WorkflowInstanceGuid ?? Guid.Empty,
                TaskType = task?.TaskStatus?.ToString() ?? string.Empty,
            },
        };
    }
}
