using Umbraco.Automate.Core.Settings;
using Umbraco.Automate.Core.Triggers;
using Umbraco.Cms.Core.Events;
using Umbraco.Workflow.Automate.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.Email.Models;
using Umbraco.Workflow.Core.Email.Notifications;
using Umbraco.Workflow.Core.Interfaces;
using Umbraco.Workflow.Core.Models.Pocos;

namespace Umbraco.Workflow.Automate.Tests.Unit.Triggers;

public class ReminderEmailsSentTriggerTests
{
    private readonly ReminderEmailsSentTrigger _trigger = new(
        new TriggerInfrastructure(Mock.Of<IEditableModelResolver>()));

    [Fact]
    public void MapEvent_ReturnsCorrectAlias()
    {
        var notification = BuildNotification([], []);

        var events = _trigger.MapEvent(notification).ToList();

        events.ShouldHaveSingleItem();
        events[0].TriggerAlias.ShouldBe("umbracoWorkflow.reminderEmailsSent");
    }

    [Fact]
    public void MapEvent_ReturnsSystemInitiatorType()
    {
        var notification = BuildNotification([], []);

        var events = _trigger.MapEvent(notification).ToList();

        events[0].InitiatorType.ShouldBe("system");
    }

    [Fact]
    public void MapEvent_MapsCounts()
    {
        var instances = new List<IWorkflowInstance>
        {
            new WorkflowInstancePoco(),
            new WorkflowInstancePoco(),
        };
        var tasks = new List<EmailTaskListModel>
        {
            new(new EmailUserModel { Email = "user@example.com" }),
            new(new EmailUserModel { Email = "admin@example.com" }),
            new(new EmailUserModel { Email = "editor@example.com" }),
        };
        var notification = BuildNotification(instances, tasks);

        var events = _trigger.MapEvent(notification).ToList();

        var output = ((TriggerEvent<ReminderEmailsSentTriggerOutput>)events[0]).Output;
        output.InstanceCount.ShouldBe(2);
        output.TaskCount.ShouldBe(3);
    }

    private static WorkflowEmailRemindersSentNotification BuildNotification(
        IEnumerable<IWorkflowInstance> instances,
        List<EmailTaskListModel> tasks)
    {
        return new WorkflowEmailRemindersSentNotification(instances, tasks, new EventMessages());
    }
}
