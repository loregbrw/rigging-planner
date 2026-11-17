# Etapa 2A — Relatório

> Branch: `feature/pdf-stage-2a` · Data: 17/11/2026 · Status: **2A concluída — fontes escolhidas pelo responsável (Questrial e Open Sans SemiBold), moldura e carimbo implementadas; build Release 0 erros/0 avisos; 118/118 testes passando, 0 ignorados.**
>
> Seções 1–10 abaixo registram o **Passo 1 (medição de fontes)**. As seções 11–12 registram a escolha do responsável, a chegada da implementação (via cherry-pick de um commit que havia sido feito fora desta branch) e a validação final.

## 1. Escopo deste passo

Conforme `docs/pdf-export/etapa-2-desenhos.md` (seções 1–4 e 6) e `docs/pdf-export/00-decisoes-e-arquitetura.md` (seção 7), esta sub-etapa cobre apenas **moldura + carimbo** da folha de desenho (sem imagens, que ficam na 2B). Antes de qualquer código de desenho, o Passo 1 mede **fontes substitutas** para as duas fontes do exemplo:

- **Gótica** (`CenturyGothic`, usada nos rótulos e textos técnicos do desenho);
- **Marca** (`ErasITC-Demi`, usada no logotipo textual "Guindastes Ribas Ltda." e na assinatura "Giuliano Breda").

O objetivo é escolher substitutas **livres** (OFL) cujas larguras se aproximem do exemplo, para que o carimbo mantenha o mesmo encaixe nas células.

## 2. Preparação do diretório de trabalho

- Branch `feature/pdf-stage-2a` criada a partir de `feature/pdf-stage-1` (HEAD `7598e2f "feat: stage 1"`).
- `drawing-sheet.reference.json` (22.880 bytes) movido para `tests/RiggingPlanner.Infrastructure.Tests/Fixtures/`, como pedido, e **não commitado**.
- Regra adicionada ao `.gitignore` (após o bloco `**/artifacts/`):

  ```gitignore
  tests/RiggingPlanner.Infrastructure.Tests/Fixtures/*
  !tests/RiggingPlanner.Infrastructure.Tests/Fixtures/*.cs
  ```

  Confirmado com `git check-ignore -v` que o JSON é ignorado e que os `.cs` de fixture (ex.: `TestPlans.cs`) continuam versionáveis. Fixtures já rastreadas não são destrackeadas pela regra.
- Artefatos de revisão em `artifacts/pdf-review/stage-2a/` (ignorado): `fonts/`, `font-measures.txt`, `font-candidates.png`.

## 3. Fontes candidatas

Todas com licença **SIL Open Font License 1.1**, baixadas do Google Fonts (instâncias estáticas TTF obtidas via API CSS v1 com user-agent de Safari 4; famílias variáveis — Montserrat, Source Sans 3, Nunito Sans, Open Sans — foram convertidas em instância estática do peso desejado). Os arquivos ficam **apenas** em `artifacts/` (ignorado); nenhum TTF foi commitado neste passo.

| Papel | Fonte | Arquivo | Peso | Origem |
|---|---|---|---|---|
| Gótica | Montserrat | `Montserrat-Regular.ttf` | 400 | https://fonts.google.com/specimen/Montserrat |
| Gótica | Questrial | `Questrial-Regular.ttf` | 400 | https://fonts.google.com/specimen/Questrial |
| Gótica | Didact Gothic | `DidactGothic-Regular.ttf` | 400 | https://fonts.google.com/specimen/Didact+Gothic |
| Gótica | Poppins | `Poppins-Regular.ttf` | 400 | https://fonts.google.com/specimen/Poppins |
| Marca | Source Sans 3 | `SourceSans3-SemiBold.ttf` | 600 | https://fonts.google.com/specimen/Source+Sans+3 |
| Marca | Nunito Sans | `NunitoSans-SemiBold.ttf` | 600 | https://fonts.google.com/specimen/Nunito+Sans |
| Marca | Open Sans | `OpenSans-SemiBold.ttf` | 600 | https://fonts.google.com/specimen/Open+Sans |

> Se uma fonte for escolhida no Passo 2, o TTF correspondente e o arquivo `OFL.txt` deverão ser embutidos como `EmbeddedResource` (mesmo padrão do logo da etapa 1), pois `artifacts/` não é versionado.

## 4. Método de medição

Ferramenta: `tests/RiggingPlanner.Infrastructure.Tests/Pdf/FontCandidateTests.cs` (teste de revisão manual).

- **Largura de referência:** `x1 − x0` de cada palavra no `drawing-sheet.reference.json` (caixas do próprio exemplo).
- **Largura medida:** caixa de tinta (`SKFont.MeasureText(text, out SKRect bounds, paint).bounds.Width`) no **mesmo tamanho em pt** do exemplo, com **kerning desativado** — o exemplo foi gerado sem kerning/ligaduras (achado da etapa 1), então a comparação coerente é por avanços puros.
- **Textos medidos:** 51 textos do desenho nas fontes góticas (rótulos, unidades, datas, números, letras de zona A–F) + 2 textos de marca nas fontes de marca ("Guindastes Ribas Ltda.", "Giuliano Breda").
- **Encaixe na célula:** para textos centralizados exige-se `(célula − largura) / 2 ≥ 1 pt` de cada lado; para alinhados à esquerda, `origem = x0 do exemplo` e sobra à direita ≥ 1 pt. Textos que estouram são listados com proposta de redução de 0,5 pt e remedição.
- **Caveat:** as caixas do JSON são caixas de **em** (`bottom − top = tamanho`), não caixas de tinta; portanto "erro relativo" compara tinta (medida) com caixa do exemplo. É a melhor aproximação possível sem rasterizar o exemplo, e é a mesma métrica aplicada a todas as candidatas.
- **CI:** sem os TTFs em `artifacts/`, o teste retorna cedo com mensagem no output (1 passando, 0 ignorados — xUnit 2.9.3 não tem `Assert.Skip`). A medição é ferramenta de revisão, não teste de regressão.

## 5. Resultados

### 5.1 Resumo geral (erro relativo médio |Δ|)

| Fonte | Erro médio | Erro máx. | Textos que estouram |
|---|---|---|---|
| **Questrial** | **5,29 %** | 19,43 % | **0** |
| Poppins | 5,72 % | 25,47 % | **0** |
| Montserrat | 8,03 % | 20,19 % | **3** |
| Didact Gothic | 10,92 % | 28,38 % | 0 |
| **Open Sans SemiBold** | **1,34 %** | 2,43 % | 0 |
| Nunito Sans SemiBold | 3,39 % | 4,81 % | 0 |
| Source Sans 3 SemiBold | 9,29 % | 10,06 % | 0 |

### 5.2 Textos mínimos exigidos pela especificação (seção 3)

| Fonte | Média | Observações |
|---|---|---|
| Poppins | **3,15 %** | Melhor nas 6 strings exigidas; "Chiller" +9,15 % (mais larga) |
| Questrial | 3,67 % | Sistematicamente mais estreita nas strings longas/grandes: "Içamento Chiller" −7,38 %, "Rua Justo Manfron, 2845 G" −6,98 %, "Kaefer Rip Ltda." −5,59 % |
| Montserrat | 7,53 % | "COMPRIMENTO" +10,64 %, "UTILIZAÇÃO" +11,80 % |
| Didact Gothic | 8,52 % | Muito estreita em strings longas (−11,5 %) |
| Open Sans SemiBold (marca) | **1,34 %** | "Guindastes Ribas Ltda." +2,43 %, "Giuliano Breda" +0,24 % |
| Nunito Sans SemiBold (marca) | 3,39 % | −1,98 % / −4,81 % |
| Source Sans 3 SemiBold (marca) | 9,29 % | −8,51 % / −10,06 % |

### 5.3 Estouros (somente Montserrat)

| Texto | Margens | Proposta |
|---|---|---|
| COMPRIMENTO (8 pt) | −0,63 / −0,63 pt | 7,50 pt → 61,43 pt, cabe |
| ESFORÇO NAS SAPATAS (6 pt) | 2,84 / −2,56 pt | 5,50 pt → 68,65 pt, cabe |
| RIGGER - RPP-23-1040 (7 pt) | 0,52 / 0,52 pt | 6,50 pt → 73,95 pt, cabe |

As demais candidatas têm **zero** estouros, ou seja, atendem à restrição rígida de encaixe sem alterar tamanhos do exemplo.

## 6. Prévia visual (`font-candidates.png`)

- Recorte do exemplo em 150 dpi (página 2, índice 1) com as tarjas de redação originais, e abaixo uma linha por candidata desenhada **na mesma escala e posições** do exemplo.
- As palavras foram agrupadas em *runs* (mesma linha/fonte/tamanho, lacuna ≤ 0,30 × tamanho) para não exagerar espaços; textos centralizados usam o centro da tinta do run do exemplo; alinhamento vertical pelo centro da caixa de tinta.
- O carimbo completo (rótulos, valores, unidades, letras de zona) fica legível; as tarjas aparecem apenas na linha do exemplo (privacidade do original).

## 7. Recomendação

- **Gótica: Questrial** — menor erro médio geral (5,29 %), zero estouros e gênero geométrico próximo do Century Gothic.
  *Alternativa:* **Poppins** se a prioridade for fidelidade nas 6 strings exigidas (3,15 % vs 3,67 %), com a ressalva de ficar mais larga em "Chiller" (+9,15 %). Ambas cabem sem reduzir tamanhos.
- **Marca: Open Sans SemiBold** — 1,34 % médio, muito próximo das larguras do Eras ITC Demi; **Nunito Sans SemiBold** como alternativa.
- **Não recomendadas:** Montserrat (3 estouros exigiriam reduzir tamanhos do exemplo) e Didact Gothic / Source Sans 3 (estreitas demais).

## 8. Artefatos de revisão

- `artifacts/pdf-review/stage-2a/font-measures.txt` — tabela completa por fonte/texto (tamanho, largura do exemplo, largura medida, erro, célula, cabe) + resumo + lista de strings exigidas.
- `artifacts/pdf-review/stage-2a/font-candidates.png` — prévia visual comparativa.
- `artifacts/pdf-review/stage-2a/fonts/` — TTFs baixados (todos OFL 1.1).

## 9. Divergências e pontos de atenção

1. **Fixtures versionadas (P-0d, herdado da etapa 0):** o documento da etapa 2 afirma que a pasta `Fixtures/` é ignorada "como combinado para as demais fixtures", mas `reference-plan-2607036.pdf` e `summary-sheet.reference.json` estão **commitados** desde `ddc7d74`, com dados reais de cliente, num repositório de remote público. O novo `drawing-sheet.reference.json` está ignorado conforme instruído; a decisão sobre os fixtures antigos (reescrever histórico, tornar o repositório privado ou aceitar) continua pendente com o responsável. Nenhuma alteração de histórico foi feita.
2. **Caixas do JSON são de em, não de tinta** (ver seção 4) — o erro medido é uma aproximação conservadora.
3. **Medição manual, não regressão:** a ferramenta é no-op na ausência dos TTFs (CI), por isso não há teste "ignorado".

## 10. Próximo passo (bloqueado até a escolha)

Aguardando a decisão do responsável sobre:

1. **Fonte gótica** — recomendação Questrial (alternativa Poppins);
2. **Fonte de marca** — recomendação Open Sans SemiBold (alternativa Nunito Sans SemiBold).

Com a escolha, o Passo 2 embute os TTFs + `OFL.txt` como `EmbeddedResource` e implementa a moldura e o carimbo da 2A (sem imagens, que ficam na 2B).

---

## 11. Escolha das fontes e reorganização das branches (17/11/2026)

### 11.1 Escolha do responsável

Confirmado nesta sessão, com base nas medições das seções 5–7:

- **Gótica: Questrial** (recomendada; alternativa Poppins não escolhida);
- **Marca: Open Sans SemiBold** (recomendada; alternativa Nunito Sans SemiBold não escolhida).

### 11.2 Divergência encontrada e reorganização das branches

Investigação nesta sessão encontrou a **implementação completa da 2A** (moldura, carimbo, `DrawingSheetLayout`, `DrawingSheetStampBuilder`, `DrawingSheetsRenderer.RenderFrameOnly`, fontes Questrial + Open Sans já embutidas, testes) commitada como `3a11cac "feat: stage 2a"` **na branch `feature/pdf-stage-2b`** — commit cujo pai é o tip da stage-1 (`7598e2f`), ou seja, criado fora da branch da 2A e sem conter o Passo 1 (`71bcf87`). As fontes embutidas eram exatamente as que o responsável escolheu nesta sessão, indicando que uma sessão anterior já as havia aplicado.

Decisão do responsável (17/11/2026): **cherry-pick para a branch correta**. Executado:

1. Trabalho 2B em andamento no diretório (não commitado) preservado em stash;
2. `3a11cac` cherry-pickado para `feature/pdf-stage-2a` (`41b825e`);
3. Branch `feature/pdf-stage-2b` reposicionada sobre a nova 2A;
4. Nenhuma branch remota afetada (2a e 2b não estavam no remote).

> Nota: `AGENTS.md` e `docs/agent-guide.md` ficaram trackados nesta branch (foram commitados em `71bcf87`); nas demais branches são arquivos não trackados. Sem alteração de histórico.

### 11.3 Correções de build (avisos)

A implementação chegada na 2A tinha **5 avisos** de build; corrigidos (commit `4616246 "fix: stage 2a build warnings"`):

- 4× **CS8625** nos testes "nulos imprimem -" (`DrawingSheetStampBuilderTests.cs:40,49` e `DrawingSheetPdfTests.cs:355,364`): `OperationName` e `LoadDescription` são `string` **não-nulável** na entidade (campos obrigatórios); o teste passou a usar `string.Empty` (o `PlanValueFormatter.Text` mapeia vazio → "-", mesmo comportamento).
- 1× **CS0618** em `PdfTextReader.cs:93`: `Letter.GlyphRectangle` (obsoleto) → `Letter.BoundingBox` (API recomendada do PdfPig). Suíte completa revalidada após a troca: 118/118 passando, sem regredir os testes de letras da etapa 1.

## 12. Validação final (2A)

- `dotnet build RiggingPlanner.slnx -c Release` — **0 erros, 0 avisos**.
- `dotnet test RiggingPlanner.slnx -c Release --no-build` — **97/97 Domain + 21/21 Infrastructure = 118/118**, 0 falhas, 0 ignorados.
- Artefatos de revisão da 2A em `artifacts/pdf-review/stage-2a/`: `drawing-sheet.pdf` / `.png`, `overlay.png`, `anchoring-deviations.txt`, `long-texts.*`, `null-optionals.pdf`, `diff-metrics.txt`, `font-candidates.png`, `font-measures.txt`.

## 13. Pendências da 2A para revisão humana

- **Revisar** `artifacts/pdf-review/stage-2a/overlay.png` e `drawing-sheet.pdf` ao lado da página 2 do exemplo (conforme seção 7 do documento da etapa 2): linhas, letras A–F e 1–4, textos dentro das células, esforço nas sapatas em kgf, cliente do plano e as fontes escolhidas.
- **P-0d (herdado):** fixtures antigos com dados reais ainda commitados (seção 9.1).
- **PENDING-CONFIRMATION no código:** `DrawingSheetStampBuilder.RiggerLabel`/`CreaLabel` — quando `RiggerRpp`/`RiggerCrea` vazio, imprime "RIGGER -" / "CREA-PR -" (prefixo fixo mantido, número como "-"); confirmar com o responsável.
