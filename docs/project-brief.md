# Rigging Planner — Jornada de Aprendizagem: Sistemas Inteligentes Conectados

> **Fonte oficial da verdade do projeto.** Em qualquer divergência entre este documento e os protótipos de tela, vale este documento. Os protótipos servem apenas como inspiração visual.
> Progresso do desenvolvimento: ver `docs/progress.md`.

## 1. Informações Gerais da Disciplina
- **Matéria:** Jornada de Aprendizagem
- **Tema do Semestre:** Sistemas Inteligentes Conectados
- **Equipe:** XHG
- **Integrantes:** Adrian Gobara Falci, Lorena Gobara Falci, Nilton Prudêncio Oliveira de Meira (todos atuam como desenvolvedores no projeto)
- **Entregáveis Finais:**
  - Solução prática para o problema
  - Artigo acadêmico
  - Vídeo/pitch de apresentação

## 2. Empresa Parceira
- **Nome:** Guindastes Ribas Ltda.
- **Contato focal/padrinho:** Giuliano Breda (engenheiro responsável pela parte de engenharia da Guindastes Ribas; planejador de rigging)
- **Área de atuação:** empresa familiar de Curitiba/PR (região metropolitana; sede no bairro Santa Felicidade), atualmente na segunda geração do negócio (há 15 anos). Atua em: remoção industrial (movimentação pesada para alteração/adequação de layouts fabris), içamento de cargas (guindastes e guindastes articulados) e transportes especiais (carga, descarga e transporte rodoviário de maquinário de grande porte). Frota: guindastes até 30t, guindastes articulados até 15t, empilhadeiras até 7,5t, transporte com carreta.

## 3. Tema de Pesquisa
- Conversão e Integração de Informações através de Sistemas Inteligentes Conectados
- **Detalhamento técnico:** uso de IA para buscar, centralizar e gerar informações com qualidade, através de arquiteturas de software

## 4. Contexto do Problema — Estado Atual
*(Relatado pelo engenheiro na reunião com o professor.)*

O engenheiro monta cada **Plano de Rigging** manualmente, reunindo e "costurando à mão" vários documentos independentes, cada um produzido em um software diferente e depois convertido para PDF:

- **Excel** → dados técnicos (dimensões da carga, peso, cargas, taxas de utilização) e o **memorial de cálculo**, que alimenta manualmente as outras partes do plano.
- **SolidWorks** → desenhos técnicos da operação (posicionamento do caminhão/guindaste/munck, ângulos, raio de operação, croquis 3D da carga içada).
- **Word** → procedimento operacional, laudo técnico de segurança do guindaste e textos padronizados (isolamento de área, NRs aplicáveis, patolamento etc.), que mudam pouco de um plano para outro.
- **Portal do CREA** → emissão da ART (Anotação de Responsabilidade Técnica), tanto a do plano de rigging quanto a das condições do guindaste; os dados da ART também precisam ser preenchidos manualmente dentro do plano.

O plano final pode ter de ~10 até 20+ páginas, dependendo da complexidade da operação (ex.: número de folhas de desenho; **cada plano envolve um único guindaste**, decisão de 27/10/2026). Cada plano é identificado por um **número de plano de rigging** e associado a uma empresa cliente (ex.: Kaefer, Bosch, Volvo, Electrolux, Volkswagen, Lenzgear, Convai, Renault, Gerdau/"Gelopar", Polux, TechVent, TechChiller, Florestal, entre outras).

Os cálculos de engenharia (carga bruta, taxa de utilização do guindaste e dos acessórios de içamento, interpolação de capacidade por raio de operação, margem de segurança de 85% da capacidade nominal etc.) são feitos manualmente pelo engenheiro em uma planilha/memorial de cálculo, e os resultados são copiados manualmente para o Excel de dados, para o desenho no SolidWorks e para o corpo do plano em Word — **sem nenhuma automação ou validação cruzada** entre esses locais.

## 5. O Problema Central

O desafio **não é** criar um sistema que faça os cálculos de engenharia por conta própria — isso é reconhecido pelo próprio engenheiro como uma frente à parte, fora do escopo desta jornada.

O problema que a equipe precisa resolver é de **integração e centralização de informação**: hoje, um mesmo dado (ex.: capacidade de carga, número do plano, dimensões, peso) é digitado manualmente várias vezes, em documentos e formatos diferentes (Excel, SolidWorks, Word, PDFs de ART do CREA), sem nenhuma garantia de que os valores permaneçam consistentes entre eles.

Isso gera:
- **Retrabalho** (o engenheiro "garimpa" seus próprios arquivos para lembrar de onde tirou cada número);
- **Inconsistência de unidades** (quilograma, tonelada, quilograma-força usados de forma não padronizada);
- **Risco de erro humano** na transcrição dos dados entre os documentos — risco crítico, pois qualquer inversão de valores pode comprometer a segurança da operação de içamento em campo.

## 6. Composição do Plano de Rigging

> **Regra:** um plano de rigging é sempre de **um único guindaste** (decisão de 27/10/2026).

### 6.1 Ordem das partes no PDF final
*(Confirmada com o plano de rigging final de exemplo: nº 2607036, cliente Kaefer, 24/07/2026 — 10 páginas.)*

| Páginas | Parte | Origem hoje | Como entra no sistema |
|---|---|---|---|
| 1 | **Capacidade de carga** (folha-resumo do plano) | Excel | Gerada pelo sistema a partir dos dados cadastrados |
| 2–4 | **Desenhos técnicos** (3 folhas A4 com carimbo) | SolidWorks | Imagens anexadas pelo usuário (PNG ou outro formato de imagem), em quantidade livre e na ordem que ele definir |
| 5 | **ART do plano de rigging** | Portal do CREA | PDF anexado ao plano pelo usuário (uma ART por plano) |
| 6–7 | **Procedimento operacional** (5 seções) | Word | Gerado pelo sistema (texto padrão único + campos variáveis) |
| 8–10 | **Laudo técnico de segurança** + ART do laudo | Word + CREA | **Um único PDF** (laudo + ART do laudo), cadastrado uma vez por equipamento e vinculado ao plano |

> O número de páginas varia por plano (ex.: número de folhas de desenho). As ARTs e o laudo entram como PDF inteiro, sem edição.

### 6.2 Detalhamento de cada parte

**1. Capacidade de carga (página 1)**
Campos identificados no exemplo:
- *Cabeçalho:* logotipo, Cliente, Local, Data, número do plano de rigging.
- *Dados da carga:* descrição, comprimento (m), largura (m), altura (m), peso (kg).
- *Lingada / amarras:* tabela com item, descrição (ex.: "Cinta plana 3 metros de comprimento - Capacidade 6t"; "Manilha Curva 7/8" com pino roscado - GRAU 6 - FS 6:1"), quantidade, peso unitário (kg), peso total (kg) e peso total da lingada.
- *Dispositivos / balancim:* item, descrição, referência/desenho, quantidade, peso unitário, peso total.
- *Informações do guindaste:* modelo, caminhão, placas, chassi, ano de fabricação.
- *Configuração do guindaste:* capacidade nominal (t), comprimento da lança (m), raio de operação (m), capacidade de carga (t).
- *Composição da carga bruta (kg):* carga líquida, lingada/amarração, dispositivos/balancim, cargas extras, carga bruta. Os totais (peso total de cada linha, total da lingada, total de dispositivos e carga bruta) são **calculados na exibição**: não são digitados nem gravados.
- *Quadrante operacional:* inicial e final (ex.: LATERAL / LATERAL). **Campo de texto livre** (os valores possíveis não são conhecidos).
- *Resumo dos cálculos:* total da carga bruta (t) *(calculado na exibição)*, capacidade de carga do guindaste (t) *(digitada)*, taxas de utilização (%): guindaste *(calculada na exibição: carga bruta ÷ capacidade de carga; no exemplo, 3746,62 ÷ 7950 = 47,1%)* e cinta plana, cinta tubular, laço de cabo de aço e manilha *(digitadas)*, esforço nas sapatas (**kgf**), resistência mínima do terreno (**kgf/cm²**), velocidade máxima do vento (m/s).
- *Observações / Anexos:* número da ART. Texto livre.
- *Assinaturas:* Rigger responsável (nome e registro RPP) e Supervisor Responsável.

**2. Desenhos técnicos (SolidWorks)**
Três folhas A4 no exemplo: (1) vistas lateral e superior com raio de operação e ângulo; (2) vistas com raio de isolamento; (3) desenho de amarração (cinta plana, manilha, balancim, gancho, com medidas e especificações a seguir em campo). Cada folha tem um **carimbo** (rodapé) que repete dados do plano: descrição e dimensões da carga, rigger/CREA, carga bruta, capacidade, taxa de utilização, esforço nas sapatas, velocidade do vento, ART, nº do rigging, cliente, operação, revisão e "folha X de Y". Esses dados já vêm "assados" na imagem (hoje preenchidos a partir de uma tabela Excel).
O usuário anexa quantas imagens quiser e define a ordem; o sistema encaixa cada imagem numa página do PDF. As margens das folhas são sempre as mesmas — só o conteúdo do carimbo muda.

**3. ART do plano de rigging**
Emitida no portal do CREA-PR; o plano só é válido com a RT recolhida, feita por um planejador de rigging. A emissão **não** é automatizada: o sistema apenas recebe o PDF e o inclui no plano. Cada plano tem **uma** ART. O número da ART (ex.: 1720264496217) aparece na página 1, no carimbo dos desenhos e no cabeçalho do procedimento.

**4. Procedimento operacional**
Cinco seções fixas: 1. Isolamento da área, 2. Patolamento do guindaste (o título traz o modelo, ex.: MD45007), 3. Posicionamento do veículo de carga, 4. Içamento da carga, 5. Descarga da carga — cada uma com objetivo, passos e referências a NRs. O cabeçalho de cada página traz o nome da operação (ex.: "Içamento Chiller"), o número da ART e o número do rigging; o rodapé traz o endereço e o CNPJ da empresa. **Só mudam** o modelo do guindaste, o nome da operação e os dois números do cabeçalho.
O texto é **padrão e único para todos os guindastes**, sem controle de versões (decisão de 27/10/2026); fica no sistema, não no banco de dados.

**5. Laudo técnico de segurança**
Referente ao guindaste/caminhão específico (ex.: Palfinger MD 45.007 sobre Volvo/VM 270 6X2R). Traz dados da empresa, dados do equipamento, **validade do laudo** (início e fim), lista de trabalhadores autorizados, itens inspecionados e observações. Tem ART própria (distinta da ART do rigging), no mesmo PDF. O laudo e a ART cobrem o **conjunto caminhão + guindaste** e valem por um período, sendo reaproveitados em vários planos; por isso são **cadastrados uma única vez por equipamento** (Seção 12), com validade e número da ART, e apenas vinculados aos planos. Os dados do equipamento repetem os da página 1 do plano.

**Documento à parte — memorial de cálculo (não entra no plano final)**
Documento em Word com o desenho da carga içada, os ângulos, os pontos de fixação, a taxa de utilização e a interpolação de capacidade por raio de operação. Alimenta os outros documentos, mas deixou de ser incluído no plano final para evitar que a metodologia de cálculo seja copiada por terceiros.

### 6.3 Observações da análise do plano de exemplo
- **Formato do número do plano:** "2607036" sugere AAMMNNN (ano 26, mês 07, sequência 036). **A sequência reinicia a cada mês** e o mês é o da **data do plano**; se a data do plano mudar de mês, o número é gerado de novo (decisão de 27/10/2026). Formato AAMMNNN adotado com base no exemplo (ver Seção 16).
- **Arredondamentos:** carga bruta = 3450 + 46,62 + 250 = 3746,62 kg, exibida como "3747" (kg) e "3,75" (t); taxa de utilização 47,1% (3746,62 ÷ 7950). **Padrão adotado (27/10/2026):** a conversão kg → t é feita pelo sistema; carga bruta em kg sem casas decimais e em t com duas casas (3746,62 → 3747 kg → 3,75 t); taxas de utilização com uma casa decimal (47,1%); totais da lingada com duas casas (46,62 kg), como no exemplo. O arredondamento vale só para a exibição: os valores guardados e usados nas contas não são arredondados.
- **Rótulo repetido:** na tabela de dispositivos/balancim, o total aparece com o rótulo "PESO TOTAL DA LINGADA / AMARRAÇÃO", igual ao da tabela de lingada. Manter fiel ou corrigir? (Seção 16)
- **Esforço nas sapatas × resistência do terreno (decidido):** esforço nas sapatas é em **kgf**; resistência mínima do terreno é em **kgf/cm²**. No carimbo do exemplo o valor 3,17 aparece como "esforço nas sapatas" em toneladas; como o carimbo vem "assado" na imagem do SolidWorks, o sistema não o altera — a divergência deve ser avisada ao engenheiro.

## 7. Requisitos da Solução
*(Descritos pelo engenheiro na reunião.)*

Uma solução ideal deveria:

- Ter um **cadastro único de dados de entrada**: nome/identificação do guindaste (placa, chassi, capacidade), dimensões e peso da carga, dados dos acessórios de içamento (nome, peso unitário e tags), dados do planejador/rigger (nome, RPP, CREA) e da empresa cliente/CNPJ.
- **Reaproveitar automaticamente** esses dados no documento final, preenchendo as seções que hoje são copiadas manualmente entre Excel, SolidWorks e Word (ex.: primeira página do plano, com a maior parte das informações).
- Permitir **anexar imagens/desenhos técnicos** (as folhas geradas no SolidWorks) e os **PDFs de ART e laudo** emitidos fora do sistema, integrando tudo num único PDF final.
- **Manter o texto padrão** das seções que quase não mudam entre planos (procedimento de içamento, isolamento de área, NRs aplicáveis, patolamento do guindaste), alterando apenas o que é específico de cada operação (ex.: modelo do guindaste — MD45.007, MD55, DM130, MD300L etc.).
- **Zero tolerância a erro de dados**: em nenhuma hipótese o sistema pode alterar, inverter ou perder valores/informações entre etapas — restrição crítica (Seção 14).

Referência mental citada pelo engenheiro: o conceito de uma "macro" que, a partir dos dados preenchidos, gera automaticamente o relatório final pronto — ou seja, **automação de preenchimento e formatação, não de cálculo**.

> **Nota (atualizada em 27/10/2026):** as informações específicas de cada trabalho (peso da carga, local, descrição) são digitadas a cada novo trabalho. Já **clientes** (com CNPJ), **equipamentos com laudo**, **acessórios** e **riggers** ficam em cadastros reaproveitáveis entre planos. O cliente é escolhido na tela do plano e, se não existir, pode ser cadastrado ali mesmo; o mesmo vale para o laudo do equipamento. O "cadastro único" também garante que os dados sejam reaproveitados **dentro de um mesmo plano**, sem redigitação.

## 8. Fora de Escopo (por enquanto)

- Um motor de cálculo estrutural/de engenharia que substitua o memorial de cálculo — os cálculos continuam sendo feitos pelo engenheiro (capacidade, taxas dos acessórios, esforço nas sapatas etc. são digitados). O sistema faz apenas **contas simples e verificáveis** (somas de pesos, conversão kg → t e taxa de utilização do guindaste), sempre na exibição.
- **Interpolação automática de capacidade** do guindaste por raio de operação — decisão da equipe: fora do escopo por enquanto (a resposta "automático" da empresa foi superada pelo pedido do engenheiro de que o sistema não faça cálculos; pode ser reavaliada com o engenheiro/empresa; ver Seção 16).
- Um aplicativo interativo de cálculo de capacidade em tempo real — projeto de **outra equipe de Jornada de Aprendizagem** (aparentemente liderada por alguém chamado Elton).
- **Backup do banco de dados** — será feito eventualmente (backup local), mas não é prioridade agora.
- **Exportação em `.docx`** — a saída é apenas PDF.

### 8.1 Ciclo de vida dos cadastros e histórico

Cadastros reutilizáveis, como riggers e acessórios, não devem ser apagados imediatamente quando deixam de ser usados. O fluxo padrão é:

- **Ativo:** aparece nos cadastros e pode ser selecionado em novos planos; pode ser editado.
- **Inativo:** fica separado dos ativos, com aparência esmaecida; não pode ser editado nem selecionado em novos planos. Pode ser reativado ou excluído definitivamente.
- **Exclusão definitiva:** exige confirmação explícita. É bloqueada quando o cadastro está sendo usado por um plano em andamento. Se estiver ligado somente a planos finalizados, o cadastro pode ser removido, pois o plano mantém os dados copiados no momento da operação.

Planos finalizados são somente leitura e preservam seu histórico mesmo que cadastros reutilizáveis sejam posteriormente excluídos. O plano mantém os valores necessários para reconstrução do documento, incluindo descrição e peso dos itens de acessórios e nome, RPP e CREA do rigger. A finalização registra `FinalizedAt`; planos em rascunho continuam editáveis.

Esse fluxo diferencia **inativar** (remoção lógica e reversível) de **excluir definitivamente** (remoção física protegida por verificação de vínculos). Tags podem ser excluídas com desvinculação dos acessórios, sem alterar o histórico dos planos.
- Usuários, perfis de acesso, autenticação, fluxo de aprovação e compartilhamento em rede/internet — o sistema é local e de uso individual (Seção 11).

## 9. Respostas da Empresa — Levantamento de Requisitos
*(Respostas a um questionário enviado pela equipe. Itens marcados como "atualizado" foram alterados por decisões posteriores.)*

1. **Quem utilizará o sistema no dia a dia?** Apenas equipe interna. *(Atualizado: uma única pessoa, o engenheiro Giuliano; os Riggers recebem o plano pronto em PDF.)*
2. **Formato de acesso desejado:** sistema desktop, instalado diretamente nas máquinas da empresa. *(Confirmado: aplicação desktop local.)*
3. **Fluxo formal de aprovação por etapas?** Não — preenchimento direto e emissão simples.
4. **Emissão direta ou validação prévia obrigatória?** Pode ser emitido automaticamente.
5. **ART emitida via integração com o CREA-PR?** Não — o sistema apenas organiza os dados; a ART vem como PDF anexado.
6. **De onde virão as informações iniciais do serviço (peso da carga, cliente, local)?** Digitadas do zero a cada novo trabalho. *(Atualizado: o cliente vem de um cadastro de clientes com CNPJ — ver Seção 7.)*
7. **Capacidade em raios de operação quebrados?** A empresa respondeu "automático", mas a equipe decidiu deixar a interpolação **fora do escopo por enquanto** (Seção 8).
8. **Cadastro de acessórios (cintas, manilhas e laços):** banco de dados com controle — cadastro fixo com histórico e alertas de desgaste/descarte.
9. **Numeração dos Planos de Rigging e ARTs:** a empresa respondeu "numeração sequencial automática". Como a ART é emitida no CREA e entra como PDF, o sistema **não gera número de ART**; a numeração automática vale para o plano e **reinicia a cada mês**, pela data do plano (formato AAMMNNN — Seção 16).
10. **Relatório resumido para campo?** Não.

### 9.1 Alteração de regra de negócio (pedido do Giuliano)
Antes: aplicação web com autenticação por e-mail. **Agora:** aplicação **desktop local, para uma única pessoa, sem autenticação, sem perfis de acesso e sem usuários**.

## 10. Decisões e Preferências Técnicas

| Tema | Decisão |
|---|---|
| Plataforma | App desktop **Windows**, instalável (o usuário instala e apenas abre) |
| Frontend | **Blazor Hybrid (.NET MAUI)**, UI **100% em português (pt-BR)** |
| Versão | **.NET 10** |
| Persistência | **SQLite** local com **EF Core + migrations** |
| Banco de dados | Arquivo único em `%LOCALAPPDATA%\RiggingPlanner\rigging-planner.db`; `IDbContextFactory` (contexto curto por operação); migrations aplicadas ao abrir o app |
| Anexos | **Arquivos gravados em disco** (pasta própria do app); o banco guarda apenas o caminho e os metadados de cada arquivo |
| Dados fixos | Dados da empresa (logo, endereço, CNPJ, IE) e texto do procedimento ficam **dentro do aplicativo** (recursos/arquivo de configuração embutidos), não no banco; só mudam com uma nova versão do app |
| Cálculos | Contas simples (somas, conversão kg → t, taxa de utilização do guindaste) feitas só na **exibição**; nada derivado é gravado no banco |
| Arquitetura | Arquitetura limpa, **simples** (sem overengineering): `Domain`, `Application`, `Infrastructure`, `App` |
| Estilo visual | **Claymorphism**, com azul como cor central; CSS próprio, sem biblioteca de componentes |
| Código | Nomes (classes, variáveis, métodos) em **inglês** |
| Testes | xUnit |
| Saída | **PDF** (gerar páginas do plano e do procedimento, juntar ART, laudo e imagens). Biblioteca(s) a definir na feature de exportação |
| Organização dos planos | Planos soltos ou dentro de **pastas de um único nível** |
| Repositório | `main` + um branch por etapa, com PR; CI no GitHub Actions |

## 11. Arquitetura do Sistema

### 11.1 Contexto
Apenas o engenheiro opera o sistema; os Riggers recebem o plano pronto em PDF. Por isso o sistema é um app desktop local de usuário único, sem API HTTP, sem rede e sem autenticação.

### 11.2 Visão geral (máquina do engenheiro, Windows)
```
App (Blazor Hybrid / .NET MAUI)  — telas Razor, UI em português
        │
Application  — casos de uso, interfaces de persistência/arquivos
        │
Domain       — entidades e regras de negócio
        ▲
Infrastructure — EF Core + SQLite, armazenamento de anexos, geração/junção de PDF
```
Dependências: `App → Application, Infrastructure` (raiz de composição); `Infrastructure → Application`; `Application → Domain`. O `Domain` não depende de nada.

### 11.3 Componentes
- **App (UI):** Razor Components num WebView nativo (via .NET MAUI). Telas: lista de planos (com pastas; clicar em um plano abre a edição), lixeira de planos, novo/editar plano em 5 etapas (dados de entrada, capacidade de carga, anexos, preview, exportação), e cadastros de acessórios, equipamentos e laudos, clientes e riggers.
- **Domain:** cadastro único de dados do plano (guindaste, carga, acessórios, cliente, planejador/RT), numeração sequencial de planos (reinicia a cada mês, pela data do plano), cálculos simples de exibição, regras de consistência de dados e estado do plano (rascunho/finalizado/na lixeira).
- **Application:** casos de uso (criar/editar/finalizar plano, gerenciar pastas, gerenciar acessórios, equipamentos, laudos e clientes, anexar arquivos, gerar PDF) e as interfaces que a infraestrutura implementa.
- **Infrastructure:** SQLite com EF Core (`AppDbContext` e migrations em `Persistence/`; dados de entrada, cadastros, planos, numeração, pastas, histórico de exportações), armazenamento dos anexos em disco (inclusive a remoção dos arquivos na exclusão definitiva) e geração/junção do PDF final.
- **Backup:** previsto para o futuro (cópia local do banco); fora do escopo agora.

### 11.4 Modelo de Dados (decisões de 27/10/2026)
> Estrutura lógica; os tipos reais seguem o mapeamento do EF Core para SQLite (datas e decimais ficam como texto, por isso as somas são feitas no código, nunca no SQL). Itens marcados com *(premissa)* foram adotados por padrão e estão na Seção 16.

**Princípios**
- **Plano autossuficiente:** ao escolher cliente, equipamento, rigger ou acessório, os valores são **copiados** para o plano; a ligação com o cadastro original é mantida só como referência. Alterar ou inativar um cadastro não muda planos existentes.
- **Nada derivado é gravado:** pesos por linha, totais da lingada e dos dispositivos, carga bruta (kg e t), taxa de utilização do guindaste, número do item, código do modelo no procedimento e laudo válido/vencido são calculados na exibição.
- **Arquivos em disco:** o banco guarda só caminho e metadados, todos em uma única tabela de arquivos.
- **Números de plano nunca são reutilizados**, nem os de planos excluídos.
- **Dados fixos fora do banco:** dados da empresa e texto do procedimento ficam embutidos no aplicativo.

**Cadastros**

| Tabela | Colunas |
|---|---|
| `clients` | `id` PK; `name`; `cnpj` (único quando preenchido); `is_active`; `created_at`; `updated_at` |
| `riggers` *(premissa)* | `id` PK; `full_name`; `display_name`; `rpp_number`; `crea_number`; `is_active` |
| `equipments` (conjunto caminhão + guindaste) | `id` PK; `equipment_type`; `truck_model`; `license_plate`; `chassis_number` (único); `manufacture_year`; `crane_brand`; `crane_model`; `nominal_capacity`; `is_active`; `created_at`; `updated_at` |
| `equipment_reports` (laudo + ART do laudo, um PDF) | `id` PK; `equipment_id` FK; `file_id` FK; `art_number`; `valid_from`; `valid_to`; `created_at` |
| `accessories` *(premissa)* | `id` PK; `name`; `unit_weight_kg`; `status`; `review_due_date` (nulo); `created_at`; `updated_at` |
| `tags` | `id` PK; `name` (único); `created_at` |
| `accessories` → `tags` | `tag_id` FK opcional; cada acessório possui no máximo uma tag |
| `accessory_events` *(premissa)* | `id` PK; `accessory_id` FK; `event_date`; `event_type` (texto livre); `notes` |

**Arquivos, pastas e numeração**

| Tabela | Colunas |
|---|---|
| `stored_files` | `id` PK; `relative_path`; `original_name`; `content_type`; `size_bytes`; `sha256`; `created_at` |
| `folders` | `id` PK; `name`; `created_at` |
| `plan_number_counters` | `year_month` PK (ex.: "2607"); `last_sequence` |

**Plano: `rigging_plans`**

| Grupo | Colunas |
|---|---|
| Identificação | `id` PK; `plan_number` (único, inclusive os da lixeira); `folder_id` FK (nulo = plano solto); `status` (rascunho ou finalizado); `finalized_at` (nulo); `deleted_at` (nulo; preenchido = na lixeira); `created_at`; `updated_at` |
| Cabeçalho | `plan_date`; `location`; `operation_name` (texto livre) |
| Cliente | `client_id` FK (nulo); `client_name`; `client_cnpj` (copiados) |
| Carga | `load_description`; `length_m`; `width_m`; `height_m`; `weight_kg`; `extra_load_kg` (nulo) |
| Equipamento (copiados) | `equipment_id` FK (nulo); `truck_model`; `license_plate`; `chassis_number`; `manufacture_year`; `crane_brand`; `crane_model`; `crane_nominal_capacity` |
| Configuração do guindaste | `boom_length_m`; `operating_radius_m`; `load_capacity_t`; `quadrant_start`; `quadrant_end` (texto livre) |
| Resultados digitados (nulos quando "-") | `flat_strap_utilization_pct`; `tubular_strap_utilization_pct`; `wire_rope_sling_utilization_pct`; `shackle_utilization_pct`; `footing_force_kgf`; `min_ground_resistance_kgf_cm2`; `max_wind_speed_ms` |
| Responsáveis | `rigger_id` FK (nulo); `rigger_name`; `rigger_rpp` (copiados); `supervisor_name` (nulo, texto livre) |
| ART e laudo | `art_number`; `art_file_id` FK (nulo); `equipment_report_id` FK (nulo; deve ser laudo do mesmo equipamento do plano) |
| Observações | `observations` (texto livre) |

**Itens do plano**

| Tabela | Colunas |
|---|---|
| `plan_accessories` | `id` PK; `plan_id` FK; `section` (lingada ou dispositivo); `accessory_id` FK (nulo); `description`; `drawing_ref` (nulo); `quantity`; `unit_weight_kg`; `sort_order` |
| `plan_drawings` | `id` PK; `plan_id` FK; `file_id` FK; `sort_order` (únicos por plano) |
| `plan_exports` | `id` PK; `plan_id` FK; `exported_at`; `file_path`; `file_sha256`; `was_final` |

**Relacionamentos e cardinalidades**

| Relação | Cardinalidade | Ao excluir |
|---|---|---|
| Pasta → Planos | 1 : N (plano pode ficar sem pasta) | Pendente (Seção 16) |
| Cliente / Rigger / Equipamento → Planos | 1 : N cada | Cadastro em uso não é excluído, só inativado |
| Equipamento → Laudos | 1 : N | Laudo em uso por plano não pode ser excluído |
| Laudo → Planos | 1 : N | — |
| Plano → Itens (`plan_accessories`) | 1 : N | Apagados com o plano (exclusão definitiva) |
| Plano → Desenhos (`plan_drawings`) | 1 : N, ordenados | Apagados com o plano, junto dos arquivos |
| Plano → Exportações (`plan_exports`) | 1 : N | Apagadas com o plano |
| Plano → ART (`art_file_id`) | 1 : 1 | Arquivo apagado com o plano |
| Acessório → Eventos | 1 : N | Apagados com o acessório (que não pode ser excluído se estiver em uso) |
| Laudo / Desenho / ART → Arquivo | N : 1 | O arquivo do laudo não é apagado com o plano |

## 12. Funcionalidades
> Pensadas como um **editor de documentos estruturado com limitações de edição**, específico para planos de rigging — não um editor genérico de Word/Excel.

- **Planos e pastas:** criar planos soltos ou dentro de pastas (um nível); abrir (clicando na lista) e editar. Não há função de duplicar plano. O sistema **guarda todos os parâmetros** do plano para que o engenheiro possa voltar e alterar qualquer informação depois de exportar.
- **Finalizar plano:** enquanto não finalizado, o plano pode ser editado sempre. O usuário pode **finalizar** o plano (com confirmação), o que o **trava** — usado quando o plano foi entregue definitivamente. Como alterar um plano finalizado: ver Seção 16.
- **Lixeira:** ao excluir, o plano vai para a lixeira e pode ser restaurado. Na lixeira, a **exclusão definitiva** (com confirmação) apaga tudo do plano: dados, itens, desenhos, ART do plano e histórico de exportações. O laudo não é apagado, pois pertence ao equipamento. O número de um plano excluído nunca é reutilizado.
- **Dados de entrada:** cliente, local, data, guindaste (placa, chassi, capacidade), carga (descrição, dimensões, peso), acessórios, planejador/rigger (nome, RPP, CREA).
- **Riggers:** cadastro (nome completo, nome de exibição, RPP, CREA), escolhido no plano e cadastrável ali mesmo. O "Supervisor Responsável" é texto livre e opcional no plano.
- **Cadastros em uso:** clientes, equipamentos, riggers e acessórios usados em algum plano não podem ser excluídos, apenas **inativados** (deixam de aparecer para escolha; os planos mantêm seus dados).
- **Clientes:** cadastro de clientes com CNPJ; na tela do plano o cliente é selecionado e, se não existir, pode ser cadastrado direto ali.
- **Equipamentos e laudos:** cadastro do **equipamento (caminhão + guindaste)**. Para cada equipamento, o usuário cadastra o **laudo técnico de segurança** (PDF único com a ART do laudo), informando a **data de validade** (início e fim) e o **número da ART do laudo**. A tela separa laudos **válidos** e **vencidos**. Na tela de criação/edição do plano, o laudo é escolhido entre os cadastrados e, se não houver, pode ser cadastrado direto ali.
- **Capacidade de carga:** interface tipo planilha com os mesmos grupos de campos da folha-resumo (Seção 6.2), com os dados reaproveitados dos demais passos (sem redigitação). Capacidade de carga, taxas dos acessórios, esforço nas sapatas, resistência do terreno e vento são digitados pelo engenheiro; pesos totais, carga bruta, conversão para t e taxa de utilização do guindaste são calculados na exibição.
- **Cadastro de acessórios:** banco com histórico, alertas de desgaste/descarte e controle de situação (regras detalhadas na feature).
- **Anexos do plano:** imagens das folhas do SolidWorks (PNG ou outros formatos de imagem; quantidade livre, ordem definida pelo usuário) e PDF da ART do plano; o número da ART é digitado uma vez no plano e reaproveitado nas demais páginas (leitura automática do PDF fica como evolução futura).
- **Dados congelados no plano:** ao escolher cliente, equipamento, acessórios etc., os valores são copiados para o plano. Alterar um cadastro depois **não altera planos já criados**; no máximo o sistema informa ou avisa que o cadastro mudou.
- **Preview** do plano completo antes de finalizar.
- **Exportação em PDF:** página-resumo e procedimento gerados pelo sistema, com os desenhos, a ART e o laudo inseridos **no meio** do documento, na ordem da Seção 6.1. O PDF fica gravado no disco, o sistema pergunta a pasta a cada exportação (sugerindo a última usada) e mantém um **histórico de exportações** de cada plano.
- **Numeração sequencial automática** dos planos (AAMMNNN), reiniciando a cada mês pela data do plano. Enquanto o plano for rascunho, mudar a data para outro mês gera um novo número (com aviso se o plano já foi exportado).

### 12.1 Fluxo de cadastro e vínculo ao plano (decisões de 27/10/2026)

| Caso | Onde é cadastrado | Como entra no plano |
|---|---|---|
| **Cliente** | Tela de clientes (nome e CNPJ) **ou** direto na tela do plano, se não existir | Escolhido na lista; nome e CNPJ são copiados para o plano |
| **Equipamento (caminhão + guindaste)** | Tela de equipamentos | Escolhido na lista; dados do caminhão e do guindaste copiados para o plano |
| **Laudo + ART do laudo** | Tela de laudos do equipamento (PDF único, início e fim de validade, nº da ART) **ou** direto na tela do plano, se o equipamento não tiver laudo | Escolhido entre os laudos do equipamento do plano; a tela separa **válidos** e **vencidos** |
| **Rigger** *(premissa)* | Tela de riggers **ou** direto na tela do plano | Escolhido na lista; nome de exibição e RPP copiados; "Supervisor Responsável" é texto livre |
| **Acessórios** *(premissa)* | Tela de acessórios (nome, peso unitário, tags, situação, histórico) | Tags são selecionadas por autocomplete e administradas separadamente; escolhidos na lista, com quantidade; nome e peso unitário copiados; o item fica em "lingada" ou "dispositivo" |
| **ART do plano** | Não há cadastro: é anexada ao plano | PDF anexado e número da ART digitado uma vez no plano (reaproveitado nas outras páginas) |
| **Desenhos (SolidWorks)** | Não há cadastro: são anexados ao plano | Quantidade livre; o usuário define a ordem |
| **Carga, local, data, configuração do guindaste, resultados** | Digitados no plano a cada trabalho | Gravados no plano; totais e taxa do guindaste calculados na exibição |
| **Número do plano** | Gerado pelo sistema ao criar o plano | Mês e ano vêm da data do plano; muda se a data passar para outro mês (só em rascunho, com aviso se já exportado) |
| **Finalização** | Ação no plano, com confirmação | Plano finalizado fica travado; antes disso é sempre editável (como alterar depois: Seção 16) |
| **Exclusão** | Ação no plano | Vai para a lixeira; a exclusão definitiva (com confirmação) apaga o plano e seus arquivos |
| **Exportação** | Ação no plano | Pergunta a pasta *(premissa)*, grava o PDF e registra no histórico de exportações |
| **Cadastros em uso** *(premissa)* | Telas de cadastro | Só podem ser inativados; deixam de aparecer na escolha, mas os planos já criados não mudam |

## 13. Organização dos Planos
- Os planos ficam **soltos** ou dentro de **pastas**, com **no máximo um nível** de pastas (sem categorias).
- Planos excluídos ficam na lixeira até a exclusão definitiva (Seção 12).
- Onde o PDF exportado é gravado: o sistema pergunta a cada exportação, sugerindo a última pasta usada (premissa adotada — Seção 16).

## 14. Restrições Críticas
- Em hipótese alguma podem ocorrer alterações de dados ou inversão de valores/informações, pois isso pode gerar acidentes graves em campo.

## 15. Glossário / Termos Técnicos

- **Plano de Rigging:** documento final que orienta a operação de içamento em campo.
- **RT:** Responsabilidade Técnica, registrada no CREA.
- **ART:** Anotação de Responsabilidade Técnica, emitida pelo CREA; há duas por plano — a do plano de rigging (anexada ao plano) e a do laudo das condições do guindaste (junto do laudo, cadastrada no equipamento).
- **CREA:** órgão onde a RT é registrada e a ART é emitida.
- **Equipamento:** conjunto caminhão + guindaste ao qual um laudo se refere.
- **Laudo válido / vencido:** laudo cuja data final de validade ainda não passou / já passou.
- **Plano finalizado:** plano travado para edição, depois da entrega definitiva.
- **Lixeira:** área onde ficam os planos excluídos até a exclusão definitiva.
- **Balancim:** acessório de içamento (no exemplo citado, pesa 250 kg).
- **Lingada / amarras:** conjunto de cintas, manilhas e laços que prende a carga ao gancho.
- **Taxa de utilização:** percentual da capacidade nominal (do guindaste ou de um acessório) efetivamente utilizado numa operação.
- **Carga bruta:** carga líquida somada à lingada/amarração, aos dispositivos/balancim e a cargas extras.
- **Margem de segurança:** capacidade máxima de cálculo considerada é de 85% da capacidade nominal do guindaste.
- **Interpolação de capacidade:** cálculo de capacidade para um raio de operação que não consta na tabela do guindaste (fora do escopo por enquanto).
- **Munck:** tipo de guindaste citado como exemplo de equipamento usado nas operações.
- **Rigger:** profissional de campo que atua junto ao engenheiro; recebe o plano pronto em PDF.
- **Blazor Hybrid:** interface web (Razor) rodando dentro de um app nativo.
- **.NET MAUI:** framework da Microsoft usado aqui para empacotar a UI Blazor como app desktop Windows.
- **SQLite / EF Core:** banco local em arquivo único / biblioteca de acesso a dados com migrations.
- **Claymorphism:** estilo visual de elementos "de argila": cantos muito arredondados, sombras suaves e volume.

## 16. Dúvidas Pendentes
*(Não devem ser respondidas por suposição.)*

1. **Plano finalizado:** como alterar um plano já finalizado — reabrir (destravar com confirmação) ou criar uma nova revisão (o carimbo dos desenhos traz "Revisão 1 de 1")? Como não há função de duplicar, uma nova revisão seria uma funcionalidade à parte.
2. **Exclusão de pasta:** o que acontece com os planos de uma pasta excluída (ficam soltos? a pasta só pode ser excluída vazia?).
3. **Exclusão definitiva:** além dos dados e dos anexos guardados pelo app, os PDFs já exportados para o disco também devem ser apagados?
4. **Rótulo repetido** no total de dispositivos/balancim (Seção 6.3): manter fiel ao original ou corrigir?
5. **Carimbo dos desenhos:** o sistema deve mostrar os dados "assados" da imagem ao lado dela para conferência visual?
6. **Capacidade nominal "45,0 t":** o laudo traz 10,45 ton na placa e o 45 pode ser o momento de carga do modelo (MD 45.007). Sem confirmação do engenheiro, o rótulo e a unidade seguem o exemplo.
7. **Validar com o engenheiro quando possível:** formato AAMMNNN do número do plano; regra do código do modelo no procedimento; fórmula da taxa de utilização do guindaste (conferida em um único exemplo); divergência do carimbo do exemplo (3,17 t como "esforço nas sapatas").
8. **Templates originais** (Excel, Word) continuam indisponíveis; margens e formatação exatas do PDF seguem o plano de exemplo.
9. **Interpolação de capacidade** (reavaliar depois) e **backup local** (definir destino quando for implementado).
10. **Cadastro de acessórios:** como controlar desgaste (por unidade ou por tipo), o que entra no histórico e o que dispara os alertas (regras detalhadas na feature).

### Premissas adotadas por padrão (27/10/2026) — a validar
- O número da ART do plano é digitado, não lido do PDF.
- O modelo do guindaste é guardado em marca e modelo ("Palfinger" / "MD 45.007"); o título do procedimento usa o modelo sem espaços e pontos ("MD45007").
- "Supervisor Responsável" é texto livre e opcional; o nome da operação é texto livre, com sugestão "Içamento" + descrição da carga.
- Acessórios são cadastrados por tipo/especificação (não por unidade física), com situação, data de revisão opcional e histórico de eventos em texto livre.
- Cadastros usados em planos só podem ser inativados, não excluídos.
- Dados da empresa e texto do procedimento ficam embutidos no aplicativo.
- A pasta do PDF exportado é perguntada a cada exportação.

## 17. Cronograma / Sprints
- **Metodologia:** Scrum, com planejamento acompanhado via GitHub Projects.
- **Sprints:** ciclos de duas semanas.
- **Prazo final do projeto:** 20/11/2026.
- **Status atual:** arquitetura definida (Seção 11); protótipos de tela feitos como inspiração; desenvolvimento iniciado — esqueleto do repositório (commit 1) e banco de dados com EF Core + SQLite (commit 2). Modelagem das tabelas em definição (27/10/2026). Etapas realizadas e próximas: `docs/progress.md`.

## 18. Notas Gerais / Pontos em Aberto do Time
- Os detalhes de UX/fluxo de telas seguem em aberto: os protótipos são só inspiração e podem ser alterados.
- **Restrições enfrentadas:** (tempo) a resposta da empresa ao levantamento de requisitos chegou só no fim da semana anterior a 21/09/2026; (conhecimento técnico) dificuldade em entender o que já existe na documentação/processo atual da empresa e falta dos templates originais.

## 19. Histórico de Alterações deste Documento
- **20/10/2026:** removidos usuários, perfis, autenticação e compartilhamento; sistema passa a ser desktop local de uso individual; stack fixada (.NET 10, MAUI Blazor Hybrid Windows, EF Core + SQLite); interpolação e backup fora do escopo; saída só em PDF; organização por pastas de um nível; estrutura do PDF final confirmada com plano de exemplo; corrigida a codificação de caracteres do arquivo.
- **22/10/2026:** definido onde fica o banco, o uso de `IDbContextFactory` e a aplicação automática das migrations (commit 2).
- **27/10/2026:** um plano = um guindaste; laudo (com a ART do laudo, em um PDF) cadastrado uma vez por equipamento (caminhão + guindaste) com validade e nº da ART, separando válidos e vencidos, e cadastrável também na tela do plano; uma ART por plano; imagens em quantidade e ordem livres; anexos gravados em disco; procedimento com texto padrão único; numeração do plano reinicia a cada mês; padrão de arredondamento da carga bruta; unidades kgf (sapatas) e kgf/cm² (terreno); quadrante e observações em texto livre; cadastro de clientes com CNPJ (também criável na tela do plano); planos editáveis pela lista até serem finalizados (com confirmação, trava); planos mantêm os valores da época se um cadastro mudar; histórico de exportações; sistema não faz cálculos de engenharia.
- **27/10/2026 (2ª rodada):** conversões kg → t e contas simples (somas, taxa de utilização do guindaste) feitas só na exibição, sem gravar valores derivados; mês do número do plano vem da data do plano; removida a função de duplicar plano; adicionada lixeira com exclusão definitiva; dados da empresa e texto do procedimento embutidos no app; cadastro de riggers; cadastros em uso só são inativados; corrigida a grafia "Keffer" para "Kaefer" e a menção a vários guindastes por plano.
- **27/10/2026 (3ª rodada):** incluídas as Seções 11.4 (modelo de dados: tabelas, colunas, chaves, cardinalidades e regras) e 12.1 (fluxo de cadastro e vínculo ao plano, por caso).