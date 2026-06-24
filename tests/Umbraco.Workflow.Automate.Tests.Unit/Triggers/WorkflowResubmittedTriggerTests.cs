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

public class WorkflowResubmittedTriggerTests
{
    private readonly WorkflowResubmittedTrigger _trigger = new(
        new TriggerInfrastructure(Mock.Of<IEditableModelResolver>()),
        NullLogger<WorkflowResubmittedTrigger>.Instance);

    [Fact]
    public void MapEvent_ReturnsCorrectAlias()
    {
        var notification = BuildNotification();

        var events = _trigger.MapEvent(notification).ToList();

        events.ShouldHaveSingleItem();
        events[0].TriggerAlias.ShouldBe("umbracoWorkflow.resubmitted");
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
        var instance = new WorkflowInstanceDto
        {
            NodeId = 100,
            Type = (int)WorkflowType.Publish,
            AuthorUserId = authorId,
            AuthorComment = "Resubmitting after fixes",
            TotalSteps = 2,
        };
        var notification = new WorkflowInstanceResubmittedNotification(instance, new EventMessages());

        var events = _trigger.MapEvent(notification).ToList();

        var output = ((TriggerEvent<WorkflowInstanceTriggerOutput>)events[0]).Output;
        output.NodeId.ShouldBe(100);
        output.WorkflowType.ShouldBe("Publish");
        output.AuthorUserId.ShouldBe(authorId);
        output.AuthorComment.ShouldBe("Resubmitting after fixes");
        output.TotalSteps.ShouldBe(2);
    }

    [Fact]
    public void MapEvent_WithNonPocoTarget_YieldsNoEvent()
    {
        var instance = Mock.Of<IWorkflowInstance>();
        var notification = new WorkflowInstanceResubmittedNotification(instance, new EventMessages());

        var events = _trigger.MapEvent(notification).ToList();

        events.ShouldBeEmpty();
    }

    private static WorkflowInstanceResubmittedNotification BuildNotification()
    {
        var instance = new WorkflowInstanceDto { Type = (int)WorkflowType.Publish };
        return new WorkflowInstanceResubmittedNotification(instance, new EventMessages());
    }
}
