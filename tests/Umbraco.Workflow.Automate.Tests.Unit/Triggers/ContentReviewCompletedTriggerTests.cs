using Umbraco.Automate.Core.Settings;
using Umbraco.Automate.Core.Triggers;
using Umbraco.Cms.Core.Events;
using Umbraco.Workflow.Automate.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.ContentReviews.Models;
using Umbraco.Workflow.Core.ContentReviews.Notifications;
using Umbraco.Workflow.Core.ViewModels;

namespace Umbraco.Workflow.Automate.Tests.Unit.Triggers;

public class ContentReviewCompletedTriggerTests
{
    private readonly ContentReviewCompletedTrigger _trigger = new(
        new TriggerInfrastructure(Mock.Of<IEditableModelResolver>()));

    [Fact]
    public void MapEvent_ReturnsCorrectAlias()
    {
        var notification = BuildNotification();

        var events = _trigger.MapEvent(notification).ToList();

        events.ShouldHaveSingleItem();
        events[0].TriggerAlias.ShouldBe("umbracoworkflow.contentReviewCompleted");
    }

    [Fact]
    public void MapEvent_ReturnsSystemInitiatorType()
    {
        var notification = BuildNotification();

        var events = _trigger.MapEvent(notification).ToList();

        events[0].InitiatorType.ShouldBe("system");
    }

    [Fact]
    public void MapEvent_MapsReviewProperties()
    {
        var documentKey = Guid.NewGuid();
        var dueOn = DateTime.UtcNow.AddDays(-7);
        var reviewedOn = DateTime.UtcNow;
        var review = new ContentReviewRequestModel
        {
            Document = new DocumentItemResponseModel
            {
                Unique = documentKey,
                Name = "Home Page",
                Culture = "en-US",
            },
            DueOn = dueOn,
            ReviewedOn = reviewedOn,
        };
        var notification = new WorkflowContentReviewsReviewedNotification(review, new EventMessages());

        var events = _trigger.MapEvent(notification).ToList();

        var output = ((TriggerEvent<ContentReviewCompletedTriggerOutput>)events[0]).Output;
        output.DocumentKey.ShouldBe(documentKey.ToString());
        output.DocumentName.ShouldBe("Home Page");
        output.DueOn.ShouldBe(dueOn);
        output.ReviewedOn.ShouldBe(reviewedOn);
    }

    [Fact]
    public void MapEvent_WithNullDocumentName_UsesEmptyString()
    {
        var review = new ContentReviewRequestModel
        {
            Document = new DocumentItemResponseModel
            {
                Unique = Guid.NewGuid(),
                Name = null,
                Culture = "en-US",
            },
            DueOn = DateTime.UtcNow,
        };
        var notification = new WorkflowContentReviewsReviewedNotification(review, new EventMessages());

        var events = _trigger.MapEvent(notification).ToList();

        var output = ((TriggerEvent<ContentReviewCompletedTriggerOutput>)events[0]).Output;
        output.DocumentName.ShouldBeEmpty();
    }

    private static WorkflowContentReviewsReviewedNotification BuildNotification()
    {
        var review = new ContentReviewRequestModel
        {
            Document = new DocumentItemResponseModel
            {
                Unique = Guid.NewGuid(),
                Name = "Test Document",
                Culture = "en-US",
            },
            DueOn = DateTime.UtcNow,
        };
        return new WorkflowContentReviewsReviewedNotification(review, new EventMessages());
    }
}
