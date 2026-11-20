# Fluxo de Dados do Plano de Rigging — Cadastro, Banco e Geração do Documento

> **Base:** leitura do código em 16/11/2026. Onde o código diverge da documentação de produto (`project-brief.md`, `progress.md`), o código vence e a divergência está marcada.
>
> **Documentos de apoio:** `project-brief.md` (escopo e regras de negócio), `procedure-spec.md` (texto do procedimento), `agent-guide.md` (invariantes para agentes), `progress.md` (histórico).

---

## 1. Resumo executivo

- App **desktop Windows, usuário único, local, sem rede e sem autenticação**. UI 100% pt-BR; código 100% inglês.
- Persistência: **SQLite** em `%LOCALAPPDATA%\RiggingPlanner\rigging-planner.db`, via **EF Core 10** e `IDbContextFactory<AppDbContext>` (contexto curto por operação).
- O plano é **autossuficiente**: ao selecionar Cliente, Equipamento, Responsável ou itens de lingada/dispositivo, os valores são **copiados** para colunas do próprio `RiggingPlan` / `PlanAccessory`. Alterar o cadastro depois **não** altera planos históricos.
- **Nada derivado é gravado**: totais, carga bruta, taxa de utilização e código do modelo são calculados na exibição.
- O wizard tem 5 etapas. As etapas **1, 2 e 3** só persistem no **"Salvar"** (transação única). Na etapa 3, os arquivos escolhidos ficam em uma área de staging em disco e os metadados em memória; "Descartar" apaga o staging. As etapas **4 (Preview)** e **5 (Exportação)** são **placeholders de uma linha** (`RpStagePlaceholder`).
- **Não existe biblioteca de PDF** no projeto e **não existe serviço de exportação** — `PlanExport` é só uma entity mapeada.
- **Atenção:** não há, no código atual, nenhuma ação que defina `Status = "finalized"`. O campo, o bloqueio de leitura e o snapshot existem, mas o caminho de finalização não está implementado na UI/persistência (detalhe na Seção 8).

---

## 2. Stack e camadas

| Camada | Projeto | Conteúdo |
|---|---|---|
| UI | `src/RiggingPlanner.App` | Blazor Hybrid (.NET MAUI), `Components/Pages`, `Components/Plan/*`, `Components/Shared/*` |
| Infra | `src/RiggingPlanner.Infrastructure` | `AppDbContext`, migrations, `IDbContextFactory`, `DependencyInjection` |
| Domínio | `src/RiggingPlanner.Domain` | Entities + `OperationalProcedureService` |
| Application | `src/RiggingPlanner.Application` | **ainda sem uso** — os fluxos rodam direto nos componentes Razor via `IDbContextFactory` |

- `TargetFramework` do App: `net10.0-windows10.0.19041.0`; Infrastructure/Domain: `net10.0`.
- Cultura pt-BR é fixada em `MauiProgram.CreateMauiApp`.
- Migrations são aplicadas na inicialização (`app.Services.ApplyMigrations()`), somente se houver pendentes.
- Comandos: `dotnet build RiggingPlanner.slnx -c Debug`, `dotnet test RiggingPlanner.slnx`. **Não usar `dotnet run` em CLI automatizada** (janela gráfica bloqueia).

---

## 3. Banco de dados

### 3.1 Conexão e ciclo de vida

- Caminho: `%LOCALAPPDATA%\RiggingPlanner\rigging-planner.db` (`MauiProgram.GetDatabasePath`).
- Registro: `services.AddDbContextFactory<AppDbContext>(...)` (`Infrastructure/DependencyInjection.cs`).
- Design-time: `AppDbContextFactory` usa `Data Source=design-time.db`.
- Todo acesso em runtime cria um contexto curto: `await using var db = await DbFactory.CreateDbContextAsync();`.
- `ApplyMigrations` sobe o banco na abertura do app.

### 3.2 Tabelas atuais (schema real)

Tabela = nome do `DbSet` (PascalCase). Confirmado em `AppDbContextModelSnapshot.cs`:

```
Clients
Riggers
Equipments
EquipmentReports
Tags                 (mapeada, mas sem uso na UI hoje)
StoredFiles
Folders              (mapeada, mas sem uso na UI hoje)
PlanNumberCounters
RiggingPlans
PlanAccessories
PlanDrawings
PlanExports
PlanProcedureSnapshots
```

> **Removidas:** `Accessories` e `AccessoryEvents` existiam na migration inicial e foram removidas em `20261109004550_RemoveGlobalAccessoriesAndAccessoryId`. Hoje os itens são **livres por plano** (`PlanAccessory`), sem catálogo global.

### 3.3 Diagrama de relacionamentos (texto)

```
Clients 1────N RiggingPlans N────1 Folders
Riggers 1────N RiggingPlans
Equipments 1────N RiggingPlans
Equipments 1────N EquipmentReports N────1 StoredFiles
RiggingPlans 1────N PlanAccessories
RiggingPlans 1────N PlanDrawings   N────1 StoredFiles
RiggingPlans 1────1 StoredFiles            (ArtFile — ART do plano)
RiggingPlans 1────N PlanExports
RiggingPlans 1────0..1 PlanProcedureSnapshots
PlanNumberCounters (isolada, PK YearMonth)
```

### 3.4 Detalhamento das tabelas

#### `RiggingPlans` (entidade principal)

| Grupo | Colunas |
|---|---|
| Identificação | `Id` (Guid PK), `PlanNumber` (7, único), `FolderId` (nulo), `Status` (30, default `"draft"`), `FinalizedAt` (nulo), `DeletedAt` (nulo), `CreatedAt`, `UpdatedAt` |
| Cabeçalho | `PlanDate`, `Location` (300), `OperationName` (200) |
| Cliente (snapshot) | `ClientId` (nulo), `ClientName` (200), `ClientCnpj` (18) |
| Carga | `LoadDescription` (500), `LengthM`, `WidthM`, `HeightM`, `WeightKg`, `ExtraLoadKg` |
| Equipamento (snapshot) | `EquipmentId` (nulo), `TruckModel`, `LicensePlate`, `ChassisNumber`, `ManufactureYear`, `CraneBrand`, `CraneModel`, `CraneNominalCapacity` |
| Configuração | `BoomLengthM`, `OperatingRadiusM`, `LoadCapacityT`, `QuadrantStart`, `QuadrantEnd` |
| Resultados digitados | `FlatStrapUtilizationPct`, `TubularStrapUtilizationPct`, `WireRopeSlingUtilizationPct`, `ShackleUtilizationPct`, `FootingForceKgf`, `MinGroundResistanceKgfCm2`, `MaxWindSpeedMs` |
| Responsáveis (snapshot) | `RiggerId` (nulo), `RiggerName`, `RiggerRpp`, `RiggerCrea`, `SupervisorName` (nulo; **fora do formulário atual**) |
| ART / laudo | `ArtNumber`, `ArtFileId` (nulo), `EquipmentReportId` (nulo; **nunca preenchido pelo fluxo atual**) |
| Observações | `Observations` (4000) |
| Navegações | `Folder`, `Client`, `Equipment`, `Rigger`, `ArtFile`, `EquipmentReport`, `ProcedureSnapshot`, `Accessories`, `Drawings`, `Exports` |

- Índices: `PlanNumber` único; `(Status, DeletedAt)`.
- FKs: `Folder/Client/Equipment/Rigger` → `SetNull`; `ArtFile/EquipmentReport` → `Restrict`.
- Decimais: todos `HasPrecision(18, 3)` (`ConfigurePlanDecimals`).

#### `PlanAccessories` (lingadas e dispositivos)

| Coluna | Observação |
|---|---|
| `Id` (Guid PK), `PlanId` (FK Cascade) | |
| `Section` (40) | `"lingada"` ou `"dispositivo"` |
| `Description` (500) | texto livre (não há vínculo com catálogo) |
| `DrawingRef` (100, nulo) | só usado para `dispositivo` |
| `Quantity` (18,3), `UnitWeightKg` (18,3) | |
| `SortOrder` (int) | índice único `(PlanId, SortOrder)` |

#### `PlanDrawings` / `StoredFiles` (anexos)

- `PlanDrawings`: `Id`, `PlanId` (FK Cascade), `FileId` (FK Restrict), `SortOrder` (único por plano). Navega `File`.
- `StoredFiles`: `Id`, `RelativePath` (500), `OriginalName` (255), `ContentType` (150), `SizeBytes`, `Sha256` (64, indexado), `CreatedAt`. Navegações: `EquipmentReports`, `PlanDrawings`, `PlanArtFiles`.
- **Arquivos ficam em disco**, e o banco guarda só caminho + metadados. Subpastas relativas a `FileSystem.AppDataDirectory`:
  - `rigging-plan-art\` — PDF da ART do plano;
  - `rigging-plan-drawings\` — imagens do SolidWorks;
  - `equipment-art\` — PDFs de ART de equipamentos.
- `RelativePath` é relativo ao `AppDataDirectory`; para ler: `Path.Combine(FileSystem.AppDataDirectory, file.RelativePath)`.

#### `PlanProcedureSnapshots` (1:0..1 com o plano)

- `PlanId` (PK/FK Cascade), `CraneModelCode` (100), `Content` (texto completo), `ContentSha256` (64), `CreatedAt`.

#### `PlanExports` (sem uso)

- `Id`, `PlanId` (FK Cascade), `ExportedAt`, `FilePath` (500), `FileSha256` (64), `WasFinal`. Índice `(PlanId, ExportedAt)`.

#### Cadastros

- `Clients`: `Id`, `Name` (200), `Cnpj` (18, único quando não nulo), `IsActive`, `CreatedAt`, `UpdatedAt`.
- `Riggers`: `Id`, `FullName` (200), `DisplayName` (120), `RppNumber` (50), `CreaNumber` (50, nulo), `IsActive`.
- `Equipments`: `Id`, `EquipmentType` (100), `TruckModel` (150), `LicensePlate` (20), `ChassisNumber` (100, único), `ManufactureYear`, `CraneBrand` (100), `CraneModel` (100), `IsActive`, `CreatedAt`, `UpdatedAt`.
- `EquipmentReports`: `Id`, `EquipmentId` (Restrict), `FileId` (Restrict), `ArtNumber` (50), `ValidFrom`/`ValidTo` (obrigatórios), `CreatedAt`.
- `PlanNumberCounters`: `YearMonth` (PK, 4, ex.: `"2611"`), `LastSequence`.

---

## 4. Princípios de persistência (invariantes)

1. **Snapshot (plano autossuficiente).** Ao salvar, `PlanEntryPersistence.ApplyReferences` copia para o plano: `TruckModel`, `LicensePlate`, `ChassisNumber`, `ManufactureYear`, `CraneBrand`, `CraneModel` (do equipamento) e `RiggerName`, `RiggerRpp`, `RiggerCrea` (do rigger). `ClientName`/`ClientCnpj` vêm de `ApplyToSavedPlan`. **Não remover essas colunas.**
2. **Nada derivado é gravado.** Não criar colunas/propriedades para: total por linha (`Quantity * UnitWeightKg`), totais de lingada/dispositivos, carga bruta (kg/t), taxa de utilização, número do item, código do modelo, laudo válido/vencido.
3. **Numeração `AAMMNNN`.** Gerada no primeiro `Save`: `yearMonth = form.PlanDate.ToString("yyMM")`; incrementa `PlanNumberCounters.LastSequence`; `PlanNumber = $"{yearMonth}{seq:000}"`. A sequência reinicia por mês da **data do plano**. Números nunca são reutilizados.
4. **Status.** `"draft"` (editável) ou `"finalized"` (somente leitura). O codebase trata `"finalized"` em vários pontos, mas **não há ação que o defina** hoje.
5. **Exceção única ao "nada derivado":** o snapshot do procedimento, gravado na finalização, é cópia congelada de conteúdo fixo.
6. **Imutabilidade do finalizado:** planejada para a camada de persistência (Seção 6.5 do brief), ainda **não implementada**.
7. **Anexos transacionais.** Uploads vão para staging (`rigging-plan-staging/{sessão}/`) e só viram `StoredFile`/`PlanDrawing` no "Salvar", dentro da mesma transação do plano. Substituições/remoções só apagam os arquivos definitivos após o commit; falhas no save limpam as cópias novas e a transação é revertida.

---

## 5. Fluxo de cadastro (wizard de 5 etapas)

### 5.1 Estado em memória: `PlanDraftState`

- Serviço **Scoped** registrado em `MauiProgram` (`AddScoped<PlanDraftState>`).
- Guarda: `Form` (`PlanFormModel`), `Plan` (`RiggingPlan?`), `Accessories` (`List<PlanAccessory>`), `Attachments` (`AttachmentDraftState`, dono da sessão de staging), `PlanId`, `IsInitialized`, `IsPersisted`, `SuppressLeaveConfirmation`.
- `GetPageTitle()` gera o título `Plano {PlanNumber} - {OperationName}` (ou "Novo plano").
- `Reset()` limpa tudo, apaga o diretório de staging e inicia nova sessão de anexos (usado ao salvar/descartar).
- **Navegar entre etapas não salva no banco.** O rascunho vive em memória.

### 5.2 Rotas e componentes

| Etapa | Rota | Componente de página | Conteúdo |
|---|---|---|---|
| 1 Dados de entrada | `/new-plan` e `/new-plan/{PlanId:guid}` | `Pages/NewPlan.razor` | `RpPlanEntryForm` |
| 2 Capacidade | `/new-plan/{PlanId:guid}/capacity` | `Pages/Capacity.razor` | seções `RpCapacity*` |
| 3 Anexos | `/new-plan/{PlanId:guid}/attachments` | `Pages/PlanPlaceholder.razor` | `RpAttachmentsStage` |
| 4 Preview | `/new-plan/{PlanId:guid}/preview` | `Pages/PlanPlaceholder.razor` | `RpPreviewStage` (placeholder) |
| 5 Exportação | `/new-plan/{PlanId:guid}/export` | `Pages/PlanPlaceholder.razor` | `RpExportStage` (placeholder) |

- O layout comum é `RpPlanStepLayout` (cabeçalho + `RpPlanActions` + `RpPlanLeaveGuard` + stepper + navegação).
- `RpPlanActions` exibe "Salvar"/"Descartar"; para as etapas 3–5, "Salvar" apenas navega para `/new-plan/{PlanId}?save=true` (a etapa 1 faz o save de fato).
- `RpPlanLeaveGuard` usa `NavigationLock`: bloqueia saída para fora do plano sem salvar; navegação interna entre etapas do mesmo plano é liberada.

### 5.3 Etapa 1 — Dados de entrada

- **Lookups:** `PlanEntryState.LoadActiveLookupsAsync` carrega `Equipments`, `Riggers`, `Clients` **ativos** (`AsNoTracking`). Em modo somente leitura, carrega todos (`LoadAllLookupsAsync`).
- **Seleção com autocomplete/criação rápida:** `SelectClient`, `SelectEquipment`, `SelectRigger` preenchem o formulário; `CreateXFromInput` marca como novo.
- **Formulário:** `PlanFormModel` (`IValidatableObject`) valida no `EditForm`; obrigatórios: operação, local, data, cliente (nome + CNPJ se novo), equipamento (modelo; caminhão/chassi/placa se novo), responsável (nome de exibição; nome completo + RPP se novo). Erros em pt-BR.
- **Persistência:** `PlanEntryPersistence.SaveAsync(db, draft, planId, isDraftPlan, state)` em **transação única**:
  1. `ResolveClientAsync` — se `ClientId` existe, busca; senão valida CNPJ, impede duplicidade e **cria** o cliente.
  2. `ResolveEquipmentAsync` — se `EquipmentId` existe, busca; senão procura por `CraneModel` idêntico ou **cria** (`EquipmentType = "munck"`).
  3. `ResolveRiggerAsync` — se `RiggerId` existe, busca; senão procura por `DisplayName` idêntico ou **cria**.
  4. Plano existente: `ApplyToSavedPlan` + `CopyCapacityFields` + `ApplyReferences` + `SaveChanges`.
  5. Plano novo: gera `PlanNumber` via `PlanNumberCounters`, cria `RiggingPlan`, aplica os mesmos mapeamentos.
  6. `SaveAccessoriesAsync` — **substitui** todos os `PlanAccessories` do plano pelos itens do rascunho (delete + insert).
- Depois do save: `Draft.Reset()`, `SuppressLeaveConfirmation = true`, `NavigateTo("/")`.
- `PlanFormMapper` é o mapeador bidirecional `PlanFormModel ↔ RiggingPlan` (inclui `LoadFromPlan` para reidratar o formulário).

### 5.4 Etapa 2 — Capacidade de carga

- `CapacityState` expõe `Lingada` e `Device` (`CapacityAccessoryState`), `GrossWeight`, `GrossWeightTons`, `Utilization`, `Items` (espelha `draft.Accessories`).
- **Os itens são editados em memória** e só vão para o banco no "Salvar" da etapa 1 (`SaveAccessoriesAsync`). `SortOrder` é sequencial por plano.
- **Campos digitados** (vindos de `RpCapacity*Section`): `CraneNominalCapacity`, `BoomLengthM`, `OperatingRadiusM`, `LoadCapacityT`, `ExtraLoadKg`, `QuadrantStart`, `QuadrantEnd`, `FlatStrapUtilizationPct`, `TubularStrapUtilizationPct`, `WireRopeSlingUtilizationPct`, `ShackleUtilizationPct`, `FootingForceKgf`, `MinGroundResistanceKgfCm2`, `MaxWindSpeedMs`, `ArtNumber`. (`ArtFileId` é tratado à parte, na etapa 3.)
- Ao abrir a capacidade, `PlanFormMapper.ApplyToDraftPlan(Draft.Form, State.Plan)` sincroniza os dados da etapa 1 **sem salvar**.
- Ao salvar, `CopyCapacityFields(draft.Plan, target)` copia esses campos do rascunho para o plano persistido; os anexos são aplicados por `AttachmentPersistence.ApplyAsync`.

### 5.5 Etapa 3 — Anexos (staging em disco + persistência no "Salvar")

- `AttachmentsState` não grava mais no banco durante o upload:
  - `OnArtFileChangeAsync`: valida PDF, copia o arquivo para `rigging-plan-staging/{sessão}/` e substitui `Plan.ArtFileId`/`ArtFile` em memória (o anterior de staging é apagado).
  - `RemoveArtFileAsync`: limpa `ArtFileId`/`ArtFile` em memória e apaga o arquivo de staging, se houver.
  - `OnDrawingsChangeAsync`: aceita várias imagens (PNG/JPG/WEBP/BMP), copia cada uma para o staging e adiciona um `PlanDrawing` em memória (`SortOrder` sequencial).
  - `RemoveDrawingAsync`: remove o `PlanDrawing` da memória e apaga o staging (se novo); removeções de desenhos já persistidos só valem no "Salvar".
  - `GetFileAsDataUri(StoredFile)`: lê o arquivo (staging ou definitivo) e devolve `data:{mime};base64,...` para exibição.
- `AttachmentPersistence.ApplyAsync` roda **dentro da transação do `PlanEntryPersistence.SaveAsync`**: copia os arquivos de staging para `rigging-plan-art`/`rigging-plan-drawings`, cria `StoredFile` e `PlanDrawing`, atualiza `ArtFileId` e remove os registros substituídos (arquivos definitivos só são apagados após o commit).
- `PlanDraftState.Reset()` (Descartar, sair sem salvar ou novo plano) apaga o diretório de staging. `MauiProgram` chama `AttachmentDraftState.CleanupStaleSessions()` no início para remover sobras de sessões anteriores.
- Se o save falhar, as cópias novas são apagadas, a transação é revertida e o staging permanece para nova tentativa.
- Limites: ART até 10 MB; desenhos até 20 MB por arquivo, 20 por vez.
- O número da ART é digitado no próprio plano (`Plan.ArtNumber`) e reaproveitado nas demais páginas.

### 5.6 Etapas 4 e 5 — Preview e Exportação (estado atual)

- `RpPreviewStage.razor` e `RpExportStage.razor` contêm apenas `<RpStagePlaceholder ... />` ("Esta etapa ainda está em preparação").
- `PlanPlaceholder.razor` carrega o plano com `PlanWorkflowLoader.LoadAsync(..., useDraftFallback: true)` e repassa `IsReadOnly`.
- **Não há:** renderização de folhas A4, visualizador de páginas, serviço de PDF, escolha de pasta de destino, registro em `PlanExports`.

---

## 6. Como os dados são lidos para gerar o documento

### 6.1 Leitura do plano

`PlanWorkflowLoader.LoadAsync(db, draft, planId, includeAccessories, useDraftFallback)`:

- Se o rascunho já estiver inicializado para aquele `planId`, devolve `draft.Plan` (memória).
- Caso contrário, busca no banco com:
  - `AsNoTracking()`
  - `Include(x => x.ArtFile)`
  - `Include(x => x.Drawings).ThenInclude(x => x.File)`
  - filtro `DeletedAt == null`
- `includeAccessories: true` preenche `draft.Accessories` com `PlanAccessories` ordenados por `SortOrder`.
- `useDraftFallback: true` devolve o rascunho em memória se o plano não existir no banco (usado por `PlanPlaceholder`).

> **Limitação importante para Preview/Export:** o loader **não** inclui `Client`, `Equipment`, `Rigger`, `ProcedureSnapshot`, `Exports` nem `EquipmentReport`. Como o plano já traz os snapshots dessas entidades, o documento pode ser montado sem eles; mas se precisar de dados do cadastro ou do snapshot, será necessário estender o loader ou fazer query direta.

### 6.2 Procedimento operacional

`OperationalProcedureService` (Domínio, estático):

- `BuildDocument(plan)` → `OperationalProcedureDocument` com `OperationName`, `ArtNumber`, `PlanNumber`, `CraneModelCode` e `Pages` (5 seções; 3 na página 1 e 2 na página 2).
- `DeriveCraneModelCode(craneModel, craneBrand)`: remove a marca quando prefixa o modelo; remove espaços e pontos; caixa alta. Ex.: `"MD 45.007"` → `"MD45007"`.
- `GenerateContent(plan)` / `RenderContent(document)`: texto completo (cabeçalho, seções, rodapé com endereço/CNPJ, numeração).
- `CreateSnapshot(plan)`: gera `PlanProcedureSnapshot` com `Content` + `ContentSha256`.
- `ResolveContent(plan, snapshot)`: **plano finalizado sempre usa o snapshot**; rascunho gera na hora.
- O texto fixo está embutido no serviço (espelhado em `docs/procedure-spec.md`).

### 6.3 Valores computados (exibição)

| Valor | Fórmula | Onde |
|---|---|---|
| Peso total do item | `Quantity * UnitWeightKg` | não persistido |
| Total lingada | `sum(itens Section == "lingada")` | `CapacityAccessoryState.Total` |
| Total dispositivos | `sum(itens Section == "dispositivo")` | `CapacityAccessoryState.Total` |
| Carga bruta (kg) | `WeightKg + Lingada + Dispositivos + ExtraLoadKg` | `CapacityState.GrossWeight` |
| Carga bruta (t) | `GrossWeight / 1000` | `CapacityState.GrossWeightTons` |
| Taxa de utilização | `GrossWeightTons / LoadCapacityT * 100` (nulo se `LoadCapacityT <= 0`) | `CapacityState.Utilization` |
| Código do modelo | derivado de `CraneModel`/`CraneBrand` | `OperationalProcedureService` |

### 6.4 Arredondamento

- UI atual: totais e carga bruta em `N2`; `LoadCapacityT` em `N2`; utilização em `N1`.
- Brief (`project-brief.md` §6.3) define para o **PDF final**: carga bruta em kg **sem casas**, em t com **2 casas**; utilização com **1 casa**; totais de lingada com **2 casas**. O arredondamento é só de exibição — os valores persistidos não são arredondados.

---

## 7. Mapeamento campo → parte do documento

Referência: `project-brief.md` §6.1/§6.2 (exemplo real: plano 2607036, Kaefer, 10 páginas).

| Parte do PDF | Origem dos dados |
|---|---|
| **1. Capacidade de carga (folha-resumo)** | |
| Cabeçalho (cliente, local, data, nº plano) | `ClientName`, `Location`, `PlanDate`, `PlanNumber` |
| Dados da carga | `LoadDescription`, `LengthM`, `WidthM`, `HeightM`, `WeightKg` |
| Lingada / amarras (tabela) | `PlanAccessories` com `Section = "lingada"` |
| Dispositivos / balancim (tabela) | `PlanAccessories` com `Section = "dispositivo"` (inclui `DrawingRef`) |
| Informações do guindaste | `CraneModel`, `TruckModel`, `LicensePlate`, `ChassisNumber`, `ManufactureYear` |
| Configuração do guindaste | `BoomLengthM`, `OperatingRadiusM`, `LoadCapacityT` |
| Composição da carga bruta | `WeightKg` + totais calculados + `ExtraLoadKg` |
| Quadrante operacional | `QuadrantStart`, `QuadrantEnd` (texto livre) |
| Resumo dos cálculos | `LoadCapacityT`, utilização calculada, `FlatStrapUtilizationPct`, `TubularStrapUtilizationPct`, `WireRopeSlingUtilizationPct`, `ShackleUtilizationPct`, `FootingForceKgf`, `MinGroundResistanceKgfCm2`, `MaxWindSpeedMs` |
| ART / observações | `ArtNumber`, `Observations` |
| Assinaturas | `RiggerName` + `RiggerRpp` (+ `SupervisorName`, hoje sem UI) |
| **2. Desenhos técnicos** | `PlanDrawings` (ordem por `SortOrder`) → `StoredFiles` (imagem em disco) |
| **3. ART do plano** | `ArtFile` (`StoredFiles`, PDF) + `ArtNumber` |
| **4. Procedimento operacional** | gerado por `OperationalProcedureService` a partir de `OperationName`, `ArtNumber`, `PlanNumber`, `CraneModel`/`CraneBrand`; se finalizado, de `PlanProcedureSnapshots.Content` |
| **5. Laudo técnico + ART do laudo** | `EquipmentReports` + `StoredFiles`, vinculados por `RiggingPlan.EquipmentReportId` — **hoje não vinculados no fluxo** |

---

## 8. Estado atual vs. pendências (o que a IA de Preview/Export precisa saber)

### Já existe e é utilizável

- [x] Plano completo com snapshots de cliente, equipamento e responsável.
- [x] Itens de lingada/dispositivo persistidos e ordenados.
- [x] Anexos: ART do plano (PDF) e desenhos (imagens) staged em disco e persistidos transacionalmente no "Salvar", com limpeza ao descartar.
- [x] Procedimento operacional gerado + snapshot + hash, com componente de visualização `RpOperationalProcedure` pronto.
- [x] Cálculos de exibição (totais, carga bruta, utilização).
- [x] Navegação entre as 5 etapas com guarda de saída e título dinâmico.

### Ainda não existe (não presumir)

- [ ] **Preview** (etapa 4): nenhum renderizador de folhas A4/páginas.
- [ ] **Exportação** (etapa 5): nenhuma biblioteca de PDF, nenhum serviço, nenhum uso de `PlanExports`.
- [ ] **Finalização na UI/persistência:** não há ação que grave `Status = "finalized"`/`FinalizedAt` nem que crie o snapshot. O snapshot só é criado por `OperationalProcedureService.CreateSnapshot` (testado), mas não é chamado pelo fluxo da aplicação.
- [ ] **Imutabilidade fora da UI** (Seção 6.5 do brief).
- [ ] **`plan_change_log`** (histórico de alterações) — não modelado.
- [ ] **Vínculo de laudo no plano** (`EquipmentReportId`) — nunca preenchido.
- [ ] **Pastas** (`Folder`) e **lixeira** (`DeletedAt` com restauração) — sem UI.
- [ ] **Seleção de laudo válido** do equipamento no plano.

### Riscos e observações para o projeto de Preview/Export

1. **Rascunho não persistido:** para um plano novo, os dados podem existir só em memória (`PlanDraftState`). O `PlanPlaceholder` usa `useDraftFallback: true`; ao recarregar a URL de capacidade/preview de um rascunho nunca salvo, o plano pode não ser encontrado. O Preview deve decidir como tratar esse caso.
2. **Includes do loader:** para montar o documento completo pode ser necessário carregar `ProcedureSnapshot` (e, se for vincular laudo, `EquipmentReport` + `File`). O loader atual não traz isso.
3. **Somente leitura:** `PlanWorkflowLoader` usa `AsNoTracking()` ao vir do banco. Preview/Export devem ser read-only; para registrar `PlanExport` será preciso um contexto próprio.
4. **Finalizado:** `OperationalProcedureService.ResolveContent` já encapsula a regra "finalizado usa snapshot". Reutilizar.
5. **Valores ausentes:** exibir `"-"` (mesma convenção do brief).
6. **Não gravar dados derivados** durante a exportação (nem totais, nem texto do procedimento em rascunho).
7. **Arquivos em disco:** o PDF final precisa ler os arquivos de `rigging-plan-art`, `rigging-plan-drawings` e `equipment-art` a partir de `FileSystem.AppDataDirectory`.
8. **Arredondamento:** definir o do PDF conforme o brief (kg sem casas, t com 2, utilização com 1).

---

## 9. Referências de arquivos

| Tema | Caminho |
|---|---|
| Contexto de produto e regras | `docs/project-brief.md` |
| Texto do procedimento | `docs/procedure-spec.md` |
| Guia para agentes | `docs/agent-guide.md` |
| Histórico | `docs/progress.md` |
| DbContext e mapeamento | `src/RiggingPlanner.Infrastructure/Persistence/AppDbContext.cs` |
| DI e migrations | `src/RiggingPlanner.Infrastructure/DependencyInjection.cs` |
| Entidades | `src/RiggingPlanner.Domain/Entities/*.cs` |
| Procedimento/snapshot | `src/RiggingPlanner.Domain/Services/OperationalProcedureService.cs` |
| Estado do rascunho | `src/RiggingPlanner.App/Components/Shared/PlanDraftState.cs` |
| Formulário e validação | `src/RiggingPlanner.App/Components/Shared/PlanFormModel.cs` |
| Mapeador | `src/RiggingPlanner.App/Components/Plan/Shared/PlanFormMapper.cs` |
| Leitura do plano | `src/RiggingPlanner.App/Components/Plan/Workflow/PlanWorkflowLoader.cs` |
| Persistência do plano | `src/RiggingPlanner.App/Components/Plan/Entry/PlanEntryPersistence.cs` |
| Lookups da etapa 1 | `src/RiggingPlanner.App/Components/Plan/Entry/PlanEntryState.cs` |
| Capacidade e cálculos | `src/RiggingPlanner.App/Components/Plan/Capacity/CapacityState.cs` |
| Anexos/arquivos | `src/RiggingPlanner.App/Components/Plan/Attachments/{AttachmentsState,AttachmentDraftState,AttachmentPersistence}.cs` |
| Preview/Export (placeholders) | `src/RiggingPlanner.App/Components/Plan/Preview/RpPreviewStage.razor`, `.../Export/RpExportStage.razor` |
| Páginas do wizard | `src/RiggingPlanner.App/Components/Pages/{NewPlan,Capacity,PlanPlaceholder,Plans}.razor` |
| Testes de banco | `tests/RiggingPlanner.Infrastructure.Tests/DatabaseMigrationTests.cs` |
