# Progresso do desenvolvimento

> Atualizado a cada commit. Regras e escopo: `docs/project-brief.md`.

## Etapas realizadas

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
- [x] Cadastro persistente de equipamentos (caminhão + guindaste), com ativação/inativação
- [x] Cadastro persistente de riggers, com nome completo, nome de exibição, RPP e CREA
- [x] Seleção de equipamento e rigger no plano, copiando os dados para manter o plano autossuficiente
- [x] Tela de capacidade de carga vinculada ao plano
- [x] Inclusão e remoção de itens de lingada e dispositivos, usando acessórios cadastrados ou descrição avulsa
- [x] Persistência dos itens em `plan_accessories` e dos parâmetros digitados da configuração do guindaste
- [x] Exibição dos totais de lingada, dispositivos, carga bruta em kg/t e taxa de utilização do guindaste
- [x] Tags de acessórios normalizadas em tabela própria, com autocomplete, chips coloridos, uma tag por acessório e administração
- [x] Ciclo de vida de cadastros: ativos separados de inativos, reativação e exclusão definitiva protegida por vínculos
- [x] Finalização de planos com bloqueio de edição e snapshot histórico do rigger, incluindo CREA

## Estado atual

### Implementado na aplicação
- [x] Lista de planos não excluídos, criação e edição de planos em rascunho
- [x] Numeração automática no formato AAMMNNN, reiniciada por mês da data do plano
- [x] Cadastro e seleção de equipamentos ativos, com cópia dos dados para o plano
- [x] Cadastro e seleção de riggers ativos, com cópia de nome, RPP e CREA para o plano
- [x] Cadastro de acessórios com nome, peso unitário e uma tag opcional
- [x] Tags normalizadas, autocomplete com criação, seleção por teclado, chips coloridos e administração
- [x] Pesquisa automática de acessórios por nome ou tag e filtro por tag
- [x] Tela de capacidade com acessórios cadastrados ou itens avulsos, quantidade, peso unitário e seções de lingada/dispositivo
- [x] Totais de itens, carga bruta, conversão kg/t e taxa de utilização calculados na exibição
- [x] Campos decimais com entrada pt-BR por meio de `RpDecimalInput`
- [x] Finalização de plano com confirmação e bloqueio de edição
- [x] Separação visual de ativos/inativos, reativação e exclusão definitiva protegida para riggers, equipamentos e acessórios
- [x] Confirmações de exclusão posicionadas junto ao registro selecionado, inclusive no administrador de tags

### Modelado, mas ainda sem fluxo completo na interface
- [ ] Clientes como cadastro independente
- [ ] Pastas de um nível
- [ ] Laudos de equipamento e seus arquivos
- [ ] Arquivos armazenados, desenhos anexados e ART do plano
- [ ] Histórico de exportações
- [ ] Lixeira e exclusão definitiva de planos

### Próximas etapas

| # | Etapa | Situação atual |
|---|---|---|
| 1 | Cadastro independente de clientes | A entidade existe; a UI atual digita cliente e CNPJ diretamente no plano |
| 2 | Pastas e organização | Entidade prevista; sem tela ou operações |
| 3 | Laudos e anexos | Entidades previstas; sem cadastro, seleção ou armazenamento operacional |
| 4 | Preview | Ainda não implementado |
| 5 | Exportação em PDF | Ainda não implementada |
| 6 | Lixeira de planos | Ainda não implementada |
| 7 | Instalador | Ainda não validado/entregue |

O histórico de eventos, alertas de desgaste de acessórios e interpolação de capacidade permanecem fora da implementação atual; interpolação e backup local continuam fora do escopo definido.

## Fora do escopo por enquanto
Interpolação de capacidade, backup do banco, exportação em DOCX.
