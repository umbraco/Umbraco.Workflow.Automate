using Umbraco.Automate.Core.Settings;
using Umbraco.Automate.Core.Triggers;
using Umbraco.Cms.Core.Events;
using Umbraco.Workflow.Automate.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.Models.Enums;
using Umbraco.Workflow.Core.Models.Pocos;
using Umbraco.Workflow.Core.Notifications;

namespace Umbraco.Workflow.Automate.Tests.Unit.Triggers;

public class WorkflowCompletedTriggerTests
{
    private readonly WorkflowCompletedTrigger _trigger = new(
        new TriggerInfrastructure(Mock.Of<IEditableModelResolver>()));

    [Fact]
    public void MapEvent_ReturnsCorrectAlias()
    {
        var notification = BuildNotification();

        var events = _trigger.MapEvent(notification).ToList();

        events.ShouldHaveSingleItem();
        events[0].TriggerAlias.ShouldBe("umbracoworkflow.completed");
    }

    [Fact]
    public void MapEvent_ReturnsSystemInitiatorType()
    {
        var notification = BuildNotification();

        var events = _trigger.MapEvent(notification).ToList();

        events[0].InitiatorType.ShouldBe("system");
    }

    [Fact]
    public void MapEvent_MapsCompletedProperties()
    {
        var completedDate = DateTime.UtcNow;
        var instance = new WorkflowInstancePoco
        {
            NodeId = 42,
            Type = (int)WorkflowType.Unpublish,
            TotalSteps = 2,
            CompletedDate = completedDate,
        };
        var notification = new WorkflowInstanceCompletedNotification(instance, new EventMessages());

        var events = _trigger.MapEvent(notification).ToList();

        var output = ((TriggerEvent<WorkflowCompletedTriggerOutput>)events[0]).Output;
        output.NodeId.ShouldBe(42);
        output.WorkflowType.ShouldBe("Unpublish");
        output.TotalSteps.ShouldBe(2);
        output.CompletedDate.ShouldBe(completedDate);
    }

    private static WorkflowInstanceCompletedNotification BuildNotification()
    {
        var instance = new WorkflowInstancePoco { Type = (int)WorkflowType.Publish };
        return new WorkflowInstanceCompletedNotification(instance, new EventMessages());
    }
}
