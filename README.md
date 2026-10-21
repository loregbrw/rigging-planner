# Rigging Planner

Aplicativo desktop (Windows) para montar o **Plano de Rigging** da Guindastes Ribas: cadastra os dados uma única vez, reaproveita em todas as partes do plano e exporta o PDF final (com desenhos, ART e laudo técnico).

Projeto da matéria *Jornada de Aprendizagem — Sistemas Inteligentes Conectados* (equipe XHG).

## Documentação
- [`docs/project-brief.md`](docs/project-brief.md): escopo, regras de negócio e decisões (fonte oficial da verdade).
- [`docs/progress.md`](docs/progress.md): features e etapas já realizadas.

## Requisitos para desenvolver
- Windows 10/11
- .NET 10 SDK
- Workload do MAUI: `dotnet workload install maui` (terminal como administrador)

## Como rodar
```powershell
dotnet run --project src/RiggingPlanner.App
```

## Estrutura
```
src/
  RiggingPlanner.Domain/          entidades e regras de negócio
  RiggingPlanner.Application/     casos de uso e interfaces
  RiggingPlanner.Infrastructure/  EF Core + SQLite, arquivos, PDF
  RiggingPlanner.App/             interface (Blazor Hybrid / .NET MAUI)
tests/
  RiggingPlanner.Domain.Tests/    testes xUnit
docs/
```

## Convenções
- Código (classes, variáveis, métodos) em inglês; interface 100% em português.
- Branch `main` + um branch por etapa (`feature/...`), com Pull Request.
