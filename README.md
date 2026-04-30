# Umbraco.Workflow.Automate

Connects [Umbraco Workflow](https://umbraco.com/products/umbraco-workflow/) to [Umbraco Automate](https://umbraco.com/products/umbraco-automate/), exposing workflow events as first-class triggers in Automate flows.

> **Built heavily with [Claude](https://claude.ai) (Anthropic)** — this package was designed and implemented with AI-assisted development as an experiment in how far you can take AI pair programming on a real Umbraco package.

---

## What's in the box

### Triggers

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

---

## Trigger Outputs

#### Workflow Started / Approved / Rejected / Cancelled / Resubmitted Output

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

#### Workflow Completed Output

All properties from above, plus:

| Property | Type | Description |
|---|---|---|
| `CompletedDate` | `DateTime?` | When the workflow finished |

#### Task Assigned Output

| Property | Type | Description |
|---|---|---|
| `ApprovalStep` | `int` | The step index (0-based) |
| `GroupId` | `Guid?` | The approval group assigned to this task |
| `WorkflowInstanceGuid` | `Guid` | The workflow instance this task belongs to |
| `TaskType` | `string` | The task status type |

#### Workflow Email Sent Output

| Property | Type | Description |
|---|---|---|
| `EmailType` | `string` | Type of email (e.g. `"ApprovalRequest"`, `"ApprovedNotification"`) |
| `RecipientCount` | `int` | Number of recipients |
| `RecipientEmails` | `IEnumerable<string>` | Recipient email addresses (null/empty entries are filtered out) |

#### Reminder Emails Sent Output

| Property | Type | Description |
|---|---|---|
| `InstanceCount` | `int` | Number of workflow instances reminders were sent for |
| `TaskCount` | `int` | Number of pending tasks across all instances |

#### Content Review Completed Output

| Property | Type | Description |
|---|---|---|
| `DocumentKey` | `string` | The unique key of the reviewed document |
| `DocumentName` | `string` | The name of the reviewed document |
| `DueOn` | `DateTime` | When the content review was due |
| `ReviewedOn` | `DateTime` | When the content review was completed |

---

## Installation

```bash
dotnet add package Umbraco.Workflow.Automate
```

`WorkflowAutomateComposer` is auto-discovered by Umbraco's composition system. No manual registration is required.

### Requirements

| Dependency | Version |
|---|---|
| .NET | 10 |
| Umbraco CMS | 17.x |
| Umbraco Workflow | 17.x |
| Umbraco Automate | 0.1+ |

---

## Usage Examples

### Notify a Slack channel when a workflow is rejected

```
Trigger: Workflow Rejected (umbracoWorkflow.rejected)
  → Action: Post Slack message to #content-team
            "Workflow rejected for node {NodeId}: {AuthorComment}"
```

### Send an alert when content review is overdue

```
Trigger: Reminder Emails Sent (umbracoWorkflow.reminderEmailsSent)
  → Condition: TaskCount > 5
  → Action: Send escalation email to content manager
```

### Log all completed publish workflows

```
Trigger: Workflow Completed (umbracoWorkflow.completed)
  → Condition: WorkflowType == "Publish"
  → Action: Write to external log
            "Published by {AuthorUserId} after {TotalSteps} steps"
```

### React when a specific approval group is assigned

```
Trigger: Task Assigned (umbracoWorkflow.taskAssigned)
  → Condition: GroupId == "your-group-guid"
  → Action: Send notification to group members
```

### Audit all outgoing workflow emails

```
Trigger: Workflow Email Sent (umbracoWorkflow.emailSent)
  → Action: Write recipients and email type to audit log
```

---

## How it works

Umbraco Workflow publishes notifications directly through Umbraco CMS's standard `IEventAggregator` (as `INotification`). Because of this, **no bridge handlers are required** — triggers subscribe to Workflow notifications natively, exactly like any other Umbraco CMS notification.

`WorkflowAutomateComposer` is minimal and exists only to ensure the assembly is discovered by Umbraco.

---

## Future Triggers (Not Yet Implemented)

The following Umbraco Workflow notifications exist and could be exposed as triggers in a future version:

| Notification | Description |
|---|---|
| `WorkflowInstanceCreatingNotification` | Cancelable — fires before a workflow starts |
| `WorkflowInstanceApprovingNotification` | Cancelable — fires before an approval is recorded |
| `WorkflowInstanceRejectingNotification` | Cancelable — fires before a rejection is recorded |
| `WorkflowInstanceCancellingNotification` | Cancelable — fires before a cancellation |
| `WorkflowInstanceResubmittingNotification` | Cancelable — fires before a resubmission |
| `WorkflowResubmitTaskCreatedNotification` | Fires when a resubmit task is created |
| `WorkflowTaskUpdatedNotification` | Fires when a task is updated |

> **Note on cancelable notifications:** These fire before the action occurs. Cancellation is not available through the Automate trigger system — they would fire for observation only.

---

## Future Actions (Not Yet Implemented)

The following Workflow operations could be exposed as Automate actions in a future version:

| Action | Description |
|---|---|
| Approve workflow step | Programmatically approve a pending task |
| Reject workflow step | Programmatically reject a pending task |
| Cancel workflow | Cancel a running workflow instance |
| Initiate workflow | Start a workflow for a given content node |
| Send reminder emails | Trigger reminder emails manually |

---

## Development

### Building

```bash
dotnet restore
dotnet build
dotnet test
```

### Project layout

```
src/
  Umbraco.Workflow.Automate/       # Package source
    Triggers/                      # Automate triggers
    Triggers/Outputs/              # Trigger output types
    WorkflowAutomateComposer.cs    # DI composition entry point (minimal — no bridge needed)
tests/
  Umbraco.Workflow.Automate.Tests.Unit/
    Triggers/                      # 34 unit tests covering all 10 triggers
```

### CI pipeline

The Azure Pipelines workflow (`azure-pipelines.yml`) builds, runs tests cross-platform (Windows, Linux, macOS), and packages to a pipeline artifact. **It does not push to NuGet** — publishing is a manual, deliberate step.

---

## License

[MIT](LICENSE)

---

> *This package was built heavily with [Claude](https://claude.ai) by Anthropic as part of an experiment in AI-assisted Umbraco package development. The architecture, implementation, tests, and documentation were all produced through an iterative conversation with Claude Code.*
