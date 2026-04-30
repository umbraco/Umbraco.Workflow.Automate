# Umbraco.Workflow.Automate

Exposes [Umbraco Workflow](https://docs.umbraco.com/umbraco-workflow) events as triggers for [Umbraco Automate](https://github.com/umbraco/Umbraco.Automate).

> **Heavily built with [Claude](https://claude.ai)** — this package was designed and implemented with the help of Claude Code (Anthropic's AI coding assistant).

---

## Triggers

### Must-Have Triggers

| Trigger | Alias | Description |
|---|---|---|
| Workflow Started | `umbracoworkflow.started` | Fires when a new workflow instance is created |
| Workflow Completed | `umbracoworkflow.completed` | Fires when a workflow instance completes successfully |
| Workflow Approved | `umbracoworkflow.approved` | Fires when a workflow step is approved |
| Workflow Rejected | `umbracoworkflow.rejected` | Fires when a workflow step is rejected |
| Workflow Cancelled | `umbracoworkflow.cancelled` | Fires when a workflow instance is cancelled |
| Task Assigned | `umbracoworkflow.taskAssigned` | Fires when a new approval task is created and assigned |

### Nice-to-Have Triggers

| Trigger | Alias | Description |
|---|---|---|
| Workflow Resubmitted | `umbracoworkflow.resubmitted` | Fires when a rejected workflow is resubmitted |
| Workflow Email Sent | `umbracoworkflow.emailSent` | Fires when a workflow notification email is sent |
| Reminder Emails Sent | `umbracoworkflow.reminderEmailsSent` | Fires when reminder emails are sent to pending approvers |
| Content Review Completed | `umbracoworkflow.contentReviewCompleted` | Fires when a content review is completed |

---

## Trigger Outputs

### WorkflowInstanceTriggerOutput
Shared by: Workflow Started, Approved, Rejected, Cancelled, Resubmitted

| Property | Type | Description |
|---|---|---|
| `NodeId` | `int` | The node ID of the content being approved |
| `EntityKey` | `Guid?` | The unique key of the content entity |
| `WorkflowType` | `string` | `Publish` or `Unpublish` |
| `WorkflowStatus` | `string` | Current status (e.g. `PendingApproval`) |
| `AuthorUserId` | `Guid` | The user who initiated the workflow |
| `AuthorComment` | `string` | Comment left by the author |
| `Culture` | `string` | The culture variant (empty string for invariant) |
| `TotalSteps` | `int` | Total number of approval steps |
| `CreatedDate` | `DateTime` | When the workflow was started |

### WorkflowCompletedTriggerOutput
Extends WorkflowInstanceTriggerOutput with:

| Property | Type | Description |
|---|---|---|
| `CompletedDate` | `DateTime?` | When the workflow completed |

### TaskAssignedTriggerOutput

| Property | Type | Description |
|---|---|---|
| `ApprovalStep` | `int` | The step index (0-based) |
| `GroupId` | `Guid?` | The approval group assigned to this task |
| `WorkflowInstanceGuid` | `Guid` | The workflow instance this task belongs to |
| `TaskType` | `string` | The task status type |

### EmailSentTriggerOutput

| Property | Type | Description |
|---|---|---|
| `EmailType` | `string` | The type of email (e.g. `ApprovalRequest`, `ApprovedNotification`) |
| `RecipientCount` | `int` | Number of recipients |
| `RecipientEmails` | `IEnumerable<string>` | Email addresses (excludes group emails and null addresses) |

### ReminderEmailsSentTriggerOutput

| Property | Type | Description |
|---|---|---|
| `InstanceCount` | `int` | Number of workflow instances for which reminders were sent |
| `TaskCount` | `int` | Number of pending tasks across all instances |

### ContentReviewCompletedTriggerOutput

| Property | Type | Description |
|---|---|---|
| `DocumentKey` | `string` | The unique key of the reviewed document |
| `DocumentName` | `string` | The name of the reviewed document |
| `DueOn` | `DateTime` | When the content review was due |
| `ReviewedOn` | `DateTime` | When the content review was completed |

---

## Future Triggers

The following Umbraco Workflow notifications exist and could be exposed as triggers in a future version:

| Notification | Description |
|---|---|
| `WorkflowInstanceCreatingNotification` | Cancelable — fires before a workflow starts |
| `WorkflowInstanceApprovingNotification` | Cancelable — fires before an approval |
| `WorkflowInstanceRejectingNotification` | Cancelable — fires before a rejection |
| `WorkflowInstanceCancellingNotification` | Cancelable — fires before a cancellation |
| `WorkflowInstanceResubmittingNotification` | Cancelable — fires before a resubmission |
| `WorkflowResubmitTaskCreatedNotification` | Fires when a resubmit task is created |
| `WorkflowTaskUpdatedNotification` | Fires when a task is updated |

---

## Future Actions

The following Workflow operations could be exposed as Automate actions:

- **Approve a workflow step** — programmatically approve a pending task
- **Reject a workflow step** — programmatically reject a pending task
- **Cancel a workflow** — cancel a running workflow instance
- **Create a workflow** — initiate a workflow for a given node
- **Send reminder emails** — trigger reminder emails manually

---

## Usage Examples

### Notify a Slack channel when a workflow is rejected

1. Trigger: **Workflow Rejected** (`umbracoworkflow.rejected`)
2. Action: Post message to Slack with `AuthorComment` and `NodeId` from the output

### Log all completed publish workflows

1. Trigger: **Workflow Completed** (`umbracoworkflow.completed`)
2. Filter: `WorkflowType == "Publish"`
3. Action: Write to log or external system

### Alert when a review is overdue

1. Trigger: **Reminder Emails Sent** (`umbracoworkflow.reminderEmailsSent`)
2. Filter: `TaskCount > 5`
3. Action: Send escalation notification

---

## Installation

This package is not yet published on NuGet. For local development, add a project reference:

```xml
<ProjectReference Include="path/to/Umbraco.Workflow.Automate.csproj" />
```

### Requirements

- Umbraco CMS 17.x
- Umbraco Workflow 17.x
- Umbraco Automate 0.1.0+

---

## Project Structure

```
src/
  Umbraco.Workflow.Automate/
    Triggers/
      Outputs/               # Output model classes
      WorkflowStartedTrigger.cs
      WorkflowCompletedTrigger.cs
      WorkflowApprovedTrigger.cs
      WorkflowRejectedTrigger.cs
      WorkflowCancelledTrigger.cs
      WorkflowResubmittedTrigger.cs
      TaskAssignedTrigger.cs
      EmailSentTrigger.cs
      ReminderEmailsSentTrigger.cs
      ContentReviewCompletedTrigger.cs
    WorkflowAutomateComposer.cs

tests/
  Umbraco.Workflow.Automate.Tests.Unit/
    Triggers/                # 34 unit tests covering all triggers
```
