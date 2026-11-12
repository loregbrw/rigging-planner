# AGENTS.md — Instruções para Agentes de IA e CLI

> **Referência Completa:** Veja [`docs/agent-guide.md`](docs/agent-guide.md) para o guia detalhado de arquitetura, dicionário de domínio e regras de negócio.

Este arquivo é lido automaticamente por ferramentas de IA e agentes de CLI (Antigravity CLI, Claude Code, Aider, Codex, Copilot CLI) que operam neste repositório.

---

## ⚡ Comandos Rápidos de Execução

```powershell
# Compilação
dotnet build RiggingPlanner.slnx -c Debug

# Testes automatizados (SEMPRE execute após alterações)
dotnet test RiggingPlanner.slnx

# Criação de Migration do EF Core (quando alterar entidades/banco)
dotnet tool restore
dotnet ef migrations add NomeDaMigration --project src/RiggingPlanner.Infrastructure --startup-project src/RiggingPlanner.Infrastructure --output-dir Persistence/Migrations
dotnet test --filter DatabaseMigrationTests
```

> [!CAUTION]
> **NÃO execute `dotnet run` em segundo plano ou em scripts síncronos de CLI.**
> O projeto `src/RiggingPlanner.App` é um app desktop gráfico (.NET MAUI / WinUI 3) e bloqueia a saída do terminal esperando a janela ser fechada. Use `dotnet build` e `dotnet test` para validar suas alterações.

---

## 🛡️ Regras Inegociáveis do Repositório

1. **Separação de Idiomas:**
   - **Código (100% em Inglês):** classes, métodos, propriedades, tabelas, migrações, commits e testes.
   - **Interface e Mensagens (100% em Português pt-BR):** labels, botões, placeholders e validações (`[Required(ErrorMessage = "...")]`).
2. **Princípio do Snapshot (Plano Autossuficiente):**
   - Ao vincular Cliente, Equipamento, Rigger ou Acessório, seus dados são **copiados** para as colunas do plano (`RiggingPlan` e `PlanAccessory`).
   - Alterar cadastros no futuro nunca pode modificar dados de planos históricos emitidos. Não remova as colunas desnormalizadas de snapshot.
3. **Valores Derivados NÃO são Persistidos:**
   - Pesos totais, carga bruta (kg e t) e taxa de utilização do guindaste são calculados **em tempo de exibição**.
   - Nunca crie colunas no SQLite para valores computados.
4. **Numeração Mensal `AAMMNNN`:**
   - O número do plano (ex.: `2611001`) reinicia a cada mês com base no mês e ano da **data do plano (`PlanDate`)**, nunca no relógio atual.
5. **Sem Bibliotecas de Componentes Externas:**
   - Não adicione MudBlazor, Radzen ou Bootstrap. O design system próprio está estruturado em `wwwroot/css/` e componentes Razor em `Components/Shared/`.
6. **Inputs Decimais:**
   - Sempre utilize `RpDecimalInput` para campos numéricos com decimais no padrão pt-BR.
7. **Estado do Plano (`PlanDraftState`):**
   - A navegação entre as 5 etapas do plano não salva no banco automaticamente. A persistência e validação ocorrem exclusivamente na ação explícita de "Salvar" (`SaveAsync`).
8. **Proteção ao Sair e Título Dinâmico:**
   - Sair da criação/edição sem salvar (menu lateral, voltar ou navegação externa) dispara confirmação avisando sobre perda de alterações (`RpPlanLeaveGuard`).
   - Para planos existentes, o cabeçalho exibe o número e a descrição do plano (`Plano {PlanNumber} — {OperationName}`) em todas as etapas, em vez de "Novo plano".

---

## 🗺️ Mapa Rápido da Solução

- `src/RiggingPlanner.Domain`: Entidades puras C# (sem dependências).
- `src/RiggingPlanner.Infrastructure`: EF Core, SQLite (`%LOCALAPPDATA%\RiggingPlanner\rigging-planner.db`), migrações e `IDbContextFactory`.
- `src/RiggingPlanner.App`: Telas Blazor Hybrid (`Pages/`) e componentes reutilizáveis (`Shared/`).
- `tests/RiggingPlanner.Infrastructure.Tests`: Validações automáticas de schema e migrations.

Consulte [`docs/agent-guide.md`](docs/agent-guide.md) para especificações completas de domínio e regras adicionais.
