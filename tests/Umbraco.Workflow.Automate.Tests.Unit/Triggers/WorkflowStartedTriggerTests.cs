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

public class WorkflowStartedTriggerTests
{
    private readonly WorkflowStartedTrigger _trigger = new(
        new TriggerInfrastructure(Mock.Of<IEditableModelResolver>()));

    [Fact]
    public void MapEvent_ReturnsCorrectAlias()
    {
        var notification = BuildNotification();

        var events = _trigger.MapEvent(notification).ToList();

        events.ShouldHaveSingleItem();
        events[0].TriggerAlias.ShouldBe("umbracoWorkflow.started");
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
        var entityKey = Guid.NewGuid();
        var instance = new WorkflowInstancePoco
        {
            NodeId = 1234,
            EntityKey = entityKey,
            Type = (int)WorkflowType.Publish,
            AuthorUserId = authorId,
            AuthorComment = "Please review",
            Culture = "en-US",
            TotalSteps = 3,
        };
        var notification = new WorkflowInstanceCreatedNotification(instance, new EventMessages());

        var events = _trigger.MapEvent(notification).ToList();

        var output = ((TriggerEvent<WorkflowInstanceTriggerOutput>)events[0]).Output;
        output.NodeId.ShouldBe(1234);
        output.EntityKey.ShouldBe(entityKey);
        output.WorkflowType.ShouldBe("Publish");
        output.AuthorUserId.ShouldBe(authorId);
        output.AuthorComment.ShouldBe("Please review");
        output.Culture.ShouldBe("en-US");
        output.TotalSteps.ShouldBe(3);
    }

    [Fact]
    public void MapEvent_WithNonPocoTarget_YieldsNoEvent()
    {
        var instance = Mock.Of<IWorkflowInstance>(x => x.Type == (int)WorkflowType.Publish && x.WorkflowType == WorkflowType.Publish);
        var notification = new WorkflowInstanceCreatedNotification(instance, new EventMessages());

        var events = _trigger.MapEvent(notification).ToList();

        events.ShouldBeEmpty();
    }

    private static WorkflowInstanceCreatedNotification BuildNotification()
    {
        var instance = new WorkflowInstancePoco { Type = (int)WorkflowType.Publish };
        return new WorkflowInstanceCreatedNotification(instance, new EventMessages());
    }
}
