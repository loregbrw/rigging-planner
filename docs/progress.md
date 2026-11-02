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

## Próximas etapas (previstas — ordem a confirmar)

| # | Etapa | Observação |
|---|---|---|
| 3 | Planos e pastas | Lista de planos, pastas de um nível, criar/abrir/duplicar |
| 4 | Dados de entrada | Cliente, guindaste, carga, planejador |
| 5 | Cadastro de acessórios | Histórico, alertas de desgaste/descarte |
| 6 | Capacidade de carga | Interface tipo planilha, reaproveitando os dados |
| 7 | Anexos | Imagens do SolidWorks, PDF da ART, PDF do laudo |
| 8 | Preview | Visualização do plano antes de finalizar |
| 9 | Exportação em PDF | Gerar páginas + juntar desenhos, ART e laudo |
| 10 | Instalador | App instalável, pronto para abrir |

## Fora do escopo por enquanto
Interpolação de capacidade, backup do banco, exportação em DOCX.
