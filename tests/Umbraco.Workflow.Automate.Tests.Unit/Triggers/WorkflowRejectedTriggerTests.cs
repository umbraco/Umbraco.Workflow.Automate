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

public class WorkflowRejectedTriggerTests
{
    private readonly WorkflowRejectedTrigger _trigger = new(
        new TriggerInfrastructure(Mock.Of<IEditableModelResolver>()),
        NullLogger<WorkflowRejectedTrigger>.Instance);

    [Fact]
    public void MapEvent_ReturnsCorrectAlias()
    {
        var notification = BuildNotification();

        var events = _trigger.MapEvent(notification).ToList();

        events.ShouldHaveSingleItem();
        events[0].TriggerAlias.ShouldBe("umbracoWorkflow.rejected");
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
        var instance = new WorkflowInstanceDto
        {
            NodeId = 55,
            Type = (int)WorkflowType.Publish,
            AuthorComment = "Needs rework",
            Culture = "da",
        };
        var notification = new WorkflowInstanceRejectedNotification(instance, new EventMessages());

        var events = _trigger.MapEvent(notification).ToList();

        var output = ((TriggerEvent<WorkflowInstanceTriggerOutput>)events[0]).Output;
        output.NodeId.ShouldBe(55);
        output.AuthorComment.ShouldBe("Needs rework");
        output.Culture.ShouldBe("da");
    }

    [Fact]
    public void MapEvent_WithNonPocoTarget_YieldsNoEvent()
    {
        var instance = Mock.Of<IWorkflowInstance>();
        var notification = new WorkflowInstanceRejectedNotification(instance, new EventMessages());

        var events = _trigger.MapEvent(notification).ToList();

        events.ShouldBeEmpty();
    }

    private static WorkflowInstanceRejectedNotification BuildNotification()
    {
        var instance = new WorkflowInstanceDto { Type = (int)WorkflowType.Publish };
        return new WorkflowInstanceRejectedNotification(instance, new EventMessages());
    }
}
