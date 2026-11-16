# Etapa 1 — Folha-resumo (página 1 do plano) — prompt para o agente de CLI

## Contexto
Pré-requisito: etapa 0 concluída e revisada (formatador, fontes Carlito, ferramentas de comparação, relatório). Leia novamente `docs/pdf-export/00-decisoes-e-arquitetura.md` (decisões D1–D12, diferenças intencionais, regras de formatação) e o relatório da etapa 0 (respostas sobre responsável, supervisor, marca/modelo, logo).
Objetivo: gerar a **página 1** do plano (folha "Capacidade de carga"), em A4, que com os dados do plano 2607036 reproduza o PDF de exemplo, exceto pelas diferenças intencionais da seção 3 do documento de decisões.
Branch: `feature/pdf-stage-1`.

Se o relatório da etapa 0 indicar que `RiggerName` guarda o nome completo e não há nome de exibição no plano, **pare e proponha opções** (por exemplo, coluna de snapshot `RiggerDisplayName` com migration) antes de seguir. Não decida sozinho.

## Entregas
1. **Domain:** `SummarySheetModel` (imutável, todas as strings já formatadas) e `SummarySheetBuilder` (plano + itens → modelo). Reutilizar as fórmulas de `CapacityState`; se não der para reutilizar sem acoplar à UI, extrair a lógica para o Domain e fazer `CapacityState` usá-la (sem mudar comportamento; os testes existentes devem continuar passando).
2. **Application:** `ISummarySheetRenderer.Render(SummarySheetModel) : byte[]`.
3. **Infrastructure/Pdf:** `SummarySheetRenderer` (QuestPDF) e `SummarySheetLayout` (constantes em pt, derivadas do JSON de referência).
4. **Testes** (seção 6).
5. **Botão provisório de revisão** (seção 7).
6. Relatório `docs/pdf-export/etapa-1-relatorio.md` (seção 8).

## 1. Fontes de dados (`plan-data-flow.md` §7)
Todos vêm do `RiggingPlan` e de seus `PlanAccessories`; nada do cadastro. A tabela mostra o campo, o formato (doc. de decisões §4), o estilo e o valor no exemplo. As posições exatas de cada texto estão em `summary-sheet.reference.json` (palavras `words`, em pt, origem no canto superior esquerdo).

| Campo impresso | Origem | Formato | Estilo (Carlito = Calibri) | Exemplo |
|---|---|---|---|---|
| Cliente | `ClientName` | texto | 11 regular | Kaefer Rip Serviços Industriais Ltda. |
| Local | `Location` | texto | 11 regular | Curitiba |
| Data | `PlanDate` | `dd/MM/yyyy` | 11 regular | 24/07/2026 |
| Título | "PLANO DE RIGGING nº " + `PlanNumber` | texto | 16 **bold** (uma só string) | PLANO DE RIGGING nº 2607036 |
| Descrição da carga | `LoadDescription` | texto | 11 regular | Chiller |
| Comp., Largura, Altura | `LengthM`, `WidthM`, `HeightM` | `Trim2` | 11 regular | 4,7 · 1,95 · 2,3 |
| Peso (kg) | `WeightKg` | `Trim2` | 11 regular | 3450 |
| Item (nº) | contínuo por `SortOrder` (lingada, depois dispositivos) | inteiro | 11 regular | 1…5 |
| Descrição do item | `PlanAccessory.Description` | texto | lingada: **10** regular; dispositivo: 11 regular | Cinta plana 3 metros… |
| Ref./Desenho (dispositivos) | `DrawingRef` | texto | 11 regular | - |
| Qtd., Unitário | `Quantity`, `UnitWeightKg` | `Trim2` | 11 regular | 4 · 4,87 |
| Total da linha | `Quantity × UnitWeightKg` | `Trim2` | 11 regular | 19,48 |
| Total da lingada / dos dispositivos | soma por `Section` | `Trim2` | 11 regular | 46,62 · 250 |
| Modelo | `CraneBrand` + " " + `CraneModel` (sem repetir a marca se o modelo já a contém) | texto | 11 **bold** | Palfinger MD 45.007 |
| Caminhão, Placas, Chassi, Ano | `TruckModel`, `LicensePlate`, `ChassisNumber`, `ManufactureYear` | texto/inteiro | 11 **bold** | Volvo/VM 270 6X2R · IUL8A16 · 93KP0R1C7DE141594 · 2013 |
| Capacidade nominal (t) | `CraneNominalCapacity` | `Min1Max2` | 11 **bold** | 45,0 |
| Comprimento da lança (m) | `BoomLengthM` | `Min1Max2` | 11 **bold** | 5,1 |
| Raio de operação (m) | `OperatingRadiusM` | `Min1Max2` | 11 **bold** | 5,0 |
| Capacidade de carga (t) | `LoadCapacityT` | `Min1Max2` | 11 **bold** | **7,95** (o exemplo imprime 7,9: diferença intencional nº 2) |
| Carga líquida (kg) | `WeightKg` | `Trim2` | 11 **bold** | 3450 |
| Lingada / Amarração (kg) | total da lingada | `Trim2` | 11 **bold** | 46,62 |
| Dispositivos / Balancim (kg) | total dos dispositivos | `Trim2` | 11 **bold** | 250 |
| Cargas extras (kg) | `ExtraLoadKg` | `Trim2` (nulo = `-`) | 11 **bold** | - |
| Carga bruta (kg) | `WeightKg` + lingada + dispositivos + extras | `Int0` | 11 **bold** | 3747 |
| Quadrante inicial / final | `QuadrantStart`, `QuadrantEnd` | texto | 11 regular | LATERAL · LATERAL |
| Total da carga bruta (t) | carga bruta ÷ 1000 | `Fixed2` | 11 **bold** | 3,75 |
| Capacidade de carga do guindaste (t) | `LoadCapacityT` | `Min1Max2` | 11 **bold** | 7,95 |
| Taxa de utilização do guindaste (%) | calculada (decisões §4) | `Fixed1` | 11 **bold** | 47,1 |
| Taxa cinta plana / tubular / laço de cabo de aço / manilha (%) | `FlatStrap…`, `TubularStrap…`, `WireRopeSling…`, `Shackle…` `UtilizationPct` | `Min1Max2` (nulo = `-`) | 11 **bold** | 60,0 · - · - · 70,0 |
| Esforço nas sapatas (kgf) | `FootingForceKgf` | `Trim2` | 11 **bold** | 7914 |
| Resistência mínima do terreno (kgf/cm²) | `MinGroundResistanceKgfCm2` | `Trim2` | 11 **bold** | 3,17 |
| Velocidade máxima do vento (m/s) | `MaxWindSpeedMs` | `Min1Max2` | 11 **bold** (unidade em 10) | 7,0 |
| ART (caixa de observações) | `ArtNumber` | texto, `-` se vazio | 14 **bold**, centralizado na caixa (horizontal e vertical) | ART 1720264496217 |
| Assinatura 1 | "Rigger " + nome de exibição + " - RPP-" + `RiggerRpp` | texto | 11 regular, centralizado na linha | Rigger Giuliano Breda - RPP-23-1040 |
| Assinatura 2 | rótulo fixo "Supervisor Responsável" | texto | 11 regular, centralizado na linha | Supervisor Responsável |

Notas do exemplo a respeitar: "kgf/cm²" tem o "2" sobrescrito em tamanho 7,3 bold; unidades (`t`, `m`, `kg`, `%`, `kgf`, `m/s`) são **bold** e fixas; rótulos são regular 11. Nos pontos de total (lingada e dispositivos) o valor fica dentro da caixa "Pesos".

**Diferença intencional nº 1:** o rótulo do total de dispositivos é "PESO TOTAL DOS DISPOSITIVOS / BALANCIM:" (alinhado à direita, terminando no mesmo ponto do rótulo original). O da lingada continua "PESO TOTAL DA LINGADA / AMARRAÇÃO:".

**Observações (`Observations`) e supervisor — pendências P-OBS e P-SUP.** Implementar as propostas abaixo, **isoladas em um único ponto do código e marcadas com `// PENDING-CONFIRMATION`**:
- P-OBS (proposta): a linha "ART …" fica centralizada como no exemplo. Se `Observations` não estiver vazio, imprimir o texto (11 regular, quebra de linha, alinhado à esquerda, 4 pt de margem interna) logo abaixo do rótulo "Observações / Anexos:", limitado à metade superior da caixa. Se o texto não couber, reduzir a fonte até 7 pt; **nunca truncar**.
- P-SUP (proposta): a assinatura 2 imprime só o rótulo "Supervisor Responsável". Pela pré-checagem da etapa 0, se `SupervisorName` existir e estiver preenchido, imprimir o nome em 11 regular, centralizado, **imediatamente acima da linha** de assinatura 2. Descrever no relatório o que foi encontrado no código.

## 2. Geometria (resumo; o JSON é a fonte exata)
Unidade pt, origem no canto superior esquerdo, página 595,32 × 841,92 (usar A4 do QuestPDF; diferença de 0,04 pt é irrelevante). Bordas são **retângulos preenchidos de preto**: espessura 1,92 (grossa) ou 0,96 (fina). Reproduzir com retângulos, não com traços (`lw`).

| Bloco | Faixa vertical (aprox.) | Observação |
|---|---|---|
| Moldura externa | 24,96 → 781,9; x 39,12 → 552,22 | borda grossa |
| Cabeçalho | 30,72 → 96,5; x 44,76 → 546,58 | célula do logo até x 227,57; "CLIENTE:" (x 293,09–544,66, y 32,64–47,16); "LOCAL:" e "DATA:" (y 47,16–62,64, divisórias em 419,11 e 461,14); título (y 64,56–94,58). **Logo:** caixa 86,4/34,1 → 190,6/90,0, proporção preservada |
| DADOS DA CARGA | título y≈101; tabela 116,3 → 150,14 | 5 colunas (divisórias em 301,97 · 356,11 · 419,11 · 482,14); linha de cabeçalho até 132,74 |
| LINGADA / AMARRAS | título y≈155; tabela 169,94 → 251,66 | caixa "Pesos" (x 418,63 → 546,58) sobre as colunas Unitário e Total; colunas em 87,26 · 356,11 · 482,14; **altura de linha 15,96**; 4 linhas no modelo; caixa do total em 251,67 → 267,65 |
| DISPOSITIVOS / BALANCIM | título y≈272; tabela 287,45 → 321,29 | colunas em 87,26 · 272,09 · 356,11 · 482,14; 1 linha no modelo; caixa do total em 321,29 → 337,25 |
| INFORMAÇÕES DO GUINDASTE | faixa 346,85 → 417,77 | duas barras grossas (346,85 e 415,85) e linhas finas em 381,53, 396,05 e 410,57 |
| CONFIGURAÇÃO / COMPOSIÇÃO | 426 → 518,83 | duas colunas (x 45,72–293,69 e 308,93–545,74), 5 linhas de 15,96; subtítulo "QUADRANTE OPERACIONAL" com divisória vertical em 171,26 |
| Caixa "Observações / Anexos:" | 531,91 → 744,22; x 307,97 → 546,58 | título bold 10 no canto superior esquerdo |
| RESUMO DOS CÁLCULOS | título y≈566; linhas até 737,98 | 10 linhas (x 45,72–293,69), valor central em torno de x≈240, unidade em x≈253 |
| Assinaturas | linhas em y≈758,3–759,1 | x 49,48–279,01 e 308,69–537,62; texto centralizado abaixo |

O alinhamento de cada valor variável **deve ser derivado das medidas**: compare o texto de exemplo com a célula que o contém no JSON. Se o centro do texto coincide (±1 pt) com o centro da célula, é centralizado; se `x0` está a poucos pt da borda esquerda, é à esquerda; se `x1` está perto da borda direita, é à direita. Pelo que se vê no exemplo: valores numéricos das duas tabelas de itens e a descrição do balancim são centralizados; a descrição da lingada é à esquerda. **Registre a tabela de alinhamentos derivada no relatório** para revisão humana.

## 3. Política de excesso de itens (decisão D3)
- Altura de linha dos itens: 15,96 pt. Cada item além de 4 (lingada) ou de 1 (dispositivos) acrescenta uma linha e **empurra para baixo** todos os blocos seguintes em 15,96 pt (Δ).
- Construir a página em uma área virtual de 595,32 × (841,92 + Δ) e **reduzir uniformemente** para caber no A4 (escala = 841,92 ÷ (841,92 + Δ); centralizar na horizontal). Com Δ = 0 a escala é exatamente 1.
- Descrição que não cabe na largura da célula: reduzir a fonte (mínimo 6 pt). Nunca truncar nem quebrar em duas linhas.
- Teste obrigatório: 12 itens de lingada + 4 de dispositivos, com uma descrição de 120 caracteres. Todos os textos devem aparecer na extração do PDF, em ordem, e o PDF deve ter **uma página** A4.

## 4. Fonte
Usar Carlito (registrada na etapa 0) com os mesmos tamanhos do exemplo. Carlito é métrica-compatível com Calibri; pequenas diferenças de forma de glifo são esperadas.

## 5. Dados de teste (fixture do plano 2607036)
Criar um construtor de teste com **exatamente** estes dados:
`ClientName` "Kaefer Rip Serviços Industriais Ltda." · `Location` "Curitiba" · `PlanDate` 24/07/2026 · `PlanNumber` "2607036" · `LoadDescription` "Chiller" · `LengthM` 4,7 · `WidthM` 1,95 · `HeightM` 2,3 · `WeightKg` 3450 · `ExtraLoadKg` nulo.
Lingada: (1) "Cinta plana 3 metros de comprimento - Capacidade 6t", 4, 4,87 · (2) "Manilha Curva 7/8"com pino roscado - GRAU 6 - FS 6:1", 6, 1,7 · (3) "Cinta plana 4 metros de comprimento - Capacidade 6t", 2, 5,97 · (4) "Manilha Curva 1"com pino roscado - GRAU 6 - FS 6:1", 2, 2,5. Dispositivo: "Balancim", `DrawingRef` nulo, 1, 250.
Guindaste: `CraneBrand` "Palfinger" · `CraneModel` "MD 45.007" · `TruckModel` "Volvo/VM 270 6X2R" · `LicensePlate` "IUL8A16" · `ChassisNumber` "93KP0R1C7DE141594" · `ManufactureYear` 2013 · `CraneNominalCapacity` 45 · `BoomLengthM` 5,1 · `OperatingRadiusM` 5 · `LoadCapacityT` 7,95.
`QuadrantStart` e `QuadrantEnd` "LATERAL" · `FlatStrapUtilizationPct` 60 · `TubularStrapUtilizationPct` nulo · `WireRopeSlingUtilizationPct` nulo · `ShackleUtilizationPct` 70 · `FootingForceKgf` 7914 · `MinGroundResistanceKgfCm2` 3,17 · `MaxWindSpeedMs` 7 · `ArtNumber` "1720264496217" · responsável: nome de exibição "Giuliano Breda", `RiggerRpp` "23-1040".
(Se o repositório for público, conforme a seção 7 do documento de decisões, avise o usuário antes de commitar estes valores reais.)

## 6. Testes
**Domain.Tests (builder):** com a fixture, afirmar cada string do modelo (tabela da seção 1), incluindo: 46,62 · 250 · 3747 · 3,75 · 47,1 · `-` para nulos · numeração 1–5 · "Palfinger MD 45.007" (e sem duplicar a marca quando `CraneModel` já é "Palfinger MD 45.007") · "7,95" nos dois lugares.
**Infrastructure.Tests (renderizador):**
1. Gerar o PDF da fixture. Afirmar: 1 página, tamanho A4, fonte Carlito embutida, nenhuma fonte inesperada.
2. **Comparação com o exemplo:** para cada palavra de `summary-sheet.reference.json`, achar a palavra correspondente no PDF gerado (mesmo texto, mesma região) e afirmar `|Δx0| ≤ 1,0`, `|Δtop| ≤ 1,0` e tamanho igual. **Lista de exceções explícita** (somente estas): o rótulo do total de dispositivos (texto diferente) e o valor "7,9" → "7,95" (texto diferente; posição centralizada pode variar). Qualquer outra diferença faz o teste falhar e o relatório do teste deve listar as 10 piores.
3. Todos os textos da fixture devem aparecer no PDF gerado (nenhum perdido).
4. Teste de excesso (seção 3).
5. Teste de valores ausentes: plano com todos os opcionais nulos imprime `-` onde o modelo imprime valor, sem exceção.
6. Salvar em `artifacts/pdf-review/stage-1/`: `summary-sheet.pdf`, `summary-sheet.png`, `overlay.png`, `diff-metrics.txt` e o relatório das 10 piores diferenças.

## 7. Botão provisório de revisão (App)
Em `RpPreviewStage` (hoje placeholder), adicionar um botão **"Gerar folha-resumo (PDF)"**, claramente marcado como provisório no código (`// TEMPORARY: removed in stage 6`):
- Lê o plano **direto do banco** (query nova com `AsNoTracking` e acessórios ordenados por `SortOrder`), **sem usar o rascunho em memória**. Exibe, junto ao botão, o texto: "O PDF usa os dados salvos do plano."
- Desabilitado, com explicação em pt-BR, se o plano não estiver salvo. Na versão atual de `plan-data-flow.md`, `PlanPlaceholder` repassa só `IsReadOnly`; use `PlanDraftState.IsPersisted` (injetado) e confirme no código que ele existe e é confiável. Se houver alterações em memória ainda não salvas, mostrar o aviso "Há alterações não salvas; o PDF usa a versão salva."
- Grava em `%LOCALAPPDATA%\RiggingPlanner\pdf-review\folha-resumo-{PlanNumber}.pdf` e abre o arquivo com o aplicativo padrão (`Launcher`).
- Textos de interface em pt-BR; não alterar o restante do wizard.

## 8. Relatório (`docs/pdf-export/etapa-1-relatorio.md`)
Incluir: resultado dos testes; tabela de alinhamentos derivada; as 10 maiores diferenças contra o exemplo; todos os pontos `PENDING-CONFIRMATION`; o que foi encontrado sobre supervisor e responsável; qualquer divergência encontrada entre a documentação e o código; caminho dos arquivos de revisão.

## Critérios de aceite
- `dotnet build` e `dotnet test` passam, sem testes ignorados.
- Com a fixture, o PDF gerado difere do exemplo **apenas** nas duas exceções declaradas.
- Todos os itens do teste de excesso aparecem; nenhum texto é truncado.
- Nenhuma coluna ou tabela nova no banco (exceto o que o usuário autorizar após a pré-checagem do responsável).
- O app compila e o botão provisório gera e abre o PDF de um plano salvo.

## Revisão humana (depois do agente)
Abrir `overlay.png` (vermelho = exemplo, ciano = gerado) e `summary-sheet.pdf` ao lado do PDF de exemplo. Conferir: bordas, posição do logo, texto cortado, valores trocados de lugar, unidades, kgf/cm², "-" nos nulos e as duas diferenças intencionais.