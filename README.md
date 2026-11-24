# Rigging Planner

Desktop app (Windows) for building the Guindastes Ribas rigging plan: register the operation data once, reuse it across every step, and export the final PDF (summary, drawings, ART, operational procedure and equipment report).

School project for *Jornada de Aprendizagem — Sistemas Inteligentes Conectados* (team XHG).

## Documentation
- [`docs/architecture.md`](docs/architecture.md): layers, dependency flow, composition and state.
- [`docs/domain-rules.md`](docs/domain-rules.md): plan numbering, snapshots, derived values and lifecycle.
- [`docs/design-system.md`](docs/design-system.md): CSS tokens, layers and shared components.
- [`docs/procedure-spec.md`](docs/procedure-spec.md): fixed operational procedure text and variables.
- [`docs/pdf-export.md`](docs/pdf-export.md): PDF assembly, fonts and reference fixtures.
- [`docs/release.md`](docs/release.md): packaging and versioning.

## Requirements
- Windows 10/11
- .NET 10 SDK (`global.json`)
- MAUI workload: `dotnet workload install maui`
- Inno Setup 6 only when building an installer

## Run and test
```powershell
dotnet run --project src/RiggingPlanner.App
dotnet build RiggingPlanner.slnx -c Debug
dotnet test RiggingPlanner.slnx -c Debug
```

The app is a GUI; do not run it from a non-interactive terminal.

## Data
SQLite at `%LOCALAPPDATA%\RiggingPlanner\rigging-planner.db`; migrations are applied on startup. Attachments live in the same user data directory. To add a migration:

```powershell
dotnet tool restore
dotnet ef migrations add NomeDaMigration --project src/RiggingPlanner.Infrastructure --startup-project src/RiggingPlanner.Infrastructure --output-dir Persistence/Migrations
```

## Structure
```
src/RiggingPlanner.Domain          entities, value objects, domain services
src/RiggingPlanner.Application     ports, use-case DTOs, document builders, PDF ports
src/RiggingPlanner.Infrastructure  EF Core/SQLite, file storage, PDF renderers
src/RiggingPlanner.App             Blazor Hybrid UI and presentation state
tests/                             xUnit tests for Domain and Infrastructure
```

## Conventions
- Code identifiers in English; all user-facing text in Portuguese (`pt-BR`).
- The UI never accesses EF Core or the file system directly; it uses Application ports.
- Commit messages follow Conventional Commits in lowercase English.
