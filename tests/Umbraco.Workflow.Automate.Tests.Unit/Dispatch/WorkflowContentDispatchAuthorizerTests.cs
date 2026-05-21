using Umbraco.Automate.Core.Dispatch.Authorization;
using Umbraco.Automate.Core.Security;
using Umbraco.Automate.Core.Triggers;
using Umbraco.Automate.Testing.Builders;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Workflow.Automate.Dispatch;
using Umbraco.Workflow.Automate.Triggers.Outputs;

namespace Umbraco.Workflow.Automate.Tests.Unit.Dispatch;

public class WorkflowContentDispatchAuthorizerTests
{
    private readonly Mock<IAutomationActionAuthorizer> _nodeAuthorizer = new();
    private readonly WorkflowContentDispatchAuthorizer _sut;

    public WorkflowContentDispatchAuthorizerTests()
    {
        _sut = new WorkflowContentDispatchAuthorizer(_nodeAuthorizer.Object);
    }

    [Fact]
    public async Task AuthorizeAsync_NonMarkerOutput_ReturnsSuccess()
    {
        // EmailSentTriggerOutput doesn't carry a content key — the authoriser must
        // short-circuit without calling IAutomationActionAuthorizer.
        var output = new EmailSentTriggerOutput
        {
            EmailType = "Approval",
            RecipientCount = 1,
            RecipientEmails = new[] { "editor@example.com" },
        };

        var result = await _sut.AuthorizeAsync(BuildContext(output), CancellationToken.None);

        result.Authorized.ShouldBeTrue();
        _nodeAuthorizer.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AuthorizeAsync_NullEntityKey_ReturnsSuccess()
    {
        // Workflows can target entities without a content key (e.g. settings nodes the
        // workflow extension supports). Absence of a key means "not applicable" — the
        // check would always fail otherwise and silently drop legitimate dispatches.
        var output = BuildInstanceOutput(entityKey: null);

        var result = await _sut.AuthorizeAsync(BuildContext(output), CancellationToken.None);

        result.Authorized.ShouldBeTrue();
        _nodeAuthorizer.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AuthorizeAsync_ContentReviewWithUnparseableKey_ReturnsSuccess()
    {
        // Defensive parse on the string-typed DocumentKey: garbage in means "skip" rather
        // than crash. Source is Document.Unique.ToString() so this is paranoid, but it
        // protects against future schema drift.
        var output = new ContentReviewCompletedTriggerOutput
        {
            DocumentKey = "not-a-guid",
            DocumentName = "Page",
            DueOn = DateTime.UtcNow,
            ReviewedOn = DateTime.UtcNow,
        };

        var result = await _sut.AuthorizeAsync(BuildContext(output), CancellationToken.None);

        result.Authorized.ShouldBeTrue();
        _nodeAuthorizer.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AuthorizeAsync_ContentReviewWithValidKey_DelegatesToContentAuthorizer()
    {
        var key = Guid.NewGuid();
        var output = new ContentReviewCompletedTriggerOutput
        {
            DocumentKey = key.ToString(),
            DocumentName = "Page",
            DueOn = DateTime.UtcNow,
            ReviewedOn = DateTime.UtcNow,
        };
        var user = Mock.Of<IUser>();

        _nodeAuthorizer
            .Setup(a => a.AuthorizeContentAsync(user, key, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AutomationAuthorizationResult.Success);

        var result = await _sut.AuthorizeAsync(BuildContext(output, user), CancellationToken.None);

        result.Authorized.ShouldBeTrue();
        _nodeAuthorizer.Verify(a => a.AuthorizeContentAsync(user, key, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthorizeAsync_WorkflowInstanceDenied_PropagatesFailureReason()
    {
        // Denial reason from IAutomationActionAuthorizer must surface so the dispatcher
        // log line names the actual block reason — matches built-in authoriser behaviour.
        var key = Guid.NewGuid();
        var output = BuildInstanceOutput(entityKey: key);

        _nodeAuthorizer
            .Setup(a => a.AuthorizeContentAsync(It.IsAny<IUser>(), key, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AutomationAuthorizationResult.Fail("Outside start-node path."));

        var result = await _sut.AuthorizeAsync(BuildContext(output), CancellationToken.None);

        result.Authorized.ShouldBeFalse();
        result.FailureReason.ShouldBe("Outside start-node path.");
    }

    [Fact]
    public async Task AuthorizeAsync_WorkflowCompletedWithKey_DelegatesToContentAuthorizer()
    {
        // Sanity: WorkflowCompletedTriggerOutput shares the marker via its EntityKey and
        // routes through the same path. Not testing every Workflow* output because they
        // all share WorkflowInstanceTriggerOutput; this confirms the second distinct type.
        var key = Guid.NewGuid();
        var output = new WorkflowCompletedTriggerOutput
        {
            NodeId = 1234,
            EntityKey = key,
            WorkflowType = "Publish",
            AuthorUserId = Guid.NewGuid(),
            AuthorComment = string.Empty,
            Culture = "en-US",
            TotalSteps = 1,
            CreatedDate = DateTime.UtcNow,
            CompletedDate = DateTime.UtcNow,
        };
        var user = Mock.Of<IUser>();

        _nodeAuthorizer
            .Setup(a => a.AuthorizeContentAsync(user, key, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AutomationAuthorizationResult.Success);

        var result = await _sut.AuthorizeAsync(BuildContext(output, user), CancellationToken.None);

        result.Authorized.ShouldBeTrue();
        _nodeAuthorizer.Verify(a => a.AuthorizeContentAsync(user, key, It.IsAny<IReadOnlySet<string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static WorkflowInstanceTriggerOutput BuildInstanceOutput(Guid? entityKey) => new()
    {
        NodeId = 1234,
        EntityKey = entityKey,
        WorkflowType = "Publish",
        WorkflowStatus = "Approved",
        AuthorUserId = Guid.NewGuid(),
        AuthorComment = string.Empty,
        Culture = "en-US",
        TotalSteps = 1,
        CreatedDate = DateTime.UtcNow,
    };

    private static TriggerDispatchAuthorizationContext BuildContext(object output, IUser? user = null)
        => new()
        {
            Trigger = Mock.Of<ITrigger>(),
            TypedOutput = output,
            ServiceAccount = user ?? Mock.Of<IUser>(),
            Automation = new AutomationBuilder().WithTrigger("any").Build(),
        };
}
