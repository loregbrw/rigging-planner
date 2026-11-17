# Etapa 1 — Relatório (folha-resumo / página 1)

> Branch: `feature/pdf-stage-1` · Data: 17/11/2026 · Status: **Concluída — build Release 0 erros/0 avisos; 101/101 testes passando (87 Domain + 14 Infrastructure), 0 ignorados. Folha gerada com a fixture: 0 diferenças de letras fora das 2 exceções declaradas (tolerância 1,0 pt em X e linha de base).**

## 1. Aviso: trabalho pré-existente no diretório de trabalho

A branch `feature/pdf-stage-1` já existia (criada a partir de `feature/pdf-stage-0`, merge-base `ddc7d74`) e o diretório de trabalho já continha, **não commitados**, uma implementação prévia parcial da etapa (modelo/builder Domain, renderer/layout/measurer, ferramentas de teste e fixtures). Como na etapa 0, o trabalho pré-existente foi revisado linha a linha contra a especificação e contra o exemplo, corrigido (não passava nos testes — ver Seção 4) e validado. O commit da etapa inclui esse trabalho revisado.

## 2. Decisões do responsável aplicadas (ajustes A–F)

| Ajuste | Decisão | Implementação |
|---|---|---|
| **A** | Comparação de posição **contra o próprio PDF de exemplo** (não o JSON, que tem offset de ~1 pt), **letra a letra** (texto, X, linha de base, tamanho, fonte): para cada letra do exemplo procura-se no gerado a letra igual mais próxima não usada; exigido \|Δx\| ≤ 1,0 pt, \|Δbase\| ≤ 1,0 pt e tamanho igual; exceções **somente** letras do rótulo do total de dispositivo e "7,9"→"7,95"; listar as 10 piores diferenças | `SummarySheetPdfTests.Letters_match_the_example_within_tolerance` reescrito na direção exemplo→gerado, com filtragem de glifos de espaço e cobertura recíproca (letras geradas não usadas também falham o teste) |
| **B** | Marca do guindaste hoje sempre vazia; "Modelo" = marca + modelo ignorando a parte vazia, sem espaço sobrando; fixture com marca "Palfinger" e modelo "MD 45.007"; junção isolada num único ponto | `SummarySheetBuilder.CraneModelDisplay` (único ponto, com comentário de pendência); fixture corrigida em `TestPlans.cs` |
| **C** | Assinatura 2 imprime **somente o rótulo** "Supervisor Responsável"; não imprimir nome | Rótulo é run fixo do layout; nenhum código imprime `SupervisorName` |
| **D** | Logo `wwwroot/images/guindastes-ribas-logo.png` (511×278) **copiado** para `Infrastructure/Pdf/Images/` como `EmbeddedResource`; caixa 86,4/34,1→190,6/90,0 preservando proporção, centralizado; diferença de proporção (1,84 vs 1,86) **não** deve falhar teste | Cópia commitada; `SummarySheetRenderer.ComposeLogo` ajusta por proporção e centraliza; teste de A4 não depende da proporção |
| **E** | Imprimir "Rigger " + `RiggerName` + " - RPP-" + `RiggerRpp` | `SummarySheetBuilder.RiggerSignature`; quando RPP não preenchido, o trecho " - RPP-…" é omitido (decisão local — ver P-1c) |
| **F** | Observações (P-OBS): manter a proposta do documento, isolada, marcada `// PENDING-CONFIRMATION` | `SummarySheetRenderer.ComposeObservations` isolado com a marcação |

## 3. O que foi implementado

- **Domain:** `SummarySheetModel` (modelo imutável já formatado), `SummarySheetBuilder` (constrói a partir de `RiggingPlan` + acessórios ordenados por `SortOrder`; numeração sequencial entre tabelas; totais por seção) e `PlanTotals` (fórmulas display-only compartilhadas com a UI). Nenhum dado derivado é persistido (regra do repositório).
- **Infrastructure:** `SummarySheetRenderer` (QuestPDF, posicionamento absoluto de ~90 retângulos de borda e ~90 runs de texto medidos do exemplo; escala reduzida quando as tabelas de acessórios excedem o layout fixo), `SummarySheetLayout` (constantes medidas + enum `SummarySheetRectKind` para o comportamento de cada retângulo no excesso), `CarlitoMeasurer` (medidas de largura com a mesma pipeline Skia do QuestPDF) e logo embutido.
- **App:** botão provisório **"Gerar folha-resumo (PDF)"** em `RpPreviewStage.razor` (`// TEMPORARY: removed in stage 6`): lê o plano **direto do banco** (`AsNoTracking`, acessórios por `SortOrder`, sem usar o rascunho), grava em `%LOCALAPPDATA%\RiggingPlanner\pdf-review\folha-resumo-{PlanNumber}.pdf` e abre com `Launcher`. Desabilitado quando `PlanDraftState.IsPersisted` é falso, com texto "O PDF usa os dados salvos do plano." e aviso de alterações não salvas (ver P-1d).
- **Testes:** 13 novos no Domain (`SummarySheetBuilderTests`) e 5 novos no Infrastructure (`SummarySheetPdfTests`), além dos 9 da etapa 0.

## 4. Diagnóstico e correções realizadas

1. **Crash do `CarlitoMeasurer` (0xC0000005):** acesso concorrente ao builder nativo SkParagraph compartilhado sob xUnit paralelo. Corrigido com `lock` em `MeasureWidth`/`MeasureWrapped`.
2. **Causa raiz do desalinhamento de letras (294 diferenças → 0):** o exemplo foi produzido pelo Word **sem kerning (GPOS) nem ligaduras**; Skia/HarfBuzz aplica ambos por padrão, deslocando glifos até ~3 pt em runs longos. Corrigido com `DisableFontFeature(FontFeatures.Kerning)` e `DisableFontFeature(FontFeatures.StandardLigatures)` no estilo padrão do renderer **e** `kern=0/liga=0` no `CarlitoMeasurer` (medição coerente com a renderização). Verificação de suporte: os avanços puros (hmtx) de Carlito e Calibri são idênticos (Δ 0,000 pt em 11,04 pt).
3. **`Pesos (kg)` → `Pesos`:** o exemplo imprime apenas "Pesos" nos títulos das caixas de pesos das duas tabelas (confirmado por dump de letras: nenhum glifo após x=492,45 na linha 165,50/283,01).
4. **Centros de "LATERAL" (quadrante):** constantes corrigidas de 112,30/236,20 para os centros medidos do exemplo **109,21/233,08** (as 14 letras dessas células eram as únicas diferenças "OFF" restantes, Δx +3,1).
5. **Espaços na assinatura do rigger:** o exemplo não emite glifos de espaço nessa run (Word posiciona por avanço); glifos de espaço são filtrados dos dois lados na comparação (posicionamento já garantido pelas letras reais).
6. **Rótulo do total de dispositivo (diferença intencional nº 1):** impresso "PESO TOTAL DOS DISPOSITIVOS / BALANCIM:", alinhado à direita terminando no mesmo ponto do rótulo da lingada (borda direita = 288,05 + largura medida do rótulo da lingada = 480,44 pt, igual ao exemplo).
7. **Reta transcription de `BorderRects`:** a barra superior do cabeçalho `[46,68, 546,58, 30,72, 32,64]` estava transcrita como um bloco maciço `[46,68, 546,58, 32,64, 96,50]`, pintando o cabeçalho de preto no PDF gerado. **Detectado apenas pela sobreposição visual** (o teste de letras passava, pois as letras ficam acima das camadas). Corrigido; verificação automática confirma os 87 retângulos iguais ao JSON.
8. **Escala do excesso:** a transformação centralizava o conteúdo na página em vez do conteúdo deslocado — com 12 lingadas + 4 dispositivos a parte inferior saía da página. Corrigido para escalar em torno do centro do conteúdo deslocado (`(PageHeight + deviceShift)/2`); com escala 1 (fixture) a transformação é idêntica à anterior.
9. **Valores das informações do guindaste sem deslocamento:** `Modelo`, `Caminhão`, `Placas`, `Chassi` e `Ano de Fabricação` não recebiam `deviceShift` e sobre punham a tabela de dispositivos no excesso. Corrigido.
10. **Comportamento de retângulos no excesso:** tipos explícitos (`SlingWall`, `SlingBottom`, `DeviceWall`, `DeviceBottom`, `FrameWall`) substituem as heurísticas frágeis; separadores de linhas extras inseridos a partir da borda inferior original (`249,74`/`319,37` + i×15,96), espessura fina **0,96 pt** (como as medidas) e colunas de itens apenas.
11. **`PdfRasterizer` (ferramenta):** PDFtoImage 5.x é **0-based**; o padrão `page = 1` quebrava rasterização de PDFs de 1 página (inclusive um teste da etapa 0). Corrigido para 0.
12. **Assinatura sem RPP:** "RPP-" + "-" (formatador de nulo) produzia "RPP--"; agora o trecho " - RPP-…" é omitido quando `RiggerRpp` está vazio (P-1c).
13. **Teste de letras na direção correta** (ajuste A): itera o **exemplo** buscando no **gerado**; letras geradas excedentes (não usadas) também são diferenças.

## 5. Resultado dos testes (final)

- `dotnet build RiggingPlanner.slnx -c Release` — **0 erros, 0 avisos**.
- `dotnet test RiggingPlanner.slnx -c Release --no-build`:
  - `RiggingPlanner.Domain.Tests` — **87/87** (74 anteriores + 13 novos; 0 ignorados).
  - `RiggingPlanner.Infrastructure.Tests` — **14/14** (9 anteriores + 5 novos; 0 ignorados).
- **`Letters_match_the_example_within_tolerance`: 0 diferenças** (1.565 letras do exemplo × 1.567 do gerado; tolerância 1,0 pt em X e linha de base; tamanhos iguais; fonte Carlito em 100% das letras). As 2 únicas diferenças são as exceções declaradas (rótulo do total de dispositivo e "7,9"→"7,95").
- **As 10 maiores diferenças:** não há diferenças a listar — o relatório `artifacts/pdf-review/stage-1/letter-diff-report.txt` contém apenas o cabeçalho "letter diffs: 0".
- **`Every_example_word_appears_in_the_generated_sheet`:** todas as palavras do exemplo aparecem no gerado (exceto "7,9", contida em "7,95").
- **`Excess_items_scale_down_to_a_single_A4_page`:** 12 lingadas + 4 dispositivos com descrições de 120 caracteres caem em **1 página A4** com escala ≈ 0,827, **tudo presente, nada truncado**.
- **`Optional_fields_print_dash_when_null`:** 29 opcionais nulos imprimem "-", sem exceção; PDF continua em 1 página A4.
- **Sobreposição visual:** divergência de 7,493% dos pixels — esperado: tarjas pretas de redação do exemplo (cliente e nº do plano, que o gerado imprime), serrilhado de anti-aliasing sub-pixel e traço levemente mais fino do bold Carlito vs Calibri, além da diferença intencional nº 1.

## 6. Tabela de alinhamentos derivados

| Constante | Valor (pt) | Derivação |
|---|---|---|
| `RowStep` | 15,96 | distância entre linhas consecutivas das tabelas (exemplo) |
| `SlingRowNumberBaseline` / `SlingRowDescriptionBaseline` | 198,14 / 197,78 | primeira linha da tabela de lingada |
| Base do total da lingada | 198,14 + qtd×15,96 | rótulo +0,15 / valor +0,03 (medidos: 262,13 / 262,01 com 4 linhas) |
| Base do total de dispositivo | 315,77 + deslocLingada + qtd×15,96 | rótulo +0,00 / valor −0,12 (medidos: 331,73 / 331,61 com 1 linha) |
| `TotalsLabelLeftX` | 288,05 | início do rótulo "PESO TOTAL…" (duas linhas) |
| Borda direita do rótulo | 480,44 | fim do ':' no exemplo (477,49 + avanço); usada pelo rótulo de dispositivo alinhado à direita |
| `DeviceTotalCenterX` / `SlingTotalCenterX` | 514,60 | centro do valor "250"/"46,62" do exemplo |
| `QuadrantInitialCenterX` / `QuadrantFinalCenterX` | 109,21 / 233,08 | centros medidos das palavras "LATERAL" |
| `RiggerSignatureCenterX` / baseline | 159,38 / 771,38 | centro/extensão da assinatura do exemplo |
| `DeviceSectionBaselineY` | 315,77 | primeira linha da tabela de dispositivos |
| Deslocamentos de seção | y1 ≤ 250 → 0; ≤ 320,5 → desloc. lingada; > 320,5 → desloc. total | aplicados pela borda superior **original** de cada retângulo/run |

## 7. Pendências `PENDING-CONFIRMATION`

- **P-1a (marca do guindaste, ajuste B):** a junção marca+modelo está isolada em `SummarySheetBuilder.CraneModelDisplay`. Se a UI passar a capturar a marca, revisar esse único ponto.
- **P-1b (observações, ajuste F):** proposta do documento mantida, isolada em `ComposeObservations`, marcada no código.
- **P-1c (assinatura sem RPP):** quando `RiggerRpp` é vazio imprime-se "Rigger {nome}" (sem " - RPP-"). Decisão local do agente para evitar "RPP--"; confirmar.
- **P-1d (aviso de alterações não salvas):** não existe rastreio de "sujo" no rascunho; o aviso "Há alterações não salvas; o PDF usa a versão salva." é exibido sempre que o plano está persistido (mesma filosofia do guarda de saída, que alerta em qualquer navegação). Se o responsável quiser o aviso condicional, será preciso um flag de sujo em `PlanDraftState`.
- **P-1e (descrições longas):** no teste de excesso, descrições de 120 caracteres transbordam a coluna em linha única (informação preservada, sobreposição visual). Definir com o responsável se a regra final será truncar, quebrar ou reduzir fonte.
- **P-0d (gravável — da etapa 0):** **os fixtures com dados reais (`reference-plan-2607036.pdf` e `summary-sheet.reference.json`) foram commitados no estágio 0** (`ddc7d74`) e o repositório tem remoto público (github.com/loregbrw/rigging-planner). O relatório da etapa 0 dizia que ficariam fora do git. Decidir: remover do histórico, tornar o repositório privado ou aceitar exposição. O agente não alterou histórico sem autorização.

## 8. Supervisor e responsável (achados)

- **Supervisor:** `SupervisorName` existe na entidade/banco mas **nenhuma tela o preenche** (confirmado na etapa 0 e re-verificado); a assinatura 2 imprime apenas o rótulo "Supervisor Responsável" (ajuste C). O campo da fixture foi corrigido para `null` (nada o imprime).
- **Responsável:** `RiggerName` = `DisplayName` e `RiggerRpp` sem prefixo (etapa 0); assinatura conforme ajuste E: "Rigger Giuliano Breda - RPP-23-1040" — idêntica ao exemplo (letra a letra).

## 9. Divergências documentação × código

- A especificação pedia comparação de posições contra `summary-sheet.reference.json`; o ajuste **A** do responsável substitui a fonte pelo **próprio PDF de exemplo** lido com `PdfTextReader` (o JSON tem offset sistemático de ~1 pt — P-0a' da etapa 0).
- O teste de "todas as palavras" do documento foi implementado por **letras** (ajuste A) e complementado por um teste de palavras (exemplo→gerado) sem critério de posição.
- PDFtoImage 5.x usa índice de página **0-based** (a documentação da ferramenta da etapa 0 presumia 1-based); ferramenta corrigida.
- O botão provisório exibe o aviso de alterações não salvas de forma incondicional (P-1d) — a especificação condicionava a existência de alterações não salvas, não detectáveis no fluxo atual.

## 10. Artefatos de revisão

- `artifacts/pdf-review/stage-1/summary-sheet.pdf` — folha gerada com a fixture.
- `artifacts/pdf-review/stage-1/summary-sheet.png` — rasterização 150 dpi do gerado.
- `artifacts/pdf-review/stage-1/overlay.png` — sobreposição (vermelho = exemplo, ciano = gerado, cinza = coincidente).
- `artifacts/pdf-review/stage-1/excess-summary-sheet.pdf` / `.png` — 12 lingadas + 4 dispositivos, escala reduzida.
- `artifacts/pdf-review/stage-1/diff-metrics.txt` — métricas (letras, tamanhos, divergência da sobreposição).
- `artifacts/pdf-review/stage-1/letter-diff-report.txt` — as 10 maiores diferenças (vazio: 0 diferenças).

**Revisão humana sugerida (docs, seção "Revisão humana"):** abrir `overlay.png` e `summary-sheet.pdf` ao lado do exemplo e conferir bordas, posição do logo, texto cortado, valores trocados de lugar, unidades, kgf/cm², "-" nos nulos e as duas diferenças intencionais. Observar também o traço do bold (Carlito) e as tarjas de redação do exemplo.
