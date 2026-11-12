# Procedimento Operacional — Texto de referência

> Referência da Seção 6.4 do `project-brief.md`. O texto é **padrão e único para todos os guindastes**, fica **embutido no app** (não no banco) e **não tem cadastro, edição pelo usuário nem versões**. Alterá-lo exige uma nova versão do app e a atualização deste arquivo.
> Origem: transcrito do plano de rigging de exemplo nº 2607036 (Kaefer, 24/07/2026). Tomado como definitivo para todos os modelos de guindaste por decisão de 12/11/2026, **sem confirmação do engenheiro**.

## 1. Estrutura do documento

- **Páginas:** 2 no exemplo (seções 1 a 3 na primeira, 4 e 5 na segunda).
- **Cabeçalho (repetido em cada página):** logotipo da empresa; título "Procedimento"; nome da operação; "ART" + número da ART; "RIGGING" + número do plano.
- **Rodapé (repetido em cada página):** `Rua Justo Manfron, 2845 G, Santa Felicidade, Curitiba - PR - CEP 82.410-540 – CNPJ 81.076.242/0001-60` e o número da página do procedimento (1, 2).
- **Valores variáveis:**

| Campo | Origem |
|---|---|
| Nome da operação (ex.: "Içamento Chiller") | `operation_name` do plano |
| Número da ART (ex.: 1720264496217) | `art_number` do plano |
| Número do plano (ex.: 2607036) | `plan_number` do plano |
| Código do modelo no título da seção 2 (ex.: `MD45007`) | Derivado do modelo do guindaste (`crane_model`), sem a marca, removendo espaços e pontos |

Valor ausente é exibido como "-".

## 2. Texto das seções

Todas as seções seguem o mesmo formato: **Objetivo**, **Passos** e **Referências**.

### 1. ISOLAMENTO DA ÁREA

**Objetivo:** Garantir segurança de pessoas e equipamentos durante a operação.

**Passos:**
- Delimitar e sinalizar a área de risco.
- Impedir circulação de pessoas não autorizadas.
- Verificar ausência de interferências: rede elétrica, estruturas, veículos.
- Liberar área somente após checagem.

**Referências:**
- NR-18.5.3 / NR-18.4.2(a) – Isolamento e sinalização de área de movimentação.
- NR-11.7.1 / NR-11.7.2 – Espaço de circulação seguro e sinalização de transporte de cargas.
- NR-18.15.47 – Proibição de permanência sob cargas suspensas.

### 2. PATOLAMENTO DO GUINDASTE {código do modelo}

**Objetivo:** Garantir estabilidade e segurança do equipamento de içamento.

**Passos:**
- Posicionar o guindaste conforme plano.
- Avaliar capacidade do solo (RMET) e posicionar os dormentes
- Estender e nivelar estabilizadores a 100%.

**Referências:**
- NR-18.14.22 / NR-18.14.24 – Estabilidade e operação conforme manuais.
- NR-11.5.1 – Equipamentos devem estar em condições seguras e inspecionados.
- NR-18.14.4 – Inspeção diária obrigatória dos equipamentos de movimentação.

### 3. POSICIONAMENTO DO VEÍCULO DE CARGA

**Objetivo:** Facilitar içamento e transporte seguro da carga.

**Passos:**
- Aproximar carreta ou veículo guiado pelo sinaleiro.
- Acionar freio, calçar rodas e desligar motor.
- Garantir espaço para manobra do gancho e acesso à carga.

**Referências:**
- NR-18.5.5 / NR-18.15.40 – Organização do fluxo de veículos no canteiro.
- NR-11.7.2 / NR-11.7.4 – Transporte de cargas e permanência de veículos estacionados.

### 4. IÇAMENTO DA CARGA

**Objetivo:** Elevar e movimentar carga com segurança e estabilidade.

**Passos:**
- Verificar cintas, manilhas e pontos de ancoragem.
- Montar amarração conforme plano de rigging.
- Realizar elevação de teste (10–20 cm).
- Elevar carga lentamente, mantendo comunicação com sinaleiro.
- Não movimentar carga sobre pessoas.
- Respeitar limites do equipamento e condições ambientais (vento, visibilidade).

**Referências:**
- NR-18.14.20 / NR-18.14.25 / NR-18.14.12–18.14.18 – Capacitação, inspeção e comunicação de operação.
- NR-18.5.3 / NR-18.15.47 – Proibição de pessoas sob cargas suspensas.
- NR-11.6.1 / NR-11.6.2 – Transporte seguro de cargas, estabilidade e movimentação.
- NR-11.7.6 – Planejamento da movimentação de cargas.

### 5. DESCARGA DA CARGA

**Objetivo:** Depositar carga com segurança e estabilidade.

**Passos:**
- Aproximar lentamente da posição de apoio.
- Baixar carga de forma controlada.
- Garantir apoio estável antes de soltar amarrações.
- Afastar equipe da zona de risco.
- Liberar área após conferência final.

**Referências:**
- NR-18.4.2(d) / NR-18.5.3 / NR-18.14.26 – Deposição segura e liberação da área.
- NR-11.6.3 / NR-11.7.5 – Evitar tombamento, esmagamento ou movimentação insegura.

## 3. Observações da transcrição

- O texto foi transcrito literalmente. O segundo passo da seção 2 ("Avaliar capacidade do solo (RMET) e posicionar os dormentes") está sem ponto final no exemplo, ao contrário dos demais; foi mantido como está. A sigla RMET não é explicada no exemplo.
- Algumas linhas do exemplo (por exemplo, o último item das referências da seção 2 e o último da seção 4) usam fonte menor que o restante. Isso é detalhe de formatação do arquivo de origem e não faz parte do conteúdo.
- Se o engenheiro revisar o texto, a atualização deve ser feita aqui e no recurso embutido do app, em uma nova versão. Planos já finalizados não são afetados, pois guardam o próprio snapshot (`project-brief.md` §6.4).