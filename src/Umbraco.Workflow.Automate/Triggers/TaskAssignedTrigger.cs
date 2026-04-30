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
    public TaskAssignedTrigger(TriggerInfrastructure infrastructure) : base(infrastructure) { }

    public override IEnumerable<TriggerEvent> MapEvent(WorkflowTaskCreatedNotification notification)
    {
        if (notification.Target is not WorkflowTaskPoco task)
        {
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
