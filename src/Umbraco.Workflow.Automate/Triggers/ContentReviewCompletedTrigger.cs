using Umbraco.Automate.Core.Triggers;
using Umbraco.Workflow.Automate.Triggers.Outputs;
using Umbraco.Workflow.Core.ContentReviews.Notifications;

namespace Umbraco.Workflow.Automate.Triggers;

[Trigger("umbracoWorkflow.contentReviewCompleted", "Content Review Completed",
    Description = "Fires when a content review is completed.",
    Group = "Workflow",
    Icon = "icon-document",
    RequiredSections = [Constants.Sections.Workflow])]
public sealed class ContentReviewCompletedTrigger
    : NotificationTriggerBase<object, ContentReviewCompletedTriggerOutput, WorkflowContentReviewsReviewedNotification>
{
    public ContentReviewCompletedTrigger(TriggerInfrastructure infrastructure) : base(infrastructure) { }

    public override IEnumerable<TriggerEvent> MapEvent(WorkflowContentReviewsReviewedNotification notification)
    {
        var review = notification.ReviewedEntity;
        yield return new TriggerEvent<ContentReviewCompletedTriggerOutput>
        {
            TriggerAlias = Alias,
            InitiatorType = "system",
            Output = new ContentReviewCompletedTriggerOutput
            {
                DocumentKey = review.Document.Unique.ToString(),
                DocumentName = review.Document.Name ?? string.Empty,
                DueOn = review.DueOn,
                ReviewedOn = review.ReviewedOn,
            },
        };
    }
}
