# Etapa 2 — Folhas de desenho (moldura, carimbo e imagens) — prompt para o agente de CLI

> Dividida em duas sub-etapas, cada uma com revisão humana:
> - **2A — Moldura e carimbo** (sem imagens) e escolha das fontes substitutas.
> - **2B — Encaixe das imagens** (agrupamento dinâmico, várias folhas, "Folha X de Y").
> Execute **só a sub-etapa pedida pelo usuário**. Pré-requisito: etapa 1 aprovada. Leia antes `00-decisoes-e-arquitetura.md` (D1–D13, regras gerais) e os relatórios das etapas 0 e 1.

## 0. O que se reproduz e o que não

Referência: folha 1 de 3 do plano 2607036 (página 2 do PDF de exemplo, A4 retrato, 595,276 × 841,89 pt). `drawing-sheet.reference.json` traz **só** a moldura e o carimbo (linhas cinza e textos em Century Gothic e Eras ITC Demi). Medidas em pt, origem no canto superior esquerdo.

- **Gerado pelo sistema:** moldura com letras A–F e números 1–4, carimbo completo (com os dados do plano) e o posicionamento das imagens.
- **Não gerado:** cotas, ângulos, textos "Raio de operação", "Cinta plana" etc. Vêm dentro da imagem anexada (decisão D4). Os textos em Arial Narrow e as linhas cinza finas do miolo do exemplo são **conteúdo das vistas** e ficam de fora do layout.
- **O que o usuário anexa:** capturas de tela (PNG/JPG) das vistas, de resolução e proporção livres, em geral com fundo branco.
- Privacidade: o JSON contém dados reais (cliente, ART, nº do plano). Mantê-lo **fora do git** (pasta ignorada, como combinado para os demais fixtures).

## 1. Moldura (2A)

Todas as linhas em cinza `#C0C0C0` (0,753). Retângulos de traço, não preenchidos.

| Elemento | Medida (pt) | Espessura |
|---|---|---|
| Moldura externa | x 42,5 → 581,1 · y 14,19 → 827,79 | 1,98 |
| Moldura interna | x 56,5 → 566,9 · y 28,39 → 813,59 | 1,98 |
| Marcas de zona verticais (topo e base, entre as molduras) | x = 170,0 · 311,8 · 453,5; y 14,19 → 28,39 (topo) e 813,59 → 827,79 (base) | 0,99 |
| Marcas de zona horizontais (esquerda e direita) | y = 137,49 · 279,29 · 420,99 · 562,69 · 704,49; x 42,5 → 56,6 (esq.) e 566,9 → 581,1 (dir.) | 0,99 |

**Letras e números** (Century Gothic substituta, 15 pt, preto): números `4 3 2 1` no topo e na base, da esquerda para a direita; letras `F E D C B A` nas bordas esquerda e direita, de cima para baixo. Cada rótulo é **centralizado na célula** entre marcas (horizontal e vertical): células horizontais 56,5–170 · 170–311,8 · 311,8–453,5 · 453,5–566,9; células verticais 28,39–137,49 · 137,49–279,29 · 279,29–420,99 · 420,99–562,69 · 562,69–704,49 · 704,49–813,59. As posições exatas do exemplo estão em `words` (o centro da caixa do texto do exemplo fica ~2,7 pt abaixo do centro da célula; reproduzir pela mesma regra, medindo o centro do glifo).

## 2. Carimbo (2A)

Área do carimbo: x 56,5 → 566,9, y 662,39 → 813,59. Linhas cinza `#C0C0C0` de **1,42 pt**:

- Horizontais: y 662,39 e 684,59 (x 56,5 → 566,7) · y 705,19 (56,6 → 566,9) · y 717,19 e 729,29 (211,4 → 336,7) · y 741,29 e 753,39 (56,6 → 336,7) · y 765,39 (211,4 → 566,9) · y 783,79 (211,4 → 336,7) · y 801,59 (211,4 → 566,7).
- Verticais: x 311,6 · 375,4 · 439,1 · 502,9 (y 662,39 → 704,79) · x 211,4 e 336,7 (y 705,19 → 813,59) · x 286,2 (y 705,19 → 765,39) · x 530,0 (y 765,39 → 801,59) · x 136,9 (y 741,29 → 753,39) · x 462,0 (y 801,59 → 813,59).
- Linha da assinatura (0,51 pt, cinza `#C0C0C0`): x 78,8 → 188,4, y 729,29.
- Conferir os valores **exatos** no JSON (`lines`); a lista acima é um resumo. Constantes de layout em uma classe `DrawingSheetLayout`, com teste automático que as compara com o JSON (tolerância 0,05 pt).

**Textos do carimbo** (`F` = fixo; `V` = variável; fonte "gótica" = substituta de Century Gothic; "marca" = substituta de Eras ITC Demi):

| Texto | Tipo | Fonte e tamanho | Posição (célula e alinhamento) |
|---|---|---|---|
| DESCRIÇÃO DA CARGA | F | gótica 8 | cabeçalho, coluna 1 (56,5–311,6), centralizado |
| COMPRIMENTO / LARGURA / ALTURA / PESO, com [m] [m] [m] [kg] na 2ª linha | F | gótica 8 | cabeçalho, colunas 2–5, centralizado, duas linhas |
| descrição da carga | V `LoadDescription` | gótica 8 | linha de dados (684,59–705,19), coluna 1, centralizado |
| comprimento, largura, altura, peso | V `Trim2` | gótica 8 | linha de dados, colunas 2–5, centralizado |
| CARGA BRUTA, CAPACIDADE CARGA, TAXA DE UTILIZAÇÃO, ESFORÇO NAS SAPATAS, VELOC. MÁX VENTO | F | gótica 6 | coluna 211,4–286,2, uma por linha (705,19 → 765,39), à esquerda (x≈214,2) |
| valores + unidade | V | gótica 6 | coluna 286,2–336,7, à esquerda (x≈290,5), unidade colada após 1,6 pt |
| OPERAÇÃO: | F | gótica 5,67 | célula 336,7–566,9 × 705,19–765,39, canto superior esquerdo (x≈340,6) |
| nome da operação | V `OperationName` | gótica 16 | mesma célula, centralizado |
| CLIENTE: | F | gótica 5,67 | célula 336,7–530,0 × 765,39–801,59, canto superior esquerdo |
| nome do cliente | V `ClientName` | gótica 16 | mesma célula, centralizado |
| A4 | F | gótica 11 | célula 530,0–566,9 × 765,39–801,59, centralizado |
| ART: · nº ART | F · V `ArtNumber` | gótica 5,67 · 9 | célula 211,4–336,7 × 765,39–783,79; rótulo no canto, valor à esquerda abaixo |
| Nº Rigging: · nº do plano | F · V `PlanNumber` | gótica 5,66 · 9 | célula 211,4–336,7 × 783,79–801,59 |
| Revisão: · 1 de 1 | F | gótica 5,67 · 6 | célula 211,4–336,7 × 801,59–813,59 |
| ESCALA · Diversas | F | gótica 5,67 | célula 336,7–462,0 × 801,59–813,59 |
| FOLHA {X} DE {Y} | V | gótica 5,67 | célula 462,0–566,7 × 801,59–813,59 |
| nome de exibição do responsável (assinatura) | V `RiggerName` | marca 10 | acima da linha da assinatura, centralizado em x≈133,6 |
| RIGGER - RPP-{`RiggerRpp`} | V | gótica 7 | célula 56,6–136,9 × 741,29–753,39, centralizado |
| CREA-PR {`RiggerCrea`} | V | gótica 7 | célula 136,9–211,4 × 741,29–753,39, centralizado |
| Guindastes Ribas Ltda. | F | marca 12 | bloco 56,6–211,4 × 753,39–813,59, centralizado |
| CNPJ 81.076.242/0001-60 · Rua Justo Manfron, 2845 G · Santa Felicidade - Curitiba - PR · CEP 82.410-540 | F | gótica 7 | mesmo bloco, uma linha cada, centralizado |

Regras de conteúdo:
- **Formatos de número:** iguais aos da folha-resumo (decisões §4): carga bruta `Fixed2` + " t"; capacidade `Min1Max2` + " t"; taxa do guindaste `Fixed1` + " %"; vento `Min1Max2` + " m/s"; dimensões e peso `Trim2`. Nulo → `-`.
- **Esforço nas sapatas (diferença intencional nº 3):** valor da página 1, em kgf (`Trim2` + " kgf"). O exemplo imprime "3,17 t".
- **Cliente no carimbo (diferença intencional nº 4):** `ClientName`, igual à página 1.
- Cliente e operação são longos: reduzir a fonte até o mínimo de 8 pt; se ainda não couber, quebrar em duas linhas nesse tamanho. **Nunca truncar.**
- "Revisão", "Escala" e "A4" são texto fixo, como no exemplo (D5).
- `RiggerRpp` e `RiggerCrea` guardam só os números (etapa 0).

## 3. Fontes substitutas (2A, Passo 1 — requer aprovação do usuário)

Century Gothic e Eras ITC Demi não podem ser embutidas (D9). Escolher substitutas **livres (SIL OFL)** por **medição**, não por gosto:
1. Candidatas — "gótica": Montserrat, Questrial, Didact Gothic, Poppins; "marca": Source Sans 3 (SemiBold), Nunito Sans (SemiBold), Open Sans (SemiBold). O agente pode propor outras de licença OFL/Apache, citando a licença.
2. Para cada candidata, medir a largura (pt) das cadeias do carimbo no tamanho do exemplo e compará-las com as larguras do exemplo (`x1 − x0` das palavras em `words`; use pelo menos "COMPRIMENTO", "DESCRIÇÃO", "UTILIZAÇÃO", "Içamento Chiller", "Kaefer Rip Ltda.", "Rua Justo Manfron, 2845 G", "Guindastes Ribas Ltda." e "Giuliano Breda").
3. Escolher a de menor erro relativo médio e **garantir que nenhum texto fixo estoure sua célula**. Se uma fonte ficar mais larga em algum texto fixo, registrar e propor ajuste de tamanho (reduzir 0,5 pt).
4. Entregar `artifacts/pdf-review/stage-2a/font-candidates.png`: uma imagem com o carimbo do exemplo ao lado do carimbo gerado com cada candidata, e `font-measures.txt` com os erros por texto. **Parar e aguardar a escolha do usuário** antes de seguir para o Passo 2. Depois da escolha, embutir as fontes (`EmbeddedResource` + licença) e registrá-las em `PdfFontRegistry`.

## 4. Entregas da 2A

1. Domain: `DrawingSheetStampModel` (strings já formatadas, incluindo "FOLHA X DE Y") e `DrawingSheetStampBuilder` (plano → carimbo; reutiliza `PlanTotals` e o formatador).
2. Application: `IDrawingSheetsRenderer`.
3. Infrastructure: `DrawingSheetLayout` (constantes) e `DrawingSheetsRenderer` — na 2A gera **uma folha sem imagens** (`DrawingSheetsRenderer.RenderFrameOnly`), A4, com moldura e carimbo.
4. Testes:
   - Constantes de layout × `drawing-sheet.reference.json` (linhas), tolerância 0,05 pt.
   - Todos os textos do carimbo estão no PDF gerado (extração PdfPig) com o texto esperado para a fixture do plano 2607036; cada texto, **dentro da sua célula** (caixa do texto contida na célula com margem ≥ 1 pt).
   - Ancoragem: para textos centralizados, o centro da caixa do texto difere do centro da célula em ≤ 1 pt; tamanho de fonte igual ao da tabela. **Registrar** no relatório a tabela de desvios medidos entre o exemplo e o gerado (as fontes diferentes impedem coincidência letra a letra, e o exemplo tem pequenas irregularidades de centralização de até ~5 pt na linha de dados).
   - Sobreposição visual (`PdfOverlay`) **restrita à região da moldura e do carimbo** (excluir a área das vistas, x 57 → 566, y 29 → 661): imprimir a métrica de divergência dessa região.
   - Teste de textos longos: cliente de 60 caracteres e operação de 80 caracteres cabem na célula, sem truncar.
   - Nulos: todos os opcionais nulos imprimem `-`.
5. Botão provisório (App, `// TEMPORARY: removed in stage 6`) "Gerar folha de desenho (moldura e carimbo)" em `RpPreviewStage`, ao lado do botão da etapa 1, lendo o plano salvo direto do banco.
6. `docs/pdf-export/etapa-2a-relatorio.md`: fontes escolhidas, desvios, pontos `PENDING-CONFIRMATION`, divergências entre documentação e código.

## 5. Encaixe das imagens (2B)

**Área útil das vistas:** dentro da moldura interna e acima do carimbo: x 56,5 → 566,9, y 28,39 → 662,39, com **recuo de 10 pt** em todos os lados (x 66,5 → 556,9; y 38,39 → 652,39). Espaço entre imagens: **12 pt**. (Valores propostos; ajustar após a revisão, em constantes.)

**Regras do planejador (`DrawingSheetPlanner`, no Domain, função pura e testável):**
1. Entrada: lista de imagens em ordem (`SortOrder`) com largura e altura em pixels; saída: folhas, cada uma com retângulos (x, y, largura e altura em pt) por imagem.
2. Cada imagem é ajustada **preservando a proporção**, sem cortar, sem distorcer. Todas as imagens de uma mesma folha usam o **mesmo fator pt/pixel** (escolhido para a folha caber), e ficam centralizadas dentro da sua célula.
3. Candidatos de disposição: 1 imagem (centralizada); 2 imagens lado a lado ou empilhadas; 3 imagens (empilhadas, lado a lado, ou 1 em cima e 2 embaixo). Para cada candidato, o fator pt/pixel é o maior que cabe na área útil.
4. Preferir **2 imagens por folha**. Aceitar o par se, para cada imagem, `escala_no_par ≥ 0,5 × escala_sozinha`; caso contrário, a folha leva só 1. Usar 3 numa folha apenas quando restam exatamente 3 imagens e a regra de 0,5 vale para as três (evita folha com imagem sozinha). Máximo de 3 por folha.
5. Escolher entre os candidatos de mesmo número de imagens o de **maior menor-escala**; empate: preferir a disposição empilhada.
6. Nunca perder imagem: toda imagem aparece exatamente uma vez, na ordem original.
7. Calcular a **resolução efetiva** (ppi = pixels ÷ polegadas) de cada imagem; se < 100 ppi, registrar um aviso na saída (o aviso só aparece no relatório de teste agora; a UI trata na etapa 6).
8. Sem imagens: não gerar folhas de desenho (o brief permite plano sem desenhos); o montador (etapa 6) simplesmente omite a seção. `FOLHA X DE Y`: Y = nº de folhas.

**Renderização:** embutir as imagens **sem reduzir a resolução** (configurar a qualidade de imagem do QuestPDF para a melhor opção e a resolução de raster para não reamostrar abaixo do original). Testar com PdfPig que a imagem no PDF tem as mesmas dimensões em pixels da fonte (para até 288 ppi efetivos). Imagens PNG com transparência saem sobre fundo branco.

**Testes (2B):**
1. `DrawingSheetPlanner`: casos 1, 2, 3, 4, 5 e 7 imagens; proporções largas, altas e quadradas misturadas; invariantes (todas as imagens uma vez, em ordem; retângulos dentro da área útil; distância ≥ 12 pt entre imagens; nenhuma sobreposição; Y correto).
2. PDF: número de páginas = Y; carimbo de cada folha com "FOLHA X DE Y" correto; imagens com os pixels originais.
3. Imagens sintéticas geradas no próprio teste (retângulos coloridos com régua e texto) em várias proporções, mais capturas reais quando disponíveis em `Fixtures/drawings/` (pasta ignorada pelo git; se vazia, esses testes são **pulados com mensagem explícita**, nunca falham).
4. Saída em `artifacts/pdf-review/stage-2b/`: PDF com 5 imagens sintéticas e PDF com as capturas reais, mais o PNG da rasterização de cada folha.
5. Botão provisório: "Gerar folhas de desenho (PDF)" lê o plano salvo e os desenhos **persistidos** (`StoredFile` via loader; **nunca o staging**, D13) e abre o PDF. Desabilitado, com mensagem em pt-BR, se o plano não tiver desenhos salvos.
6. `docs/pdf-export/etapa-2b-relatorio.md`: resultado dos testes, resolução efetiva por imagem, decisões locais, divergências.

## 6. Critérios de aceite

- 2A: build e testes verdes, sem testes ignorados (exceto os de capturas reais da 2B), moldura e carimbo conferidos com o JSON, fontes aprovadas pelo usuário, sobreposição da região do carimbo gerada.
- 2B: invariantes do planejador verdes, PDF com `Y` páginas, nenhuma imagem cortada ou deformada, imagens com resolução original preservada.
- Nada é gravado no banco; nenhuma coluna nova.

## 7. Revisão humana

**Como preparar as capturas (você):** salvar como PNG, com zoom que mostre as cotas legíveis, fundo branco, sem barras da janela; 2 a 4 imagens de proporções variadas (uma larga, uma alta). Anexar ao plano pela etapa 3 do app e salvar o plano. **Não** coloque as capturas no repositório.
- 2A: abrir o PDF de moldura e carimbo ao lado da página 2 do exemplo. Conferir: linhas, letras A–F e números 1–4 nas posições, textos dentro das células, esforço nas sapatas em kgf, nome do cliente do plano, e a escolha das fontes.
- 2B: abrir o PDF com as suas capturas. Conferir: imagens inteiras, legíveis, não sobrepostas, proporções corretas, "Folha X de Y", e se o agrupamento ficou bom visualmente.