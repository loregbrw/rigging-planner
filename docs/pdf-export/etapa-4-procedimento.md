# Etapa 4 — Procedimento operacional (2 páginas) — especificação

> Referência para o agente de CLI. Medidas tiradas das páginas 6 e 7 do PDF de exemplo (plano 2607036), em pt, origem no canto superior esquerdo, página 595,32 × 841,92. O texto do procedimento está em `docs/procedure-spec.md` e **já existe no app** (`OperationalProcedureService`).

## 1. O que é o procedimento e como deve ser gerado

O procedimento operacional é um documento **padrão**: as mesmas 5 seções (Isolamento da área, Patolamento do guindaste, Posicionamento do veículo de carga, Içamento da carga, Descarga da carga), com o mesmo texto para todos os guindastes. Cada seção tem **Objetivo**, **Passos** e **Referências** (normas NR-18 e NR-11). Não há cadastro nem edição pelo usuário.

**O que muda de um plano para outro são só 4 valores:**

| Valor | Onde aparece | Origem |
|---|---|---|
| Nome da operação | cabeçalho, sob "Procedimento" | `operation_name` do plano |
| Número da ART | cabeçalho, canto direito | `art_number` do plano |
| Número do plano | cabeçalho, canto direito | `plan_number` do plano |
| Código do modelo | título da seção 2 ("PATOLAMENTO DO GUINDASTE MD45007") | derivado de `crane_model`, sem marca, espaços e pontos |

Valor ausente é impresso como `-`.

**De onde vem o conteúdo (regra mais importante):** o PDF **não monta texto próprio**. Ele desenha o que `OperationalProcedureService.ResolveContent` devolver:
- **Plano em rascunho:** o serviço monta o conteúdo com o texto embutido do app e os dados atuais do plano.
- **Plano finalizado:** o serviço devolve o **snapshot** gravado na finalização (`PlanProcedureSnapshots`), com o código do modelo e o texto congelados. Assim, uma versão futura do texto do app **não altera** planos antigos.
- Exportar um rascunho **não grava** snapshot nem nada no banco.
- Não ler `procedure-spec.md` em tempo de execução e não duplicar o texto no renderizador.

**Estrutura fixa de 2 páginas:** página 1 = seções 1, 2 e 3; página 2 = seções 4 e 5. A quebra entre as páginas é fixa (não depende do espaço). Cada página repete cabeçalho e rodapé. O número no rodapé é a numeração **do procedimento** ("1" e "2"), não a do PDF completo.

## 2. Aparência (medida do exemplo)

**Fontes:** Arial → **Liberation Sans** (Regular e Bold; métrica idêntica à da Arial; licença SIL OFL; embutir com a licença, como foi feito com a Carlito). Calibri → Carlito (já embutida). Sem kerning e sem ligaduras, como na etapa 1.

**Cabeçalho (repetido em cada página):**
- Logo `guindastes-ribas-logo.png` (o mesmo da folha-resumo) dentro da caixa x 43,12 → 146,82, y 27,3 → 85,85, preservando a proporção, centralizado.
- Duas linhas verticais, cor `#18436E`, 0,75 pt, y 29,02 → 83,97, em x = 160,8 e x = 438,55.
- Centro (x 161 → 438): "Procedimento" (Liberation Sans Bold 18, centralizado em x≈299,9, topo y 36,1) e, abaixo, o nome da operação (Bold 12, centralizado, topo y 61,8). Nome que não couber em 270 pt: reduzir a fonte até 8 pt e depois quebrar em 2 linhas. Nunca truncar.
- Direita (centro x = 501,52): "ART" (Regular 11,04, topo 26,0); número da ART (Bold 11,04, topo 43,1); linha horizontal x 445,42 → 557,62, y 56,04 → 56,52, cor `#2F5496`; "RIGGING" (Regular 11,04, topo 61,2); número do plano (Bold 11,04, topo 78,4). Todos centralizados em x = 501,52.
- Linha sob o cabeçalho, 0,48 pt de espessura, y 91,94 → 92,42: x 35,4 → 445,41 na cor `#657C9C` e x 445,42 → 557,62 na cor `#2F5496`.

**Corpo** (texto em Carlito, cor preta; margem esquerda x 78,02; limite direito x 538,7):
- Título da seção: Carlito Bold 14,04, x 78,02. Ex.: "1. ISOLAMENTO DA ÁREA".
- "Objetivo:" em Bold 12 seguido do texto em Regular 12, na mesma linha.
- "Passos:" e "Referências:" em Bold 12.
- Itens (passos e referências): bolinha "•" (10 pt) em x 81,38 e texto em x 99,26, Regular 12. Linha continuada alinha em x 99,26. Texto justificado; a última linha de cada item fica à esquerda.
- Nas referências, o **código da norma é Bold** e o restante Regular: tudo antes do primeiro " – " fica Bold; o " – " e a descrição ficam Regular.
- **Ritmo vertical** (distâncias entre os topos dos textos; `T` = topo do título da seção): Objetivo = T + 27,4 · "Passos:" = T + 52,3 · 1º passo = T + 75,4 · passo seguinte = +16,97 · "Referências:" = último passo + 24,9 · 1ª referência = "Referências:" + 23,0 · referência seguinte (ou linha continuada) = +16,97.
- **Separador** entre seções: faixa cinza `#A0A0A0`, x 78,0 → 538,7, espessura 1,55 pt, no topo y = última referência + 23,85; o próximo título começa 15,7 pt abaixo da base da faixa. **Há separador após as seções 1, 2, 3 e 4; não há após a seção 5.**
- Topo do 1º título: y = 108,6 em cada página. Títulos seguintes (página 1): seção 2 em 357,8 e seção 3 em 589,9; (página 2): seção 5 em 425,6.
- Referência que quebra linha (seção 4, 1ª referência): "NR-18.14.20 / NR-18.14.25 / NR-18.14.12–18.14.18 – Capacitação, inspeção e" / "comunicação de operação."

**Rodapé (repetido):**
- Linha preta de 0,48 pt, x 34,08 → 589,92, y 804,24.
- Texto do rodapé (Carlito 9, cor `#4472C4`, x 68,42, topo 809,4): `Rua Justo Manfron, 2845 G, Santa Felicidade, Curitiba - PR - CEP 82.410-540 – CNPJ 81.076.242/0001-60`.
- Número da página do procedimento (Carlito 11,04, cor `#4472C4`, alinhado à direita em x 555,54, topo 807,8).

## 3. Diferenças intencionais em relação ao exemplo

O exemplo tem tamanhos de fonte irregulares que `procedure-spec.md` §3 declara como erro de formatação, **não conteúdo**. O sistema imprime tudo uniforme:
1. "Objetivo:" das seções 4 e 5 em Bold 12 (no exemplo está em 11,04).
2. Último item das referências das seções 2 e 4 inteiro em tamanho 12 (no exemplo parte dele está em 11,04).
3. A seção 2 mantém "…e posicionar os dormentes" sem ponto final, como no texto aprovado.

## 4. Integração no PDF completo

O PDF final já é montado com as outras partes. Inserir o procedimento **depois da ART do plano e antes do laudo**, como 2 páginas A4. Gerar o procedimento a partir do **plano salvo** (mesma regra D13 das demais partes). Se `ResolveContent` falhar ou o plano não existir, o erro deve ser claro e nenhum PDF parcial deve ser devolvido.

## 5. Testes

1. **Conteúdo:** o texto de cada seção no PDF gerado é idêntico ao devolvido por `ResolveContent` (todas as letras, na ordem), para um plano em rascunho e para um plano finalizado com snapshot. Um plano finalizado **ignora** mudanças posteriores no texto/código do modelo dos dados vivos (usa o snapshot).
2. **Comparação com o exemplo, por letra:** como na etapa 1, comparar o PDF gerado com as **páginas 6 e 7** do PDF de exemplo (`Fixtures/reference-plan-2607036.pdf`, índices 5 e 6) usando o `PdfTextReader` por letra. Tolerância 1,0 pt em X e na linha de base; tamanho de fonte igual. Fixture: operação "Içamento Chiller", ART "1720264496217", plano "2607036", modelo "MD 45.007" (código "MD45007").
   **Exceções declaradas** (comparar só o texto, sem posição X): as duas linhas de "Objetivo:" das seções 4 e 5; o item final das referências da seção 2 e o da seção 4; a primeira referência da seção 4 (justificada; basta ter a mesma quebra de linha); as bolinhas "•" (basta estarem a ≤ 1,5 pt em X e na linha de base).
3. **Estrutura:** exatamente 2 páginas A4; seções 1–3 na primeira e 4–5 na segunda; cabeçalho e rodapé em ambas; número do rodapé "1" e "2"; separadores após as seções 1 a 4 e nenhum após a 5.
4. **Campos ausentes:** operação, ART e número do plano nulos imprimem `-`; ART e plano muito longos não estouram o cabeçalho.
5. **Operação longa** (70 caracteres) cabe sem truncar.
6. **Montagem:** o PDF completo tem o procedimento depois da ART e antes do laudo, com o total de páginas correto, e as páginas das outras partes não mudam.
7. Saída para revisão: `artifacts/pdf-review/stage-4/procedimento.pdf` e a sobreposição das 2 páginas com as do exemplo (vermelho = exemplo, ciano = gerado).

## 6. Critérios de aceite
- `dotnet build -c Release` e `dotnet test` verdes, sem testes ignorados.
- Com os dados do plano 2607036, as duas páginas coincidem com o exemplo (exceto as diferenças da seção 3).
- Nada novo no banco. Plano finalizado usa o snapshot; rascunho não cria snapshot.
- O PDF completo continua gerando as outras partes como antes.

## 7. Revisão humana
Abrir o PDF completo de um plano real e conferir: cabeçalho (logo, linhas azuis, ART e plano), título "PATOLAMENTO DO GUINDASTE {código}" com o código certo, as 5 seções na ordem, bolinhas, negrito nos códigos das normas, separadores, rodapé com a numeração 1 e 2, e que o procedimento vem depois da ART e antes do laudo. Para um plano **finalizado**, confirmar que o texto é o mesmo de quando foi finalizado.