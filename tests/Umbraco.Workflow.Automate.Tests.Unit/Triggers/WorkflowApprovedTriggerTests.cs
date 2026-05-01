using Microsoft.Extensions.Logging.Abstractions;
using Umbraco.Automate.Core.Settings;
using Umbraco.Automate.Core.Triggers;
using Umbraco.Cms.Core.Events;
using Umbraco.Workflow.Automate.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.Interfaces;
using Umbraco.Workflow.Core.Models.Enums;
using Umbraco.Workflow.Core.Models.Pocos;
using Umbraco.Workflow.Core.Notifications;

namespace Umbraco.Workflow.Automate.Tests.Unit.Triggers;

public class WorkflowApprovedTriggerTests
{
    private readonly WorkflowApprovedTrigger _trigger = new(
        new TriggerInfrastructure(Mock.Of<IEditableModelResolver>()),
        NullLogger<WorkflowApprovedTrigger>.Instance);

    [Fact]
    public void MapEvent_ReturnsCorrectAlias()
    {
        var notification = BuildNotification();

        var events = _trigger.MapEvent(notification).ToList();

        events.ShouldHaveSingleItem();
        events[0].TriggerAlias.ShouldBe("umbracoWorkflow.approved");
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
        var authorId = Guid.NewGuid();
        var instance = new WorkflowInstancePoco
        {
            NodeId = 99,
            Type = (int)WorkflowType.Publish,
            AuthorUserId = authorId,
            TotalSteps = 1,
        };
        var notification = new WorkflowInstanceApprovedNotification(instance, new EventMessages());

        var events = _trigger.MapEvent(notification).ToList();

        var output = ((TriggerEvent<WorkflowInstanceTriggerOutput>)events[0]).Output;
        output.NodeId.ShouldBe(99);
        output.WorkflowType.ShouldBe("Publish");
        output.AuthorUserId.ShouldBe(authorId);
        output.TotalSteps.ShouldBe(1);
    }

    [Fact]
    public void MapEvent_WithNonPocoTarget_YieldsNoEvent()
    {
        var instance = Mock.Of<IWorkflowInstance>();
        var notification = new WorkflowInstanceApprovedNotification(instance, new EventMessages());

        var events = _trigger.MapEvent(notification).ToList();

        events.ShouldBeEmpty();
    }

    private static WorkflowInstanceApprovedNotification BuildNotification()
    {
        var instance = new WorkflowInstancePoco { Type = (int)WorkflowType.Publish };
        return new WorkflowInstanceApprovedNotification(instance, new EventMessages());
    }
}
