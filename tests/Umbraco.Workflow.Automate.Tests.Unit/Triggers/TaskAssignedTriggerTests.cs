using Umbraco.Automate.Core.Settings;
using Umbraco.Automate.Core.Triggers;
using Umbraco.Cms.Core.Events;
using Umbraco.Workflow.Automate.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.Interfaces;
using Umbraco.Workflow.Core.Models.Enums;
using Umbraco.Workflow.Core.Models.Pocos;
using Umbraco.Workflow.Core.Notifications;
using WorkflowTaskStatus = Umbraco.Workflow.Core.Models.Enums.TaskStatus;

namespace Umbraco.Workflow.Automate.Tests.Unit.Triggers;

public class TaskAssignedTriggerTests
{
    private readonly TaskAssignedTrigger _trigger = new(
        new TriggerInfrastructure(Mock.Of<IEditableModelResolver>()));

    [Fact]
    public void MapEvent_ReturnsCorrectAlias()
    {
        var notification = BuildNotification();

        var events = _trigger.MapEvent(notification).ToList();

        events.ShouldHaveSingleItem();
        events[0].TriggerAlias.ShouldBe("umbracoworkflow.taskAssigned");
    }

    [Fact]
    public void MapEvent_ReturnsSystemInitiatorType()
    {
        var notification = BuildNotification();

        var events = _trigger.MapEvent(notification).ToList();

        events[0].InitiatorType.ShouldBe("system");
    }

    [Fact]
    public void MapEvent_MapsTaskProperties()
    {
        var groupId = Guid.NewGuid();
        var instanceGuid = Guid.NewGuid();
        var task = new WorkflowTaskPoco
        {
            ApprovalStep = 2,
            GroupId = groupId,
            WorkflowInstanceGuid = instanceGuid,
            Status = (int)WorkflowTaskStatus.PendingApproval,
        };
        var notification = new WorkflowTaskCreatedNotification(task, new EventMessages());

        var events = _trigger.MapEvent(notification).ToList();

        var output = ((TriggerEvent<TaskAssignedTriggerOutput>)events[0]).Output;
        output.ApprovalStep.ShouldBe(2);
        output.GroupId.ShouldBe(groupId);
        output.WorkflowInstanceGuid.ShouldBe(instanceGuid);
        output.TaskType.ShouldBe("PendingApproval");
    }

    [Fact]
    public void MapEvent_WithNullCast_UsesDefaults()
    {
        var task = Mock.Of<IWorkflowTask>();
        var notification = new WorkflowTaskCreatedNotification(task, new EventMessages());

        var events = _trigger.MapEvent(notification).ToList();

        var output = ((TriggerEvent<TaskAssignedTriggerOutput>)events[0]).Output;
        output.ApprovalStep.ShouldBe(0);
        output.GroupId.ShouldBeNull();
        output.WorkflowInstanceGuid.ShouldBe(Guid.Empty);
    }

    private static WorkflowTaskCreatedNotification BuildNotification()
    {
        var task = new WorkflowTaskPoco();
        return new WorkflowTaskCreatedNotification(task, new EventMessages());
    }
}
