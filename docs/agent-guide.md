# Guia de Desenvolvimento para Agentes e IA de CLI — Rigging Planner

> **Público-alvo:** Agentes autônomos de IA, ferramentas de CLI (como Antigravity CLI, Claude Code, Aider, Codex, Copilot CLI) e desenvolvedores que precisam entender rapidamente como navegar, compilar, testar e evoluir o repositório sem quebrar regras de arquitetura ou de negócio.

---

## 1. Visão Geral do Projeto

O **Rigging Planner** é um aplicativo desktop Windows desenvolvido para a empresa parceira **Guindastes Ribas Ltda.** (Curitiba/PR).
O objetivo é automatizar a montagem e integração de **Planos de Rigging** — documentos técnicos de 10 a 20+ páginas exigidos pelo CREA e normas de segurança para operações de içamento de cargas pesadas.

### O que o sistema faz:
- Centraliza dados de entrada que antes eram digitados manualmente em vários programas (Excel, SolidWorks, Word, portal do CREA).
- Mantém cadastros reutilizáveis de **Clientes**, **Equipamentos** (caminhão + guindaste com laudos e ARTs) e **Responsáveis** (Riggers com RPP e CREA). Os **itens de lingada/dispositivo** são livres, digitados por plano — **não há catálogo global de acessórios** (a tela e a entity `Accessory` foram removidas).
- Calcula automaticamente **em tempo de exibição** a carga bruta e a taxa de utilização do guindaste.
- Organiza o fluxo de montagem do plano em um **wizard de 5 etapas**:
  1. *Dados de entrada* (identificação, cliente, carga, responsável, observações);
  2. *Capacidade de carga* (guindaste, configuração de lança/raio, lingada e dispositivos, taxa de utilização);
  3. *Anexos* (desenhos técnicos SolidWorks em imagem e PDF da ART do plano);
  4. *Preview* (visualização das folhas A4);
  5. *Exportação* (compilação do PDF final consolidado).

---

## 2. Stack Tecnológica & Ambiente

| Componente | Tecnologia / Configuração |
|---|---|
| **Sistema Operacional** | Windows 10/11 (requisito obrigatório) |
| **Plataforma / Runtime** | **.NET 10** (`global.json`: SDK 10.0.402 com `rollForward: latestFeature`) |
| **Interface Desktop** | **.NET MAUI Blazor Hybrid** (`src/RiggingPlanner.App`) com WebView2 nativo |
| **Persistência / ORM** | **SQLite** local com **Entity Framework Core 10** |
| **Padrão de DbContext** | `IDbContextFactory<AppDbContext>` (contextos de ciclo de vida curto por operação) |
| **Caminho do Banco de Dados** | `%LOCALAPPDATA%\RiggingPlanner\rigging-planner.db` |
| **Design System / CSS** | CSS nativo modular com `@layer rp.*` e variáveis semânticas `--rp-*`. **Sem Bootstrap, sem Tailwind, sem bibliotecas de componentes (MudBlazor, Radzen etc.)** |
| **Testes Automatizados** | **xUnit** (`tests/RiggingPlanner.Domain.Tests` e `tests/RiggingPlanner.Infrastructure.Tests`) |
| **Solution** | Formato moderno `RiggingPlanner.slnx` |

---

## 3. Comandos Essenciais de CLI (Runbook Rápido)

Como agente de CLI, use estes comandos padronizados:

### 3.1 Restaurar Dependências e Ferramentas Locais
```powershell
dotnet restore RiggingPlanner.slnx
dotnet tool restore
```

### 3.2 Compilar a Solução
```powershell
dotnet build RiggingPlanner.slnx -c Debug
# ou em Release (usado no CI):
dotnet build RiggingPlanner.slnx -c Release
```

### 3.3 Executar a Suíte de Testes
```powershell
dotnet test RiggingPlanner.slnx
# ou filtrando apenas os testes de infraestrutura e migrações:
dotnet test tests/RiggingPlanner.Infrastructure.Tests
```

### 3.4 Gerar e Aplicar Migrações do EF Core
Quando modificar entidades em `RiggingPlanner.Domain` ou o mapeamento em `AppDbContext`:
```powershell
# 1. Garanta que a ferramenta local dotnet-ef esteja restaurada:
dotnet tool restore

# 2. Crie a nova migration (sempre com estes parâmetros exatos):
dotnet ef migrations add NomeDaMigration `
  --project src/RiggingPlanner.Infrastructure `
  --startup-project src/RiggingPlanner.Infrastructure `
  --output-dir Persistence/Migrations

# 3. Valide a migration executando os testes automatizados de banco:
dotnet test --filter DatabaseMigrationTests
```

> [!NOTE]
> As migrações são aplicadas automaticamente na inicialização da aplicação (`app.Services.ApplyMigrations()` em `MauiProgram.cs`), mas os testes de integração sobem um SQLite temporário e validam a integridade do schema.

### 3.5 Execução do App via CLI (Atenção para IA!)
```powershell
dotnet run --project src/RiggingPlanner.App
```
> [!CAUTION]
> `RiggingPlanner.App` é uma aplicação desktop gráfica com janela WinUI/MAUI.
> **Nunca execute `dotnet run` esperando término síncrono em tarefas automatizadas de CLI ou headless**, pois o processo continuará rodando até a janela ser fechada manualmente. Para validações em CLI, utilize `dotnet build` e `dotnet test`.

---

## 4. Invariantes Críticos e Regras de Negócio Inegociáveis

Se você for adicionar ou alterar código, estas regras **nunca podem ser violadas**:

### 4.1 Separação Rigorosa de Idiomas
- **Código Técnico (100% em Inglês):** Nomes de classes, métodos, propriedades, variáveis, tabelas do banco, commits do Git, comentários de código e nomes de testes devem ser estritamente em **inglês** (ex.: `RiggingPlan`, `Client`, `Equipment`, `BoomLengthM`, `PlanAccessory`).
- **Interface e Mensagens de Usuário (100% em Português pt-BR):** Textos na tela, cabeçalhos, rótulos de campos, placeholders, botões e mensagens de validação `[Required(ErrorMessage = "...")]` devem ser estritamente em **português do Brasil** (ex.: `"Informe a descrição da carga."`, `"Capacidade de Carga"`, `"Salvar plano"`).

### 4.2 O Princípio do Snapshot (Plano Autossuficiente)
Um plano de rigging é um documento técnico e jurídico assinado por um engenheiro rigger registrado no CREA.
- Ao selecionar um Cliente, Equipamento ou Responsável, os valores relevantes **são copiados diretamente para o plano (`RiggingPlan`)**:
  - `Client` → copia `ClientName`, `ClientCnpj`.
  - `Equipment` → copia `CraneBrand`, `CraneModel`, `TruckModel`, `ChassisNumber`, `LicensePlate`, `ManufactureYear`.
  - `Rigger` → copia `RiggerName`, `RiggerRpp`, `RiggerCrea`.
- Os itens de lingada/dispositivo (`PlanAccessory`) são digitados livremente por plano (não vêm de catálogo).
- **Invariante:** Alterar, inativar ou excluir cadastros globais no futuro **nunca pode alterar os dados históricos de planos já gerados**.
- **Atenção IA:** Não remova essas colunas desnormalizadas de snapshot na entidade `RiggingPlan` achando que se trata de duplicação desnecessária. É um requisito deliberado de conformidade técnica.

### 4.3 Nada Derivado é Gravado no Banco (Cálculo na Exibição)
- O sistema **não grava totais nem valores calculados no banco de dados**.
- Não persista:
  - Peso total de linha (`Quantity * UnitWeightKg`);
  - Total da lingada/amarras (`Sum(...)`);
  - Total de dispositivos/balancins (`Sum(...)`);
  - Carga bruta em kg (`WeightKg + lingada + dispositivos + ExtraLoadKg`);
  - Carga bruta em toneladas (`grossWeight / 1000`);
  - Taxa de utilização do guindaste (`(grossWeightTons / LoadCapacityT) * 100`).
- Todos esses valores são propriedades computadas no componente Razor ou modelos de exibição.
- **Atenção IA:** Não crie colunas em tabelas nem propriedades em entidades para valores derivados.

### 4.4 Regra de Numeração Sequencial (`AAMMNNN`)
- Formato: `AAMMNNN` (exemplo: `2611001` = ano 2026, mês 11, sequência 001).
- A sequência reinicia todo mês com base na **data do plano (`PlanDate`)**, e **não** na data atual do sistema.
- Gerido pela tabela `PlanNumberCounter` (`YearMonth` PK de 4 dígitos).
- Números de planos excluídos ou cancelados **nunca são reaproveitados**.

### 4.5 Ciclo de Vida: Inativação vs Exclusão Definitiva
- **Cadastros (Clientes, Riggers, Equipamentos):**
  - Podem ser **Inativados** (`IsActive = false` ou `Status = "inactive"`): ficam visíveis na seção de inativos com visual atenuado, não podem ser selecionados para novos planos, mas podem ser reativados.
  - **Exclusão Definitiva (`DELETE` físico):** É estritamente **bloqueada** se houver vínculo com algum plano em andamento/rascunho. Se estiver vinculado apenas a planos finalizados, pode ser excluído fisicamente porque os planos finalizados preservam o snapshot dos dados.
- **Planos:**
  - `Status = "draft"`: editável.
  - `Status = "finalized"`: somente leitura com bloqueio de edição na UI e registro de `FinalizedAt`.
  - Exclusão lógica com lixeira via `DeletedAt` (prevista no modelo).

### 4.6 Entrada de Dados Decimais (`RpDecimalInput`)
- Para qualquer campo numérico ou decimal com suporte a vírgula/ponto no padrão pt-BR, utilize o componente compartilhado `RpDecimalInput` em vez de inputs numéricos HTML nativos que quebram com a formatação brasileira.

---

## 5. Estrutura de Camadas e Localização de Arquivos

```
src/
├── RiggingPlanner.Domain/              # Camada Central (Pure C#)
│   ├── Entities/                       # Entidades EF Core com regras e navegação
│   │   ├── Client.cs                   # Clientes com CNPJ
│   │   ├── Equipment.cs                # Conjunto caminhão + guindaste
│   │   ├── EquipmentReport.cs          # Laudo de segurança + ART do laudo
│   │   ├── Folder.cs                   # Pastas organizacionais (1 nível, sem UI)
│   │   ├── PlanAccessory.cs            # Itens de lingada/dispositivo no plano
│   │   ├── PlanDrawing.cs              # Folhas de desenho anexadas
│   │   ├── PlanExport.cs               # Histórico de exportações PDF (sem uso)
│   │   ├── PlanNumberCounter.cs        # Contador sequencial AAMM
│   │   ├── PlanProcedureSnapshot.cs    # Snapshot congelado do procedimento
│   │   ├── Rigger.cs                   # Responsáveis técnicos (RPP, CREA)
│   │   ├── RiggingPlan.cs              # Entidade principal do plano
│   │   ├── StoredFile.cs               # Metadados de arquivos em disco
│   │   └── Tag.cs                      # Tags (entidade mantida, sem UI hoje)
│   └── Services/
│       ├── OperationalProcedureService.cs  # Texto padrão, documento e snapshot
│       └── OperationalProcedureDocument.cs # Modelos do documento gerado
│
├── RiggingPlanner.Application/         # Camada prevista (ainda sem uso)
│
├── RiggingPlanner.Infrastructure/      # Acesso a Dados
│   ├── Persistence/
│   │   ├── AppDbContext.cs             # Mapeamento Fluent API completo
│   │   ├── AppDbContextFactory.cs      # Fábrica para CLI do EF Core
│   │   └── Migrations/                 # Migrações versionadas do SQLite
│   └── DependencyInjection.cs          # AddInfrastructure() e ApplyMigrations()
│
└── RiggingPlanner.App/                 # Interface Desktop (Blazor Hybrid .NET MAUI)
    ├── MauiProgram.cs                  # Ponto de entrada MAUI, DI e cultura pt-BR
    ├── App.xaml / MainPage.xaml        # Container nativo da janela desktop
    ├── Components/
    │   ├── Pages/                      # Telas principais (roteadas)
    │   │   ├── Plans.razor             # Lista de planos (rascunhos / finalizados)
    │   │   ├── NewPlan.razor           # Etapa 1: Dados de entrada (orquestração)
    │   │   ├── Capacity.razor          # Etapa 2: Capacidade de carga (orquestração)
    │   │   ├── PlanPlaceholder.razor   # Etapas 3, 4, 5 (orquestração de Anexos, Preview e Exportação)
    │   │   ├── Clients.razor           # Cadastro e gestão de Clientes
    │   │   ├── Equipments.razor        # Cadastro de Equipamentos e ARTs
    │   │   └── Riggers.razor           # Cadastro de Responsáveis
    │   ├── Plan/
    │   │   ├── Attachments/            # Etapa 3: ART do plano e desenhos do SolidWorks
    │   │   │   ├── AttachmentsState.cs # Upload, exclusão e pré-visualização (staging em memória/disco)
    │   │   │   ├── AttachmentDraftState.cs # Sessão/diretório de staging e limpeza de arquivos pendentes
    │   │   │   ├── AttachmentPersistence.cs # Converte staging em StoredFile/PlanDrawing no "Salvar"
    │   │   │   ├── RpAttachmentsStage.razor # Composição da etapa de anexos
    │   │   │   ├── RpArtAttachmentSection.razor # Número e PDF da ART
    │   │   │   ├── RpDrawingsSection.razor # Galeria e envio de desenhos
    │   │   │   └── RpDrawingPreviewModal.razor # Visualização ampliada do desenho
    │   │   ├── Capacity/               # Etapa 2: estado, formulário e seções
    │   │   │   ├── CapacityState.cs    # Estado da etapa, totais e taxa de utilização
    │   │   │   ├── CapacityAccessoryState.cs # Adição/edição/exclusão de lingadas e dispositivos
    │   │   │   ├── PlanAccessoryForm.cs # Modelo de validação de uma lingada/dispositivo
    │   │   │   └── RpCapacity*Section.razor # Seções de acessórios, guindaste, carga, quadrante e resultados
    │   │   ├── Entry/                  # Etapa 1: formulário, estado e persistência
    │   │   │   ├── PlanEntryState.cs   # Opções, seleções, sugestões e handlers
    │   │   │   ├── PlanEntryPersistence.cs # Criação/atualização e vínculos ao salvar
    │   │   │   ├── RpPlanEntryForm.razor # Formulário e validação da etapa
    │   │   │   └── RpPlanEntry*Section.razor # Seções de operação, cliente, equipamento, rigger e carga
    │   │   ├── Export/
    │   │   │   └── RpExportStage.razor # Placeholder da etapa de exportação
    │   │   ├── Preview/
    │   │   │   └── RpPreviewStage.razor # Placeholder da etapa de preview
    │   │   ├── Shared/
    │   │   │   └── PlanFormMapper.cs   # Mapeamento PlanFormModel ↔ RiggingPlan
    │   │   └── Workflow/               # Base comum do wizard de planos
    │   │       ├── PlanStepCatalog.cs  # Nomes, índices e URLs das 5 etapas
    │   │       ├── PlanWorkflowLoader.cs # Carrega plano do Draft/banco e acessórios
    │   │       ├── RpPlanStepLayout.razor # Header, ações, guarda, stepper e navegação
    │   │       └── RpStagePlaceholder.razor # Card padrão para etapas ainda não implementadas
    │   └── Shared/                     # Estado e Componentes reutilizáveis
    │       ├── PlanDraftState.cs       # Estado em memória do wizard do plano
    │       ├── PlanFormModel.cs        # Validação do formulário (IValidatableObject)
    │       ├── CnpjFormatter.cs        # Formatação, normalização e validação de CNPJ
    │       ├── EquipmentArtDraft.cs    # Modelo em memória do editor de ARTs
    │       ├── RpDecimalInput.razor    # Input decimal com máscara pt-BR
    │       ├── RpPlanLeaveGuard.razor  # Bloqueio e confirmação ao tentar sair sem salvar
    │       ├── RpTagPicker.razor       # Seletor interativo com busca/criação
    │       ├── RpEquipmentArtEditor.razor # Sub-editor de ARTs no equipamento
    │       ├── RpEquipmentArtModal.razor  # Modal de anexo/edição de ART do equipamento
    │       ├── RpFileAttachmentBox.razor  # Caixa de anexo (arquivo + remoção)
    │       ├── RpPlanNavigation.razor  # Botões Anterior / Próximo do wizard
    │       ├── RpPlanActions.razor     # Botões globais Salvar / Descartar
    │       ├── RpOperationalProcedure.razor # Visualização do procedimento (pronta para o Preview)
    │       └── RpStepper.razor         # Indicador visual das 5 etapas
    └── wwwroot/
        └── css/                        # Design System próprio (camadas rp.*)
            ├── 00-layers.css           # Ordem das camadas CSS
            ├── 01-tokens.css           # Tokens semânticos e paleta (#003F68)
            ├── 02-reset.css            # Reset CSS moderno
            ├── 03-base.css             # Estilos base
            ├── 04-layout.css           # Estrutura do shell e grid de 12 colunas
            └── 05-components.css       # Classes de componentes (.rp-*)
```

---

## 6. Fluxo de Estado do Plano (`PlanDraftState`)

Ao trabalhar nas telas do plano de rigging, compreenda este mecanismo de estado:
1. `PlanDraftState` é um serviço registrado como **Scoped** no contêiner de injeção de dependência.
2. A etapa 1 usa `PlanEntryState` para opções, seleções e sugestões, e `PlanEntryPersistence` para validação de referências e persistência transacional.
3. A etapa 2 usa `CapacityState` para totais, taxa de utilização e acessórios, com `CapacityAccessoryState` controlando a edição inline de lingadas/dispositivos e notificando a página via callback `Changed`.
4. A etapa 3 usa `AttachmentsState` para upload, exclusão e pré-visualização da ART e dos desenhos, notificando a página via callback `Changed`. **Os arquivos escolhidos ficam em staging** (`AttachmentDraftState`, em `rigging-plan-staging/{sessão}/`) e só viram `StoredFile`/`PlanDrawing`/`ArtFileId` no **"Salvar"**, quando `AttachmentPersistence.ApplyAsync` roda na mesma transação do plano.
5. Quando o usuário inicia um novo plano ou abre um rascunho existente:
   - Os dados são carregados em `PlanDraftState.Form` (tipo `PlanFormModel`) e `PlanDraftState.Accessories`.
6. **Navegação entre etapas:** O usuário pode alternar livremente entre "Dados de entrada" (`/new-plan/{id}`), "Capacidade" (`/new-plan/{id}/capacity`) e demais abas **sem salvar no banco de dados a cada clique**. O rascunho é mantido em memória no `PlanDraftState`.
7. **Proteção contra Saída Inadvertida (`RpPlanLeaveGuard`):** Ao tentar sair da página do plano (menu lateral, botão voltar ou navegação externa) sem salvar, uma confirmação modal alerta que as alterações serão perdidas, prevenindo descarte acidental. A navegação interna entre etapas não é bloqueada.
8. **Título Dinâmico (`GetPageTitle`):** O cabeçalho exibe o número e a descrição do plano (ex.: `Plano 2611001 — Içamento Chiller`) para planos existentes em edição, e "Novo plano" para planos novos ainda não salvos.
9. **Salvamento Explícito:** A persistência no SQLite e a validação de regras obrigatórias ocorrem quando o usuário clica em **"Salvar"** no cabeçalho compartilhado (`RpPlanActions`). O formulário invoca `PlanEntryPersistence.SaveAsync`, persiste plano, itens e anexos em uma única transação e atualiza o estado para `IsPersisted = true`.
10. **Descarte:** Clicar em "Descartar" abre confirmação, invoca `Draft.Reset()` (que também apaga o staging de anexos) e redireciona para a lista de planos.

---

## 7. Dicionário de Domínio (Português da UI ↔ Inglês do Código)

Ao implementar funcionalidades, utilize este mapeamento para manter consistência semântica:

| Conceito de Negócio (pt-BR) | Entidade / Propriedade no Código | Onde se aplica |
|---|---|---|
| **Plano de Rigging** | `RiggingPlan` | Entidade principal |
| **Número do Plano** | `PlanNumber` (ex.: `"2611001"`) | Chave pública sequencial mensal |
| **Cliente / Empresa** | `Client` (`Name`, `Cnpj`) | Cadastro e snapshot no plano |
| **Responsável Técnico / Rigger** | `Rigger` (`FullName`, `DisplayName`, `RppNumber`, `CreaNumber`) | Cadastrado como Rigger, rotulado na UI como "Responsável" |
| **Equipamento** | `Equipment` (`CraneBrand`, `CraneModel`, `TruckModel`, `LicensePlate`, `ChassisNumber`, `ManufactureYear`) | Conjunto caminhão + guindaste |
| **Laudo Técnico + ART** | `EquipmentReport` (`ArtNumber`, `ValidFrom`, `ValidTo`, `FileId`) | Anexo periódico do equipamento |
| **Item de Lingada / Dispositivo** | `PlanAccessory` (`Section`, `Description`, `DrawingRef`, `Quantity`, `UnitWeightKg`) | Cintas, manilhas, laços, balancins — livres por plano |
| **Tag** | `Tag` (`Name`) | Entidade mantida no banco, sem uso na UI hoje |
| **Lingada / Amarras** | `PlanAccessory` com `Section = "lingada"` | Conjunto de amarras que prende a carga ao gancho |
| **Dispositivo / Balancim** | `PlanAccessory` com `Section = "dispositivo"` | Estruturas intermediárias de distribuição de peso |
| **Carga Líquida** | `RiggingPlan.WeightKg` | Peso próprio do item a içar |
| **Cargas Extras** | `RiggingPlan.ExtraLoadKg` | Pesos adicionais considerados |
| **Carga Bruta** | *Propriedade computada* | Carga líquida + lingada + dispositivos + extras |
| **Taxa de Utilização do Guindaste** | *Propriedade computada* | `(Carga Bruta em t / Capacidade de Carga em t) * 100` |
| **Taxas dos Acessórios** | `FlatStrapUtilizationPct`, `ShackleUtilizationPct` etc. | Digitadas pelo engenheiro no plano |
| **Esforço nas Sapatas** | `FootingForceKgf` (em **kgf**) | Digitado pelo engenheiro |
| **Resistência do Terreno** | `MinGroundResistanceKgfCm2` (em **kgf/cm²**) | Digitada pelo engenheiro |
| **Vento Máximo** | `MaxWindSpeedMs` (em **m/s**) | Digitado pelo engenheiro |

---

## 8. Status Atual vs Próximas Funcionalidades (Evite Alucinações)

Quando for solicitado a desenvolver ou refatorar, consulte esta tabela para saber o que **já existe** e o que **ainda deve ser implementado**:

### O que JÁ ESTÁ implementado e funcional:
- [x] CRUD completo de Clientes com validação de CNPJ legado e alfanumérico.
- [x] CRUD completo de Responsáveis (Riggers) com campos de RPP e CREA.
- [x] CRUD completo de Equipamentos com sub-editor de múltiplas ARTs (datas opcionais, anexo PDF).
- [x] Ciclo de vida ativo/inativo e exclusão física protegida contra planos em andamento (Clientes, Equipamentos e Responsáveis).
- [x] Wizard de planos com Etapa 1 (Dados de entrada), Etapa 2 (Capacidade de carga interativa) e Etapa 3 (Anexos).
- [x] Itens de lingada/dispositivo dinâmicos por plano (`PlanAccessory`), com adição, edição inline e remoção.
- [x] Cálculo visual em tempo real de lingada, dispositivos, carga bruta (kg e t) e taxa de utilização.
- [x] Anexos do plano: upload/substituição/remoção do PDF da ART e upload/galeria/lightbox/remoção de desenhos (staging em disco + `StoredFile`/`PlanDrawing` persistidos no "Salvar").
- [x] Geração do procedimento operacional a partir dos dados do plano (`OperationalProcedureService`) e componente de visualização `RpOperationalProcedure` prontos para o Preview.
- [x] Snapshots de procedimento e hash SHA-256 (`CreateSnapshot`/`ResolveContent`), com testes.
- [x] Numeração `AAMMNNN` gerada no primeiro salvamento, pela data do plano.

### O que está MODELADO no banco, mas AINDA NÃO possui fluxo completo:
- [ ] **Etapa 4 — Preview:** Visualizador das páginas do plano em A4 (hoje é placeholder).
- [ ] **Etapa 5 — Exportação:** Geração do PDF final e histórico (`PlanExport`) — sem biblioteca de PDF e sem serviço.
- [ ] **Finalização do plano:** o modelo trata `Status = "finalized"` como somente leitura, mas **não existe ação** que grave `Status`/`FinalizedAt` nem que crie o snapshot na aplicação.
- [ ] **Vínculo de Laudo no Plano:** `EquipmentReportId` existe, mas nunca é preenchido pelo fluxo.
- [ ] **Pastas de um nível:** `Folder` mapeada, sem tela/operações.
- [ ] **Lixeira:** `DeletedAt` mapeado, sem exclusão/restauração.
- [ ] **Histórico de alterações (`plan_change_log`):** planejado, ainda não modelado.

### Fora de Escopo Definitivo (Não implementar):
- Motor de cálculo de engenharia estrutural ou interpolação de capacidade por raio de operação.
- Autenticação de usuários, login, permissões, roles ou multi-tenancy (app de usuário único local).
- Sincronização em nuvem, APIs HTTP REST ou conexões externas de rede.
- Exportação em formato `.docx` (a saída solicitada é exclusivamente `.pdf`).

---

## 9. Armadilhas Comuns para IAs de CLI (Anti-Patterns a Evitar)

1. **NÃO instale bibliotecas de componentes externas:** Não adicione `MudBlazor`, `Radzen`, `Blazorise` ou `Bootstrap`. O app possui um design system proprietário em `wwwroot/css/05-components.css` usando classes `.rp-*` e componentes Razor em `Components/Shared/`.
2. **NÃO altere `Directory.Build.props` sem extrema necessidade:** Ele contém configurações específicas para excluir `docs`, `artifacts` e `*.md` das recompilações do `dotnet watch` e compilação incremental.
3. **NÃO quebre o SQLite com tipos incompatíveis:** No SQLite, IDs são `Guid` mapeados como string ou BLOB e números decimais usam precisão configurada em `ConfigurePlanDecimals`. Respeite o mapeamento existente em `AppDbContext.cs`.
4. **NÃO rode comandos de deleção de banco sem backup:** Se precisar reiniciar o banco de testes, o arquivo local de desenvolvimento fica em `%LOCALAPPDATA%\RiggingPlanner\rigging-planner.db`.
5. **SEMPRE rode os testes após alterações:** Antes de concluir qualquer tarefa, certifique-se de executar `dotnet test` e garantir 100% de sucesso.
