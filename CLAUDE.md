# CLAUDE.md

Guidance for Claude Code when working in this repository.

## Architecture

Umbraco.Workflow.Automate is a **provider package** that bridges [Umbraco Workflow](https://umbraco.com/products/umbraco-workflow/) (content-approval workflows) into [Umbraco Automate](https://umbraco.com/products/umbraco-automate/) (Umbraco's automation/flow product), by exposing Workflow's CMS notifications as Automate **triggers**. It contributes **no actions** — only triggers. It is one of six `Umbraco.*.Automate` satellite packages (Commerce, Forms, DXP, Engage, UIBuilder are the others) that all follow the same shape.

- **Target framework**: `net10.0` (see `Directory.Build.props:3`), C# with `Nullable` and `ImplicitUsings` enabled.
- **SDK pin**: `global.json` pins the .NET SDK to `10.0.100` with `rollForward: latestFeature`.
- **Application type**: Umbraco CMS extension library (packed as a NuGet package, no host application/demo site in this repo).
- **Composition**: A single `IComposer` (`src/Umbraco.Workflow.Automate/WorkflowAutomateComposer.cs`) is the only explicit registration point; individual triggers self-register via the `[Trigger(...)]` attribute that Automate's own composition scans for. There is no `builder.Services.AddX()` ceremony beyond the composer.
- **No bridge handlers needed**: Umbraco Workflow's own notifications already implement `INotification` and flow through the standard Umbraco CMS `IEventAggregator`/notification pipeline, so triggers are plain `NotificationHandler`-style classes subscribing directly to Workflow's notifications — unlike some Automate satellites that need adapter/bridge handlers to translate a third-party event model into something Automate-compatible.

### Solution structure

```
src/Umbraco.Workflow.Automate/
  WorkflowAutomateComposer.cs          # IComposer — registers the dispatch authorizer
  Constants.cs                         # Section alias constant (Umb.Section.Workflow)
  Dispatch/
    IContentScopedWorkflowOutput.cs    # Marker: trigger output exposes a content key
    WorkflowContentDispatchAuthorizer.cs  # ITriggerDispatchAuthorizer for content-scoped outputs
  Triggers/
    <Name>Trigger.cs                   # 10 triggers, one per Workflow lifecycle event
    Outputs/
      <Name>TriggerOutput.cs           # Plain output DTOs, some implementing the marker above

tests/Umbraco.Workflow.Automate.Tests.Unit/
  Triggers/<Name>TriggerTests.cs
  Dispatch/WorkflowContentDispatchAuthorizerTests.cs
```

There is no `demo/` site, no EF Core/migrations, no web API surface, and no `.editorconfig` in this repo — style is inherited from the shared Umbraco tooling and there's nothing project-specific to override.

### Design pattern: one trigger = one notification

Every trigger in `src/Umbraco.Workflow.Automate/Triggers/` follows the exact same shape:

```csharp
[Trigger("umbracoWorkflow.<alias>", "<Display Name>",
    Description = "...", Group = "Workflow", Icon = "icon-...",
    RequiredSections = [Constants.Sections.Workflow])]
public sealed class XTrigger : NotificationTriggerBase<object, XTriggerOutput, XNotification>
{
    public override IEnumerable<TriggerEvent> MapEvent(XNotification notification) { ... }
}
```

`NotificationTriggerBase<TFilter, TOutput, TNotification>` comes from `Umbraco.Automate.Core.Triggers` (the core Automate package this repo depends on, not something defined here). The 10 triggers (see `src/Umbraco.Workflow.Automate/Triggers/*.cs`):

| Trigger class | Alias | Notification subscribed to |
|---|---|---|
| `WorkflowStartedTrigger` | `umbracoWorkflow.started` | `WorkflowInstanceCreatedNotification` |
| `WorkflowCompletedTrigger` | `umbracoWorkflow.completed` | `WorkflowInstanceCompletedNotification` |
| `WorkflowApprovedTrigger` | `umbracoWorkflow.approved` | `WorkflowInstanceApprovedNotification` |
| `WorkflowRejectedTrigger` | `umbracoWorkflow.rejected` | `WorkflowInstanceRejectedNotification` |
| `WorkflowCancelledTrigger` | `umbracoWorkflow.cancelled` | `WorkflowInstanceCancelledNotification` |
| `WorkflowResubmittedTrigger` | `umbracoWorkflow.resubmitted` | `WorkflowInstanceResubmittedNotification` |
| `TaskAssignedTrigger` | `umbracoWorkflow.taskAssigned` | `WorkflowTaskCreatedNotification` |
| `EmailSentTrigger` | `umbracoWorkflow.emailSent` | `WorkflowEmailNotificationsSentNotification` |
| `ReminderEmailsSentTrigger` | `umbracoWorkflow.reminderEmailsSent` | `WorkflowEmailRemindersSentNotification` |
| `ContentReviewCompletedTrigger` | `umbracoWorkflow.contentReviewCompleted` | `WorkflowContentReviewsReviewedNotification` |

Five of them (`Started`/`Approved`/`Rejected`/`Cancelled`/`Resubmitted`) share `WorkflowInstanceTriggerOutput`; `Completed` has its own near-identical output (`WorkflowCompletedTriggerOutput`) that drops `WorkflowStatus` and adds `CompletedDate` instead. This duplication is intentional, not an oversight — see Clean Code below.

### Constructor pattern: `ILogger` only where there's something to log

Not every trigger takes an `ILogger<T>`. The seven triggers whose `MapEvent` has a type-check guard clause (`WorkflowStartedTrigger`, `WorkflowCompletedTrigger`, `WorkflowApprovedTrigger`, `WorkflowRejectedTrigger`, `WorkflowCancelledTrigger`, `WorkflowResubmittedTrigger`, `TaskAssignedTrigger`) inject `ILogger<TSelf>` and log a warning on mismatch. The other three (`EmailSentTrigger`, `ReminderEmailsSentTrigger`, `ContentReviewCompletedTrigger`) take only `TriggerInfrastructure` because their source notifications don't carry a loosely-typed entity that needs a runtime check — there's nothing to log. When adding a trigger, only add the logger dependency if `MapEvent` actually has a guard clause; don't inject it "for consistency" if it will never be used, and don't skip it if the notification's payload is loosely typed.

## Commands

All commands run from the repo root against the `.slnx` solution file (`Umbraco.Workflow.Automate.slnx`).

```bash
# Restore
dotnet restore Umbraco.Workflow.Automate.slnx

# Build
dotnet build Umbraco.Workflow.Automate.slnx --configuration Release

# Test (all frameworks/OSes run this same command in CI)
dotnet test Umbraco.Workflow.Automate.slnx --configuration Release

# Test a single fixture
dotnet test Umbraco.Workflow.Automate.slnx --filter "FullyQualifiedName~WorkflowStartedTriggerTests"

# Pack (produces the NuGet package CI publishes as a pipeline artifact)
dotnet pack Umbraco.Workflow.Automate.slnx --configuration Release --output ./artifacts
```

There are no EF Core migrations, no `dotnet run`/`dotnet watch` targets (this is a library, not a host app), and no user-secrets configuration — nothing in this repo talks to a database or external API directly.

### Package management

Package versions are centrally managed (`Directory.Packages.props`, `ManagePackageVersionsCentrally=true` + `CentralPackageFloatingVersionsEnabled=true`) — individual `.csproj` files only declare `<PackageReference Include="X" />` with no `Version` attribute. To change a version, edit `Directory.Packages.props`, not the `.csproj`:

```bash
# List what's actually resolved
dotnet list Umbraco.Workflow.Automate.slnx package

# Check for outdated/vulnerable packages
dotnet list Umbraco.Workflow.Automate.slnx package --outdated
dotnet list Umbraco.Workflow.Automate.slnx package --vulnerable
```

The `Umbraco.Automate`, `.Core`, and `.Testing` ranges (`[18.0.0, 18.999.999)` on `main`) are the ones the release-management skill rewrites per release — see Teamwork and Workflow. `Umbraco.Workflow.Core`'s range is maintained separately and is not touched by that skill.

### Prerequisites

- .NET SDK `10.0.100`+ (`global.json` uses `rollForward: latestFeature`, so a later 10.0.x patch is fine)
- NuGet feeds configured per `nuget.config`: `nuget.org`, `Umbraco Nightly` (myget.org), `Umbraco Prereleases` (myget.org) — package source mapping routes all `Umbraco*` packages through the two Umbraco feeds first, everything else through nuget.org.
- No local Umbraco CMS instance is required to build/test this repo (no demo site exists here) — the test project mocks Workflow's notification types directly rather than spinning up a CMS.

### Versioning

`version.json` (Nerdbank.GitVersioning) currently reads `18.0.1` on `main`. `publicReleaseRefSpec` only treats `main`, `hotfix/*`, and `release/*` as public — builds on other branches (e.g. `feature/*`) get a `-preview` suffix' worth of prerelease versioning automatically. Don't hand-edit version numbers except as part of the release process below.

## Style Guide

Nothing unusual — standard modern C#, no custom `.editorconfig`. Two patterns worth knowing because they're used consistently across every trigger/output in this repo:

- **All trigger and output classes are `sealed`** (e.g. `src/Umbraco.Workflow.Automate/Triggers/WorkflowStartedTrigger.cs:14`). There is no inheritance hierarchy among triggers beyond the shared `NotificationTriggerBase<...>` base from Automate.Core — don't introduce a project-local intermediate base class for the sake of DRYing up the near-identical `MapEvent` bodies; the duplication is deliberate (see Clean Code).
- **Output DTOs use `required init` properties exclusively** (e.g. `src/Umbraco.Workflow.Automate/Triggers/Outputs/WorkflowInstanceTriggerOutput.cs:7-15`) — no mutable setters, no constructors. Follow this for any new output type.
- **XML doc `<remarks>` comments explain *why*, not *what*** — see `WorkflowAutomateComposer.cs:11-15` and `Dispatch/IContentScopedWorkflowOutput.cs:8-12` for the pattern: a one-line summary plus a remarks block justifying the design decision. Match this style rather than terse summary-only comments when adding non-obvious code.

## Test Bench

- **Framework**: xUnit + Moq + Shouldly (`tests/Umbraco.Workflow.Automate.Tests.Unit/Umbraco.Workflow.Automate.Tests.Unit.csproj`). `Umbraco.Automate.Testing` supplies shared test builders (e.g. `AutomationBuilder` used in `tests/.../Dispatch/WorkflowContentDispatchAuthorizerTests.cs:164`).
- **Location**: one test file per trigger under `tests/Umbraco.Workflow.Automate.Tests.Unit/Triggers/`, plus `Dispatch/WorkflowContentDispatchAuthorizerTests.cs` for the authorizer. There is no integration/E2E test project — everything here is a fast in-memory unit test against `MapEvent`/`AuthorizeAsync` directly, no CMS bootstrapping.
- **Run**: `dotnet test Umbraco.Workflow.Automate.slnx` (CI runs this on Windows, Linux, and macOS in parallel — see `.devops/test.yml:8-15`).
- **Trigger test pattern**: every `<Name>TriggerTests.cs` asserts three things per trigger — alias is correct, `InitiatorType` is `"system"`, and notification fields map onto output fields 1:1 — plus edge-case tests for null/empty string fallback and for a notification whose payload isn't the expected DTO type (see `WorkflowStartedTriggerTests.cs:69-98`). When adding a new trigger, mirror this structure rather than inventing a new one.
- **What to focus on when changing triggers**: the type-check-and-log-warning-then-`yield break` guard clause (see Error Handling) is the part most likely to regress silently — always add a test asserting `events.ShouldBeEmpty()` for the wrong-payload-type case, matching the existing tests.
- **Coverage**: `coverlet.collector` is referenced (`tests/.../Umbraco.Workflow.Automate.Tests.Unit.csproj:9-12`) but CI doesn't currently collect/publish coverage output — `.devops/test.yml` runs plain `dotnet test` with a `trx` logger only. If coverage reporting is ever wanted in CI, it needs to be added to `.devops/test.yml`, not assumed to already exist.
- **Full test inventory** (one file per production class, all under `tests/Umbraco.Workflow.Automate.Tests.Unit/`): `Triggers/WorkflowStartedTriggerTests.cs`, `WorkflowCompletedTriggerTests.cs`, `WorkflowApprovedTriggerTests.cs`, `WorkflowRejectedTriggerTests.cs`, `WorkflowCancelledTriggerTests.cs`, `WorkflowResubmittedTriggerTests.cs`, `TaskAssignedTriggerTests.cs`, `EmailSentTriggerTests.cs`, `ReminderEmailsSentTriggerTests.cs`, `ContentReviewCompletedTriggerTests.cs`, and `Dispatch/WorkflowContentDispatchAuthorizerTests.cs`. There is intentionally no test for `WorkflowAutomateComposer` or `Constants` — both are trivial registration/constant classes with no branching logic to cover.

## Error Handling

- **Defensive notification payload checks, not exceptions.** Seven of the ten triggers receive a notification whose entity property is typed loosely (`object`/a shared base) by Umbraco Workflow Core, so each `MapEvent` pattern-matches to the expected DTO (`WorkflowInstanceDto`, `WorkflowTaskDto`) and, on mismatch, logs a warning and does `yield break` instead of throwing — see `src/Umbraco.Workflow.Automate/Triggers/WorkflowStartedTrigger.cs:27-35`. This means a malformed/future-incompatible Workflow payload silently produces **no trigger event** rather than crashing the notification pipeline (which would also break every *other* subscriber to that notification). If you touch this logic, preserve the "skip and log, never throw" contract.
- **Defensive parsing over trusting upstream types.** `ContentReviewCompletedTriggerOutput.GetContentKey()` (`src/Umbraco.Workflow.Automate/Triggers/Outputs/ContentReviewCompletedTriggerOutput.cs:15-16`) does a `Guid.TryParse` on `DocumentKey` even though the source is always a `Guid.ToString()` today — the comment explains this is paranoia against a future schema-widening, not a currently-reachable code path. Follow this precedent for any new content-scoped output: parse defensively, fail to "no key" (treated as not-applicable), never throw.
- **`InitiatorType` is always `"system"`** for every trigger event in this package — Workflow events are never user-initiated from Automate's perspective, so there's no branching logic here to replicate elsewhere.

## Clean Code

- **`IContentScopedWorkflowOutput` marker interface** (`src/Umbraco.Workflow.Automate/Dispatch/IContentScopedWorkflowOutput.cs`) is `internal` by design — it's a private contract between this package's outputs and its own `WorkflowContentDispatchAuthorizer`. Third-party packages needing the same content-scoping behavior are expected to declare their own marker, not depend on this one. Don't make it `public` to "help" another package; that's an explicit non-goal per the XML doc remarks.
- **Deliberate duplication over premature abstraction**: `WorkflowInstanceTriggerOutput` and `WorkflowCompletedTriggerOutput` are structurally almost identical (swaps `WorkflowStatus` for `CompletedDate`), and five triggers have near-identical `MapEvent` bodies (`WorkflowStartedTrigger`, `WorkflowApprovedTrigger`, `WorkflowRejectedTrigger`, `WorkflowCancelledTrigger`, `WorkflowResubmittedTrigger` all map the same nine fields from `WorkflowInstanceDto`). This is intentional — each trigger is a distinct, independently-versionable public contract (trigger alias + output shape are part of the package's public API surface for Automate flow authors). Do not refactor these into a shared generic base "to reduce duplication"; that would couple unrelated triggers' output schemas together and make it harder to evolve one without affecting the others.
- **`WorkflowContentDispatchAuthorizer` mirrors, but does not extend, Automate's built-in authorizer.** The XML doc on the class (`Dispatch/WorkflowContentDispatchAuthorizer.cs:7-12`) explains why: Automate.Core's own `NodeScopedTriggerDispatchAuthorizer` only recognizes Automate's own content/media trigger output types, and its marker is `internal` to Automate.Core, so this package can't reuse it — it re-implements the same start-node-path + Browse-permission check against its own marker interface instead. If Automate.Core ever exposes its marker publicly, this duplication could be revisited, but don't assume that's landed without checking the `Umbraco.Automate.Core` package version in use.

## Security

- **Dispatch-time authorization, not action-time**: `WorkflowContentDispatchAuthorizer` (`src/Umbraco.Workflow.Automate/Dispatch/WorkflowContentDispatchAuthorizer.cs`) is registered via `builder.AutomateTriggerDispatchAuthorizers().Add<...>()` in the composer, gating whether a triggered event is even dispatched to an Automate flow — it checks the flow's configured **service account** against the target content node's start-node path and `Browse` permission (`ActionBrowse.ActionLetter`). This prevents a flow's service account from reacting to (and thereby leaking information about) content it isn't scoped to see.
- **"No content key" is treated as "not applicable", not "denied"** — both for outputs that don't implement `IContentScopedWorkflowOutput` at all (e.g. `EmailSentTriggerOutput`, `ReminderEmailsSentTriggerOutput`, `TaskAssignedTriggerOutput`) and for content-scoped outputs whose `GetContentKey()` returns `null` (e.g. workflows running against non-document entities like settings nodes). This is a considered choice, not an oversight: failing closed here would silently drop legitimate dispatches for entity types the authorizer was never meant to gate. See the comment at `Dispatch/WorkflowContentDispatchAuthorizer.cs:35-37` and the corresponding test at `tests/.../WorkflowContentDispatchAuthorizerTests.cs:39-51`.
- **`RequiredSections = [Constants.Sections.Workflow]`** on every `[Trigger]` attribute means a trigger simply won't appear in the Automate flow designer for a backoffice user without access to the Workflow section (`Umb.Section.Workflow`, defined in `src/Umbraco.Workflow.Automate/Constants.cs:17`) — this is section-level gating at authoring time, separate from the dispatch-time node authorization above.
- No secrets, connection strings, or auth tokens live in this repo — it has no configuration surface of its own.

## Teamwork and Workflow

**Repository**: GitHub, `github.com/umbraco/Umbraco.Workflow.Automate` (verified via `git remote -v`). Built in Azure DevOps under project **"Umbraco Workflow"** (pipeline definition 735).

**Branch model** (verified against actual branches):
- `main` — current CMS major line, **v18** (`version.json` → `18.0.1`).
- `support/17.x` — previous CMS major line, **v17** (checked out as a persistent worktree at `.claude/worktrees/support-17.x`, `version.json` → `17.0.1`). The v17 line pins `Umbraco.Workflow.Core` to `[17.3.2, 17.999.999)` rather than `17.0.0` (see `.claude/worktrees/support-17.x/Directory.Packages.props:22`) — the floor was bumped past `17.0.0`, presumably for a fix this package needs; don't drop it back to `17.0.0` without checking why it's there.
- Other live branches at time of writing: `docs/readme-v18`, `feature/cms-v18-prep`, `feature/code-review` — short-lived work branches off `main`.
- Releases are cut as `release/YYYY.MM.N` branches, where `N` is a **counter shared across both CMS lines in a given month** (e.g. a v18 release and a v17 release in the same month get consecutive numbers, not independent per-line counters).

**CI** (`azure-pipelines.yml` → `.devops/build-and-pack.yml` + `.devops/test.yml`): triggers on push to `main`, `dev`, `release/*`, `hotfix/*`, `feature/*`, and on PRs into `main`/`dev`. Two stages only — **Build & Pack** (restore, `nbgv cloud` for version stamping, `dotnet build`, `dotnet pack`, optional SBOM generation/upload to Dependency-Track via `cdxgen`) and **Test** (matrix across Windows/Linux/macOS). **There is no Publish stage** — nothing in CI pushes the packed `.nupkg` anywhere. Publishing to the MyGet feed is a manual step a human performs after confirming the CI run for a `release/*` branch is green.

**Release process** — two Claude Code skills in `.claude/skills/` automate the mechanics; read them for exact commands rather than re-deriving the steps:
- `.claude/skills/release-management/SKILL.md` — cuts a `release/YYYY.MM.N` branch from the correct base (`main` for v18, `support/17.x` for v17), bumps `Directory.Packages.props`'s `Umbraco.Automate`/`.Core`/`.Testing` ranges to `[X.0.0, X.999.999)` for the target major `X`, and sets `version.json` to the stable `X.0.0`.
- `.claude/skills/post-release-cleanup/SKILL.md` — run only after CI is green **and** a human has manually confirmed the package was pushed to MyGet. Merges the release branch back `--no-ff`, tags the merge commit `release-<version>` (e.g. `release-18.0.0`), creates a GitHub Release via `gh release create <tag> --target <branch> --generate-notes`, patch-bumps `version.json` on the target branch for next-cycle nightlies, and deletes the release branch (local + remote).

**No `CONTRIBUTING.md` or PR template exists in this repo** — commit history shows a loose Conventional-Commits-flavored style (`build(deps): ...`, `chore(release): ...`, `docs: ...`, `ci: ...`) and PRs merged via GitHub's default merge commit message (`Merge pull request #N from umbraco/<branch>`); there's no stricter convention to enforce beyond matching what's already in `git log`.

**No changelog, no release manifest, no dependency cascade** — unlike the core `Umbraco.Automate` repo, this is a single-product satellite; the two skills above explicitly call out that adding changelog generation or manifest logic here is out of scope.

## Edge Cases

- **Notification payload isn't the expected DTO type.** Umbraco Workflow's notifications carry their entity as a loosely-typed property (`CreatedEntity`, `UpdatedEntity`, `CompletedInstance` — all effectively `object`/an interface), so a future Workflow Core change, a third-party override, or a mock in a test can hand a trigger something other than `WorkflowInstanceDto`/`WorkflowTaskDto`. Every affected trigger (`WorkflowStartedTrigger`, `WorkflowCompletedTrigger`, `WorkflowApprovedTrigger`, `WorkflowRejectedTrigger`, `WorkflowCancelledTrigger`, `WorkflowResubmittedTrigger`, `TaskAssignedTrigger`) handles this by logging a warning and yielding no event — see `WorkflowStartedTriggerTests.cs:89-98` for the covering test (`Mock.Of<IWorkflowInstance>` standing in for the "wrong type" case).
- **Workflow instance/task with no content key.** Workflow can run against entities that aren't CMS documents (e.g. settings nodes). `IContentScopedWorkflowOutput.GetContentKey()` returning `null` is the documented signal for "not applicable" — the dispatch authorizer treats this as success, not denial (see Security above).
- **`EntityKey`/`AuthorComment`/`Culture` can be `null` on the source DTO** even though the output DTO declares them as non-nullable `required` strings — every trigger normalizes with `?? string.Empty` at the mapping boundary (e.g. `WorkflowStartedTrigger.cs:48-49`). If you add a new field sourced from `WorkflowInstanceDto`, check whether it's nullable upstream and apply the same normalization rather than letting a `NullReferenceException` surface.
- **`ContentReviewCompletedTriggerOutput.DocumentKey` is a `string`, not a `Guid`**, sourced from `review.Document.Unique.ToString()` — this is a public output field, so widening it to `Guid` later would be a breaking change for existing Automate flows that reference `{DocumentKey}` as a string. The `Guid.TryParse` defensive parse exists specifically to tolerate this string typing without crashing (see Error Handling).

## Agentic Workflow

- **Adding a new trigger** means: (1) find the Workflow Core notification you want to react to (`Umbraco.Workflow.Core.Notifications`, `.Email.Notifications`, or `.ContentReviews.Notifications`), (2) add an output DTO under `Triggers/Outputs/` using `required init` properties only, (3) decide if the output is content-scoped — if the event has a resolvable content/document key, implement `IContentScopedWorkflowOutput` so dispatch authorization applies automatically; if not, don't, (4) write the trigger class following the existing pattern exactly (attribute, base class, guard-clause-and-log for loosely-typed payloads), (5) write a test file mirroring `WorkflowStartedTriggerTests.cs`'s structure (alias, initiator type, field mapping, null-fallback, wrong-type-yields-nothing).
- **Before assuming a Workflow Core type/notification exists**, check which CMS major line you're targeting — `main` targets v18 and uses the `ContentApprovals.*` namespace (`WorkflowInstanceDto`, `WorkflowTaskDto` under `Umbraco.Workflow.Core.ContentApprovals.Models`/`.Interfaces`); `support/17.x` predates that consolidation and uses `Umbraco.Workflow.Core.Models.Pocos`/`.Models.Enums` (`WorkflowInstancePoco`, etc.). Don't port a v18 trigger to `support/17.x` (or vice versa) without checking the namespace/type names actually match on that line — see Project-Specific Notes.
- **Quality gates before considering a change done**: `dotnet build` clean, `dotnet test` green (all three OSes matter less locally, but the logic must not depend on OS), and a new/changed trigger's test file must cover the wrong-payload-type case if the trigger has one — that's the one behavior in this codebase that's easy to silently regress (turning a graceful skip into an unhandled exception, or vice versa turning a should-fire case into a silent skip).
- **Don't reach for the `.claude/worktrees/support-17.x` worktree unless the task explicitly concerns the v17 line** — it's a separate checkout of `support/17.x`, not a subdirectory of `main`'s code; editing it edits a different branch's history.
- **Common pitfall**: assuming this package has "actions" because the shared Automate satellite pattern often includes them. It doesn't — check `src/Umbraco.Workflow.Automate/` before assuming an `Actions/` folder exists or should be added; this package is triggers-only by design (Workflow is an event source, not something Automate flows act *upon*).

## Project-Specific Notes

- **External integration: `Umbraco.Workflow.Core`.** This package's entire purpose is translating Workflow Core's own CMS notifications into Automate trigger events. It does not call into Workflow Core's services or APIs beyond reading notification payloads — it's a pure listener, which is why there's no `HttpClient`, no repository/service abstraction, and no caching anywhere in `src/`.
- **The v17 → v18 `ContentApprovals` namespace migration is a completed, load-bearing fact, not a TODO.** `main` (v18) imports `Umbraco.Workflow.Core.ContentApprovals.Models`/`.Interfaces` and uses `WorkflowInstanceDto`/`WorkflowTaskDto`; the `support/17.x` worktree imports `Umbraco.Workflow.Core.Models.Pocos`/`.Models.Enums` and uses the pre-consolidation POCO type names (`WorkflowInstancePoco`, etc.). These are two genuinely different type surfaces, not a rename you can find-and-replace across branches — always verify the actual type/namespace on the branch you're editing (`grep -rn "Umbraco.Workflow.Core" src/` on that checkout) rather than assuming v18 conventions apply on `support/17.x` or vice versa.
- **Zero-configuration installation is a deliberate selling point.** The README (`README.md:18-26`) advertises "no further wiring required" — `WorkflowAutomateComposer` is auto-discovered by Umbraco's composition pipeline purely by implementing `IComposer` in an assembly Umbraco scans, and triggers are auto-discovered via the `[Trigger]` attribute by Automate.Core. If a future change requires the consumer to add a `builder.Services.AddX()` call or config entry, that's a regression against this stated design goal — flag it explicitly rather than adding it quietly.
- **No demo/sample site in this repo.** Unlike some sibling satellites, there's nothing under a `demo/` folder here to run and click through — verification is unit-test-only. If manual/visual verification of a trigger firing is ever needed, it has to happen against a real Umbraco + Workflow instance outside this repo.
- **Package identity**: ships as `Umbraco.Workflow.Automate` on NuGet/MyGet, tagged `umbraco-marketplace` (`src/Umbraco.Workflow.Automate/Umbraco.Workflow.Automate.csproj:6`) and listed in the Umbraco Marketplace as a sub-package of `Umbraco.Workflow` (`umbraco-marketplace.json:13`). `PackageId` isn't overridden anywhere — it defaults to the project name.
- **No TODO/HACK/FIXME markers exist anywhere in `src/` or `tests/`** as of this writing — the codebase is deliberately minimal (10 triggers, 1 authorizer, no half-finished work in flight). If you find one while working here, it's new — treat it as worth resolving or raising, not as pre-existing accepted debt.
- **Known limitation**: because dispatch authorization only covers *content-scoped* outputs, `TaskAssignedTriggerOutput`, `EmailSentTriggerOutput`, and `ReminderEmailsSentTriggerOutput` are never gated by node-level permissions — a flow with access to these triggers sees them regardless of the service account's content access. This is consistent with what those events actually expose (no content node is directly implicated), but it's worth being aware of if a future output on one of these ever gains a content reference — it would need `IContentScopedWorkflowOutput` added explicitly; it won't happen automatically.

## Quick Reference

- **Build**: `dotnet build Umbraco.Workflow.Automate.slnx --configuration Release`
- **Test**: `dotnet test Umbraco.Workflow.Automate.slnx --configuration Release`
- **Pack**: `dotnet pack Umbraco.Workflow.Automate.slnx --configuration Release --output ./artifacts`
- **Key projects**:
  - `src/Umbraco.Workflow.Automate/Umbraco.Workflow.Automate.csproj` — the shipped package (depends on `Umbraco.Automate.Core` + `Umbraco.Workflow.Core`)
  - `tests/Umbraco.Workflow.Automate.Tests.Unit/Umbraco.Workflow.Automate.Tests.Unit.csproj` — xUnit/Moq/Shouldly unit tests
- **Important files**:
  - `src/Umbraco.Workflow.Automate/WorkflowAutomateComposer.cs` — composition entry point
  - `src/Umbraco.Workflow.Automate/Dispatch/WorkflowContentDispatchAuthorizer.cs` — security-relevant dispatch gate
  - `Directory.Packages.props` — central package version ranges; bumped by the release-management skill
  - `version.json` — Nerdbank.GitVersioning source of truth for package version
  - `azure-pipelines.yml`, `.devops/build-and-pack.yml`, `.devops/test.yml` — CI (Build+Test+SBOM, no Publish stage)
  - `.claude/skills/release-management/SKILL.md`, `.claude/skills/post-release-cleanup/SKILL.md` — release automation
- **Configuration**: none — this package has no `appsettings.json`, no environment-specific config, no user secrets.
- **Getting help**: `README.md` documents every trigger and its output shape in detail (the source of truth for the public API surface); `README.md:157-174` has a project layout summary matching the structure above.
