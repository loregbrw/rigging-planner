# Etapa 0 — Base da exportação em PDF (prompt para o agente de CLI)

## Contexto
Você trabalha no repositório Rigging Planner (.NET 10, MAUI Blazor Hybrid, SQLite/EF Core). Leia, nesta ordem: `docs/agent-guide.md`, `docs/plan-data-flow.md`, `docs/pdf-export/00-decisoes-e-arquitetura.md` (regras gerais e decisões) e `docs/project-brief.md` §6.1–6.3.
Esta etapa **não gera nenhuma página do plano**. Ela prepara a base para as etapas seguintes e valida que o ambiente consegue gerar, ler e comparar PDFs.

## Regras
Seguir a seção 7 de `00-decisoes-e-arquitetura.md`. Branch: `feature/pdf-stage-0`.

## Passo 1 — Pré-checagens (somente leitura; faça antes de qualquer alteração)
Responda cada item com evidência (arquivo e trecho):
1. **Responsável no plano:** em `PlanEntryPersistence.ApplyReferences` e na entidade `RiggingPlan`, o campo `RiggerName` recebe `Rigger.FullName` ou `Rigger.DisplayName`? Existe coluna para o outro? O formato guardado de `RiggerRpp` e `RiggerCrea` (com ou sem prefixo "RPP-"/"CREA-PR"; exemplo esperado: "23-1040" e "78755/D")?
2. **Supervisor:** `SupervisorName` existe na entidade? Existe campo na tela da etapa 1 ou em outra? Algum fluxo o preenche?
3. **Equipamento:** `CraneBrand` e `CraneModel` são gravados separados no cadastro e no plano? (O brief §16 item 8 pede essa verificação.) Mostre como um equipamento é gravado hoje (exemplo: "Palfinger" + "MD 45.007" ou junto).
4. **Local:** `Location` está no formulário da etapa 1 e é obrigatório?
5. **Logo:** onde está o arquivo do logo no projeto (caminho, formato, dimensões em pixels, fundo transparente?).
6. **Projetos:** `TargetFramework` de cada projeto, referências entre eles, se `Application` pode referenciar `Domain` (e ser referenciada por `Infrastructure`), e se existe projeto de testes de Infrastructure.
7. **CI:** o workflow do GitHub Actions roda em Windows ou Linux? Pacotes nativos (Skia, PDFium) funcionam nesse runner?
8. **Fixtures:** confirme que existem `tests/RiggingPlanner.Infrastructure.Tests/Fixtures/reference-plan-2607036.pdf` e `.../summary-sheet.reference.json`. Se não existirem, **pare** e peça ao usuário para copiá-los.

9. **Anexos em staging:** confirme em `AttachmentDraftState`, `AttachmentPersistence` e `PlanWorkflowLoader` que, depois do "Salvar", `ArtFile` e `Drawings[].File` (com `RelativePath`) chegam carregados pelo loader, e que **nenhum** caminho de staging fica gravado em `StoredFile`. Informe como ler um arquivo persistido (`Path.Combine(FileSystem.AppDataDirectory, RelativePath)`).

Se algum item contradisser os documentos, **pare aqui**, escreva a divergência em `docs/pdf-export/etapa-0-relatorio.md` e aguarde instrução. Caso contrário, continue.

## Passo 2 — Pacotes
Adicionar (verificando versão compatível com .NET 10 e **licença**, registrando no relatório):
- `QuestPDF` em `RiggingPlanner.Infrastructure`. Declarar a licença Community no ponto de inicialização, com comentário citando a condição (receita anual abaixo de US$ 1 milhão, confirmada pelo usuário em 16/11/2026).
- `PdfPig` (UglyToad.PdfPig) **somente nos projetos de teste**.
- Um rasterizador de PDF para PNG (por exemplo `PDFtoImage`, baseado em PDFium) **somente nos projetos de teste**, mais `SkiaSharp` se necessário para compor a imagem de sobreposição.
- Não adicionar a biblioteca de junção ainda (etapa 3).
Se algum pacote não restaurar ou não rodar em `net10.0`, parar e reportar alternativas.

## Passo 3 — Fontes embutidas
- Obter **Carlito Regular e Bold** da distribuição oficial (licença SIL OFL) e colocar em `src/RiggingPlanner.Infrastructure/Pdf/Fonts/` como **EmbeddedResource**, junto do arquivo de licença OFL.
- Criar `PdfFontRegistry` que registra as fontes uma única vez (`FontManager.RegisterFont`) e expõe o nome da família.
- Teste: gerar uma página A4 com texto "Íçamento — AÇÃO 7/8" em Carlito regular e bold; extrair as fontes com PdfPig e afirmar que estão **embutidas** e que o texto extraído é idêntico (acentos preservados).

## Passo 4 — Formatador numérico (Domain)
Criar `PlanValueFormatter` em `RiggingPlanner.Domain` (sem dependências de PDF), seguindo exatamente a seção 4 do documento de decisões:
- Tipos `Trim2`, `Min1Max2`, `Fixed1`, `Fixed2`, `Int0`; aceita `decimal?`; `null` → `"-"`.
- Cultura pt-BR fixa (`new CultureInfo("pt-BR")`), **nunca** a cultura atual; sem separador de milhar; `MidpointRounding.AwayFromZero`; sem passar por `double`.
- Métodos auxiliares: data `dd/MM/yyyy`; texto vazio ou só espaços → `"-"`.
- Testes xUnit (`Domain.Tests`) com tabela de casos: todos os exemplos do modelo (4,7 · 1,95 · 3450 · 19,48 · 46,62 · 250 · 45,0 · 5,1 · 7,95 · 60,0 · 47,1 · 3,75 · 3747 · 7914 · 3,17), mais bordas: 0, 0,005 → `0,01` (Trim2), 2,345 → `2,35`, 10,0 → `10` (Trim2) e `10,0` (Min1Max2), valores negativos, `null`. Todos devem passar com a cultura do sistema trocada para `en-US` e `de-DE`.
- Verificação do exemplo: 3746,62 kg → `3747` (Int0) e 3,74662 t → `3,75` (Fixed2); 3,74662 ÷ 7,95 × 100 → `47,1` (Fixed1).

## Passo 5 — Ferramentas de comparação de PDF (projeto de testes de Infrastructure)
Criar em `Infrastructure.Tests/Pdf/Tools/`:
1. `PdfTextReader`: lê um PDF (bytes ou caminho) com PdfPig e devolve, por página, as palavras com texto, `x0`, `x1`, `top`, `bottom` (pt, origem no canto superior esquerdo), nome da fonte e tamanho. As coordenadas devem ser **comparáveis** com as do JSON de referência (mesma origem e unidade).
2. `ReferenceSheet`: carrega `summary-sheet.reference.json`.
3. `PdfRasterizer`: renderiza uma página para PNG a 150 dpi.
4. `PdfOverlay`: recebe dois PNGs (referência e gerado) e escreve uma imagem de sobreposição: referência em vermelho, gerado em ciano, coincidências em preto/cinza. Escreve também `diff-metrics.txt` com percentual de pixels divergentes.
5. `ReviewOutput`: define a pasta `artifacts/pdf-review/stage-N/` (na raiz do repositório, **ignorada pelo git**) e métodos para gravar arquivos nela.
Teste de sanidade: ler a página 1 do PDF de exemplo com `PdfTextReader` e comparar com o JSON de referência (mesmas palavras, posições com diferença < 0,5 pt). Isso prova que a leitura é fiel. Gerar a sobreposição do PDF de exemplo contra ele mesmo: divergência deve ser 0%.

## Passo 6 — Contratos (sem implementação)
Em `RiggingPlanner.Application`, criar apenas a interface vazia de nomenclatura acordada: `ISummarySheetRenderer` com `byte[] Render(SummarySheetModel model)` — o tipo `SummarySheetModel` será criado na etapa 1, então deixe a interface **comentada em TODO** se isso impedir a compilação. Não criar mais nada.

## Critérios de aceite
- `dotnet build RiggingPlanner.slnx -c Release` e `dotnet test RiggingPlanner.slnx` passam, **sem** testes ignorados.
- O relatório `docs/pdf-export/etapa-0-relatorio.md` contém: respostas das pré-checagens, pacotes e versões com licença, resultado dos testes, caminho de saída da sobreposição.
- Nenhuma alteração no banco, nas telas ou no comportamento existente do app.

## O que NÃO fazer
Não criar migrations, telas, nem páginas do plano. Não executar `dotnet run`. Não commitar `artifacts/` nem os PDFs de teste (confira o `.gitignore`).