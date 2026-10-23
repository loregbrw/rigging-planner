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

O plano final pode ter de ~10 até 20+ páginas, dependendo da complexidade da operação (número de guindastes/muncks envolvidos). Cada plano é identificado por um **número de plano de rigging** e associado a uma empresa cliente (ex.: Keffer, Bosch, Volvo, Electrolux, Volkswagen, Lenzgear, Convai, Renault, Gerdau/"Gelopar", Polux, TechVent, TechChiller, Florestal, entre outras).

Os cálculos de engenharia (carga bruta, taxa de utilização do guindaste e dos acessórios de içamento, interpolação de capacidade por raio de operação, margem de segurança de 85% da capacidade nominal etc.) são feitos manualmente pelo engenheiro em uma planilha/memorial de cálculo, e os resultados são copiados manualmente para o Excel de dados, para o desenho no SolidWorks e para o corpo do plano em Word — **sem nenhuma automação ou validação cruzada** entre esses locais.

## 5. O Problema Central

O desafio **não é** criar um sistema que faça os cálculos de engenharia por conta própria — isso é reconhecido pelo próprio engenheiro como uma frente à parte, fora do escopo desta jornada.

O problema que a equipe precisa resolver é de **integração e centralização de informação**: hoje, um mesmo dado (ex.: capacidade de carga, número do plano, dimensões, peso) é digitado manualmente várias vezes, em documentos e formatos diferentes (Excel, SolidWorks, Word, PDFs de ART do CREA), sem nenhuma garantia de que os valores permaneçam consistentes entre eles.

Isso gera:
- **Retrabalho** (o engenheiro "garimpa" seus próprios arquivos para lembrar de onde tirou cada número);
- **Inconsistência de unidades** (quilograma, tonelada, quilograma-força usados de forma não padronizada);
- **Risco de erro humano** na transcrição dos dados entre os documentos — risco crítico, pois qualquer inversão de valores pode comprometer a segurança da operação de içamento em campo.

## 6. Composição do Plano de Rigging

### 6.1 Ordem das partes no PDF final
*(Confirmada com o plano de rigging final de exemplo: nº 2607036, cliente Kaefer, 24/07/2026 — 10 páginas.)*

| Páginas | Parte | Origem hoje | Como entra no sistema |
|---|---|---|---|
| 1 | **Capacidade de carga** (folha-resumo do plano) | Excel | Gerada pelo sistema a partir dos dados cadastrados |
| 2–4 | **Desenhos técnicos** (3 folhas A4 com carimbo) | SolidWorks | Imagens anexadas pelo usuário (PNG ou outro formato de imagem) |
| 5 | **ART do plano de rigging** | Portal do CREA | PDF anexado pelo usuário |
| 6–7 | **Procedimento operacional** (5 seções) | Word | Gerado pelo sistema (texto padrão + campos variáveis) |
| 8–10 | **Laudo técnico de segurança** + ART do laudo | Word + CREA | PDF anexado pelo usuário |

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
- *Composição da carga bruta (kg):* carga líquida, lingada/amarração, dispositivos/balancim, cargas extras, carga bruta.
- *Quadrante operacional:* inicial e final (ex.: LATERAL / LATERAL).
- *Resumo dos cálculos:* total da carga bruta (t), capacidade de carga do guindaste (t), taxas de utilização (guindaste, cinta plana, cinta tubular, laço de cabo de aço, manilha — %), esforço nas sapatas (kgf), resistência mínima do terreno (kgf/cm²), velocidade máxima do vento (m/s).
- *Observações / Anexos:* número da ART.
- *Assinaturas:* Rigger responsável (nome e registro RPP) e Supervisor Responsável.

**2. Desenhos técnicos (SolidWorks)**
Três folhas A4 no exemplo: (1) vistas lateral e superior com raio de operação e ângulo; (2) vistas com raio de isolamento; (3) desenho de amarração (cinta plana, manilha, balancim, gancho, com medidas e especificações a seguir em campo). Cada folha tem um **carimbo** (rodapé) que repete dados do plano: descrição e dimensões da carga, rigger/CREA, carga bruta, capacidade, taxa de utilização, esforço nas sapatas, velocidade do vento, ART, nº do rigging, cliente, operação, revisão e "folha X de Y". Esses dados já vêm "assados" na imagem (hoje preenchidos a partir de uma tabela Excel).

**3. ART do plano de rigging**
Emitida no portal do CREA-PR; o plano só é válido com a RT recolhida, feita por um planejador de rigging. A emissão **não** é automatizada: o sistema apenas recebe o PDF e o inclui no plano. O número da ART (ex.: 1720264496217) aparece na página 1, no carimbo dos desenhos e no cabeçalho do procedimento.

**4. Procedimento operacional**
Cinco seções fixas: 1. Isolamento da área, 2. Patolamento do guindaste (o título traz o modelo, ex.: MD45007), 3. Posicionamento do veículo de carga, 4. Içamento da carga, 5. Descarga da carga — cada uma com objetivo, passos e referências a NRs. O cabeçalho de cada página traz o nome da operação (ex.: "Içamento Chiller"), o número da ART e o número do rigging; o rodapé traz o endereço e o CNPJ da empresa. **Só mudam** o modelo do guindaste, o nome da operação e os dois números do cabeçalho.

**5. Laudo técnico de segurança**
Referente ao guindaste/caminhão específico (ex.: Palfinger MD 45.007 sobre Volvo/VM 270 6X2R). Traz dados da empresa, dados do equipamento, **validade do laudo** (início e fim), lista de trabalhadores autorizados, itens inspecionados e observações. Tem ART própria (distinta da ART do rigging), anexada ao final. Os dados do equipamento repetem os da página 1 do plano.

**Documento à parte — memorial de cálculo (não entra no plano final)**
Documento em Word com o desenho da carga içada, os ângulos, os pontos de fixação, a taxa de utilização e a interpolação de capacidade por raio de operação. Alimenta os outros documentos, mas deixou de ser incluído no plano final para evitar que a metodologia de cálculo seja copiada por terceiros.

### 6.3 Observações da análise do plano de exemplo
- **Formato do número do plano:** "2607036" sugere AAMMNNN (ano 26, mês 07, sequência 036). A confirmar (ver Seção 16).
- **Arredondamentos:** carga bruta = 3450 + 46,62 + 250 = 3746,62 kg, exibida como "3747" (kg) e "3,75" (t); taxa de utilização 47,1% (3746,62 ÷ 7950). As regras de arredondamento e de unidade precisam ser definidas e respeitadas em todo o documento.
- **Rótulo repetido:** na tabela de dispositivos/balancim, o total aparece com o rótulo "PESO TOTAL DA LINGADA / AMARRAÇÃO", igual ao da tabela de lingada. Manter fiel ou corrigir? (Seção 16)
- **Possível divergência de rótulo/unidade:** no carimbo dos desenhos, "esforço nas sapatas" aparece com valor em toneladas; na página 1, o mesmo valor parece estar ligado a "resistência mínima do terreno (kgf/cm²)". Conferir com o engenheiro (Seção 16).

## 7. Requisitos da Solução
*(Descritos pelo engenheiro na reunião.)*

Uma solução ideal deveria:

- Ter um **cadastro único de dados de entrada**: nome/identificação do guindaste (placa, chassi, capacidade), dimensões e peso da carga, dados dos acessórios de içamento (cinta, manilha, balancim — comprimento, capacidade, peso), dados do planejador de rigging (nome, CREA) e da empresa cliente/CNPJ.
- **Reaproveitar automaticamente** esses dados no documento final, preenchendo as seções que hoje são copiadas manualmente entre Excel, SolidWorks e Word (ex.: primeira página do plano, com a maior parte das informações).
- Permitir **anexar imagens/desenhos técnicos** (as folhas geradas no SolidWorks) e os **PDFs de ART e laudo** emitidos fora do sistema, integrando tudo num único PDF final.
- **Manter o texto padrão** das seções que quase não mudam entre planos (procedimento de içamento, isolamento de área, NRs aplicáveis, patolamento do guindaste), alterando apenas o que é específico de cada operação (ex.: modelo do guindaste — MD45.007, MD55, DM130, MD300L etc.).
- **Zero tolerância a erro de dados**: em nenhuma hipótese o sistema pode alterar, inverter ou perder valores/informações entre etapas — restrição crítica (Seção 14).

Referência mental citada pelo engenheiro: o conceito de uma "macro" que, a partir dos dados preenchidos, gera automaticamente o relatório final pronto — ou seja, **automação de preenchimento e formatação, não de cálculo**.

> **Nota:** as informações iniciais de cada trabalho (peso da carga, cliente, local) serão digitadas do zero a cada novo trabalho (Seção 9, resposta 6). O "cadastro único" vale para reaproveitar os dados **dentro de um mesmo plano**, e não como uma base de clientes reaproveitada automaticamente entre trabalhos.

## 8. Fora de Escopo (por enquanto)

- Um motor de cálculo estrutural/de engenharia que substitua o memorial de cálculo — os cálculos continuam sendo feitos pelo engenheiro.
- **Interpolação automática de capacidade** do guindaste por raio de operação — decisão da equipe: fora do escopo por enquanto (pode ser reavaliada com o engenheiro/empresa; ver Seção 16).
- Um aplicativo interativo de cálculo de capacidade em tempo real — projeto de **outra equipe de Jornada de Aprendizagem** (aparentemente liderada por alguém chamado Elton).
- **Backup do banco de dados** — será feito eventualmente (backup local), mas não é prioridade agora.
- **Exportação em `.docx`** — a saída é apenas PDF.
- Usuários, perfis de acesso, autenticação, fluxo de aprovação e compartilhamento em rede/internet — o sistema é local e de uso individual (Seção 11).

## 9. Respostas da Empresa — Levantamento de Requisitos
*(Respostas a um questionário enviado pela equipe. Itens marcados como "atualizado" foram alterados por decisões posteriores.)*

1. **Quem utilizará o sistema no dia a dia?** Apenas equipe interna. *(Atualizado: uma única pessoa, o engenheiro Giuliano; os Riggers recebem o plano pronto em PDF.)*
2. **Formato de acesso desejado:** sistema desktop, instalado diretamente nas máquinas da empresa. *(Confirmado: aplicação desktop local.)*
3. **Fluxo formal de aprovação por etapas?** Não — preenchimento direto e emissão simples.
4. **Emissão direta ou validação prévia obrigatória?** Pode ser emitido automaticamente.
5. **ART emitida via integração com o CREA-PR?** Não — o sistema apenas organiza os dados; a ART vem como PDF anexado.
6. **De onde virão as informações iniciais do serviço (peso da carga, cliente, local)?** Digitadas do zero a cada novo trabalho.
7. **Capacidade em raios de operação quebrados?** A empresa respondeu "automático", mas a equipe decidiu deixar a interpolação **fora do escopo por enquanto** (Seção 8).
8. **Cadastro de acessórios (cintas, manilhas e laços):** banco de dados com controle — cadastro fixo com histórico e alertas de desgaste/descarte.
9. **Numeração dos Planos de Rigging e ARTs:** a empresa respondeu "numeração sequencial automática". Como a ART é emitida no CREA e entra como PDF, o sistema **não gera número de ART**; a numeração automática vale para o plano (formato a confirmar — Seção 16).
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
- **App (UI):** Razor Components num WebView nativo (via .NET MAUI). Telas: lista de planos (com pastas), novo plano em 5 etapas (dados de entrada, capacidade de carga, anexos, preview, exportação) e cadastro de acessórios.
- **Domain:** cadastro único de dados do plano (guindaste, carga, acessórios, cliente, planejador/RT), numeração sequencial de planos, regras de consistência de dados.
- **Application:** casos de uso (criar/editar plano, gerenciar pastas, gerenciar acessórios, anexar arquivos, gerar PDF) e as interfaces que a infraestrutura implementa.
- **Infrastructure:** SQLite com EF Core (dados de entrada, cadastro de acessórios com histórico/alertas, planos, numeração, pastas), armazenamento dos anexos e geração/junção do PDF final.
- **Backup:** previsto para o futuro (cópia local do banco); fora do escopo agora.

## 12. Funcionalidades
> Pensadas como um **editor de documentos estruturado com limitações de edição**, específico para planos de rigging — não um editor genérico de Word/Excel.

- **Planos e pastas:** criar planos soltos ou dentro de pastas (um nível); abrir, editar, duplicar.
- **Dados de entrada:** cliente, local, data, CNPJ, guindaste (placa, chassi, capacidade), carga (descrição, dimensões, peso), acessórios, planejador (nome, CREA).
- **Capacidade de carga:** interface tipo planilha com os mesmos grupos de campos da folha-resumo (Seção 6.2), com os dados reaproveitados dos demais passos (sem redigitação).
- **Cadastro de acessórios:** banco com histórico, alertas de desgaste/descarte e controle de situação (regras detalhadas na feature).
- **Anexos:** imagens das folhas do SolidWorks (PNG ou outros formatos de imagem), PDF da ART e PDF do laudo técnico de segurança.
- **Preview** do plano completo antes de finalizar.
- **Exportação em PDF:** página-resumo e procedimento gerados pelo sistema, com os desenhos, a ART e o laudo inseridos **no meio** do documento, na ordem da Seção 6.1.
- **Numeração sequencial automática** dos planos.

## 13. Organização dos Planos
- Os planos ficam **soltos** ou dentro de **pastas**, com **no máximo um nível** de pastas (sem categorias).
- Onde o PDF exportado é gravado no disco: a confirmar (Seção 16).

## 14. Restrições Críticas
- Em hipótese alguma podem ocorrer alterações de dados ou inversão de valores/informações, pois isso pode gerar acidentes graves em campo.

## 15. Glossário / Termos Técnicos

- **Plano de Rigging:** documento final que orienta a operação de içamento em campo.
- **RT:** Responsabilidade Técnica, registrada no CREA.
- **ART:** Anotação de Responsabilidade Técnica, emitida pelo CREA; há ao menos duas por plano — a do plano de rigging e a do laudo das condições do guindaste.
- **CREA:** órgão onde a RT é registrada e a ART é emitida.
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

1. **Número do plano:** o formato é AAMMNNN (ex.: 2607036)? A sequência reinicia a cada mês ou é contínua?
2. **Laudo e ART do laudo:** chegam como **um único PDF** (como no exemplo, 3 páginas) ou como dois arquivos?
3. **Valores calculados:** carga bruta, totais da lingada e taxas de utilização são **digitados** pelo engenheiro (vindos do memorial) ou **calculados pelo sistema**? Quais as regras de arredondamento e de unidade (kg/t)?
4. **Rótulo repetido** no total de dispositivos/balancim (Seção 6.3): manter fiel ao original ou corrigir?
5. **Esforço nas sapatas × resistência mínima do terreno:** conferir rótulos e unidades entre a página 1 e o carimbo dos desenhos.
6. **Carimbo dos desenhos:** como as folhas do SolidWorks já trazem dados "assados", o sistema deve mostrar esses dados ao lado da imagem para conferência visual?
7. **Procedimento:** o texto padrão e as referências a NRs podem ser fixos no sistema, mudando apenas modelo do guindaste, nome da operação e números do cabeçalho?
8. **Campos do rodapé da página 1:** como são preenchidos "Rigger (nome e registro RPP)" e "Supervisor Responsável"?
9. **Validade do laudo:** o sistema deve alertar laudo vencido?
10. **Onde o PDF exportado é gravado** (pasta escolhida pelo usuário a cada exportação, pasta padrão?).
11. **Templates originais** (Excel, Word) continuam indisponíveis; margens e formatação exatas do PDF seguem o plano de exemplo.
12. **Interpolação de capacidade** (reavaliar depois) e **backup local** (definir destino quando for implementado).

## 17. Cronograma / Sprints
- **Metodologia:** Scrum, com planejamento acompanhado via GitHub Projects.
- **Sprints:** ciclos de duas semanas.
- **Prazo final do projeto:** 20/11/2026.
- **Status atual:** arquitetura definida (Seção 11); protótipos de tela feitos como inspiração; desenvolvimento iniciado — esqueleto do repositório (commit 1). Etapas realizadas e próximas: `docs/progress.md`.

## 18. Notas Gerais / Pontos em Aberto do Time
- Os detalhes de UX/fluxo de telas seguem em aberto: os protótipos são só inspiração e podem ser alterados.
- **Restrições enfrentadas:** (tempo) a resposta da empresa ao levantamento de requisitos chegou só no fim da semana anterior a 21/09/2026; (conhecimento técnico) dificuldade em entender o que já existe na documentação/processo atual da empresa e falta dos templates originais.

## 19. Histórico de Alterações deste Documento
- **20/10/2026:** removidos usuários, perfis, autenticação e compartilhamento; sistema passa a ser desktop local de uso individual; stack fixada (.NET 10, MAUI Blazor Hybrid Windows, EF Core + SQLite); interpolação e backup fora do escopo; saída só em PDF; organização por pastas de um nível; estrutura do PDF final confirmada com plano de exemplo; corrigida a codificação de caracteres do arquivo.
