# Progresso do desenvolvimento

> Atualizado a cada commit. Regras e escopo: `docs/project-brief.md`.

## Etapas realizadas

### Atualização — Procedimento operacional: geração e snapshot
- [x] Texto padrão do procedimento estruturado em `OperationalProcedureService`, com 5 seções fixas divididas em 2 páginas e cabeçalho/rodapé por página (`docs/procedure-spec.md`).
- [x] Campos variáveis preenchidos a partir do plano (`operation_name`, `art_number`, `plan_number`) e código do modelo derivado de `crane_model`; a marca (`crane_brand`) é removida quando aparece junto do modelo; valores ausentes exibidos como `-`.
- [x] Snapshot do procedimento criado na mesma transação da finalização do plano (`PlanProcedureSnapshots`), com código do modelo, conteúdo e hash SHA-256.
- [x] `CreateSnapshot` e `ResolveContent` prontos para reutilização pelo preview e pela exportação (plano finalizado sempre usa o snapshot).
- [x] Testes de derivação do código do modelo, estrutura das seções, valores ausentes, snapshot e persistência do snapshot.
- [ ] Preview e exportação do procedimento continuam dependentes das etapas 4 e 5 do wizard.

### Atualização — Definição documental do Procedimento Operacional (somente documentação, sem código)
- [x] Definido que o procedimento operacional **não tem cadastro**: texto padrão único embutido no app, sem edição e sem versões (`project-brief.md` §6.4).
- [x] Campos variáveis mapeados para dados que já existem no plano (`operation_name`, `art_number`, `plan_number`) e regra do código do modelo derivado de `crane_model`.
- [x] Definido snapshot do procedimento na finalização (`plan_procedure_snapshots`) e regra de imutabilidade do plano finalizado também fora da UI.
- [x] Definido histórico de alterações do plano (`plan_change_log`, somente inclusão) (`project-brief.md` §6.5).
- [x] Criado `docs/procedure-spec.md` com o texto de referência do procedimento e `agent-guide.md` §4.7 com as regras para agentes.
- [x] Implementação inicial concluída: entidades, migration, geração do procedimento, snapshot na finalização e testes (ver seção acima).

### Atualização — Acessórios dinâmicos por plano, edição inline e tabelas cinza
- [x] Remoção do catálogo global de acessórios e da tela de administração de acessórios; lingadas/amarras e dispositivos/balancins passaram a ser dinâmicos por plano.
- [x] Suporte completo a adição, edição inline e remoção de itens diretamente na tela de capacidade de carga (`Capacity.razor`).
- [x] Tabelas na página de cadastro de plano estilizadas com fundo cinza (`var(--rp-surface-subtle)`), conferindo aparência integrada e neutra.
- [x] Consolidação das seções da tela de capacidade de carga em um único formulário unificado intitulado **"Capacidade de carga"**, padronizado com 4 subseções: "Configurações do guindaste", "Composição de carga bruta", "Lingadas / Amarras" e "Dispositivos / Balancim".
- [x] Redesenho completo do gerenciamento de lingadas/amarras e dispositivos/balancins na página de capacidade de carga (`Capacity.razor`), eliminando formulários estáticos separados e adotando um fluxo 100% integrado por tabela com edição inline (linhas editáveis diretamente na tabela) e chips de modelos rápidos (presets).

### Refatoração visual — Design system e componentes
- [x] Tokens, reset, base, layout e componentes CSS separados em camadas `rp.*`
- [x] Shell com sidebar azul-marinho, topbar glass sutil e conteúdo rolável
- [x] Componentes Razor reutilizáveis para cards, botões, cabeçalhos e stepper
- [x] Remoção do CSS legado clay, Bootstrap e fonte externa

### Commit 1 — Esqueleto do repositório
- [x] Solution `RiggingPlanner.slnx` com projetos `Domain`, `Application`, `Infrastructure`, `App` e `Domain.Tests`
- [x] Referências entre projetos (arquitetura limpa simples)
- [x] App Blazor Hybrid (.NET MAUI) apenas para Windows
- [x] Layout Claymorphism (azul) com menu lateral
- [x] Páginas vazias: Planos, Novo plano (indicador das 5 etapas) e Acessórios
- [x] Roteamento (`Routes.razor`) e imports globais (`_Imports.razor`)
- [x] Textos do shell da aplicação em português (`index.html`) e título da janela "Rigging Planner"
- [x] Template MAUI limpo (Bootstrap e CSS escopado do template removidos; só CSS próprio)
- [x] `README.md`, `.gitignore`, `.editorconfig`, `global.json`
- [x] CI no GitHub Actions (build + testes)
- [x] `docs/project-brief.md` atualizado (desktop local, sem usuários/autenticação)

### Commit 2 — Banco de dados (EF Core + SQLite)
- [x] Pacotes `Microsoft.EntityFrameworkCore.Sqlite` e `.Design` na `Infrastructure`
- [x] `AppDbContext` (carrega as configurações de entidade da assembly) e fábrica de design-time
- [x] `AddInfrastructure(databasePath)`: registra `IDbContextFactory<AppDbContext>` (um contexto curto por operação)
- [x] Banco em `%LOCALAPPDATA%\RiggingPlanner\rigging-planner.db`, criado no primeiro uso
- [x] Migrations aplicadas automaticamente ao abrir o app (`ApplyMigrations`)
- [x] Migration inicial `InitialCreate` (estrutura inicial criada via EF)
- [x] Ferramenta `dotnet-ef` como tool local do repositório (`.config/dotnet-tools.json`)
- [x] Projeto `RiggingPlanner.Infrastructure.Tests` com teste que aplica as migrations em um banco temporário
- [x] CI passa a rodar os testes da solution inteira

### Commit 3 — Modelo persistente inicial
- [x] Entidades EF para clientes, riggers, equipamentos, laudos, acessórios e histórico
- [x] Entidades para arquivos, pastas, numeração mensal, planos, itens, desenhos e exportações
- [x] Relacionamentos, índices únicos e regras de exclusão mapeados conforme o modelo da seção 11.4 do brief
- [x] Campos copiados para o plano mantidos separados das referências aos cadastros
- [x] Migration `InitialCreate` regenerada com as tabelas e índices do modelo inicial
- [x] Teste de schema, migration e persistência de plano com cliente relacionado

### Etapa 3 — Planos, cadastro de acessórios e navegação
- [x] Tela de planos conectada ao SQLite via `IDbContextFactory`
- [x] Criação de planos com validação, numeração automática AAMMNNN e cliente reutilizável
- [x] Abertura e edição dos dados principais de um plano existente
- [x] Tela de acessórios com cadastro, listagem e ativação/inativação persistidas
- [x] Menu lateral organizado por seções de operação e cadastros
- [x] Estados vazios, tabelas, badges, validações e mensagens de sucesso/erro adicionados à UI

### Etapas 4 e 6 — Dados de entrada e capacidade de carga
- [x] Cadastro persistente de equipamentos com modelo, caminhão, chassi, placas, ano e ARTs opcionais com validade e histórico
- [x] Editor específico de ARTs dentro do formulário do equipamento, com adição múltipla, edição sem reenviar o PDF existente, datas obrigatórias, exclusão protegida e histórico visual de validade
- [x] Cadastro persistente de responsáveis (riggers), com nome completo, nome de exibição, RPP e CREA
- [x] Seleção de equipamento e responsável no plano, copiando os dados para manter o plano autossuficiente
- [x] Tela de capacidade de carga vinculada ao plano
- [x] Inclusão e remoção de itens de lingada e dispositivos, usando acessórios cadastrados ou descrição avulsa
- [x] Persistência dos itens em `plan_accessories` e dos parâmetros digitados da configuração do guindaste
- [x] Exibição dos totais de lingada, dispositivos, carga bruta em kg/t e taxa de utilização do guindaste
- [x] Tags de acessórios normalizadas em tabela própria, com autocomplete, chips coloridos, uma tag por acessório e administração
- [x] Ciclo de vida de cadastros: ativos separados de inativos, reativação e exclusão definitiva protegida por vínculos
- [x] Finalização de planos com bloqueio de edição e snapshot histórico do responsável, incluindo CREA
- [x] Cadastro independente de clientes, com nome, CNPJ, ativação/inativação e exclusão protegida
- [x] Cliente no plano por autocomplete: cadastro existente preenche e bloqueia o CNPJ; cliente novo exige nome e CNPJ no próprio formulário
- [x] CNPJ obrigatório com máscara `00.000.000/0000-00` no cadastro e no plano
- [x] Cliente no plano usa seletor visual semelhante ao seletor de tags
- [x] Cliente no plano usa o mesmo comportamento de teclado do seletor de tags (setas, Enter, criação e remoção)
- [x] Responsável no plano usa seção própria com autocomplete, seleção de cadastro existente, criação rápida e remoção
- [x] Novo responsável no plano exige nome completo, nome de exibição, RPP e CREA; cadastros existentes preenchem esses dados
- [x] Máscara de CNPJ aceita formatos legado numérico e novo alfanumérico
- [x] Navegação entre etapas sem salvamento intermediário
- [x] Estado de edição compartilhado entre dados de entrada e capacidade
- [x] Ações de salvar e descartar disponíveis no cabeçalho de todas as etapas
- [x] Validação obrigatória executada somente ao salvar o plano completo
- [x] Preview e exportação acessíveis pela navegação das etapas, com título global do plano
- [x] Lista de planos simplificada para a ação única de abrir
- [x] Seção independente de dados da carga no plano, com descrição, dimensões e peso
- [x] Primeira etapa do plano organizada em seções de identificação, cliente, carga, responsável e observações
- [x] Remoção da seção "Referências do documento" (ART do plano e supervisor responsável retirados do formulário)
- [x] Seção "Informações do Guindaste" transferida para a subpágina "Capacidade", centralizando o seletor tipo tag, caminhão, chassi, placa, ano e capacidade nominal com opção de criação rápida
- [x] Segunda etapa reorganizada em blocos visuais separados para informações do guindaste, configuração, resultados e composição da carga
- [x] Cargas extras editáveis e composição da carga bruta calculada na exibição
- [x] Reidratação do formulário principal ao salvar alterações feitas diretamente na etapa de capacidade
- [x] Sincronização dos dados recém-preenchidos na primeira etapa ao abrir a capacidade, sem salvamento intermediário no banco
- [x] Padronização de nomenclatura na interface de Rigger para Responsável (menu lateral, tela de cadastro e seções do plano)
- [x] Otimizações de build e watch do .NET (`Directory.Build.props`, exclusão de docs/artefatos, aceleração de compilação incremental e aplicação de migrations condicionada a pendências)
- [x] Confirmação ao tentar sair da página de novo plano ou edição sem salvar (`RpPlanLeaveGuard` com `NavigationLock` e modal de confirmação)
- [x] Título dinâmico nas etapas do plano exibindo o número e a descrição da operação para planos existentes (ex.: `Plano 2611001 — Içamento Chiller`) em vez de "Novo plano"

## Estado atual

### Implementado na aplicação
- [x] Lista de planos não excluídos, criação e edição de planos em rascunho
- [x] Numeração automática no formato AAMMNNN, reiniciada por mês da data do plano
- [x] Cadastro e seleção de equipamentos ativos, com cópia dos dados para o plano
- [x] Cadastro e seleção de responsáveis ativos, com cópia de nome, RPP e CREA para o plano
- [x] Cadastro rápido de responsável durante o preenchimento do plano, com snapshot dos dados no plano
- [x] Itens de lingada/dispositivo dinâmicos por plano (`PlanAccessory`), com adição, edição inline e remoção na etapa de Capacidade
- [x] Tela de capacidade com seleção/criação de guindaste, configuração, composição de carga e itens de lingada/dispositivo, quantidade, peso unitário e seções de lingada/dispositivo
- [x] Totais de itens, carga bruta, conversão kg/t e taxa de utilização calculados na exibição
- [x] Campos decimais com entrada pt-BR por meio de `RpDecimalInput`
- [x] Separação visual de ativos/inativos, reativação e exclusão definitiva protegida para clientes, equipamentos e responsáveis
- [x] Confirmações de exclusão posicionadas junto ao registro selecionado, inclusive no administrador de tags
- [x] Confirmação ao tentar sair da página de novo plano ou edição com alterações não salvas (`RpPlanLeaveGuard`)
- [x] Título da página de plano com número e descrição para planos existentes (ex.: `Plano 2611001 — Içamento Chiller`)
- [x] Anexos do plano: upload, substituição e exclusão do PDF da ART do plano, e upload, galeria, visualização em tela cheia (lightbox) e exclusão de desenhos técnicos do SolidWorks (imagens); arquivos ficam em staging e são persistidos no "Salvar"
- [x] Procedimento operacional gerado a partir do plano; componente de visualização pronto para o Preview; snapshots com hash SHA-256 prontos (a criação na transação de finalização ainda não está integrada ao fluxo)

### Modelado, mas ainda sem fluxo completo na interface
### Finalização e preview/exportação (ainda não implementados):
- [ ] Ação de finalização (`Status = "finalized"`, `FinalizedAt`) e congelamento do snapshot na finalização.
- [ ] Etapa 4 — Preview: visualizador das páginas do plano em A4 (hoje é placeholder).
- [ ] Etapa 5 — Exportação: geração do PDF final e histórico (`PlanExport`).

### Modelado, mas ainda sem fluxo completo na interface:
- [ ] Pastas de um nível
- [ ] Seleção de laudo de equipamento por validade no plano
- [ ] Histórico de exportações
- [ ] Lixeira e exclusão definitiva de planos
- [ ] Histórico de alterações do plano (`plan_change_log`)

### Próximas etapas

| # | Etapa | Situação atual |
|---|---|---|
| 1 | Cadastro independente de clientes | Implementado: tela de clientes, seleção no plano e cadastro rápido durante o preenchimento |
| 2 | Pastas e organização | Entidade prevista; sem tela ou operações |
| 3 | Laudos e anexos | ART do plano, desenhos do SolidWorks e ART do equipamento implementados; seleção de laudo no plano continua pendente |
| 4 | Preview | Ainda não implementado |
| 5 | Exportação em PDF | Ainda não implementada |
| 6 | Lixeira de planos | Ainda não implementada |
| 7 | Instalador | Ainda não validado/entregue |
| 8 | Procedimento operacional, snapshot na finalização, imutabilidade e histórico de alterações | Geração e componente de visualização prontos para o Preview; finalização com snapshot, preview/exportação, imutabilidade fora da UI e histórico pendentes |

O histórico de eventos e alertas de desgaste/descarte de itens de acesso permanecem fora da implementação atual; a interpolação de capacidade e o backup local continuam fora do escopo definido.

## Fora do escopo por enquanto
Interpolação de capacidade, backup do banco, exportação em DOCX.