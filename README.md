<p align="center">
  <img alt="Umbraco Automate" src="./assets/logo-128.png" width="128">
</p>

# Umbraco.Workflow.Automate

Umbraco Workflow triggers for [Umbraco Automate](https://umbraco.com/products/umbraco-automate/).

## Overview

Umbraco.Workflow.Automate is a provider package that connects [Umbraco Workflow](https://umbraco.com/products/umbraco-workflow/) to Umbraco Automate, exposing workflow events as first-class triggers in Automate flows — for example, notifying a channel when a workflow is rejected, or escalating when reminder emails pile up.

## Key Features

- **10 triggers** — react to workflow lifecycle, task assignment, email, and content review events
- **Rich trigger outputs** — node IDs, entity keys, workflow types and statuses, authors, comments, and timestamps
- **Native notifications** — Workflow publishes standard CMS notifications, so no bridge handlers are required
- **Zero configuration** — `WorkflowAutomateComposer` self-registers with Umbraco's composition pipeline

## Installation

```bash
dotnet add package Umbraco.Workflow.Automate
```

No further wiring is required — the composer is auto-discovered by Umbraco's composition system.

## Requirements

- .NET 10.0
- Umbraco CMS 17.x
- Umbraco Workflow 17.x
- Umbraco.Automate 17.0+

## Triggers

Fire an Automate flow when something happens in Workflow.

**Workflow Lifecycle**

| Trigger | Fires when… |
|---|---|
| Workflow Started | A new workflow instance is created |
| Workflow Completed | A workflow instance completes successfully |
| Workflow Approved | A workflow approval step is approved |
| Workflow Rejected | A workflow approval step is rejected |
| Workflow Cancelled | A workflow instance is cancelled |
| Workflow Resubmitted | A rejected workflow is resubmitted for approval |

**Tasks**

| Trigger | Fires when… |
|---|---|
| Task Assigned | A new approval task is created and assigned to a group |

**Emails**

| Trigger | Fires when… |
|---|---|
| Workflow Email Sent | A workflow notification email is sent |
| Reminder Emails Sent | Reminder emails are sent to pending approvers |

**Content Reviews**

| Trigger | Fires when… |
|---|---|
| Content Review Completed | A content review is completed |

## Trigger Outputs

### Workflow Started / Approved / Rejected / Cancelled / Resubmitted

| Property | Type | Description |
|---|---|---|
| `NodeId` | `int` | The node ID of the content being approved |
| `EntityKey` | `Guid?` | The unique key of the content entity |
| `WorkflowType` | `string` | `"Publish"` or `"Unpublish"` |
| `WorkflowStatus` | `string` | Current status (e.g. `"PendingApproval"`, `"Approved"`) |
| `AuthorUserId` | `Guid` | The user who initiated the workflow |
| `AuthorComment` | `string` | Comment left by the author |
| `Culture` | `string` | The culture variant (empty string for invariant) |
| `TotalSteps` | `int` | Total number of approval steps |
| `CreatedDate` | `DateTime` | When the workflow was started |

### Workflow Completed

All properties from above, plus:

| Property | Type | Description |
|---|---|---|
| `CompletedDate` | `DateTime?` | When the workflow finished |

### Task Assigned

| Property | Type | Description |
|---|---|---|
| `ApprovalStep` | `int` | The step index (0-based) |
| `GroupId` | `Guid?` | The approval group assigned to this task |
| `WorkflowInstanceGuid` | `Guid` | The workflow instance this task belongs to |
| `TaskType` | `string` | The task status type |

### Workflow Email Sent

| Property | Type | Description |
|---|---|---|
| `EmailType` | `string` | Type of email (e.g. `"ApprovalRequest"`, `"ApprovedNotification"`) |
| `RecipientCount` | `int` | Number of recipients |
| `RecipientEmails` | `IEnumerable<string>` | Recipient email addresses |

### Reminder Emails Sent

| Property | Type | Description |
|---|---|---|
| `InstanceCount` | `int` | Number of workflow instances reminders were sent for |
| `TaskCount` | `int` | Number of pending tasks across all instances |

### Content Review Completed

| Property | Type | Description |
|---|---|---|
| `DocumentKey` | `string` | The unique key of the reviewed document |
| `DocumentName` | `string` | The name of the reviewed document |
| `DueOn` | `DateTime` | When the content review was due |
| `ReviewedOn` | `DateTime` | When the content review was completed |

## Usage Examples

### Notify a Slack channel when a workflow is rejected

```
Trigger: Workflow Rejected
  → Action: Post Slack message to #content-team
            "Workflow rejected for node {NodeId}: {AuthorComment}"
```

### Log all completed publish workflows

```
Trigger: Workflow Completed
  → Condition: WorkflowType == "Publish"
  → Action: Write to external log
            "Published by {AuthorUserId} after {TotalSteps} steps"
```

### React when a specific approval group is assigned

```
Trigger: Task Assigned
  → Condition: GroupId == "your-group-guid"
  → Action: Send notification to group members
```

## How It Works

Umbraco Workflow publishes notifications directly through Umbraco CMS's standard `IEventAggregator` (as `INotification`), so triggers subscribe to Workflow notifications natively — no bridge handlers are required. `WorkflowAutomateComposer` is minimal and exists only to ensure the assembly is discovered by Umbraco.

## Development

```bash
dotnet restore
dotnet build
dotnet test
```

### Project layout

```
src/
  Umbraco.Workflow.Automate/         # Package source
    Triggers/                        # Automate triggers
    Triggers/Outputs/                # Trigger output types
tests/
  Umbraco.Workflow.Automate.Tests.Unit/
```

## License

MIT — see [LICENSE](LICENSE) for details.
