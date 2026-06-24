using Microsoft.Extensions.Logging.Abstractions;
using Umbraco.Automate.Core.Settings;
using Umbraco.Automate.Core.Triggers;
using Umbraco.Cms.Core.Events;
using Umbraco.Workflow.Automate.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.ContentApprovals.Interfaces;
using Umbraco.Workflow.Core.ContentApprovals.Models;
using Umbraco.Workflow.Core.Notifications;

namespace Umbraco.Workflow.Automate.Tests.Unit.Triggers;

public class WorkflowCancelledTriggerTests
{
    private readonly WorkflowCancelledTrigger _trigger = new(
        new TriggerInfrastructure(Mock.Of<IEditableModelResolver>()),
        NullLogger<WorkflowCancelledTrigger>.Instance);

    [Fact]
    public void MapEvent_ReturnsCorrectAlias()
    {
        var notification = BuildNotification();

        var events = _trigger.MapEvent(notification).ToList();

        events.ShouldHaveSingleItem();
        events[0].TriggerAlias.ShouldBe("umbracoWorkflow.cancelled");
    }

    [Fact]
    public void MapEvent_ReturnsSystemInitiatorType()
    {
        var notification = BuildNotification();

        var events = _trigger.MapEvent(notification).ToList();

        events[0].InitiatorType.ShouldBe("system");
    }

    [Fact]
    public void MapEvent_MapsInstanceProperties()
    {
        var entityKey = Guid.NewGuid();
        var instance = new WorkflowInstanceDto
        {
            NodeId = 77,
            EntityKey = entityKey,
            Type = (int)WorkflowType.Unpublish,
        };
        var notification = new WorkflowInstanceCancelledNotification(instance, new EventMessages());

        var events = _trigger.MapEvent(notification).ToList();

        var output = ((TriggerEvent<WorkflowInstanceTriggerOutput>)events[0]).Output;
        output.NodeId.ShouldBe(77);
        output.EntityKey.ShouldBe(entityKey);
        output.WorkflowType.ShouldBe("Unpublish");
    }

    [Fact]
    public void MapEvent_WithNonPocoTarget_YieldsNoEvent()
    {
        var instance = Mock.Of<IWorkflowInstance>();
        var notification = new WorkflowInstanceCancelledNotification(instance, new EventMessages());

        var events = _trigger.MapEvent(notification).ToList();

        events.ShouldBeEmpty();
    }

    private static WorkflowInstanceCancelledNotification BuildNotification()
    {
        var instance = new WorkflowInstanceDto { Type = (int)WorkflowType.Publish };
        return new WorkflowInstanceCancelledNotification(instance, new EventMessages());
    }
}
