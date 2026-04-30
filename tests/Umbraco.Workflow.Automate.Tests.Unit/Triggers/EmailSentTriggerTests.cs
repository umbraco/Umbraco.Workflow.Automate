using Umbraco.Automate.Core.Settings;
using Umbraco.Automate.Core.Triggers;
using Umbraco.Cms.Core.Events;
using Umbraco.Workflow.Automate.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.Email.Models;
using Umbraco.Workflow.Core.Email.Notifications;
using Umbraco.Workflow.Core.Models.Enums;

namespace Umbraco.Workflow.Automate.Tests.Unit.Triggers;

public class EmailSentTriggerTests
{
    private readonly EmailSentTrigger _trigger = new(
        new TriggerInfrastructure(Mock.Of<IEditableModelResolver>()));

    [Fact]
    public void MapEvent_ReturnsCorrectAlias()
    {
        var notification = BuildNotification([]);

        var events = _trigger.MapEvent(notification).ToList();

        events.ShouldHaveSingleItem();
        events[0].TriggerAlias.ShouldBe("umbracoworkflow.emailSent");
    }

    [Fact]
    public void MapEvent_ReturnsSystemInitiatorType()
    {
        var notification = BuildNotification([]);

        var events = _trigger.MapEvent(notification).ToList();

        events[0].InitiatorType.ShouldBe("system");
    }

    [Fact]
    public void MapEvent_MapsRecipientProperties()
    {
        var recipients = new List<EmailUserModel>
        {
            new() { Email = "alice@example.com" },
            new() { Email = "bob@example.com" },
            new() { Email = null },
        };
        var notification = BuildNotification(recipients, EmailType.ApprovalRequest);

        var events = _trigger.MapEvent(notification).ToList();

        var output = ((TriggerEvent<EmailSentTriggerOutput>)events[0]).Output;
        output.EmailType.ShouldBe("ApprovalRequest");
        output.RecipientCount.ShouldBe(3);
        output.RecipientEmails.Count.ShouldBe(2);
        output.RecipientEmails.ShouldContain("alice@example.com");
        output.RecipientEmails.ShouldContain("bob@example.com");
    }

    [Fact]
    public void MapEvent_WithNoRecipients_ReturnsEmptyEmailList()
    {
        var notification = BuildNotification([]);

        var events = _trigger.MapEvent(notification).ToList();

        var output = ((TriggerEvent<EmailSentTriggerOutput>)events[0]).Output;
        output.RecipientCount.ShouldBe(0);
        output.RecipientEmails.ShouldBeEmpty();
    }

    private static WorkflowEmailNotificationsSentNotification BuildNotification(
        List<EmailUserModel> recipients,
        EmailType emailType = EmailType.ApprovalRequest)
    {
        var emailModel = Mock.Of<Umbraco.Workflow.Core.Email.Interfaces.IHtmlEmailModel>();
        return new WorkflowEmailNotificationsSentNotification(emailModel, recipients, emailType, new EventMessages());
    }
}
