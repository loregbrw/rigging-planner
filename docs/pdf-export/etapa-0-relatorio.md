# Etapa 0 — Relatório (base da exportação em PDF)

> Branch: `feature/pdf-stage-0` · Data: 16/11/2026 · Status: **Concluída — build Release 0 erros/0 avisos; 83/83 testes passando, 0 ignorados. Divergência do fixture resolvida pela decisão do responsável (Opção B, Seção 5).**

## 1. Aviso importante: trabalho pré-existente no diretório de trabalho

A branch `feature/pdf-stage-0` já existia e o diretório de trabalho já continha, **não commitados**, uma implementação prévia dos Passos 2–6 (pacotes, fontes, formatador, ferramentas de PDF e contratos). Este relatório parte desse estado: o trabalho pré-existente foi revisado linha a linha contra a especificação, corrigido (não compilava — ver Seção 4) e validado com build e testes. Nada foi commitado.

## 2. Respostas das pré-checagens (Passo 1)

### 2.1 Responsável no plano

- **`RiggerName` recebe `DisplayName`**, não `FullName`. Evidência: `PlanEntryPersistence.ApplyReferences` — `plan.RiggerName = rigger.DisplayName;` (`src/RiggingPlanner.App/Components/Plan/Entry/PlanEntryPersistence.cs:216`).
- **Não existe coluna para `FullName` no plano.** `RiggingPlan` tem apenas `RiggerName`, `RiggerRpp`, `RiggerCrea` (`src/RiggingPlanner.Domain/Entities/RiggingPlan.cs:46-48`). `FullName`/`DisplayName` ambos existem só em `Rigger` (`src/RiggingPlanner.Domain/Entities/Rigger.cs:6-7`).
- **Formato sem prefixo:** `plan.RiggerRpp = rigger.RppNumber;` e `plan.RiggerCrea = rigger.CreaNumber;` (`PlanEntryPersistence.cs:217-218`) — o banco guarda o valor digitado, sem "RPP-"/"CREA-PR". A UI de cadastro também não adiciona prefixo (rótulos "RPP" e "CREA" em `src/RiggingPlanner.App/Components/Pages/Riggers.razor:18-19`).
- Conforme D12, o formato esperado é "23-1040" e "78755/D". **Pendente:** conferir valores reais gravados no banco de produção (`%LOCALAPPDATA%\RiggingPlanner\rigging-planner.db`) — não foi possível consultar o banco desta máquina nesta sessão.
- Consistente com D5 (assinatura cursiva = nome de exibição) e com `plan-data-flow.md` §7.

### 2.2 Supervisor

- `SupervisorName` **existe** na entidade (`RiggingPlan.cs:49`) e no banco (`AppDbContext.cs:111`, coluna desde a migration inicial).
- **Não existe** campo na etapa 1 nem em qualquer tela (grep por "Supervisor" em `src/RiggingPlanner.App`: nenhum resultado) e **nenhum fluxo o preenche**.
- Conforme documentado (pendência P-SUP e `plan-data-flow.md` §3.4 "fora do formulário atual").

### 2.3 Equipamento — ⚠️ divergência relevante

- **Colunas separadas existem** na entidade e no banco: `Equipment.CraneBrand` e `Equipment.CraneModel` (`src/RiggingPlanner.Domain/Entities/Equipment.cs:11-12`), replicadas no plano (`RiggingPlan.cs:30-31`).
- **Porém a marca nunca é preenchida.** A tela de equipamentos não tem campo de marca e grava `stored.CraneBrand = string.Empty;` (`src/RiggingPlanner.App/Components/Pages/Equipments.razor:234`). O formulário da etapa 1, ao criar equipamento novo, também grava `CraneBrand = string.Empty` (`PlanEntryPersistence.cs:151`). Hoje um equipamento é salvo **"junto": só o modelo** digitado no campo "Modelo do Guindaste" (ex.: "MD 45.007" — sem "Palfinger").
- O brief §16 item 8 pedia exatamente esta verificação ("se não forem, separar"). No nível de schema estão separados; na prática, a marca está sempre vazia. **Impacto para a etapa 1:** `DeriveCraneModelCode(craneModel, craneBrand)` não terá marca a remover; "MD 45.007" → "MD45007" funciona mesmo assim. A folha-resumo imprimiria o modelo sem a marca (diferente do exemplo, cuja página 1 traz a marca no cabeçalho? — a conferir na revisão da etapa 1).

### 2.4 Local

- `Location` está no formulário da etapa 1 e é **obrigatório**: `[Required(ErrorMessage = "Informe o local.")]` (`src/RiggingPlanner.App/Components/Shared/PlanFormModel.cs:10-11`) e campo renderizado com marcador de obrigatório (`RpPlanEntryOperationSection.razor:8-14`).

### 2.5 Logo

- `src/RiggingPlanner.App/wwwroot/images/guindastes-ribas-logo.png` — **511×278 px**, `Format32bppArgb` (canal alfa → fundo transparente suportado).
- `src/RiggingPlanner.App/wwwroot/images/guindastes-ribas-name.png` — 511×49 px, também 32bppArgb (logotipo só com o nome).

### 2.6 Projetos

| Projeto | TFM | Referências |
|---|---|---|
| `RiggingPlanner.Domain` | `net10.0` | — |
| `RiggingPlanner.Application` | `net10.0` | Domain ✔ (já referencia; pode ser referenciada por Infrastructure) |
| `RiggingPlanner.Infrastructure` | `net10.0` | Application (e transitivamente Domain) |
| `RiggingPlanner.App` | `net10.0-windows10.0.19041.0` | Application + Infrastructure |
| `tests/RiggingPlanner.Domain.Tests` | `net10.0` | Domain + Application |
| `tests/RiggingPlanner.Infrastructure.Tests` | `net10.0` | Infrastructure ✔ (projeto de testes de Infrastructure existe) |

### 2.7 CI

- `.github/workflows/ci.yml:10` — **`runs-on: windows-latest`** (Windows). Roda `dotnet build -c Release` + `dotnet test -c Release --no-build`.
- Pacotes nativos funcionam nesse runner: QuestPDF (Skia), PDFtoImage (PDFium) e SkiaSharp publicam assets nativos `win-x64`. O aviso CA1416 no consumo de `Conversion.ToImage` foi tratado com `[assembly: SupportedOSPlatform("windows")]` no projeto de testes (`tests/RiggingPlanner.Infrastructure.Tests/AssemblyInfo.cs`).

### 2.8 Fixtures

- Ambos existem: `tests/RiggingPlanner.Infrastructure.Tests/Fixtures/reference-plan-2607036.pdf` (2,9 MB, 10 páginas) e `.../summary-sheet.reference.json` (294 palavras da página 1).
- **Porém (ver Seção 5): o JSON não é comparável a <0,5 pt com a leitura PdfPig do PDF.**
- Privacidade (decisões §7): os fixtures contêm dados reais (cliente, chassi, placa, ART). Estão **sem track** no git; **não commitar** enquanto o repositório for público.

### 2.9 Anexos em staging

- Depois do "Salvar", `AttachmentPersistence.ApplyAsync` copia os arquivos de staging para as pastas definitivas e grava em `StoredFile.RelativePath` o caminho final (`rigging-plan-art\...` / `rigging-plan-drawings\...`) — `CopyToFinalFolder` sobrescreve `RelativePath` antes de persistir (`src/RiggingPlanner.App/Components/Plan/Attachments/AttachmentPersistence.cs:131-143`). **Nenhum caminho de staging fica gravado em `StoredFile`.**
- O loader traz `ArtFile` e `Drawings[].File` carregados: `Include(x => x.ArtFile)` e `Include(x => x.Drawings).ThenInclude(x => x.File)` (`src/RiggingPlanner.App/Components/Plan/Workflow/PlanWorkflowLoader.cs:29-31`).
- Staging: `rigging-plan-staging/{sessão}` (`AttachmentDraftState.cs:7-11`); detecção por prefixo `IsStaged` (`AttachmentDraftState.cs:42-43`).
- **Leitura de arquivo persistido:** `Path.Combine(FileSystem.AppDataDirectory, file.RelativePath)` (mesmo padrão de `AttachmentPersistence.cs:135`).

## 3. Pacotes adicionados e licenças (Passo 2)

| Pacote | Versão | Projeto | Licença |
|---|---|---|---|
| `QuestPDF` | 2026.9.2 | Infrastructure | Community (gratuita p/ receita anual < US$ 1 mi — condição confirmada pelo responsável em 16/11/2026). Declarada em `PdfFontRegistry.cs:30-32`. |
| `PdfPig` | 0.1.16 | Infrastructure.Tests (só testes) | Apache-2.0 |
| `PDFtoImage` | 5.4.0 | Infrastructure.Tests (só testes) | MIT |
| `SkiaSharp` | 4.150.1 (transitivo de PDFtoImage) | só testes | MIT |

- Biblioteca de junção de PDF não adicionada (fica para a etapa 3), conforme o especificado.
- Fontes Carlito: Carlito-Regular.ttf (628.032 B) e Carlito-Bold.ttf (682.468 B) como `EmbeddedResource` em `src/RiggingPlanner.Infrastructure/Pdf/Fonts/`, junto da licença `OFL.txt` (SIL OFL 1.1, "The Carlito Project Authors", github.com/googlefonts/carlito).
- `PdfFontRegistry.EnsureRegistered()` registra as fontes uma única vez (dupla verificação com lock) via `FontManager.RegisterFontFromStream` e expõe `PdfFontRegistry.FamilyName = "Carlito"`.

## 4. Implementação e correções realizadas

O trabalho pré-existente cobria os Passos 2–6 e foi mantido, com estas correções (não compilava):

1. **`PdfRasterizer.cs`** — a API do PDFtoImage 5.x mudou: `Conversion.ToImage` não tem `dpi:` nem `pngFmt:` e retorna `SKBitmap`. Reescrito para `Conversion.ToImage(pdfBytes, page, options: new RenderOptions { Dpi = dpi })` + encode PNG via SkiaSharp.
2. **`PdfTextReader.cs`** — `Word.PageHeight` não existe no PdfPig 0.1.16; a altura da página agora vem de `page.Height` e é passada ao conversor de coordenadas. Adicionada a sobrecarga `ReadPages(string pdfPath)` exigida pela especificação ("lê um PDF (bytes ou caminho)").
3. **`PlanValueFormatterTests.cs:115`** — ternário `null`/`DateTime` sem tipo comum; corrigido com cast `(DateTime?)`.
4. **`AssemblyInfo.cs`** (novo) — `[assembly: SupportedOSPlatform("windows")]` no projeto de testes para o aviso CA1416 (PDFium é específico de plataforma; CI é windows-latest).

Os demais artefatos revisados e aprovados conforme a especificação: `PlanValueFormatter` (Domain, puro, pt-BR fixo, `AwayFromZero`, sem `double`, sem separador de milhar; tipos `Trim2`/`Min1Max2`/`Fixed1`/`Fixed2`/`Int0` + `Date` + `Text`), testes do formatador (todos os exemplos do modelo + bordas 0/0,005→"0,01"/2,345→"2,35"/10,0/negativos/null, sob `en-US` e `de-DE`, e verificação do exemplo 3746,62→"3747", 3,74662→"3,75", 47,1), teste de fonte (página A4 "Íçamento — AÇÃO 7/8" regular+bold, `FontFile2` presente = fonte embutida, texto extraído idêntico com acentos), ferramentas (`ReferenceSheet`, `PdfRasterizer` 150 dpi, `PdfOverlay` vermelho/ciano/cinza com métrica de divergência, `ReviewOutput` → `artifacts/pdf-review/stage-N/`) e o contrato `ISummarySheetRenderer` comentado em TODO na Application (o tipo `SummarySheetModel` só existe na etapa 1).

## 5. ⚠️ Divergência que exige decisão (Passo 5 — teste de sanidade)

**Sintoma:** `PdfTextReader_ExamplePdf_MatchesReferenceJson` falha — o PDF extrai **295 palavras**, o JSON tem **294**, e há deslocamentos sistemáticos (~1 pt em `top`, ~2–4 pt em `x1`) e diferenças de segmentação (ex.: PDF "2607036" vs JSON "260703"; ordem de "Industriais Ltda." vs "Rip Serviços" na mesma linha).

**Causa:** o `summary-sheet.reference.json` **não foi gerado com PdfPig** — as fontes no JSON são "Calibri" (sem prefixo de subset) com tamanhos nominais (9, 11.04), enquanto o PdfPig reporta "BCDFEE+Calibri" e caixas de palavra medidas pela métrica própria. O critério da etapa ("mesmas palavras, posições com diferença < 0,5 pt") **não é atingível** com esse par de fixtures, por melhor que seja o extrator.

**Opções (não decidi sozinho):**

- **A (recomendada) — Regenerar o `summary-sheet.reference.json` com o próprio `PdfTextReader`** a partir do PDF de exemplo. Todas as comparações daqui em diante (etapas 1–6) usam o mesmo leitor para referência e gerado, então a consistência interna é o que importa; o teste de sanidade mantém o critério <0,5 pt e passa com delta 0 por construção. O JSON regenerado continua fora do git (privacidade). Risco: perde-se a "referência externa" original (as posições absolutas mudam ~1 pt em relação ao JSON atual).
- **B — Manter o JSON, rebaixar o teste de sanidade** para comparar apenas o conteúdo de texto (sem posições). As posições só seriam validadas na etapa 1 pela sobreposição visual. Risco: o `<0,5 pt` da especificação deixa de ser verificado na etapa 0; o offset sistemático de ~1 pt do JSON externo contaminaria as comparações de posição da etapa 1.
- **C — Manter o JSON e substituir o teste de sanidade por um teste só de contagem de palavras com tolerância ampliada (~4 pt).** Intermediário e frágil.

**Decisão do responsável (16/11/2026): Opção B.** Implementação do teste de sanidade (`PdfTextReader_ExamplePdf_TextContentMatchesReferenceJson`):

- **a) Fidelidade de conteúdo:** multiconjunto de caracteres idêntico entre a extração do PDF e os textos do JSON (ignorando tokens só de espaço/espaços em branco) — nenhuma letra perdida, adicionada ou alterada.
- **b) e c) Contiguidade recíproca:** cada palavra do JSON é um trecho contíguo do texto extraído do PDF na **ordem natural de extração** (e vice-versa). Isso tolera as 3 diferenças de segmentação conhecidas ("Rigger"+"Giuliano"↔"RiggerGiuliano", "-"+"RPP-23-1040"↔"-RPP-23-1040", "2607036"↔"260703"+"6"), que são mesclagens/divisões do mesmo conteúdo. A ordenação por posição (`top`, `x0`) **não** é usada: diferenças de métrica de ~1 pt reordenam palavras da mesma linha.
- Ajustes no `PdfTextReader` coerentes com a decisão: extrator **default** (`page.GetWords()`, cuja segmentação corresponde ao JSON — 295 vs 450 do `NearestNeighbourWordExtractor`) e ordem natural de extração preservada (a ordenação posicional fica a cargo dos testes da etapa 1).
- **Consequência para a etapa 1:** o JSON de referência **não pode** ser usado como fonte de posições <0,5 pt; usar a sobreposição visual (`PdfOverlay`) e as medidas do próprio `PdfTextReader` como referência de geometria, ou regenerar o JSON nessa etapa (ver P-0a' em `etapa-1-folha-resumo.md`).

## 6. Resultado dos testes (final)

- `dotnet build RiggingPlanner.slnx -c Release` — **0 erros, 0 avisos**.
- `dotnet test RiggingPlanner.slnx -c Release --no-build`:
  - `RiggingPlanner.Domain.Tests` — **74/74 passaram** (0 ignorados).
  - `RiggingPlanner.Infrastructure.Tests` — **9/9 passaram** (0 ignorados).
  - Total: **83/83**, 0 falhas, 0 ignorados — critério de aceite atendido.

## 7. Saída da sobreposição

- `artifacts/pdf-review/stage-0/self-overlay-reference-plan-2607036.png` (exemplo contra ele mesmo — divergência 0%, como exigido) e `artifacts/pdf-review/stage-0/diff-metrics.txt`. Pasta ignorada pelo git (`.gitignore`: `artifacts/`, confirmado com `git check-ignore`).

## 8. Pendências registradas

- **P-0a (resolvida):** decisão da Seção 5 — Opção B aplicada. **P-0a' (para a etapa 1):** definir a fonte de geometria das comparações de posição (regenerar JSON com `PdfTextReader` ou usar somente sobreposição visual), pois o JSON atual tem offset de ~1 pt.
- **P-0b:** conferir no banco real os formatos de `RiggerRpp`/`RiggerCrea` (D12 — "verificar nos dados reais").
- **P-0c:** decidir quando/se a marca do guindaste passa a ser capturada na UI (Seção 2.3) — impacta a etapa 1 (`DeriveCraneModelCode` e "Informações do guindaste").
- **P-0d:** fixtures com dados reais não devem ser commitados em repositório público (decisões §7).
