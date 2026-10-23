# Progresso do desenvolvimento

> Atualizado a cada commit. Regras e escopo: `docs/project-brief.md`.

## Etapas realizadas

### Commit 1 — Esqueleto do repositório
- [x] Solution `RiggingPlanner.slnx` com projetos `Domain`, `Application`, `Infrastructure`, `App` e `Domain.Tests`
- [x] Referências entre projetos (arquitetura limpa simples)
- [x] App Blazor Hybrid (.NET MAUI) apenas para Windows
- [x] Layout Claymorphism (azul) com menu lateral
- [x] Páginas vazias: Planos, Novo plano (indicador das 5 etapas) e Acessórios
- [x] Roteamento (`Routes.razor`) e imports globais (`_Imports.razor`)
- [x] Textos do shell da aplicação em português (`index.html`) e título da janela "Rigging Planner"
- [x] Template MAUI limpo (Bootstrap e CSS escopado do template removidos; só CSS próprio)
- [x] `README.md`, `.gitignore`, `.editorconfig`, `global.json`
- [x] CI no GitHub Actions (build + testes)
- [x] `docs/project-brief.md` atualizado (desktop local, sem usuários/autenticação)

### Commit 2 — Banco de dados (EF Core + SQLite)
- [x] Pacotes `Microsoft.EntityFrameworkCore.Sqlite` e `.Design` na `Infrastructure`
- [x] `AppDbContext` (carrega as configurações de entidade da assembly) e fábrica de design-time
- [x] `AddInfrastructure(databasePath)`: registra `IDbContextFactory<AppDbContext>` (um contexto curto por operação)
- [x] Banco em `%LOCALAPPDATA%\RiggingPlanner\rigging-planner.db`, criado no primeiro uso
- [x] Migrations aplicadas automaticamente ao abrir o app (`ApplyMigrations`)
- [x] Migration inicial `InitialCreate` (vazia: as tabelas chegam junto das entidades)
- [x] Ferramenta `dotnet-ef` como tool local do repositório (`.config/dotnet-tools.json`)
- [x] Projeto `RiggingPlanner.Infrastructure.Tests` com teste que aplica as migrations em um banco temporário
- [x] CI passa a rodar os testes da solution inteira

## Próximas etapas (previstas — ordem a confirmar)

| # | Etapa | Observação |
|---|---|---|
| 3 | Planos e pastas | Lista de planos, pastas de um nível, criar/abrir/duplicar |
| 4 | Dados de entrada | Cliente, guindaste, carga, planejador |
| 5 | Cadastro de acessórios | Histórico, alertas de desgaste/descarte |
| 6 | Capacidade de carga | Interface tipo planilha, reaproveitando os dados |
| 7 | Anexos | Imagens do SolidWorks, PDF da ART, PDF do laudo |
| 8 | Preview | Visualização do plano antes de finalizar |
| 9 | Exportação em PDF | Gerar páginas + juntar desenhos, ART e laudo |
| 10 | Instalador | App instalável, pronto para abrir |

## Fora do escopo por enquanto
Interpolação de capacidade, backup do banco, exportação em DOCX.
