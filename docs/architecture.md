# Architecture

## Layers and dependency flow

```
App ──────► Application ◄────── Infrastructure
                ▲                      │
                └────── Domain ◄───────┘
```

- **Domain** has no project or package references. It owns entities, the `PlanNumber` value object, `PlanTotals`, `PlanValueFormatter` and `OperationalProcedureService` (including the frozen snapshot content).
- **Application** references only Domain. It owns the ports (`IPlanService`, `ICatalogService`, `IExportService`, `IFileStore`), use-case DTOs, form validation/mapping (`PlanFormModel`, `PlanFormValidator`, `PlanFormMapper`), document builders and the PDF renderer ports plus `FullPlanPdfAssembler`. It has no EF Core, QuestPDF or Blazor dependency.
- **Infrastructure** references Application. It implements the ports with EF Core/SQLite (`AppDbContext`, `PlanService`, `CatalogService`), file storage (`FileStore`) and PDF rendering (`Infrastructure/Pdf`). It also owns the migration chain and `AddInfrastructure()`.
- **App** references Application and Infrastructure; the only composition point is `MauiProgram.CreateMauiApp()`. Components inject Application ports only. No Razor component uses `IDbContextFactory` or `Microsoft.EntityFrameworkCore`.

## Composition and startup

`AddInfrastructure(databasePath)` registers the `AppDbContext` factory, `IFileStore` (rooted at the database directory), the PDF renderers, and the Application implementations (`AddApplication()` registers the assembler). `ApplyMigrations()` runs pending migrations on startup; `AttachmentDraftState.CleanupStaleSessions()` removes abandoned staging folders.

`ApplyMigrations()` is idempotent and preserves existing databases: when a history contains migration IDs that are not part of the current assembly, those entries are reconciled without rerunning DDL. Plans and attachments are kept. New databases are migrated normally.

Culture is forced to `pt-BR` at startup because all amounts, dates and printed documents use Brazilian formatting.

## Presentation state

`PlanDraftState` is scoped and holds the in-progress form, plan, accessories and attachment draft. In Blazor Hybrid a scope lives as long as the app, so the draft is effectively app-wide; it is reset explicitly after save/discard. Feature-specific state (`PlanEntryState`, `CapacityState`, `AttachmentsState`) is constructed from the draft and passed to child components as parameters.

Cross-step commands use navigation to `?save=true`; `NewPlan` detects the query parameter and submits the form. `RpPlanLeaveGuard` blocks navigation with unsaved changes.

## Plan workflow

1. **Entry** (`/new-plan/{id}`): client, equipment, rigger and load data.
2. **Capacity** (`/new-plan/{id}/capacity`): accessories and crane capacity values.
3. **Attachments** (`/new-plan/{id}/attachments`): ART and drawings staged on disk.
4. **Preview** (`/new-plan/{id}/preview`): rasterized pages of the assembled PDF.
5. **Export** (`/new-plan/{id}/export`): save, export or finalize the plan.

`PlanDraftLoader` synchronizes the scoped draft with the persisted plan; `PlanPdfWriter` is the only App code that writes the exported PDF to Downloads and opens the viewer.

## File storage

`IFileStore` abstracts paths relative to the app data directory. Files are staged under `rigging-plan-staging/<session>` while editing and copied to `rigging-plan-art` / `rigging-plan-drawings` when the plan is saved. `StoredFile` records keep the relative path, content type, size and SHA-256.
