# Exportação do plano em PDF — decisões e arquitetura

> Documento-base de todas as etapas. Copiar para `docs/pdf-export/` no repositório.
> Em divergência entre este arquivo e o código, **parar e reportar** (não decidir sozinho).
> Origem das decisões: conversa com o responsável pelo projeto (16/11/2026). Fontes: `project-brief.md`, `plan-data-flow.md`, `agent-guide.md`, `procedure-spec.md` e o PDF de exemplo do plano 2607036.

## 1. Etapas (ordem fixa; uma por vez, com revisão humana entre elas)

| # | Etapa | Resultado revisável |
|---|---|---|
| 0 | Base: pacotes, fontes, formatador numérico, ferramentas de comparação de PDF | Testes verdes e relatório |
| 1 | Folha-resumo (página 1) | PDF avulso + sobreposição com o exemplo |
| 2 | Desenhos: moldura, carimbo e encaixe das imagens | PDF avulso com imagens de teste |
| 3 | ART do plano (anexar e juntar) | Junção sem alteração |
| 4 | Procedimento (2 páginas) | PDF avulso vs. exemplo |
| 5 | Laudo + vínculo do laudo ao plano | Junção e seleção de laudo |
| 6 | Montagem final, preview real e exportação (`PlanExports`) | PDF completo |

## 2. Decisões confirmadas

- D1. Nada derivado é gravado no banco. Totais, carga bruta, taxas, número do item e códigos são calculados na exibição (`agent-guide.md` §4.3).
- D2. Exemplo de referência: plano 2607036. Com os mesmos dados, o sistema deve gerar a mesma folha (exceto as diferenças da seção 3).
- D3. Mais itens que o modelo (4 de lingada e 1 de dispositivo): a formatação pode degradar, mas **nenhum item pode ser perdido nem truncado**.
- D4. Desenhos: o usuário anexa **só a imagem da vista** (cotas, ângulos e textos já vêm nela). O sistema gera moldura (letras A–F, números 1–4) e carimbo, **idênticos ao modelo**. Média de 2 imagens por folha, agregadas dinamicamente. O carimbo se repete em todas as folhas; muda só "Folha X de Y".
- D5. Carimbo: Revisão ("1 de 1"), Escala ("Diversas") e Papel ("A4") são **fixos**, como no modelo. A assinatura em letra cursiva é o **nome de exibição do responsável do plano**.
- D6. ART do plano (página 1, carimbo, procedimento): vem de `RiggingPlan.ArtNumber`, o mesmo valor do anexo da ART do plano. Não é a ART do laudo.
- D7. ART e laudo entram como PDF inteiro, sem alteração e sem limite de páginas. Laudo: o PDF é anexado ao equipamento (`EquipmentReport`). Laudo sem validade na data do plano: **só avisa, não bloqueia**.
- D8. Plano não finalizado **pode** ser exportado (`was_final = false`). Marca d'água: **pendente de decisão** (etapa 6).
- D9. Fontes livres no lugar das da Microsoft: Carlito no lugar de Calibri. Century Gothic e Era ITC Demi: substitutas escolhidas na etapa 2. Logo: imagem que já está no projeto.
- D10. QuestPDF (licença Community, a ser declarada no código) atrás de interface, para poder trocar de biblioteca. Junção de PDFs existentes com biblioteca separada (PDFsharp ou equivalente, licença a verificar).
- D11. Preview e exportação usam **o mesmo PDF gerado**. O preview mostra esse PDF, não uma reconstrução em HTML.
- D13. Anexos (ART do plano e desenhos): na versão atual do `plan-data-flow.md` (16/11/2026, 2ª versão), os arquivos ficam em staging (`rigging-plan-staging/{sessão}/`) até o "Salvar", quando viram `StoredFile`/`PlanDrawing` na mesma transação do plano. Os renderizadores e o montador **só leem anexos persistidos** (`StoredFile.RelativePath` sob `FileSystem.AppDataDirectory`), **nunca o staging**. Preview e exportação (etapa 6) partem de plano salvo; se houver alterações não salvas, avisar.
- D12. Formatos: `CREA-PR`, `RPP-` e `Rigger` são texto fixo na impressão; o banco guarda só os números (`RiggerCrea` = "78755/D", `RiggerRpp` = "23-1040", conforme a regra do usuário; **verificar nos dados reais**).

## 3. Diferenças intencionais em relação ao PDF de exemplo

O teste de referência deve marcá-las como esperadas (e só elas):

1. **Total de dispositivos:** o exemplo repete "PESO TOTAL DA LINGADA / AMARRAÇÃO:". O sistema imprime **"PESO TOTAL DOS DISPOSITIVOS / BALANCIM:"** (texto proposto; confirmar na revisão da etapa 1).
2. **Capacidade de carga na "Configuração do guindaste":** o exemplo mostra "7,9" e o resumo mostra "7,95". O sistema mostra **7,95 nos dois**.
3. **Esforço nas sapatas no carimbo** (etapa 2): o exemplo mostra "3,17 t" (que é a resistência do terreno). O sistema usa **o valor da página 1, em kgf** (7914 kgf no exemplo).
4. **Cliente no carimbo** (etapa 2): o exemplo mostra "Kaefer Rip Ltda."; o sistema usa `ClientName` do plano, igual à página 1.

## 4. Regras de formatação de números (tudo em `decimal`, cultura pt-BR fixa, sem separador de milhar)

Arredondamento só de exibição, `MidpointRounding.AwayFromZero`. O valor guardado nunca é arredondado.
Valor nulo ou ausente → `-` (inclusive texto).

| Tipo | Regra | Exemplos do modelo |
|---|---|---|
| `Trim2` | 0 a 2 casas, sem zeros à direita | 4,7 · 1,95 · 2,3 · 3450 · 4,87 · 19,48 · 1,7 · 5 · 46,62 · 250 · 3,17 · 7914 |
| `Min1Max2` | 1 a 2 casas | 45,0 · 5,1 · 5,0 · 7,95 · 60,0 · 70,0 · 7,0 |
| `Fixed1` | exatamente 1 casa | 47,1 (taxa de utilização do guindaste, calculada) |
| `Fixed2` | exatamente 2 casas | 3,75 (carga bruta em t) |
| `Int0` | 0 casas | 3747 (carga bruta em kg) |

Onde cada tipo se aplica está na especificação da etapa 1. Valores digitados com 3 casas (o banco guarda 18,3) serão arredondados na exibição: registrar como pendência para decidir se avisa na tela.

Cálculos de exibição (já existem em `CapacityState`; **reutilizar a mesma fórmula, sem duplicar divergente**):
- Linha = `Quantity × UnitWeightKg`; total da lingada e dos dispositivos = soma por `Section`.
- Carga bruta (kg) = `WeightKg` + lingada + dispositivos + `ExtraLoadKg` (nulo = 0).
- Carga bruta (t) = carga bruta (kg) ÷ 1000, **sem arredondar antes** de dividir ou calcular a taxa.
- Taxa do guindaste = carga bruta (t) ÷ `LoadCapacityT` × 100; `-` se `LoadCapacityT` nulo ou ≤ 0.
- Número do item: contínuo (lingada 1…n; dispositivos continuam n+1…), por `SortOrder`.

## 5. Arquitetura

```
Domain          PlanValueFormatter, PlanDocumentModel(s), PlanDocumentBuilder   (puro: sem biblioteca de PDF)
Application     interfaces dos renderizadores (ex.: ISummarySheetRenderer)
Infrastructure  Pdf/: renderizadores (QuestPDF), layouts em constantes (mm/pt), fontes embutidas, PdfMerger
App             Preview/Exportação (etapa 6); botão provisório de revisão na etapa 1 (removido na etapa 6)
Tests           Domain.Tests (formatador, builder) · Infrastructure.Tests (renderização, comparação com o exemplo)
```

- O **builder** monta modelos imutáveis já formatados (strings). O **renderizador** só posiciona. Assim os números são testados como texto, sem abrir PDF.
- Cada renderizador devolve **um PDF avulso** (`byte[]`). O `PlanPdfAssembler` (etapa 6) junta na ordem do brief §6.1.
- Medidas ficam em **uma classe de layout por renderizador** (pt), derivadas do exemplo. Proibido número solto no código de desenho.

## 6. Como garantimos o formato correto

1. **Referência medida:** `summary-sheet.reference.json` (e equivalentes por etapa) traz bordas, textos, fontes e tamanhos do exemplo.
2. **Teste de texto/posição:** extrair palavras do PDF gerado (PdfPig) e comparar com a referência: mesmo texto, mesma fonte-equivalente, mesmo tamanho, posição dentro de ±1,0 pt. Exceções = só as diferenças da seção 3.
3. **Sobreposição visual:** rasterizar o PDF gerado e o de exemplo e gerar uma imagem de sobreposição (referência em vermelho, gerado em ciano) em `artifacts/pdf-review/`. É o que o humano revisa.
4. **Aprovação por congelamento:** após a revisão humana, o PDF aprovado vira arquivo "golden" e passa a ser comparado em testes de regressão.
5. **Nunca** truncar valores, nunca corrigir entrada em silêncio, nunca gravar derivados.

## 7. Regras gerais para o agente de CLI

- Código, nomes, comentários e commits em **inglês**; textos impressos nos PDFs em **português (pt-BR)**.
- Sem bibliotecas de UI/componentes novas. Não alterar `Directory.Build.props`.
- Verificar a **licença** de cada pacote novo antes de adicionar e registrá-la no relatório.
- Rodar `dotnet build` e `dotnet test` antes de concluir. **Nunca** `dotnet run`.
- Uma branch por etapa (`feature/pdf-stage-N`), commits pequenos.
- Se a realidade do código divergir destes documentos: **parar, descrever a divergência e propor opções**. Não inventar campos, tabelas ou comportamento.
- Aviso de privacidade: o texto do PDF de exemplo "censurado" ainda contém os dados originais (chassi, placa, ART, cliente). Se o repositório for público, **não commitar** o PDF de exemplo nem o JSON de referência sem trocar esses valores.

## 8. Pendências em aberto

- P-OBS. Como imprimir `Observations` na caixa "Observações / Anexos" (etapa 1 traz uma proposta).
- P-SUP. Como imprimir o nome do supervisor (`SupervisorName`) e se o campo ainda existe no formulário (etapa 1 traz a verificação).
- P-WM. Marca d'água "RASCUNHO" em exportação de rascunho (etapa 6).
- P-ROUND. Aviso quando um valor digitado é arredondado na exibição.
- P-FONT. Substitutas de Century Gothic e Era ITC Demi (etapa 2).