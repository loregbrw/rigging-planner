# Jornada de Aprendizagem: Sistemas Inteligentes Conectados

## 1. Informações Gerais da Disciplina
- **Matéria:** Jornada de Aprendizagem
- **Tema do Semestre:** Sistemas Inteligentes Conectados
- **Equipe:** XHG
- **Integrantes:** Adrian Gobara Falci, Lorena Gobara Falci, Nilton Prudêncio Oliveira de Meira
- **Entregáveis Finais:**
  - Solução prática para o problema
  - Artigo acadêmico
  - Vídeo/pitch de apresentação

## 2. Empresa Parceira
- **Nome:** Guindastes Ribas Ltda.
- **Contato focal/padrinho:** Giuliano Breda (engenheiro responsável pela parte de engenharia da Guindastes Ribas; planejador de rigging)

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

O problema que a equipe precisa resolver é de **integração e centralização de informação**: hoje, um mesmo dado (ex.: capacidade de carga, número do plano, dimensões, peso) é digitado manualmente várias vezes, em documentos e formatos diferentes (Excel, Word, PDFs), sem nenhuma garantia de que os valores permaneçam consistentes entre eles.

Isso gera:
- **Retrabalho** (o engenheiro "garimpa" seus próprios arquivos para lembrar de onde tirou cada número);
- **Inconsistência de unidades** (quilograma, tonelada, quilograma-força usados de forma não padronizada);
- **Risco de erro humano** na transcrição dos dados entre os documentos — risco crítico, pois qualquer inversão de valores pode comprometer a segurança da operação de içamento em campo.

## 6. Composição do Plano de Rigging

### 6.1 Visão geral — ordem dos arquivos
*(Interpretação do usuário, a partir da análise direta dos arquivos enviados pelo engenheiro.)*

1. **Capacidade de carga** — `.xls`
2. **Documento principal do plano de rigging** — reaproveita dados já preenchidos na capacidade de carga e inclui imagens do SolidWorks
3. **ART do plano de rigging** — `.pdf`
4. **Procedimento operacional** — `.docx`
5. **Laudo técnico de segurança** — `.pdf`

> **Nota:** essa ordem de 5 documentos ainda não foi confirmada pelo engenheiro. Além disso, a reunião com ele (Seção 4) sugere uma estrutura com mais itens — RT e desenhos de amarração aparecem lá como partes separadas, mas não aparecem como arquivos distintos nesta lista de 5. Não está confirmado se eles estão embutidos em algum dos 5 documentos acima (ex.: dentro do "documento principal" ou da ART) ou se a lista está incompleta. Ver dúvida 6 na Seção 14.

### 6.2 Detalhamento de cada documento

**1. Capacidade de carga (.xls)**
- *Confirmado na reunião:* contém dados do guindaste (placa de fabricação, chassi, configuração, capacidade — ex.: 45 toneladas), comprimento de lança, raio de operação, capacidade de carga, carga líquida do equipamento, amarração, balancim, carga bruta, taxas de utilização (guindaste e cinta plana), número da RT, nome da empresa cliente e número do plano de rigging. Essas informações alimentam a primeira página do plano final.
- *Interpretação do usuário, a partir do arquivo preenchido:* os campos identificados são Cliente, Local, Data, Descrição da carga, Lingada/amarras, Dispositivos/balancim, Informações do guindaste, Configuração do guindaste, Composição da carga bruta, Quadrante operacional, Resumo dos cálculos, Observações/anexos (incluindo a ART).
- *Em aberto:* existe um template/base fixo? Quais campos são fixos, preenchidos manualmente ou calculados por fórmula? Há regras/comportamentos no Excel não visíveis num arquivo já preenchido? (dúvida 1)

**2. Documento principal do plano de rigging**
- *Confirmado na reunião:* reúne os desenhos técnicos do SolidWorks (posicionamento do caminhão/munck/guindaste em site, raio de operação, raio de isolamento, dimensões da carga — alimentadas por uma tabela em Excel que preenche automaticamente o SolidWorks —, dados do planejador de rigging/CREA, dados da empresa, carga bruta, capacidade de carga, taxa de utilização, esforço em sapata, velocidade do vento, número da ART e do plano de rigging) e os desenhos de amarração (cinta plana, manilha, balancim — ex.: balancim de 250 kg —, com especificações exatas a seguir em campo, ex.: cinta de 3 m e 6 toneladas com manilha de 7/8").
- *Em aberto:* existe como template editável (ex.: `.docx`)? É produzido de outra forma? Quais partes são fixas e quais variam por plano? (dúvida 2)

**3. ART (do plano de rigging)**
- *Confirmado na reunião:* emitida no portal do CREA; o plano de rigging só é válido com a RT recolhida, feita por um planejador de rigging.
- *Em aberto:* é sempre fornecida externamente em PDF, ou existe processo de geração/preenchimento que poderia ser incorporado ao sistema? (dúvida 3)

**4. Procedimento operacional (.docx)**
- *Confirmado na reunião:* traz número da RT e número do plano de rigging; conteúdo padrão sobre isolamento de área, NRs aplicáveis, espaço de circulação seguro, proibição de permanência sob carga, patolamento do guindaste — conteúdo que muda pouco de um plano para outro.
- *Interpretação do usuário, a partir do arquivo:* campos identificados são ART, Número do rigging, Nome do projeto (**incerto** — o próprio usuário não tem certeza se esse campo existe), Isolamento da área, Atolamento do guindaste, Posicionamento do veículo de carga, Içamento da carga, Descarga da carga.
- *Em aberto:* existe template padrão? Quais partes são fixas/variáveis? O engenheiro preenche campos num modelo existente ou escreve/edita boa parte do texto manualmente a cada plano? (dúvida 4)

**5. Laudo técnico de segurança**
- *Confirmado na reunião:* referente ao guindaste específico usado na operação (ex.: MD45.007), com uma ART própria — vinculada às condições do guindaste, distinta da ART do rigging — inserida como figura no documento.
- *Em aberto:* é sempre recebido/produzido em PDF? Existe template editável por trás? O engenheiro preenche apenas informações variáveis? Alguma parte poderia ser gerada automaticamente? (dúvida 5)

**Documento à parte — memorial de cálculo (não entra no plano final)**
- *Confirmado na reunião:* documento em Word que contém o desenho da carga içada, os ângulos, os pontos de fixação, a taxa de utilização e a interpolação de capacidade por raio de operação. Alimenta os outros documentos, mas deixou de ser incluído no plano final para evitar que a metodologia de cálculo seja copiada por terceiros.

## 7. Requisitos da Solução
*(Descritos pelo engenheiro na reunião.)*

Uma solução ideal deveria:

- Ter um **cadastro único de dados de entrada**: nome/identificação do guindaste (placa, chassi, capacidade), dimensões e peso da carga, dados dos acessórios de içamento (cinta, manilha, balancim — comprimento, capacidade, peso), dados do planejador de rigging (nome, CREA) e da empresa cliente/CNPJ.
- **Reaproveitar automaticamente** esses dados no documento final, preenchendo as seções que hoje são copiadas manualmente entre Excel, SolidWorks e Word (ex.: primeira página do plano, com a maior parte das informações).
- Permitir **anexar imagens/desenhos técnicos** (os desenhos gerados no SolidWorks) e os **documentos de ART** emitidos pelo CREA, integrando tudo num único documento final.
- **Manter o texto padrão** das seções que quase não mudam entre planos (procedimento de içamento, isolamento de área, NRs aplicáveis, patolamento do guindaste), alterando apenas o que é específico de cada operação (ex.: modelo do guindaste — MD45.007, MD55, DM130, MD300L etc.).
- **Zero tolerância a erro de dados**: em nenhuma hipótese o sistema pode alterar, inverter ou perder valores/informações entre etapas — restrição crítica já registrada para o projeto (Seção 12).

Referência mental citada pelo engenheiro: o conceito de uma "macro" que, a partir dos dados preenchidos, gera automaticamente o relatório final pronto — ou seja, **automação de preenchimento e formatação, não de cálculo**.

## 8. Fora de Escopo

- Um motor de cálculo estrutural/de engenharia que substitua o memorial de cálculo — os cálculos continuam sendo feitos pelo engenheiro. O sistema deve receber os resultados já calculados e apenas organizá-los/formatá-los corretamente no documento final.
- Um aplicativo interativo de cálculo de capacidade em tempo real (conforme o raio de operação é ajustado) — mencionado pelo engenheiro como projeto de **outra equipe de Jornada de Aprendizagem** (aparentemente liderada por alguém chamado Elton), não faz parte do escopo desta equipe.

## 9. Decisões e Preferências Técnicas
- **Stack escolhida:** frontend em **React**, backend em **.NET** — decisão do usuário, justificada pelo maior domínio pessoal dessas tecnologias.

## 10. Proposta de Solução
*(Hipótese/ideia inicial do usuário; ainda não validada com o engenheiro nem fechada como decisão de arquitetura.)*

- Sistema pensado como um **editor de documentos estruturado com limitações de edição** — não um editor genérico de Word/Excel, mas específico para planos de rigging, seguindo o(s) template(s)/processo que a empresa já usa.
- A etapa hoje feita em Excel ("Capacidade de carga") viraria uma **interface tipo planilha dentro do sistema**, com os mesmos tipos de campos que o engenheiro já preenche hoje.
- Dados preenchidos uma vez ficariam **estruturados e armazenados**, sendo **reaproveitados automaticamente** nos demais documentos do plano (evitando redigitação).
- Área para **anexar arquivos, imagens e ARTs**.
- Preenchimento das partes variáveis dos demais documentos, dependendo do que for descoberto sobre os templates (ver dúvidas da Seção 14).
- **Preview** do plano antes de finalizar.
- **Exportação** do resultado final para `.docx` e/ou `.pdf`.

## 11. Usuários, Permissões, Organização e Autenticação (Propostas)
*(Ideias iniciais do usuário, a serem avaliadas.)*

### 11.1 Usuários e Permissões
- **Engenheiro:** cria, visualiza e edita planos de rigging.
- **Visualizador:** visualiza os planos, sem necessariamente poder editá-los.
- **Administrador:** administra usuários, permissões e demais configurações do sistema.

### 11.2 Organização dos Planos
- Estrutura hierárquica proposta: **Categoria → Pasta → Plano de rigging**, com possibilidade de um plano ficar direto numa categoria ou pasta, sem usar os três níveis.
- Hipótese de **compartilhamento por link/código de visualização**, permitindo que um cliente acesse o conteúdo sem ter conta no sistema — possivelmente em três níveis: categoria, pasta ou plano individual.

### 11.3 Autenticação e Cadastro
- Administrador cadastra o e-mail de um novo usuário.
- Usuário recebe e-mail com link/token para finalizar cadastro e definir credenciais.

## 12. Restrições Críticas
- Em hipótese alguma podem ocorrer alterações de dados ou inversão de valores/informações, pois isso pode gerar acidentes graves em campo.

## 13. Glossário / Termos Técnicos

- **Plano de Rigging:** documento final que orienta a operação de içamento em campo.
- **RT:** Responsabilidade Técnica, registrada no CREA.
- **ART:** Anotação de Responsabilidade Técnica, emitida pelo CREA; existem ao menos duas por plano — uma do plano de rigging em si e outra das condições do guindaste.
- **CREA:** órgão onde a RT é registrada e a ART é emitida; o plano de rigging só é válido com a RT recolhida, e só pode ser feito por um planejador de rigging.
- **Balancim:** acessório de içamento (no exemplo citado, pesa 250 kg).
- **Taxa de utilização:** percentual da capacidade nominal (do guindaste ou de um acessório como a cinta plana) efetivamente utilizado numa operação.
- **Carga bruta:** carga líquida do equipamento somada à amarração e ao balancim.
- **Margem de segurança:** capacidade máxima de cálculo considerada é de 85% da capacidade nominal do guindaste.
- **Interpolação de capacidade:** cálculo de capacidade para um raio de operação que não consta diretamente na tabela do guindaste, interpolado entre os dois valores de raio mais próximos disponíveis.
- **Munck:** tipo de guindaste citado como exemplo de equipamento usado nas operações.

## 14. Dúvidas para o Engenheiro/Empresa

Perguntas que dependem de confirmação externa (não devem ser respondidas por suposição):

1. **Excel "Capacidade de carga":** existe um template/base padrão? Quais campos são fixos do template, quais são preenchidos manualmente pelo engenheiro, quais são calculados automaticamente por fórmulas, e quais variam plano a plano? Há fórmulas/regras/comportamentos no Excel que não são visíveis apenas olhando um arquivo já preenchido?
2. **Documento principal do plano de rigging:** existe como template editável (ex.: `.docx`) ou é produzido de outra forma? Quais partes são fixas e quais variam por plano?
3. **ART:** ela sempre é fornecida externamente em PDF (gerada no site do CREA), ou existe algum processo de geração/preenchimento que também poderia ser incorporado ao sistema?
4. **Procedimento operacional (.docx):** existe um template padrão? Quais partes são fixas e quais variáveis? O engenheiro apenas preenche campos específicos num modelo existente, ou escreve/edita boa parte do texto manualmente a cada plano? O campo "Nome do projeto" realmente existe nesse documento (ainda não confirmado)?
5. **Laudo técnico de segurança:** é sempre recebido/produzido em PDF? Existe um template editável por trás? O engenheiro preenche apenas informações variáveis? Alguma parte poderia ser gerada automaticamente pelo sistema?
6. **Relação entre a lista de 5 arquivos (Seção 6.1) e a estrutura de 7 itens da reunião (Seção 4):** RT e desenhos de amarração aparecem na reunião como itens à parte, mas não como arquivos distintos na lista de 5. Estão embutidos em algum dos 5 documentos, ou a lista de 5 está incompleta?
7. **Categorização dos planos:** o que o engenheiro considera "categoria" ao organizar seus arquivos — por cliente, por tipo de operação, por período? (Relevante para avaliar a proposta de organização da Seção 11.2.)

## 15. Cronograma / Sprints
*(aguardando informações)*

## 16. Notas Gerais / Pontos em Aberto do Time
- Formato exato da interface de cadastro mencionada pelo professor (web, desktop etc.) — a stack já foi decidida (Seção 9), mas os detalhes de UX/fluxo de telas ainda estão em aberto.