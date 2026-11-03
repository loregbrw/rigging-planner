# Rigging Planner — Diretrizes Visuais e Técnicas de CSS

> Design system proposto para o app desktop **Rigging Planner** (Blazor Hybrid / .NET MAUI, Windows), parceiro **Guindastes Ribas**.
> **Status:** base visual implementada no projeto. Os tokens, camadas CSS e componentes Razor descritos aqui são a referência para a evolução da interface; os módulos de negócio ainda serão construídos sobre essa base.

---

## 0. Como ler este documento

Cada afirmação importante está classificada:

| Marca | Significado |
|---|---|
| **[FATO]** | Está no brief do projeto, no pedido atual ou na análise anterior do site da empresa. |
| **[PREMISSA]** | Suposição que adotei para não bloquear o trabalho. Se for falsa, o item indicado muda. |
| **[DECISÃO]** | Escolha de design/técnica que eu proponho e justifico. Pode ser revertida. |
| **[PENDENTE]** | Depende de resposta da equipe ou do engenheiro. Está na seção 14. |

---

## 1. Fatos, premissas e lacunas

### 1.1 Fatos que sustentam o design

| # | Fato | Origem |
|---|---|---|
| F1 | App desktop **Windows**, instalável, uso individual (um único usuário), sem autenticação, sem rede. | Brief §9.1, §10, §11 |
| F2 | **Blazor Hybrid (.NET MAUI)**, **.NET 10**, UI 100% **pt-BR**. | Brief §10 |
| F3 | **CSS próprio, sem biblioteca de componentes.** | Brief §10 |
| F4 | Arquitetura `Domain / Application / Infrastructure / App`; as telas Razor ficam no projeto `App`. | Brief §10–11 |
| F5 | Telas previstas: lista de planos (com pastas de **um nível**), novo plano em **5 etapas** (dados de entrada, capacidade de carga, anexos, preview, exportação) e cadastro de acessórios. | Brief §11.3, §12 |
| F6 | "Capacidade de carga" é uma **interface tipo planilha**, com os mesmos grupos de campos da folha-resumo do plano. | Brief §12, §6.2 |
| F7 | Anexos: imagens (PNG ou outro formato) e PDFs (ART e laudo). A saída é **somente PDF**. | Brief §6.1, §8, §12 |
| F8 | Cadastro de acessórios tem **histórico** e **alertas de desgaste/descarte**. | Brief §9 (item 8), §12 |
| F9 | **Restrição crítica:** nenhum dado pode ser alterado, invertido ou perdido entre etapas. | Brief §14 |
| F10 | Os protótipos de tela são **só inspiração**; o brief prevalece. | Brief (topo), §18 |
| F11 | O brief (§10) pede estilo **Claymorphism com azul central**. | Brief §10 |
| F12 | O pedido atual pede **corporativo moderno com glassmorphism sutil**, cor primária `#003F68`, tema claro, referência na Guindastes Ribas. | Pedido atual |
| F13 | Identidade do site da empresa (análise anterior): fundo preto/branco/cinzas, logo azul-marinho, fonte extendida (Arian LT), botões com raio de 10px, cinza translúcido, sombras suaves, header translúcido. | Análise do site |
| F14 | Margem de segurança: capacidade de cálculo é **85%** da nominal; exemplo de taxa de utilização exibida: 47,1%. | Brief §4, §6.3, §15 |
| F15 | WebView2 (Chromium) é o motor de renderização de apps Blazor Hybrid no Windows. | Conhecimento do framework — **verifique a versão do runtime na máquina-alvo** |

### 1.2 Conflitos encontrados

| Conflito | Tratamento |
|---|---|
| **Claymorphism (brief §10) × glassmorphism (pedido atual).** | **[DECISÃO]** Seguir o pedido atual (glass sutil), mantendo do "clay" apenas os cantos generosos e as sombras macias. **Atualize o brief §10 e o histórico §19** para evitar que o documento "fonte da verdade" contradiga o código. |
| Pedido fala em "sistema de **gestão empresarial**", mas o brief descreve uma ferramenta de **planejamento de rigging de uso individual**. | **[PREMISSA]** Desenhei só para os módulos documentados (planos, pastas, acessórios, anexos, preview, exportação). Os componentes são genéricos e servem para módulos futuros, mas **não inventei telas** de financeiro, estoque ou usuários. |

### 1.3 Lacunas analisadas

Analisei o brief procurando informações que **mudariam significativamente** o design system. Nenhuma impede o trabalho; tratei cada uma como premissa e indiquei o impacto:

| Lacuna | Premissa adotada | Impacto se estiver errada |
|---|---|---|
| Resolução/escala do monitor e tamanho mínimo da janela | **[PREMISSA]** ≥ 1366×768 em escala 100–150%; janela redimensionável | Densidade das tabelas e largura da sidebar |
| Estado atual do CSS do repositório (esqueleto e commit 2 não foram vistos por mim) | **[PREMISSA]** Existe o CSS padrão do template MAUI Blazor (incl. Bootstrap), ainda não removido | Estratégia de migração (seção 3.4) |
| Fontes licenciadas da marca (Arian LT e fonte dos títulos dos cards) | **[PREMISSA]** Não há licença/arquivo disponível; uso fonte livre embutida no app | Identidade tipográfica |
| Tema escuro necessário? | **[PREMISSA]** Não: só tema claro (pedido atual) | Tokens precisam de uma segunda camada |
| Logo em versão branca/monocromática | **[PREMISSA]** Não existe | Marca sobre fundo escuro da sidebar |
| Estados/status de um plano (rascunho, finalizado…) | **[PREMISSA]** Não documentados; **não desenhei badges de status de plano** | Colunas da lista e filtros |
| Regras de alerta (desgaste, descarte, laudo vencido) | **[PREMISSA]** Sem regra definida; criei só os componentes visuais de alerta e badge | Cores/limiares |
| Como o PDF é gerado (biblioteca "a definir") | **[PREMISSA]** CSS **não** é usado para gerar o PDF | Se for HTML→PDF, é preciso um CSS de impressão (seção 14) |

> Se alguma das perguntas da seção 14.2 for respondida diferente, o ajuste é localizado: os tokens foram organizados para isso.

---

## 2. Direção visual

### 2.1 Princípios
1. **Dado primeiro.** Tabelas, formulários e planilha são o produto; o "vidro" fica nas molduras (navegação, barras, painéis flutuantes), nunca atrás de texto denso.
2. **Segurança visual.** Unidade sempre visível, números alinhados e sem truncar, valor digitado diferente de valor calculado (restrição F9).
3. **Corporativo moderno.** Azul `#003F68`, neutros azulados, bordas discretas, sombras tingidas de azul, cantos arredondados.
4. **Herança da Guindastes Ribas, modernizada.** Mantém: azul-marinho do logo, neutros, raio de 10px nos botões, sombra suave, header translúcido, divisor metálico (reinterpretado). Moderniza: tema claro, glass sutil, tipografia legível em dados.
5. **Consistência por tokens.** Nenhuma cor, espaçamento ou sombra "solta" em componentes.
6. **Simples.** Sem biblioteca, sem build de CSS, sem arquitetura extra (alinhado ao "sem overengineering" do brief).

### 2.2 Onde o glass entra (e onde não)

| Usar glass sutil | **Não** usar glass |
|---|---|
| Topbar sticky sobre o conteúdo que rola | Cabeçalho e corpo de **tabelas** |
| Barra de ações fixa do wizard (rodapé) | Células da **planilha** de capacidade de carga |
| Painel de resumo lateral (ex.: resumo dos cálculos) | Campos de formulário e mensagens de erro |
| Menus suspensos, popovers, toasts | Folhas A4 do preview |
| Backdrop de modal e a moldura do modal (opaco ≥ 90%) | Qualquer texto < 14px sobre fundo variável |

**[DECISÃO]** Limite de **no máximo 3 camadas com `backdrop-filter` visíveis ao mesmo tempo** (topbar + uma barra/painel + um overlay), por custo de renderização no WebView2. Validar com a máquina real (seção 13).

### 2.3 Sidebar
**[DECISÃO — pendente de aprovação]** Sidebar em **azul-marinho** (`#003F68` → `#00243B`) com leve brilho translúcido. Ela é o "âncora" de marca em um app de tema claro e faz referência ao fundo escuro do site da empresa. Alternativa: sidebar clara/glass (basta trocar 4 tokens `--rp-sidebar-*`).

---

## 3. Estratégia técnica

### 3.1 Opções avaliadas

| Opção | Avaliação para este projeto |
|---|---|
| **CSS puro global + CSS isolation + `@layer`** | ✅ **Recomendada.** Cumpre F3, zero dependências, funciona offline, sem build. |
| Bootstrap (vem no template MAUI Blazor) | ❌ Contraria F3; traz classes genéricas (`.btn`, `.form-control`, `.table`, `.modal`) que conflitam com nomes simples e **ganham da nossa cascata se ficarem sem `@layer`**. Remover. |
| Tailwind | ❌ Exige pipeline de build; peso desnecessário para app individual. |
| MudBlazor / Radzen / Fluent UI | ❌ Contraria F3; menos controle sobre o visual glass. |
| Sass/LESS | ❌ Custom properties + `@layer` cobrem a necessidade sem build. |

### 3.2 O que vai onde

| Camada | Conteúdo | Local |
|---|---|---|
| **Global** | tokens, reset, base, layout do shell, **todos os componentes `rp-*`** (botão, campo, tabela, card, modal…) | `wwwroot/css/` |
| **CSS isolation** (`*.razor.css`) | **somente layout específico de uma página/componente** (ex.: proporção das colunas da tela de planos, ordem dos blocos no preview) | ao lado do `.razor` |
| **Inline** (`style=`) | só valores dinâmicos via variável CSS (ex.: `style="--value: 47.1"`) | Razor |

Regras:
- **Tokens e primitivos nunca ficam em CSS isolation** (o isolamento adiciona atributo de escopo e impede reuso).
- CSS isolation **não estiliza elementos de componentes filhos**, a menos que se use `::deep`. Use `::deep` apenas em **layout** (largura, posição), nunca para recolorir primitivos.
- Estilos de isolation **não estão em `@layer`**, portanto vencem os estilos em camadas. Isso é útil para ajustes de página, mas exige **baixa especificidade** (um seletor de classe) e **sem `!important`**.
- Dependências externas: **nenhuma** (fontes, ícones e CSS ficam embutidos no app; CDN não funciona offline).

### 3.3 Particularidades do WebView2 / Blazor Hybrid
- **[FATO F15]** Chromium moderno: custom properties, `@layer`, `backdrop-filter`, `:has()`, `color-mix()`, `dvh` e container queries funcionam em versões atuais. **Confirme a versão do WebView2 Runtime** exigida pelo instalador antes de usar recursos recentes (ex.: nesting nativo de CSS).
- **Fontes embutidas:** `@font-face` apontando para `wwwroot/fonts/` (caminho relativo ao CSS). Nada de Google Fonts por URL.
- **Escala DPI do Windows:** use `rem` para tipografia/espaçamento e `px` só para bordas e sombras.
- **Bundle de CSS isolation:** o build gera `<NomeDoAssembly>.styles.css`, que precisa estar referenciado no `index.html` (o template costuma trazer essa linha — **verifique o nome real no seu projeto**).
- **Seleção de texto:** desativar em chrome de UI (botões, navegação) e **manter ativa em dados** (o engenheiro pode copiar números).
- Template MAUI Blazor pode incluir `#blazor-error-ui` e `.blazor-error-boundary`: estilizar (seção 8.15) ou remover.

### 3.4 Migração do template
1. Localizar e **remover** `wwwroot/css/bootstrap/` e o `app.css` do template (se existirem) e as referências no `index.html`.
2. Conferir `*.razor.css` herdados (ex.: `MainLayout.razor.css`, `NavMenu.razor.css`) e substituir.
3. Só então adicionar os arquivos da seção 9. Se algo do template ficar sem `@layer`, ele **vencerá** o design system.

### 3.5 Estado de edição do plano
- **[DECISÃO]** A navegação entre as cinco etapas não persiste alterações intermediárias.
- **[DECISÃO]** `PlanDraftState`, registrado como serviço scoped, mantém em memória os dados de entrada, os parâmetros de capacidade e os itens da planilha durante a edição.
- **[DECISÃO]** As ações globais de salvar e descartar ficam no cabeçalho de todas as etapas. Salvar retorna ao fluxo da primeira etapa, executa a validação obrigatória e persiste o plano completo em uma transação; descartar limpa o estado e solicita confirmação.
- **[RESTRIÇÃO]** Campos obrigatórios não bloqueiam a troca de etapa; são verificados somente no salvamento explícito.
- **[DECISÃO]** Os dados da carga pertencem exclusivamente ao plano em edição. A seção registra descrição, comprimento, largura, altura e peso, sem entidade ou seletor para reutilização entre planos.
- **[DECISÃO]** Equipamentos são cadastros reutilizáveis com modelo, caminhão, chassi, placas e ano de fabricação. O formulário inclui um editor específico de ARTs: o usuário monta uma ou várias relações com número obrigatório, datas opcionais e anexo PDF obrigatório, podendo editar ou remover itens antes de salvar o equipamento. Na edição, o anexo existente é preservado quando não houver substituição. ARTs persistidas permanecem no histórico e recebem status visual de válida ou vencida; sem data de término, a ART permanece válida. A exclusão é bloqueada quando uma ART está vinculada a um plano. No plano, o equipamento usa o mesmo seletor interativo de tags/clientes; ao selecionar um cadastro, os demais dados são exibidos automaticamente, e a criação rápida permite informar um novo equipamento sem sair do plano.

---

## 4. Tokens CSS

> Tokens ficam em `@layer rp.tokens`. Componentes usam **tokens semânticos** (`--rp-surface`, `--rp-border-default`, `--rp-accent`), não a paleta bruta.
> A rampa de azul e os neutros abaixo são **derivados por mim** a partir de `#003F68` (matiz ≈ 204°). Os contrastes citados foram **calculados manualmente (aproximados)**; **valide com uma ferramenta** antes de homologar.

### 4.1 `00-layers.css` (carregar primeiro)

```css
/* Ordem de prioridade: da menor para a maior */
@layer rp.reset, rp.tokens, rp.base, rp.layout, rp.components, rp.utilities;
```

### 4.2 `01-tokens.css`

```css
@layer rp.tokens {
  :root {
    color-scheme: light;

    /* ── Paleta bruta: primária (base #003F68 = 700) ───────────── */
    --rp-primary-50:  #EEF5FA;
    --rp-primary-100: #DBE9F3;
    --rp-primary-200: #B7D3E6;
    --rp-primary-300: #86B5D4;
    --rp-primary-400: #4A8DBD;
    --rp-primary-500: #1F6CA3;
    --rp-primary-600: #0A5588;
    --rp-primary-700: #003F68;   /* cor primária da aplicação */
    --rp-primary-800: #003252;
    --rp-primary-900: #00243B;
    --rp-primary-950: #001726;

    /* ── Paleta bruta: neutros azulados ────────────────────────── */
    --rp-neutral-0:   #FFFFFF;
    --rp-neutral-50:  #F6F8FA;
    --rp-neutral-100: #EEF1F5;
    --rp-neutral-200: #E1E6ED;
    --rp-neutral-300: #CBD3DD;
    --rp-neutral-400: #9AA6B5;
    --rp-neutral-500: #6B7888;
    --rp-neutral-600: #4F5B6B;
    --rp-neutral-700: #3A4452;
    --rp-neutral-800: #262E3A;
    --rp-neutral-900: #151B24;

    /* ── Paleta bruta: status ──────────────────────────────────── */
    --rp-success-50:  #E7F5EE; --rp-success-200: #A8DBC1; --rp-success-600: #1F8A5B; --rp-success-800: #0F5C3A;
    --rp-warning-50:  #FFF4DD; --rp-warning-200: #F2D08A; --rp-warning-600: #D98E04; --rp-warning-800: #7A4B00;
    --rp-danger-50:   #FDEBEC; --rp-danger-200:  #F2B1B6; --rp-danger-600:  #D13B47; --rp-danger-700: #B7303B; --rp-danger-800: #8F1D27;
    /* "info" reutiliza a rampa primária */

    /* ── Tokens semânticos (use estes nos componentes) ─────────── */
    --rp-accent:          var(--rp-primary-700);
    --rp-accent-hover:    var(--rp-primary-600);
    --rp-accent-active:   var(--rp-primary-800);
    --rp-on-accent:       #FFFFFF;          /* ≈ 11:1 sobre #003F68 (calc. manual) */

    --rp-surface:         var(--rp-neutral-0);
    --rp-surface-subtle:  var(--rp-neutral-50);
    --rp-surface-sunken:  var(--rp-neutral-100);

    --rp-text:            var(--rp-neutral-900);
    --rp-text-secondary:  var(--rp-neutral-600);  /* ≈ 6,9:1 sobre branco */
    --rp-text-muted:      #5F6C7D;                /* ≈ 5,2:1 sobre branco */
    --rp-text-disabled:   var(--rp-neutral-400);
    --rp-link:            var(--rp-primary-600);  /* ≈ 7,9:1 sobre branco */

    --rp-border-subtle:   var(--rp-neutral-200);
    --rp-border-default:  var(--rp-neutral-300);
    --rp-border-strong:   var(--rp-neutral-400);

    --rp-hover-bg:        var(--rp-primary-50);
    --rp-selected-bg:     var(--rp-primary-100);
    --rp-focus:           var(--rp-primary-500);
    --rp-overlay:         rgba(0, 23, 38, .45);

    /* Sidebar (troque estes 4 se optar pela sidebar clara) */
    --rp-sidebar-bg:      linear-gradient(180deg, rgba(0,63,104,.97), rgba(0,36,59,.98));
    --rp-sidebar-text:    rgba(255,255,255,.80);
    --rp-sidebar-hover:   rgba(255,255,255,.09);
    --rp-sidebar-active:  rgba(255,255,255,.16);

    /* ── Tipografia ────────────────────────────────────────────── */
    --rp-font-ui:    "Fredoka", "Segoe UI", system-ui, sans-serif;
    --rp-font-brand: "Fredoka", "Segoe UI", system-ui, sans-serif;
    --rp-font-mono:  "Cascadia Mono", Consolas, ui-monospace, monospace;

    --rp-text-xs:  .8125rem;   /* 13 */
    --rp-text-sm:  .875rem;     /* 14 – tabelas, rótulos */
    --rp-text-md:  1rem;        /* 16 – corpo padrão */
    --rp-text-lg:  1.125rem;    /* 18 */
    --rp-text-xl:  1.25rem;     /* 20 */
    --rp-text-2xl: 1.5rem;      /* 24 – título de página */
    --rp-text-3xl: 1.875rem;    /* 30 */

    --rp-weight-regular: 400;
    --rp-weight-medium:  500;
    --rp-weight-semibold:600;
    --rp-weight-bold:    700;

    --rp-lh-tight:  1.2;
    --rp-lh-snug:   1.35;
    --rp-lh-normal: 1.5;

    /* ── Espaçamento (base 4px, 1rem = 16px) ───────────────────── */
    --rp-space-1:  .25rem;   /*  4 */
    --rp-space-2:  .5rem;    /*  8 */
    --rp-space-3:  .75rem;   /* 12 */
    --rp-space-4:  1rem;     /* 16 */
    --rp-space-5:  1.25rem;  /* 20 */
    --rp-space-6:  1.5rem;   /* 24 */
    --rp-space-8:  2rem;     /* 32 */
    --rp-space-10: 2.5rem;   /* 40 */
    --rp-space-12: 3rem;     /* 48 */

    /* ── Raios ─────────────────────────────────────────────────── */
    --rp-radius-xs: 4px;
    --rp-radius-sm: 6px;
    --rp-radius-md: 10px;    /* botões/campos (igual ao raio do site da empresa) */
    --rp-radius-lg: 14px;    /* cards, tabelas */
    --rp-radius-xl: 20px;    /* modais */
    --rp-radius-pill: 999px;

    /* ── Bordas ────────────────────────────────────────────────── */
    --rp-border-width: 1px;

    /* ── Sombras (tingidas de azul-marinho) ────────────────────── */
    --rp-shadow-xs: 0 1px 2px rgba(0,36,59,.06);
    --rp-shadow-sm: 0 1px 3px rgba(0,36,59,.08), 0 1px 2px rgba(0,36,59,.04);
    --rp-shadow-md: 0 4px 12px -2px rgba(0,36,59,.10), 0 2px 4px rgba(0,36,59,.05);
    --rp-shadow-lg: 0 12px 28px -6px rgba(0,36,59,.16), 0 4px 8px rgba(0,36,59,.06);
    --rp-shadow-xl: 0 24px 56px -12px rgba(0,36,59,.24);
    --rp-shadow-inset: inset 0 1px 2px rgba(0,36,59,.15);
    --rp-ring: 0 0 0 3px rgba(31,108,163,.30);
    --rp-ring-danger: 0 0 0 3px rgba(209,59,71,.28);

    /* ── Glass ─────────────────────────────────────────────────── */
    --rp-glass-bg:        rgba(255,255,255,.68);
    --rp-glass-bg-strong: rgba(255,255,255,.88);
    --rp-glass-edge:      rgba(0,63,104,.10);
    --rp-glass-highlight: rgba(255,255,255,.65);
    --rp-glass-blur:      14px;
    --rp-glass-filter:    blur(var(--rp-glass-blur)) saturate(140%);

    /* ── Fundo do app (dá "o que desfocar" ao glass) ───────────── */
    --rp-bg-app:
      radial-gradient(1100px 600px at 8% -10%, rgba(74,141,189,.18), transparent 60%),
      radial-gradient(900px 500px at 100% 0%, rgba(0,63,104,.10), transparent 55%),
      var(--rp-neutral-50);

    /* ── Dimensões ─────────────────────────────────────────────── */
    --rp-sidebar-w: 264px;
    --rp-sidebar-w-collapsed: 72px;
    --rp-topbar-h: 56px;
    --rp-content-max: 1280px;          /* formulários; tabelas usam 100% */
    --rp-control-h-sm: 32px;
    --rp-control-h-md: 38px;
    --rp-control-h-lg: 44px;
    --rp-row-h: 40px;                  /* altura de linha de tabela (padrão) */
    --rp-icon-sm: 16px; --rp-icon-md: 20px; --rp-icon-lg: 24px;
    --rp-modal-sm: 440px; --rp-modal-md: 640px; --rp-modal-lg: 880px;
    --rp-drawer-w: 480px;

    /* ── Camadas (z-index) ─────────────────────────────────────── */
    --rp-z-sticky: 10;
    --rp-z-sidebar: 20;
    --rp-z-topbar: 30;
    --rp-z-dropdown: 40;
    --rp-z-overlay: 50;
    --rp-z-modal: 60;
    --rp-z-toast: 70;

    /* ── Movimento ─────────────────────────────────────────────── */
    --rp-dur-fast: 120ms;
    --rp-dur-base: 180ms;
    --rp-dur-slow: 280ms;
    --rp-ease: cubic-bezier(.2, 0, 0, 1);
  }

  /* Usuário reduziu transparência no Windows (quando suportado pelo motor) */
  @media (prefers-reduced-transparency: reduce) {
    :root {
      --rp-glass-bg: rgba(255,255,255,.97);
      --rp-glass-bg-strong: #fff;
      --rp-glass-filter: none;
    }
  }

  /* Sem suporte a backdrop-filter → fundo quase opaco */
  @supports not ((backdrop-filter: blur(1px)) or (-webkit-backdrop-filter: blur(1px))) {
    :root {
      --rp-glass-bg: rgba(255,255,255,.96);
      --rp-glass-bg-strong: #fff;
      --rp-glass-filter: none;
    }
  }

  @media (prefers-reduced-motion: reduce) {
    :root { --rp-dur-fast: 1ms; --rp-dur-base: 1ms; --rp-dur-slow: 1ms; }
  }
}
```

### 4.3 Tipografia — decisões
- **[DECISÃO]** UI e dados em **Inter** (licença OFL; **você precisa baixar e embutir o arquivo** em `wwwroot/fonts/`), com fallback **Segoe UI Variable/Segoe UI**, que já existem no Windows. Se a fonte não for embutida, o app ainda fica coerente com o sistema.
- **[DECISÃO]** A fonte extendida da marca (Arian LT / similares) fica **só** no logotipo e, opcionalmente, no título de página, via `--rp-font-brand`. Ela é larga demais para tabelas e campos. Se não houver licença, a substituta livre sugerida é **Michroma**.
- **[DECISÃO]** Dados numéricos sempre com `font-variant-numeric: tabular-nums`; código de plano/ART em `--rp-font-mono`.
- **[PENDENTE P3]** Aprovar Inter × outra fonte e confirmar licenciamento.

### 4.4 `fonts.css` (exemplo)

```css
/* Assumindo arquivos em wwwroot/fonts/ e este CSS em wwwroot/css/ */
@font-face {
  font-family: "Inter";
  src: url("../fonts/InterVariable.woff2") format("woff2");
  font-weight: 100 900;
  font-style: normal;
  font-display: swap;
}
```

---

## 5. Reset e base

### 5.1 `02-reset.css`
```css
@layer rp.reset {
  *, *::before, *::after { box-sizing: border-box; }
  html { -webkit-text-size-adjust: 100%; }
  body, h1, h2, h3, h4, p, figure, ul, ol, dl, dd { margin: 0; }
  ul[role="list"], ol[role="list"] { list-style: none; padding: 0; }
  img, svg, video, canvas { display: block; max-width: 100%; }
  button, input, select, textarea { font: inherit; color: inherit; }
  button { cursor: pointer; }
  table { border-collapse: collapse; }
  [hidden] { display: none !important; }
}
```

### 5.2 `03-base.css`
```css
@layer rp.base {
  html { font-size: 100%; color-scheme: light; }

  body {
    min-height: 100vh;
    background: var(--rp-bg-app);
    background-attachment: fixed;     /* custo de repaint: validar em scroll (seção 13) */
    color: var(--rp-text);
    font: var(--rp-weight-regular) var(--rp-text-md)/var(--rp-lh-normal) var(--rp-font-ui);
    -webkit-font-smoothing: antialiased;
    text-rendering: optimizeLegibility;
  }

  h1 { font-size: var(--rp-text-2xl); font-weight: var(--rp-weight-semibold); line-height: var(--rp-lh-tight); letter-spacing: -.01em; }
  h2 { font-size: var(--rp-text-xl);  font-weight: var(--rp-weight-semibold); line-height: var(--rp-lh-snug); }
  h3 { font-size: var(--rp-text-lg);  font-weight: var(--rp-weight-semibold); line-height: var(--rp-lh-snug); }
  h4 { font-size: var(--rp-text-md);  font-weight: var(--rp-weight-semibold); }

  a { color: var(--rp-link); text-decoration: none; }
  a:hover { text-decoration: underline; }

  ::selection { background: var(--rp-primary-200); color: var(--rp-primary-950); }

  /* Foco visível global (acessível também em alto contraste) */
  :focus-visible { outline: 2px solid var(--rp-focus); outline-offset: 2px; }

  /* Rolagem discreta */
  * { scrollbar-width: thin; scrollbar-color: var(--rp-neutral-400) transparent; }

  /* App desktop: UI não selecionável, dados selecionáveis */
  .rp-nav, .rp-btn, .rp-stepper, .rp-topbar { user-select: none; }
  .rp-table td, .rp-sheet td, .rp-doc-page { user-select: text; }

  /* Números */
  .rp-num { font-variant-numeric: tabular-nums; text-align: right; }
  .rp-code { font-family: var(--rp-font-mono); font-size: .92em; letter-spacing: .01em; }
}
```

---

## 6. Layout, navegação e organização das telas

### 6.1 Mapa de telas

| Tela | Layout | Componentes principais |
|---|---|---|
| **Planos** (lista + pastas) | `rp-split`: lista de pastas à esquerda (260px) + tabela à direita | `rp-folder-list`, `rp-toolbar`, `rp-table`, `rp-btn`, `rp-menu` |
| **Novo/Editar plano** (wizard 5 etapas) | Cabeçalho com `rp-stepper` + conteúdo + rodapé fixo glass | `rp-stepper`, `rp-card`, `rp-form-grid`, `rp-field`, `rp-sheet`, `rp-wizard__footer` |
| 1. Dados de entrada | Seções separadas de identificação, cliente, carga, equipamento, rigger e referências do documento em grade de 12 colunas | `rp-form-grid`, `rp-client-section`, `rp-tag-picker` |
| 2. Capacidade de carga | Seções separadas para informações/configuração do guindaste, resultados informados e composição da carga, com resumo calculado | `rp-capacity-section`, `rp-summary-grid`, `rp-table` |
| 3. Anexos | 3 zonas: desenhos (imagens), ART do plano (PDF), laudo (PDF) | `rp-dropzone`, `rp-attachment-list` |
| 4. Preview | Visualizador de folhas A4 com zoom | `rp-doc-preview`, `rp-doc-page` |
| 5. Exportação | Lista ordenada das partes do PDF + ação principal | `rp-parts`, `rp-alert`, `rp-progress` |
| **Cadastros** (clientes, equipamentos, riggers e acessórios) | Tabelas separadas por ativos/inativos, formulário de cadastro/edição e confirmação de exclusão protegida | `rp-table`, `rp-badge`, `rp-form-grid`, `rp-alert`, `rp-btn` |
| **Cliente no plano** | Mesmo seletor interativo de tags, com chip, setas, Enter, criação e remoção; cliente existente preenche o CNPJ bloqueado; nome e CNPJ ocupam colunas lado a lado | `rp-tag-picker`, `rp-autocomplete`, `rp-input`, `rp-help` |
| **Rigger no plano** | Seção própria com seletor interativo de tags, chip, setas, Enter, criação e remoção; rigger existente preenche nome completo, RPP e CREA bloqueados; novo rigger informa esses dados no próprio formulário | `rp-tag-picker`, `rp-autocomplete`, `rp-input`, `rp-help` |

### 6.2 Estrutura do shell
```
┌───────────┬───────────────────────────────────────────┐
│ Sidebar   │ Topbar (glass, sticky)                    │
│ (264px,   ├───────────────────────────────────────────┤
│ azul)     │ Conteúdo (rola) → page header + página    │
│           │                                           │
│           │ [Wizard: rodapé glass sticky]             │
└───────────┴───────────────────────────────────────────┘
```
- **A topbar e o rodapé do wizard rolam "por cima" do conteúdo** (por isso o glass tem o que desfocar).
- Navegação principal: **Planos**, **Novo plano** e os cadastros de **clientes, acessórios, equipamentos e riggers**. Itens como "Configurações" **não** estão no brief **[PENDENTE]**.
- **Breadcrumb** na topbar: `Planos / <Pasta> / Plano 2607036`.
- Tela de planos: ação primária **"Novo plano"** sempre no canto superior direito do `rp-page-header`.
- **Pastas de um único nível** (F5): lista simples, sem árvore recursiva.

### 6.3 `04-layout.css`

```css
@layer rp.layout {
  /* ── Shell ─────────────────────────────────────────────────── */
  .rp-shell {
    display: grid;
    grid-template-columns: var(--rp-sidebar-w) minmax(0, 1fr);
    height: 100vh;
    height: 100dvh;
    transition: grid-template-columns var(--rp-dur-base) var(--rp-ease);
  }
  .rp-shell--collapsed { --rp-sidebar-w: var(--rp-sidebar-w-collapsed); }

  .rp-sidebar {
    position: relative;
    z-index: var(--rp-z-sidebar);
    display: flex;
    flex-direction: column;
    gap: var(--rp-space-4);
    padding: var(--rp-space-4) var(--rp-space-3);
    background: var(--rp-sidebar-bg);
    color: var(--rp-sidebar-text);
    box-shadow: var(--rp-shadow-lg);
    overflow: hidden auto;
  }

  /* Área que rola; a topbar fica sticky dentro dela */
  .rp-content { position: relative; overflow: auto; }

  .rp-topbar {
    position: sticky; top: 0;
    z-index: var(--rp-z-topbar);
    display: flex; align-items: center; justify-content: space-between;
    gap: var(--rp-space-4);
    height: var(--rp-topbar-h);
    padding: 0 var(--rp-space-6);
    background: var(--rp-glass-bg);
    -webkit-backdrop-filter: var(--rp-glass-filter);
    backdrop-filter: var(--rp-glass-filter);
    border-bottom: var(--rp-border-width) solid var(--rp-glass-edge);
    box-shadow: var(--rp-shadow-xs);
  }

  .rp-main {
    width: 100%;
    max-width: var(--rp-content-max);
    margin-inline: auto;
    padding: var(--rp-space-6);
  }
  .rp-main--full { max-width: none; }          /* listas/tabelas */

  /* ── Cabeçalho de página ───────────────────────────────────── */
  .rp-page-header {
    display: flex; align-items: flex-end; justify-content: space-between;
    gap: var(--rp-space-4);
    margin-bottom: var(--rp-space-6);
  }
  .rp-page-header__title { display: grid; gap: var(--rp-space-1); }
  .rp-page-header__subtitle { color: var(--rp-text-secondary); font-size: var(--rp-text-md); }
  .rp-page-header__actions { display: flex; gap: var(--rp-space-2); }

  /* ── Duas colunas (pastas + tabela) ────────────────────────── */
  .rp-split {
    display: grid;
    grid-template-columns: 260px minmax(0, 1fr);
    gap: var(--rp-space-5);
    align-items: start;
  }
  .rp-split__aside { position: sticky; top: calc(var(--rp-topbar-h) + var(--rp-space-4)); }

  /* ── Barra de ferramentas (busca, filtros, ações) ──────────── */
  .rp-toolbar {
    display: flex; flex-wrap: wrap; align-items: center;
    gap: var(--rp-space-3);
    margin-bottom: var(--rp-space-4);
  }
  .rp-toolbar__spacer { flex: 1; }

  /* ── Grade de formulário (12 colunas) ──────────────────────── */
  .rp-form-grid {
    display: grid;
    grid-template-columns: repeat(12, minmax(0, 1fr));
    gap: var(--rp-space-4);
  }
  .rp-col-12 { grid-column: span 12; }
  .rp-col-6  { grid-column: span 6; }
  .rp-col-4  { grid-column: span 4; }
  .rp-col-3  { grid-column: span 3; }
  .rp-col-2  { grid-column: span 2; }

  @media (max-width: 1100px) {
    .rp-split { grid-template-columns: 1fr; }
    .rp-split__aside { position: static; }
    .rp-col-3, .rp-col-4 { grid-column: span 6; }
    .rp-col-6 { grid-column: span 12; }
  }
  @media (max-width: 900px) {
    .rp-shell { --rp-sidebar-w: var(--rp-sidebar-w-collapsed); }
  }

  /* ── Wizard ────────────────────────────────────────────────── */
  .rp-wizard__header { margin-bottom: var(--rp-space-6); }
  .rp-wizard__body   { display: grid; gap: var(--rp-space-5); padding-bottom: var(--rp-space-12); }
  .rp-wizard__body--with-aside { grid-template-columns: minmax(0, 1fr) 340px; align-items: start; }
  .rp-wizard__footer {
    position: sticky; bottom: 0;
    z-index: var(--rp-z-sticky);
    display: flex; align-items: center; justify-content: space-between;
    padding: var(--rp-space-3) var(--rp-space-6);
    background: var(--rp-glass-bg-strong);
    -webkit-backdrop-filter: var(--rp-glass-filter);
    backdrop-filter: var(--rp-glass-filter);
    border-top: var(--rp-border-width) solid var(--rp-glass-edge);
  }
  @media (max-width: 1100px) { .rp-wizard__body--with-aside { grid-template-columns: 1fr; } }
}
```

---

## 7. Padrão de nomenclatura (resumo)

Detalhes completos na seção 10. Em uma linha: **prefixo `rp-`, BEM (`rp-bloco__elemento--modificador`), estados via atributos ARIA primeiro** (`aria-selected`, `aria-busy`, `aria-invalid`, `aria-current`, `disabled`) e classes `is-*` só quando não houver equivalente ARIA.

---

## 8. Componentes

> Todos os arquivos abaixo ficam em `@layer rp.components { … }`. Omiti o envelope `@layer` nos blocos para encurtar; **inclua-o** ao salvar cada arquivo.

### 8.1 Botões — `components/button.css`

```css
@layer rp.components {
  .rp-btn {
    /* variáveis privadas */
    --_bg: var(--rp-surface);
    --_fg: var(--rp-text);
    --_bd: var(--rp-border-default);
    --_sh: var(--rp-shadow-xs);
    --_spinner: var(--rp-accent);

    position: relative;
    display: inline-flex; align-items: center; justify-content: center;
    gap: var(--rp-space-2);
    height: var(--rp-control-h-md);
    padding: 0 var(--rp-space-4);
    border: var(--rp-border-width) solid var(--_bd);
    border-radius: var(--rp-radius-md);
    background: var(--_bg);
    color: var(--_fg);
    font-weight: var(--rp-weight-semibold);
    font-size: var(--rp-text-md);
    line-height: 1;
    white-space: nowrap;
    box-shadow: var(--_sh);
    transition: background-color var(--rp-dur-fast) var(--rp-ease),
                border-color var(--rp-dur-fast) var(--rp-ease),
                box-shadow var(--rp-dur-fast) var(--rp-ease),
                transform var(--rp-dur-fast) var(--rp-ease);
  }

  /* Variantes */
  .rp-btn--primary   { --_bg: var(--rp-accent); --_fg: var(--rp-on-accent); --_bd: var(--rp-accent); --_sh: var(--rp-shadow-sm); --_spinner: #fff; }
  .rp-btn--secondary { --_bg: var(--rp-surface); --_fg: var(--rp-primary-700); --_bd: var(--rp-border-default); }
  .rp-btn--ghost     { --_bg: transparent; --_fg: var(--rp-text-secondary); --_bd: transparent; --_sh: none; }
  .rp-btn--danger    { --_bg: var(--rp-danger-600); --_fg: #fff; --_bd: var(--rp-danger-600); --_spinner: #fff; }

  /* Tamanhos */
  .rp-btn--sm { height: var(--rp-control-h-sm); padding: 0 var(--rp-space-3); font-size: var(--rp-text-sm); border-radius: var(--rp-radius-sm); }
  .rp-btn--lg { height: var(--rp-control-h-lg); padding: 0 var(--rp-space-6); font-size: var(--rp-text-lg); }
  .rp-btn--icon { width: var(--rp-control-h-md); padding: 0; }
  .rp-btn--icon.rp-btn--sm { width: var(--rp-control-h-sm); }

  /* Hover (só com ponteiro real) */
  @media (hover: hover) {
    .rp-btn--primary:hover   { --_bg: var(--rp-accent-hover); --_bd: var(--rp-accent-hover); }
    .rp-btn--secondary:hover { --_bg: var(--rp-primary-50); --_bd: var(--rp-primary-300); }
    .rp-btn--ghost:hover     { --_bg: var(--rp-primary-50); --_fg: var(--rp-primary-700); }
    .rp-btn--danger:hover    { --_bg: var(--rp-danger-700); --_bd: var(--rp-danger-700); }
  }

  /* Active (pressionado) */
  .rp-btn:active { transform: translateY(1px); box-shadow: none; }
  .rp-btn--primary:active   { --_bg: var(--rp-accent-active); --_bd: var(--rp-accent-active); }
  .rp-btn--secondary:active { --_bg: var(--rp-primary-100); }
  .rp-btn--ghost:active     { --_bg: var(--rp-primary-100); }
  .rp-btn--danger:active    { --_bg: var(--rp-danger-800); --_bd: var(--rp-danger-800); }

  /* Selected / pressed toggle (ex.: filtro ativo) */
  .rp-btn[aria-pressed="true"] { --_bg: var(--rp-selected-bg); --_fg: var(--rp-primary-800); --_bd: var(--rp-primary-300); }

  /* Focus: usa o outline global; reforça com anel */
  .rp-btn:focus-visible { box-shadow: var(--_sh), var(--rp-ring); }

  /* Disabled */
  .rp-btn:disabled, .rp-btn[aria-disabled="true"] {
    --_bg: var(--rp-surface-sunken); --_fg: var(--rp-text-disabled); --_bd: var(--rp-border-subtle); --_sh: none;
    cursor: not-allowed; transform: none; pointer-events: none;
  }

  /* Loading (ação em andamento) — mantém largura, esconde o texto, mostra spinner */
  .rp-btn[aria-busy="true"] { --_fg: transparent; pointer-events: none; cursor: progress; }
  .rp-btn[aria-busy="true"]::after {
    content: ""; position: absolute; inset: 0; margin: auto;
    width: 1.1em; height: 1.1em;
    border: 2px solid var(--_spinner); border-right-color: transparent; border-radius: 50%;
    animation: rp-spin .7s linear infinite;
  }
  @keyframes rp-spin { to { transform: rotate(360deg); } }
}
```

**Regras de uso**
- Uma única ação `primary` por área (ex.: "Avançar" no wizard, "Exportar PDF" na etapa 5).
- Texto de botão com **verbo + objeto**: "Novo plano", "Anexar ART", "Exportar PDF".
- Ação destrutiva: `rp-btn--danger` **sempre com confirmação** (seção 8.9). Se o brief não prevê excluir plano, **não exponha** essa ação **[PENDENTE P16]**.

### 8.2 Formulários — `components/form.css`

```css
@layer rp.components {
  .rp-field { display: grid; gap: var(--rp-space-1); align-content: start; }
  .rp-label { font-size: var(--rp-text-sm); font-weight: var(--rp-weight-semibold); color: var(--rp-neutral-700); }
  .rp-label--required::after { content: " *"; color: var(--rp-danger-600); }
  .rp-field__hint  { font-size: var(--rp-text-xs); color: var(--rp-text-muted); }
  .rp-field__error,
  .validation-message {                    /* classe gerada pelo <ValidationMessage> do Blazor */
    display: flex; gap: var(--rp-space-1); align-items: flex-start;
    font-size: var(--rp-text-xs); font-weight: var(--rp-weight-medium); color: var(--rp-danger-800);
  }
  .validation-errors { margin: 0; padding-left: var(--rp-space-5); color: var(--rp-danger-800); } /* <ValidationSummary> */

  .rp-input, .rp-select, .rp-textarea {
    width: 100%;
    height: var(--rp-control-h-md);
    padding: 0 var(--rp-space-3);
    background: var(--rp-surface);
    color: var(--rp-text);
    border: var(--rp-border-width) solid var(--rp-border-default);
    border-radius: var(--rp-radius-md);
    box-shadow: var(--rp-shadow-xs);
    transition: border-color var(--rp-dur-fast) var(--rp-ease), box-shadow var(--rp-dur-fast) var(--rp-ease), background-color var(--rp-dur-fast) var(--rp-ease);
  }
  .rp-textarea { height: auto; min-height: 96px; padding: var(--rp-space-2) var(--rp-space-3); resize: vertical; }
  .rp-input::placeholder, .rp-textarea::placeholder { color: var(--rp-text-muted); opacity: .8; }

  /* hover */
  @media (hover: hover) { .rp-input:hover:not(:disabled):not(:read-only), .rp-select:hover:not(:disabled), .rp-textarea:hover:not(:disabled):not(:read-only) { border-color: var(--rp-border-strong); } }

  /* focus */
  .rp-input:focus, .rp-select:focus, .rp-textarea:focus {
    outline: 2px solid transparent;              /* mantém foco visível em alto contraste */
    border-color: var(--rp-focus);
    box-shadow: var(--rp-ring);
  }

  /* readonly / disabled */
  .rp-input:read-only, .rp-textarea:read-only { background: var(--rp-surface-subtle); }
  .rp-input:disabled, .rp-select:disabled, .rp-textarea:disabled {
    background: var(--rp-surface-sunken); color: var(--rp-text-disabled);
    border-color: var(--rp-border-subtle); box-shadow: none; cursor: not-allowed;
  }

  /* erro — classe do Blazor (invalid) + ARIA */
  .rp-input.invalid, .rp-select.invalid, .rp-textarea.invalid,
  .rp-input[aria-invalid="true"], .rp-select[aria-invalid="true"], .rp-textarea[aria-invalid="true"] {
    border-color: var(--rp-danger-600); background: var(--rp-danger-50);
  }
  .rp-input.invalid:focus, .rp-input[aria-invalid="true"]:focus { box-shadow: var(--rp-ring-danger); }
  /* [DECISÃO] não estilizar ".valid" (sem "verde de sucesso" em todo campo): reduz ruído visual. */

  /* Campo CALCULADO (derivado) — visualmente distinto do digitado */
  .rp-input--derived {
    background: var(--rp-primary-50);
    border: 1px dashed var(--rp-primary-300);
    color: var(--rp-primary-800);
    font-weight: var(--rp-weight-semibold);
    box-shadow: none; cursor: default;
  }

  /* Numéricos */
  .rp-input--num { text-align: right; font-variant-numeric: tabular-nums; }

  /* Campo com unidade (kg, t, m, %, kgf/cm²…) — a unidade NUNCA some */
  .rp-input-group { display: flex; width: 100%; }
  .rp-input-group .rp-input { border-top-right-radius: 0; border-bottom-right-radius: 0; text-align: right; font-variant-numeric: tabular-nums; }
  .rp-input-group__unit {
    display: inline-flex; align-items: center; padding: 0 var(--rp-space-3);
    background: var(--rp-surface-sunken); color: var(--rp-text-secondary);
    border: var(--rp-border-width) solid var(--rp-border-default); border-left: 0;
    border-radius: 0 var(--rp-radius-md) var(--rp-radius-md) 0;
    font-size: var(--rp-text-sm); font-weight: var(--rp-weight-semibold); white-space: nowrap;
  }
  .rp-input-group:focus-within .rp-input-group__unit { border-color: var(--rp-focus); }

  /* Checkbox / radio / switch simples */
  .rp-check { display: inline-flex; align-items: center; gap: var(--rp-space-2); cursor: pointer; }
  .rp-check input { inline-size: 16px; block-size: 16px; accent-color: var(--rp-accent); }
  .rp-check input:disabled + span { color: var(--rp-text-disabled); }

  /* Seção de formulário */
  .rp-fieldset { display: grid; gap: var(--rp-space-4); }
  .rp-fieldset__legend { font-size: var(--rp-text-sm); font-weight: var(--rp-weight-bold); color: var(--rp-primary-800); text-transform: uppercase; letter-spacing: .04em; }

  /* Busca */
  .rp-search { position: relative; min-width: 280px; }
  .rp-search .rp-input { padding-left: calc(var(--rp-space-3) + var(--rp-icon-md) + var(--rp-space-2)); }
  .rp-search .rp-icon { position: absolute; left: var(--rp-space-3); top: 50%; translate: 0 -50%; color: var(--rp-text-muted); pointer-events: none; }

  /* Dropzone de anexos (imagens e PDFs) */
  .rp-dropzone {
    position: relative; display: grid; place-items: center; gap: var(--rp-space-2);
    padding: var(--rp-space-8) var(--rp-space-6);
    text-align: center; color: var(--rp-text-secondary);
    background: color-mix(in srgb, var(--rp-primary-50) 70%, transparent);
    border: 2px dashed var(--rp-primary-300); border-radius: var(--rp-radius-lg);
    transition: border-color var(--rp-dur-fast) var(--rp-ease), background-color var(--rp-dur-fast) var(--rp-ease);
  }
  .rp-dropzone input[type="file"] { position: absolute; inset: 0; opacity: 0; cursor: pointer; } /* sobrepõe o <InputFile> */
  .rp-dropzone:hover, .rp-dropzone.is-dragover { border-color: var(--rp-primary-500); background: var(--rp-primary-100); }
  .rp-dropzone:focus-within { outline: 2px solid var(--rp-focus); outline-offset: 2px; }
  .rp-dropzone[aria-disabled="true"] { opacity: .6; pointer-events: none; }
  .rp-dropzone.invalid { border-color: var(--rp-danger-600); background: var(--rp-danger-50); }
}
```

> **Cuidado fora do CSS (relevante pela restrição F9):** `InputNumber` renderiza `type="number"` e o separador decimal depende de cultura/navegador. Defina e teste a cultura **pt-BR** e o formato numérico **[PENDENTE P17]** antes de confiar nos valores exibidos.
> **[A VALIDAR]** Arrastar-e-soltar arquivos em cima do `InputFile` dentro do WebView2 do MAUI.

### 8.3 Tabelas — `components/table.css`

```css
@layer rp.components {
  .rp-table-wrap {
    position: relative; overflow: auto;
    background: var(--rp-surface);
    border: var(--rp-border-width) solid var(--rp-border-subtle);
    border-radius: var(--rp-radius-lg);
    box-shadow: var(--rp-shadow-sm);
  }

  .rp-table {
    width: 100%;
    border-collapse: separate; border-spacing: 0;
    font-size: var(--rp-text-sm);
    font-variant-numeric: tabular-nums;
  }

  .rp-table th, .rp-table td {
    height: var(--rp-row-h);
    padding: 0 var(--rp-space-3);
    text-align: left; vertical-align: middle;
    border-bottom: var(--rp-border-width) solid var(--rp-border-subtle);
  }

  /* Cabeçalho: OPACO (nunca glass) e fixo ao rolar */
  .rp-table thead th {
    position: sticky; top: 0; z-index: 1;
    background: var(--rp-surface-subtle);
    color: var(--rp-text-secondary);
    font-weight: var(--rp-weight-semibold);
    white-space: nowrap;
    border-bottom: var(--rp-border-width) solid var(--rp-border-default);
  }

  .rp-table tbody tr:last-child td { border-bottom: 0; }

  /* Números alinhados à direita, sem truncar */
  .rp-table th.rp-num, .rp-table td.rp-num { text-align: right; white-space: nowrap; }

  /* Truncar APENAS texto livre — nunca números/unidades. Sempre com title="…" */
  .rp-cell-truncate { max-width: 28ch; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

  /* Densidade */
  .rp-table--compact     { --rp-row-h: 32px; }
  .rp-table--comfortable { --rp-row-h: 48px; }

  /* Zebra opcional */
  .rp-table--zebra tbody tr:nth-child(even) { background: var(--rp-surface-subtle); }

  /* Linhas clicáveis (acessíveis: tabindex="0" + Enter/Espaço no Razor) */
  .rp-table tbody tr.is-clickable { cursor: pointer; }

  /* Hover */
  @media (hover: hover) { .rp-table tbody tr:hover { background: var(--rp-hover-bg); } }

  /* Selecionada */
  .rp-table tbody tr[aria-selected="true"] { background: var(--rp-selected-bg); }
  .rp-table tbody tr[aria-selected="true"] td:first-child { box-shadow: inset 3px 0 0 var(--rp-accent); }

  /* Foco de teclado na linha */
  .rp-table tbody tr:focus-visible { outline-offset: -2px; background: var(--rp-primary-50); }

  /* Linha desabilitada/arquivada */
  .rp-table tbody tr[aria-disabled="true"] { color: var(--rp-text-disabled); }

  /* Linha com erro (ex.: anexo faltando) */
  .rp-table tbody tr.is-error td:first-child { box-shadow: inset 3px 0 0 var(--rp-danger-600); background: var(--rp-danger-50); }

  /* Ordenação */
  .rp-table th[aria-sort] { cursor: pointer; }
  .rp-table th[aria-sort="ascending"]::after  { content: " ▲"; font-size: .7em; color: var(--rp-accent); }
  .rp-table th[aria-sort="descending"]::after { content: " ▼"; font-size: .7em; color: var(--rp-accent); }

  /* Carregando */
  .rp-table-wrap[aria-busy="true"] { opacity: .65; pointer-events: none; }

  /* Ações de linha alinhadas à direita */
  .rp-table .rp-row-actions { display: flex; justify-content: flex-end; gap: var(--rp-space-1); }

  /* Estado vazio */
  .rp-empty { display: grid; place-items: center; gap: var(--rp-space-2); padding: var(--rp-space-12) var(--rp-space-6); text-align: center; color: var(--rp-text-secondary); }
  .rp-empty__title { color: var(--rp-text); font-weight: var(--rp-weight-semibold); font-size: var(--rp-text-lg); }
}
```

**Notas de produtividade**
- **Linhas de altura fixa** (token `--rp-row-h`) permitem usar `<Virtualize>` do Blazor se a lista crescer. **[PENDENTE P10]** Definir colunas da lista (o brief cita número, cliente, local e data como dados do plano) e se haverá paginação ou virtualização.
- Colunas de dados do mesmo tipo mantêm alinhamento fixo (texto à esquerda, números à direita, datas em formato único).
- Cabeçalho e primeira coluna importantes (número do plano) ficam visíveis sem rolagem horizontal.

### 8.4 Planilha de capacidade de carga — `components/sheet.css`

Reutiliza a lógica de tabela, com células editáveis. Os grupos de linhas seguem a folha-resumo do brief (§6.2): dados da carga, lingada/amarras, dispositivos/balancim, informações e configuração do guindaste, composição da carga bruta, quadrante operacional, resumo dos cálculos.

```css
@layer rp.components {
  .rp-sheet { width: 100%; border-collapse: separate; border-spacing: 0; font-size: var(--rp-text-sm); font-variant-numeric: tabular-nums; }
  .rp-sheet th, .rp-sheet td { border-bottom: 1px solid var(--rp-border-subtle); border-right: 1px solid var(--rp-border-subtle); height: var(--rp-row-h); padding: 0; }
  .rp-sheet th { padding: 0 var(--rp-space-3); background: var(--rp-surface-subtle); color: var(--rp-text-secondary); font-weight: var(--rp-weight-semibold); text-align: left; }

  /* Título de grupo */
  .rp-sheet__group th {
    background: var(--rp-primary-50); color: var(--rp-primary-800);
    font-size: var(--rp-text-xs); letter-spacing: .05em; text-transform: uppercase;
    border-bottom: 1px solid var(--rp-primary-200);
  }

  /* Célula editável */
  .rp-cell-input {
    width: 100%; height: 100%; padding: 0 var(--rp-space-3);
    background: transparent; border: 0; border-radius: 0;
    text-align: right; font: inherit; color: var(--rp-text);
  }
  .rp-sheet td:focus-within { outline: 2px solid var(--rp-focus); outline-offset: -2px; background: var(--rp-surface); position: relative; z-index: 1; }
  @media (hover: hover) { .rp-sheet td:hover:not(:focus-within):not(.is-derived) { background: var(--rp-hover-bg); } }

  /* Célula calculada: não editável, visual distinto */
  .rp-sheet td.is-derived { background: var(--rp-primary-50); color: var(--rp-primary-800); font-weight: var(--rp-weight-semibold); padding: 0 var(--rp-space-3); text-align: right; }
  .rp-sheet td.is-error   { outline: 2px solid var(--rp-danger-600); outline-offset: -2px; background: var(--rp-danger-50); }
  .rp-sheet td[aria-disabled="true"] { background: var(--rp-surface-sunken); color: var(--rp-text-disabled); }

  /* Coluna de unidade */
  .rp-sheet td.rp-unit { padding: 0 var(--rp-space-3); color: var(--rp-text-secondary); font-weight: var(--rp-weight-semibold); background: var(--rp-surface-subtle); width: 1%; white-space: nowrap; }
}
```

**[PENDENTE P3-negócio / Dúvida 3 do brief]** Quais valores são digitados e quais são calculados pelo sistema. Os estilos `is-derived` e `rp-input--derived` já cobrem os dois casos.

### 8.5 Cards e painéis — `components/card.css`

```css
@layer rp.components {
  .rp-card {
    background: var(--rp-surface);
    border: var(--rp-border-width) solid var(--rp-border-subtle);
    border-radius: var(--rp-radius-lg);
    box-shadow: var(--rp-shadow-sm);
  }
  .rp-card__header { display: flex; align-items: center; justify-content: space-between; gap: var(--rp-space-3); padding: var(--rp-space-4) var(--rp-space-5); border-bottom: 1px solid var(--rp-border-subtle); }
  .rp-card__title  { font-size: var(--rp-text-lg); font-weight: var(--rp-weight-semibold); }
  .rp-card__body   { padding: var(--rp-space-5); }
  .rp-card__footer { padding: var(--rp-space-3) var(--rp-space-5); border-top: 1px solid var(--rp-border-subtle); display: flex; justify-content: flex-end; gap: var(--rp-space-2); }

  /* Interativo (ex.: anexos, partes do PDF) */
  .rp-card--interactive { cursor: pointer; transition: transform var(--rp-dur-base) var(--rp-ease), box-shadow var(--rp-dur-base) var(--rp-ease), border-color var(--rp-dur-base) var(--rp-ease); }
  @media (hover: hover) { .rp-card--interactive:hover { transform: translateY(-1px); box-shadow: var(--rp-shadow-md); border-color: var(--rp-primary-200); } }
  .rp-card--interactive:active { transform: none; box-shadow: var(--rp-shadow-xs); }
  .rp-card[aria-selected="true"] { border-color: var(--rp-primary-500); box-shadow: var(--rp-shadow-sm), 0 0 0 1px var(--rp-primary-500); }
  .rp-card[aria-disabled="true"] { opacity: .6; pointer-events: none; }
  .rp-card[aria-busy="true"]     { position: relative; }
  .rp-card.is-error              { border-color: var(--rp-danger-600); }

  /* Utilitário glass — usar em barras/painéis/menus, NÃO em tabelas/campos */
  .rp-glass, .rp-card--glass {
    background: var(--rp-glass-bg);
    -webkit-backdrop-filter: var(--rp-glass-filter);
    backdrop-filter: var(--rp-glass-filter);
    border: var(--rp-border-width) solid var(--rp-glass-edge);
    box-shadow: var(--rp-shadow-md), inset 0 1px 0 var(--rp-glass-highlight);
  }
  .rp-card--glass { border-radius: var(--rp-radius-lg); }

  /* Divisor (reinterpretação sutil do divisor metálico do site) */
  .rp-divider { height: 1px; border: 0; margin: var(--rp-space-5) 0; background: linear-gradient(to right, transparent, var(--rp-neutral-300) 20%, var(--rp-neutral-300) 80%, transparent); }
}
```

### 8.6 Navegação — `components/nav.css`

```css
@layer rp.components {
  /* Marca */
  .rp-brand { display: flex; align-items: center; gap: var(--rp-space-3); padding: var(--rp-space-2) var(--rp-space-3) var(--rp-space-4); color: #fff; }
  .rp-brand__name { font-family: var(--rp-font-brand); font-size: var(--rp-text-sm); letter-spacing: .06em; text-transform: uppercase; }

  /* Menu lateral */
  .rp-nav { display: grid; gap: var(--rp-space-1); }
  .rp-nav__item {
    display: flex; align-items: center; gap: var(--rp-space-3);
    height: 40px; padding: 0 var(--rp-space-3);
    color: var(--rp-sidebar-text); border-radius: var(--rp-radius-md);
    font-weight: var(--rp-weight-medium); text-decoration: none;
    transition: background-color var(--rp-dur-fast) var(--rp-ease), color var(--rp-dur-fast) var(--rp-ease);
  }
  @media (hover: hover) { .rp-nav__item:hover { background: var(--rp-sidebar-hover); color: #fff; text-decoration: none; } }
  .rp-nav__item:active { background: var(--rp-sidebar-active); }
  .rp-nav__item.is-active,
  .rp-nav__item[aria-current="page"] { background: var(--rp-sidebar-active); color: #fff; box-shadow: inset 3px 0 0 #fff; }
  .rp-nav__item:focus-visible { outline-color: #fff; }
  .rp-nav__item[aria-disabled="true"] { opacity: .45; pointer-events: none; }
  .rp-shell--collapsed .rp-nav__label { display: none; }

  /* Lista de pastas (um nível) */
  .rp-folder-list { display: grid; gap: var(--rp-space-1); padding: var(--rp-space-2); }
  .rp-folder-list__item {
    display: flex; align-items: center; justify-content: space-between; gap: var(--rp-space-2);
    height: 36px; padding: 0 var(--rp-space-3);
    background: transparent; border: 0; border-radius: var(--rp-radius-md);
    color: var(--rp-text); text-align: left; width: 100%;
  }
  @media (hover: hover) { .rp-folder-list__item:hover { background: var(--rp-hover-bg); } }
  .rp-folder-list__item[aria-current="true"] { background: var(--rp-selected-bg); color: var(--rp-primary-800); font-weight: var(--rp-weight-semibold); }
  .rp-folder-list__count { font-size: var(--rp-text-xs); color: var(--rp-text-muted); font-variant-numeric: tabular-nums; }

  /* Breadcrumb */
  .rp-breadcrumb { display: flex; align-items: center; gap: var(--rp-space-2); font-size: var(--rp-text-sm); color: var(--rp-text-secondary); }
  .rp-breadcrumb li + li::before { content: "/"; margin-right: var(--rp-space-2); color: var(--rp-neutral-400); }
  .rp-breadcrumb [aria-current="page"] { color: var(--rp-text); font-weight: var(--rp-weight-semibold); }

  /* Menu suspenso (ações de linha: abrir, editar, duplicar…) */
  .rp-menu {
    position: absolute; z-index: var(--rp-z-dropdown);
    min-width: 200px; padding: var(--rp-space-1);
    background: var(--rp-glass-bg-strong);
    -webkit-backdrop-filter: var(--rp-glass-filter); backdrop-filter: var(--rp-glass-filter);
    border: 1px solid var(--rp-glass-edge); border-radius: var(--rp-radius-md);
    box-shadow: var(--rp-shadow-lg);
  }
  .rp-menu__item { display: flex; align-items: center; gap: var(--rp-space-2); width: 100%; height: 34px; padding: 0 var(--rp-space-3); background: transparent; border: 0; border-radius: var(--rp-radius-sm); text-align: left; }
  @media (hover: hover) { .rp-menu__item:hover { background: var(--rp-hover-bg); } }
  .rp-menu__item[aria-disabled="true"] { color: var(--rp-text-disabled); pointer-events: none; }
  .rp-menu__item--danger { color: var(--rp-danger-800); }
  .rp-menu__sep { height: 1px; margin: var(--rp-space-1) 0; background: var(--rp-border-subtle); }
}
```

### 8.7 Stepper do wizard — `components/stepper.css`

```css
@layer rp.components {
  .rp-stepper { display: flex; gap: var(--rp-space-2); list-style: none; padding: 0; margin: 0; counter-reset: step; }
  .rp-step { flex: 1; position: relative; display: flex; align-items: center; gap: var(--rp-space-3); min-width: 0; }
  .rp-step__button { display: flex; align-items: center; gap: var(--rp-space-3); padding: var(--rp-space-2); background: transparent; border: 0; border-radius: var(--rp-radius-md); color: var(--rp-text-secondary); text-align: left; }
  .rp-step__dot {
    flex: none; display: grid; place-items: center; width: 28px; height: 28px;
    border-radius: 50%; background: var(--rp-surface); border: 2px solid var(--rp-border-strong);
    font-size: var(--rp-text-sm); font-weight: var(--rp-weight-bold); color: var(--rp-text-secondary);
  }
  .rp-step__label { font-weight: var(--rp-weight-semibold); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
  .rp-step:not(:last-child)::after { content: ""; flex: 1; height: 2px; margin-inline: var(--rp-space-2); background: var(--rp-border-default); border-radius: 2px; }

  @media (hover: hover) { .rp-step__button:hover:not(:disabled) { background: var(--rp-hover-bg); } }

  /* Concluída */
  .rp-step.is-complete .rp-step__dot { background: var(--rp-accent); border-color: var(--rp-accent); color: #fff; }
  .rp-step.is-complete:not(:last-child)::after { background: var(--rp-accent); }
  .rp-step.is-complete .rp-step__label { color: var(--rp-text); }

  /* Atual */
  .rp-step[aria-current="step"] .rp-step__dot { border-color: var(--rp-accent); color: var(--rp-accent); box-shadow: var(--rp-ring); }
  .rp-step[aria-current="step"] .rp-step__label { color: var(--rp-primary-800); }

  /* Com erro de validação */
  .rp-step.is-error .rp-step__dot { border-color: var(--rp-danger-600); background: var(--rp-danger-50); color: var(--rp-danger-800); }

  /* Ainda não acessível */
  .rp-step__button:disabled { cursor: not-allowed; opacity: .6; }

  @media (max-width: 1100px) { .rp-step__label { display: none; } .rp-step[aria-current="step"] .rp-step__label { display: block; } }
}
```

Etapas, na ordem do brief: **1 Dados de entrada · 2 Capacidade de carga · 3 Anexos · 4 Preview · 5 Exportação**.

### 8.8 Badges, alertas, medidor de utilização — `components/feedback.css`

```css
@layer rp.components {
  /* Badge */
  .rp-badge {
    --_bg: var(--rp-neutral-100); --_fg: var(--rp-neutral-700); --_bd: var(--rp-neutral-200);
    display: inline-flex; align-items: center; gap: var(--rp-space-1);
    height: 22px; padding: 0 var(--rp-space-2);
    background: var(--_bg); color: var(--_fg); border: 1px solid var(--_bd);
    border-radius: var(--rp-radius-pill); font-size: var(--rp-text-xs); font-weight: var(--rp-weight-semibold); white-space: nowrap;
  }
  .rp-badge--primary { --_bg: var(--rp-primary-50);  --_fg: var(--rp-primary-800); --_bd: var(--rp-primary-200); }
  .rp-badge--success { --_bg: var(--rp-success-50);  --_fg: var(--rp-success-800); --_bd: var(--rp-success-200); }
  .rp-badge--warning { --_bg: var(--rp-warning-50);  --_fg: var(--rp-warning-800); --_bd: var(--rp-warning-200); }
  .rp-badge--danger  { --_bg: var(--rp-danger-50);   --_fg: var(--rp-danger-800);  --_bd: var(--rp-danger-200); }
  /* Cor nunca é o único sinal: inclua texto e/ou ícone dentro do badge. */

  /* Alerta / banner */
  .rp-alert {
    --_bg: var(--rp-primary-50); --_bd: var(--rp-primary-200); --_fg: var(--rp-primary-900); --_bar: var(--rp-primary-500);
    display: flex; gap: var(--rp-space-3); align-items: flex-start;
    padding: var(--rp-space-3) var(--rp-space-4);
    background: var(--_bg); color: var(--_fg);
    border: 1px solid var(--_bd); border-left: 4px solid var(--_bar); border-radius: var(--rp-radius-md);
  }
  .rp-alert--success { --_bg: var(--rp-success-50); --_bd: var(--rp-success-200); --_fg: var(--rp-success-800); --_bar: var(--rp-success-600); }
  .rp-alert--warning { --_bg: var(--rp-warning-50); --_bd: var(--rp-warning-200); --_fg: var(--rp-warning-800); --_bar: var(--rp-warning-600); }
  .rp-alert--danger  { --_bg: var(--rp-danger-50);  --_bd: var(--rp-danger-200);  --_fg: var(--rp-danger-800);  --_bar: var(--rp-danger-600); }
  .rp-alert__title { font-weight: var(--rp-weight-semibold); }

  /* Medidor (ex.: taxa de utilização). A COR é opcional; o NÚMERO é obrigatório ao lado. */
  .rp-meter { position: relative; height: 8px; background: var(--rp-neutral-200); border-radius: var(--rp-radius-pill); overflow: hidden; }
  .rp-meter__bar { height: 100%; width: calc(var(--value, 0) * 1%); background: var(--rp-primary-500); border-radius: inherit; transition: width var(--rp-dur-slow) var(--rp-ease); }
  .rp-meter[data-level="attention"] .rp-meter__bar { background: var(--rp-warning-600); }
  .rp-meter[data-level="critical"]  .rp-meter__bar { background: var(--rp-danger-600); }
  /* [PENDENTE P8] Os limiares de "attention"/"critical" devem vir de uma regra do domínio
     (o brief só cita a margem de 85%). Não definir cores por conta própria na UI. */

  /* Progresso indeterminado (gerando PDF) */
  .rp-progress { position: relative; height: 4px; background: var(--rp-primary-100); border-radius: var(--rp-radius-pill); overflow: hidden; }
  .rp-progress::after { content: ""; position: absolute; inset: 0; width: 40%; background: var(--rp-accent); border-radius: inherit; animation: rp-indeterminate 1.1s var(--rp-ease) infinite; }
  @keyframes rp-indeterminate { from { translate: -100% 0; } to { translate: 250% 0; } }

  /* Skeleton */
  .rp-skeleton { background: linear-gradient(90deg, var(--rp-neutral-100), var(--rp-neutral-200), var(--rp-neutral-100)); background-size: 200% 100%; border-radius: var(--rp-radius-sm); animation: rp-shimmer 1.2s linear infinite; min-height: 1em; }
  @keyframes rp-shimmer { to { background-position: -200% 0; } }
  @media (prefers-reduced-motion: reduce) { .rp-skeleton, .rp-progress::after { animation: none; } }

  /* Toast */
  .rp-toast-region { position: fixed; right: var(--rp-space-6); bottom: var(--rp-space-6); z-index: var(--rp-z-toast); display: grid; gap: var(--rp-space-2); width: min(380px, calc(100vw - 2rem)); }
  .rp-toast { padding: var(--rp-space-3) var(--rp-space-4); background: var(--rp-glass-bg-strong); -webkit-backdrop-filter: var(--rp-glass-filter); backdrop-filter: var(--rp-glass-filter); border: 1px solid var(--rp-glass-edge); border-left: 4px solid var(--rp-primary-500); border-radius: var(--rp-radius-md); box-shadow: var(--rp-shadow-lg); animation: rp-toast-in var(--rp-dur-base) var(--rp-ease); }
  .rp-toast--success { border-left-color: var(--rp-success-600); }
  .rp-toast--danger  { border-left-color: var(--rp-danger-600); }
  @keyframes rp-toast-in { from { opacity: 0; translate: 0 8px; } }
}
```

### 8.9 Modal, confirmação e drawer — `components/overlay.css`

```css
@layer rp.components {
  .rp-modal-backdrop {
    position: fixed; inset: 0; z-index: var(--rp-z-modal);
    display: grid; place-items: center; padding: var(--rp-space-6);
    background: var(--rp-overlay);
    -webkit-backdrop-filter: blur(4px); backdrop-filter: blur(4px);
    animation: rp-fade var(--rp-dur-base) var(--rp-ease);
  }
  .rp-modal {
    width: min(var(--rp-modal-md), 100%); max-height: calc(100vh - 3rem); max-height: calc(100dvh - 3rem);
    display: flex; flex-direction: column;
    background: rgba(255,255,255,.94);                 /* quase opaco: formulário legível */
    -webkit-backdrop-filter: var(--rp-glass-filter); backdrop-filter: var(--rp-glass-filter);
    border: 1px solid var(--rp-glass-edge); border-radius: var(--rp-radius-xl);
    box-shadow: var(--rp-shadow-xl), inset 0 1px 0 var(--rp-glass-highlight);
    animation: rp-modal-in var(--rp-dur-base) var(--rp-ease);
  }
  .rp-modal--sm { width: min(var(--rp-modal-sm), 100%); }
  .rp-modal--lg { width: min(var(--rp-modal-lg), 100%); }
  .rp-modal__header { display: flex; align-items: center; justify-content: space-between; gap: var(--rp-space-3); padding: var(--rp-space-5) var(--rp-space-6) var(--rp-space-3); }
  .rp-modal__title  { font-size: var(--rp-text-xl); font-weight: var(--rp-weight-semibold); }
  .rp-modal__body   { padding: var(--rp-space-3) var(--rp-space-6) var(--rp-space-5); overflow: auto; }
  .rp-modal__footer { display: flex; justify-content: flex-end; gap: var(--rp-space-2); padding: var(--rp-space-4) var(--rp-space-6); border-top: 1px solid var(--rp-border-subtle); }
  .rp-modal--danger .rp-modal__title { color: var(--rp-danger-800); }

  /* Drawer lateral (ex.: edição de acessório) */
  .rp-drawer-backdrop { position: fixed; inset: 0; z-index: var(--rp-z-overlay); background: var(--rp-overlay); animation: rp-fade var(--rp-dur-base) var(--rp-ease); }
  .rp-drawer {
    position: fixed; top: 0; right: 0; bottom: 0; z-index: var(--rp-z-modal);
    display: flex; flex-direction: column; width: min(var(--rp-drawer-w), 100vw);
    background: rgba(255,255,255,.96);
    -webkit-backdrop-filter: var(--rp-glass-filter); backdrop-filter: var(--rp-glass-filter);
    border-left: 1px solid var(--rp-glass-edge); box-shadow: var(--rp-shadow-xl);
    animation: rp-drawer-in var(--rp-dur-slow) var(--rp-ease);
  }
  @keyframes rp-fade { from { opacity: 0; } }
  @keyframes rp-modal-in { from { opacity: 0; translate: 0 8px; scale: .98; } }
  @keyframes rp-drawer-in { from { translate: 100% 0; } }
}
```

**Regras de comportamento (não são CSS, mas afetam os dados)**
- **Não fechar por clique no backdrop** quando houver dados digitados (risco de perda — F9). Fechar apenas por botão, `Esc` com confirmação, ou após salvar.
- Título claro, uma ação primária e "Cancelar" sempre visível; em confirmação destrutiva, descreva **o que será perdido**.
- Foco inicial no primeiro campo (ou no botão seguro, em confirmações) e retorno do foco ao acionador exigem pequeno código em C#/JS — **não resolvido por CSS**.
- **[PENDENTE P15]** Cadastro de acessórios em modal ou drawer. Recomendo **drawer** (formulário + histórico lado a lado).

### 8.10 Preview do documento — `components/doc-preview.css`

```css
@layer rp.components {
  .rp-doc-preview {
    --rp-doc-zoom: 1;
    display: flex; flex-direction: column; align-items: center; gap: var(--rp-space-6);
    padding: var(--rp-space-6);
    background: var(--rp-neutral-200);
    border: 1px solid var(--rp-border-default); border-radius: var(--rp-radius-lg);
    overflow: auto; max-height: calc(100dvh - var(--rp-topbar-h) - 200px);
  }
  .rp-doc-page {
    width: calc(210mm * var(--rp-doc-zoom));
    aspect-ratio: 210 / 297;                       /* A4 retrato */
    background: #fff; color: #000;
    box-shadow: var(--rp-shadow-lg);
  }
  .rp-doc-page__caption { margin-top: var(--rp-space-2); text-align: center; font-size: var(--rp-text-xs); color: var(--rp-text-secondary); }
}
```
**Recomendação (restrição F9):** o preview deve exibir **o mesmo PDF que será exportado** (ou ser gerado pela mesma rotina), e não uma reconstrução em HTML, para que o que o engenheiro confere seja exatamente o que sai. **[PENDENTE P13]**

### 8.11 Partes do PDF (etapa de exportação) — `components/parts.css`

Lista ordenada conforme brief §6.1; cada item diz **como entra** (gerado pelo sistema ou anexado).

```css
@layer rp.components {
  .rp-parts { display: grid; gap: var(--rp-space-2); list-style: none; padding: 0; margin: 0; counter-reset: part; }
  .rp-parts__item { display: grid; grid-template-columns: 32px 1fr auto auto; align-items: center; gap: var(--rp-space-3); padding: var(--rp-space-3) var(--rp-space-4); background: var(--rp-surface); border: 1px solid var(--rp-border-subtle); border-radius: var(--rp-radius-md); }
  .rp-parts__item::before { counter-increment: part; content: counter(part); display: grid; place-items: center; width: 28px; height: 28px; border-radius: 50%; background: var(--rp-primary-50); color: var(--rp-primary-800); font-weight: var(--rp-weight-bold); font-size: var(--rp-text-sm); }
  .rp-parts__item.is-missing { border-color: var(--rp-danger-200); background: var(--rp-danger-50); }
}
```
Exemplo de conteúdo: `1 Capacidade de carga — [Gerado pelo sistema]`, `2 Desenhos técnicos — [3 imagens anexadas]`, `3 ART do plano — [PDF anexado]`, `4 Procedimento operacional — [Gerado pelo sistema]`, `5 Laudo técnico + ART do laudo — [PDF anexado]`.

### 8.12 Ícones — `components/icon.css`

```css
@layer rp.components {
  .rp-icon { width: var(--rp-icon-md); height: var(--rp-icon-md); flex: none; fill: none; stroke: currentColor; stroke-width: 1.75; stroke-linecap: round; stroke-linejoin: round; }
  .rp-icon--sm { width: var(--rp-icon-sm); height: var(--rp-icon-sm); }
  .rp-icon--lg { width: var(--rp-icon-lg); height: var(--rp-icon-lg); }
}
```
**[DECISÃO]** Ícones como **SVG embutido** (componente Razor `<Icon Name="folder" />` ou sprite local), traço de 1,75px, `currentColor`. **[PENDENTE P7]** Escolher o conjunto (ex.: um conjunto de licença permissiva) e **baixar/embutir** os arquivos; sem CDN.

### 8.13 Tabs e demais componentes
O brief **não** exige abas. **Não os crie até haver necessidade** (menos CSS para manter). Quando surgirem, siga o mesmo padrão: `aria-selected`, foco visível, `rp-` + BEM.

### 8.14 Acessibilidade e alto contraste — `components/a11y.css`

```css
@layer rp.components {
  @media (forced-colors: active) {
    .rp-card, .rp-table-wrap, .rp-modal, .rp-drawer, .rp-menu, .rp-toast, .rp-alert, .rp-dropzone { border: 1px solid CanvasText; }
    .rp-btn { border: 1px solid ButtonText; }
    .rp-btn:disabled { border-color: GrayText; color: GrayText; }
    .rp-table tbody tr[aria-selected="true"] td:first-child { box-shadow: none; border-left: 4px solid Highlight; }
    .rp-step__dot { border-color: CanvasText; }
  }
  .rp-sr-only { position: absolute; width: 1px; height: 1px; margin: -1px; padding: 0; overflow: hidden; clip: rect(0 0 0 0); white-space: nowrap; border: 0; }
}
```

### 8.15 Erros globais do Blazor (se o template os incluir)
```css
@layer rp.components {
  #blazor-error-ui { display: none; position: fixed; inset: auto 0 0 0; z-index: var(--rp-z-toast); padding: var(--rp-space-3) var(--rp-space-6); background: var(--rp-danger-800); color: #fff; box-shadow: var(--rp-shadow-lg); }
  #blazor-error-ui .reload { color: #fff; text-decoration: underline; margin-left: var(--rp-space-3); }
  .blazor-error-boundary { padding: var(--rp-space-4); background: var(--rp-danger-50); border: 1px solid var(--rp-danger-200); border-left: 4px solid var(--rp-danger-600); border-radius: var(--rp-radius-md); color: var(--rp-danger-800); }
  .blazor-error-boundary::after { content: "Ocorreu um erro nesta área. Tente novamente ou reinicie o aplicativo."; }
}
```
> O Blazor mostra `#blazor-error-ui` mudando o `display` por script; a regra acima assume que o template o faz via estilo inline/JS. **Verifique.** O texto da `ErrorBoundary` precisa estar em **pt-BR** (F2).

---

## 9. Organização dos arquivos

```
RiggingPlanner.App/
└─ wwwroot/
   ├─ index.html                      ← <link> dos CSS abaixo, na ordem
   ├─ fonts/
   │   └─ InterVariable.woff2         ← baixar e embutir (P3)
   └─ css/
      ├─ 00-layers.css                ← declaração de @layer (primeiro!)
      ├─ 01-tokens.css
      ├─ 02-reset.css
      ├─ 03-base.css
      ├─ 04-layout.css
      ├─ fonts.css
      └─ components/
          ├─ button.css  form.css  table.css  sheet.css  card.css
          ├─ nav.css  stepper.css  feedback.css  overlay.css
          ├─ doc-preview.css  parts.css  icon.css  a11y.css
```

`index.html` (ordem importa; **sem Bootstrap**):
```html
<link rel="stylesheet" href="css/00-layers.css" />
<link rel="stylesheet" href="css/fonts.css" />
<link rel="stylesheet" href="css/01-tokens.css" />
<link rel="stylesheet" href="css/02-reset.css" />
<link rel="stylesheet" href="css/03-base.css" />
<link rel="stylesheet" href="css/04-layout.css" />
<link rel="stylesheet" href="css/components/button.css" />
<!-- … demais componentes … -->
<link rel="stylesheet" href="NomeDoAssembly.styles.css" /> <!-- CSS isolation: confirmar nome -->
```
**[PENDENTE P19]** Manter vários `<link>` (simples, sem build) ou concatenar em um `app.css` no futuro. Por ser local/offline, o custo de vários arquivos é desprezível.

---

## 10. Convenções

### 10.1 Nomenclatura
| Item | Regra | Exemplo |
|---|---|---|
| Prefixo | `rp-` em toda classe e custom property global | `.rp-btn`, `--rp-space-4` |
| Bloco | substantivo | `rp-card`, `rp-table`, `rp-stepper` |
| Elemento | `bloco__elemento` | `rp-card__header` |
| Modificador | `bloco--variação` | `rp-btn--primary`, `rp-table--compact` |
| Estado | **ARIA/atributo nativo primeiro**; classe `is-*` só se não houver equivalente | `aria-selected="true"`, `aria-busy="true"`, `aria-invalid="true"`, `disabled`, `.is-dragover`, `.is-complete`, `.is-derived`, `.is-error` |
| Variável privada de componente | `--_nome` | `--_bg`, `--_fg` |
| Tokens | `--rp-{categoria}-{variação}` e sempre **semântico** no componente | `--rp-border-default`, não `--rp-neutral-300` |
| Classes geradas pelo Blazor | **só** `valid`, `invalid`, `modified`, `validation-message`, `validation-errors`, `blazor-*` — estilizadas **em um único lugar** (`form.css` / 8.15) | — |

### 10.2 Regras
1. **Sem cores, sombras, raios ou espaços "soltos"** em componentes: use token. Se faltar um token, **crie-o** em `01-tokens.css`.
2. **Sem `!important`.** Resolver conflitos com `@layer` e baixa especificidade.
3. **Um seletor de classe** por regra sempre que possível (sem encadear `div.rp-x > ul li a`).
4. Estados em ordem: base → hover → focus-visible → active → selected → disabled → busy → erro. Hover dentro de `@media (hover: hover)`.
5. Todo componente interativo tem **foco visível** e funciona só com teclado.
6. Texto de UI **sempre em pt-BR**; nomes de classes e arquivos **em inglês** (F2 e brief §10).
7. **Cor nunca é o único sinal** (status = cor + texto/ícone).
8. Unidade de medida **sempre** exibida ao lado do valor (`kg`, `t`, `m`, `%`, `kgf`, `kgf/cm²`, `m/s`).
9. Reaproveitar antes de criar: a PR que cria componente novo deve justificar por que `rp-card`, `rp-table`, `rp-field` etc. não servem.
10. **Wrappers Razor só quando houver lógica** (ex.: `RpModal`, `RpStepper`, `RpField`, `RpDataTable`). Para o resto, HTML + classes. Evita camadas extras (brief: "sem overengineering").

### 10.3 Matriz de estados

| Componente | hover | focus | active | selected | disabled | loading | erro |
|---|---|---|---|---|---|---|---|
| Botão | fundo mais claro/tinta | outline + anel | desce 1px, sem sombra | `aria-pressed` | cinza, sem ponteiro | `aria-busy` + spinner | — (use `danger` p/ ações) |
| Campo | borda mais forte | borda azul + anel | — | — | fundo cinza, cursor `not-allowed` | `aria-busy` no campo (opcional) | borda e fundo vermelhos + mensagem |
| Campo calculado | — | foco normal | — | — | — | — | — (borda tracejada azul = "não digitar") |
| Linha de tabela | fundo azul 50 | outline interno | — | fundo azul 100 + barra lateral | texto cinza | `aria-busy` no wrapper | barra vermelha + fundo vermelho 50 |
| Célula da planilha | fundo azul 50 | outline 2px na célula | — | — | fundo cinza | — | outline vermelho |
| Item de menu lateral | fundo +9% branco | outline branco | fundo +16% | `is-active`/`aria-current` + barra | 45% opacidade | — | — |
| Pasta | fundo azul 50 | outline | — | `aria-current="true"` | — | — | — |
| Card interativo | sobe 1px + sombra | outline | volta ao lugar | borda azul | 60% opacidade | `aria-busy` | borda vermelha |
| Stepper | fundo azul 50 | outline | — | `aria-current="step"` | 60% + `not-allowed` | — | `.is-error` |
| Dropzone | borda/fundo azuis | outline | — | — | 60% | — | `.invalid` |

---

## 11. Regras de interface específicas do domínio (consequência de F9)

1. **Valores digitados × calculados** têm estilos diferentes (`rp-input--derived`, `.is-derived`). Um campo calculado **nunca** parece editável.
2. **Não arredondar na tela por conta própria.** Exiba o valor como o domínio o entrega; regras de arredondamento/unidade são decisão de negócio (brief, Dúvida 3).
3. **Nunca truncar números** com reticências; permitir rolagem horizontal da tabela em vez disso.
4. **Não unificar rótulos** que o brief mantém em dúvida (ex.: "Esforço nas sapatas" × "Resistência mínima do terreno", Dúvida 5; rótulo repetido do total de dispositivos, Dúvida 4) até o engenheiro responder.
5. **Não corrigir entrada do usuário em silêncio** (sem trocar vírgula por ponto, sem inverter, sem limitar valor "no blur"). Valide e **mostre o erro**.
6. **Confirmar antes de perder dados** (sair do wizard com alterações, substituir anexo, fechar modal).
7. **Mostrar origem** de cada parte do PDF: "gerado pelo sistema" ou "anexado" (seção 8.11).
8. Alertas de segurança (laudo vencido, acessório em desgaste) usam `rp-alert`/`rp-badge` com **texto explícito**; a regra que dispara o alerta é do domínio **[PENDENTE P11/P12]**.

---

## 12. Exemplos Razor de referência

> Exemplos ilustrativos escritos por mim. **Não foram compilados nem executados.** Ajuste namespaces, rotas e nomes ao projeto real.

### 12.1 `MainLayout.razor`
```razor
@inherits LayoutComponentBase

<div class="rp-shell @(_collapsed ? "rp-shell--collapsed" : null)">
    <aside class="rp-sidebar" aria-label="Navegação principal">
        <div class="rp-brand">
            <span class="rp-brand__name">Rigging Planner</span>
        </div>

        <nav class="rp-nav">
            <NavLink class="rp-nav__item" ActiveClass="is-active" href="/" Match="NavLinkMatch.All">
                <svg class="rp-icon" aria-hidden="true"><use href="icons.svg#folder" /></svg>
                <span class="rp-nav__label">Planos</span>
            </NavLink>
            <NavLink class="rp-nav__item" ActiveClass="is-active" href="/planos/novo">
                <svg class="rp-icon" aria-hidden="true"><use href="icons.svg#plus" /></svg>
                <span class="rp-nav__label">Novo plano</span>
            </NavLink>
            <NavLink class="rp-nav__item" ActiveClass="is-active" href="/acessorios">
                <svg class="rp-icon" aria-hidden="true"><use href="icons.svg#link" /></svg>
                <span class="rp-nav__label">Acessórios</span>
            </NavLink>
        </nav>
    </aside>

    <div class="rp-content">
        <header class="rp-topbar">
            <button class="rp-btn rp-btn--ghost rp-btn--icon" aria-label="Recolher menu"
                    @onclick="() => _collapsed = !_collapsed">
                <svg class="rp-icon" aria-hidden="true"><use href="icons.svg#menu" /></svg>
            </button>
            <!-- breadcrumb aqui -->
        </header>

        <main class="rp-main">
            @Body
        </main>
    </div>
</div>

@code {
    private bool _collapsed;
}
```
*(O sprite `icons.svg` e o uso de `<use>` no WebView2 do MAUI precisam ser validados — P7.)*

### 12.2 Campo com unidade, validação e botão em carregamento
```razor
<EditForm Model="_load" OnValidSubmit="SaveAsync">
    <DataAnnotationsValidator />

    <div class="rp-form-grid">
        <div class="rp-field rp-col-4">
            <label class="rp-label rp-label--required" for="load-weight">Peso da carga</label>
            <div class="rp-input-group">
                <InputNumber id="load-weight" class="rp-input" @bind-Value="_load.WeightKg" />
                <span class="rp-input-group__unit">kg</span>
            </div>
            <ValidationMessage For="() => _load.WeightKg" />
        </div>

        <div class="rp-field rp-col-4">
            <label class="rp-label" for="gross-load">Carga bruta (calculada)</label>
            <div class="rp-input-group">
                <input id="gross-load" class="rp-input rp-input--derived" readonly
                       value="@_load.GrossLoadDisplay" aria-describedby="gross-hint" />
                <span class="rp-input-group__unit">kg</span>
            </div>
            <span id="gross-hint" class="rp-field__hint">Valor calculado; não editável.</span>
        </div>
    </div>

    <button type="submit" class="rp-btn rp-btn--primary"
            aria-busy="@(_saving ? "true" : null)" disabled="@_saving">
        Salvar
    </button>
</EditForm>
```
*(Esse trecho assume que o domínio entrega `GrossLoadDisplay` já formatado; quem calcula e arredonda é decisão de negócio — Dúvida 3.)*

### 12.3 Tabela de planos (clicável e acessível)
```razor
<div class="rp-table-wrap" aria-busy="@(_loading ? "true" : null)">
    <table class="rp-table">
        <thead>
            <tr>
                <th>Nº do plano</th>
                <th>Cliente</th>
                <th>Local</th>
                <th>Data</th>
                <th class="rp-num">Ações</th>
            </tr>
        </thead>
        <tbody>
        @foreach (var plan in _plans)
        {
            <tr class="is-clickable" tabindex="0"
                aria-selected="@(plan.Id == _selectedId ? "true" : "false")"
                @onclick="() => Open(plan.Id)"
                @onkeydown="e => OnKey(e, plan.Id)">
                <td><span class="rp-code">@plan.Number</span></td>
                <td><span class="rp-cell-truncate" title="@plan.Client">@plan.Client</span></td>
                <td><span class="rp-cell-truncate" title="@plan.Location">@plan.Location</span></td>
                <td>@plan.Date.ToString("dd/MM/yyyy")</td>
                <td class="rp-num">
                    <div class="rp-row-actions">
                        <button class="rp-btn rp-btn--ghost rp-btn--sm" @onclick:stopPropagation>Duplicar</button>
                    </div>
                </td>
            </tr>
        }
        </tbody>
    </table>
</div>
```

### 12.4 Modal de confirmação
```razor
@if (Open)
{
    <div class="rp-modal-backdrop">
        <div class="rp-modal rp-modal--sm rp-modal--danger" role="alertdialog" aria-modal="true"
             aria-labelledby="confirm-title" aria-describedby="confirm-text">
            <div class="rp-modal__header">
                <h2 id="confirm-title" class="rp-modal__title">@Title</h2>
            </div>
            <div class="rp-modal__body">
                <p id="confirm-text">@Message</p>
            </div>
            <div class="rp-modal__footer">
                <button class="rp-btn rp-btn--secondary" @onclick="Cancel">Cancelar</button>
                <button class="rp-btn rp-btn--danger" @onclick="Confirm">@ConfirmText</button>
            </div>
        </div>
    </div>
}

@code {
    [Parameter] public bool Open { get; set; }
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string Message { get; set; } = "";
    [Parameter] public string ConfirmText { get; set; } = "Confirmar";
    [Parameter] public EventCallback Confirm { get; set; }
    [Parameter] public EventCallback Cancel { get; set; }
}
```
*(Sem fechar no backdrop de propósito — seção 8.9. Gestão de foco exige código adicional.)*

### 12.5 Stepper
```razor
<ol class="rp-stepper" aria-label="Etapas do plano">
@for (var i = 0; i < _steps.Length; i++)
{
    var index = i;
    <li class="rp-step @(index < _current ? "is-complete" : null)"
        aria-current="@(index == _current ? "step" : null)">
        <button type="button" class="rp-step__button" disabled="@(index > _maxReached)"
                @onclick="() => Go(index)">
            <span class="rp-step__dot">@(index < _current ? "✓" : index + 1)</span>
            <span class="rp-step__label">@_steps[index]</span>
        </button>
    </li>
}
</ol>

@code {
    private readonly string[] _steps =
        { "Dados de entrada", "Capacidade de carga", "Anexos", "Preview", "Exportação" };
    private int _current, _maxReached;
    private void Go(int i) => _current = i;
}
```

---

## 13. Validação necessária (nada disto foi feito)

Antes de dizer que o design system está "pronto", é preciso **executar e conferir** no app real:

- [ ] Versão do **WebView2 Runtime** na máquina-alvo suporta tudo o que foi usado (`@layer`, `backdrop-filter`, `color-mix()`, `dvh`, `scrollbar-color`).
- [ ] **Desempenho do `backdrop-filter`** e do `background-attachment: fixed` ao rolar uma lista grande, em notebook modesto, em 100%, 125% e 150% de escala.
- [ ] **Contraste** (WCAG AA) de todos os pares de cor, usando ferramenta (os valores deste documento são cálculos manuais).
- [ ] Navegação **só com teclado** (Tab/Shift+Tab/Enter/Espaço/Esc) em tabela, wizard, modal e menu.
- [ ] Modo **alto contraste** do Windows (`forced-colors`).
- [ ] Textos longos em pt-BR (clientes, locais, descrições de acessórios) sem quebrar layout.
- [ ] Tabela com centenas de linhas (e `Virtualize`, se adotado).
- [ ] Carregamento da fonte embutida sem rede; ausência de qualquer requisição externa.
- [ ] Nome real do bundle de **CSS isolation** e ordem de carregamento no `index.html`.
- [ ] Remoção completa dos estilos do template (Bootstrap etc.) e **ausência de CSS sem `@layer`** vencendo o design system.
- [ ] **Cultura pt-BR**, separador decimal e `InputNumber` (seção 8.2).
- [ ] Arrastar-e-soltar de arquivos no `InputFile` dentro do WebView2.

### Plano sugerido de implementação
1. Auditar o repositório e remover CSS do template (3.4).
2. Embutir fonte e ícones; criar `00-layers`, `01-tokens`, `02-reset`, `03-base`.
3. Implementar shell e navegação (`04-layout`, `nav`).
4. Primitivos: botão, campo, card, badge, alerta.
5. Tabela e lista de planos; depois modal/drawer e toasts.
6. Wizard: stepper, planilha, dropzone, preview, exportação.
7. Rodar a lista da seção 13 e só então marcar como concluído.

---

## 14. Pendências

### 14.1 Decisões pendentes

| ID | Decisão | Quem decide | Impacto |
|---|---|---|---|
| P1 | Confirmar glassmorphism no lugar do claymorphism do brief e **atualizar brief §10/§19** | Equipe | Coerência da "fonte da verdade" |
| P2 | Sidebar azul (proposta) × sidebar clara/glass | Equipe | 4 tokens `--rp-sidebar-*` |
| P3 | Fonte de UI (Inter proposta), licença e arquivo | Equipe / cliente | `fonts.css`, tokens |
| P4 | Haverá tema escuro? | Equipe | Camada extra de tokens |
| P5 | Logo em versão branca/monocromática | Cliente | Marca sobre a sidebar azul |
| P6 | Resolução/escala mínima e tamanho mínimo da janela | Equipe / Giuliano | Densidade, sidebar |
| P7 | Conjunto de ícones e como embutir | Equipe | `icon.css`, componente `Icon` |
| P8 | Limiares de "atenção/crítico" para taxas de utilização (brief só cita 85% de margem) | Giuliano | `rp-meter`, alertas |
| P9 | Estados de um plano (rascunho/finalizado?) e autosave | Equipe / Giuliano | Badges e fluxo do wizard |
| P10 | Colunas da lista de planos, ordenação, busca, paginação/virtualização | Equipe | Tabela |
| P11 | Valores de "situação" do acessório e regras de alerta de desgaste/descarte | Giuliano | `rp-badge`, `rp-alert` |
| P12 | Alerta de **laudo vencido** (Dúvida 9) | Giuliano | `rp-alert` |
| P13 | Preview = o próprio PDF exportado? | Equipe | Segurança dos dados |
| P14 | Biblioteca de PDF (se HTML→PDF, será preciso CSS de impressão) | Equipe | Novo arquivo `print.css` |
| P15 | Cadastro de acessórios em **drawer** ou modal | Equipe | Layout da tela |
| P16 | Existe exclusão de plano/pasta/acessório? | Giuliano | Ações perigosas |
| P17 | Cultura e formato numérico (pt-BR) e campos numéricos | Equipe | Campos de dados |
| P18 | Onde o PDF é gravado (Dúvida 10) | Giuliano | Mensagens da etapa 5 |
| P19 | Vários `<link>` ou CSS concatenado | Equipe | Organização |
| P20 | Estado real do repositório: Bootstrap/CSS do template ainda presentes? | Equipe | Seção 3.4 |

### 14.2 Perguntas objetivas que podem alterar o design system
1. **Resolução e escala** dos monitores usados pelo Giuliano (ex.: 1366×768, 1920×1080, escala 125%)? Isso define a densidade padrão das tabelas.
2. A **sidebar azul-marinho** agrada, ou prefere uma barra lateral clara?
3. Existe **licença/arquivo** das fontes da marca (Arian LT e a fonte dos títulos)? Se sim, entram nos títulos; se não, mantenho Inter + Michroma.
4. Há **versão branca do logo** (ou SVG)?
5. O app terá **tema escuro** em alguma fase?
6. O repositório ainda contém o **CSS/Bootstrap do template MAUI Blazor**?

---

## 15. Resumo executivo

- **Estratégia:** CSS puro, tokens em custom properties, `@layer`, componentes `rp-*` globais e CSS isolation só para layout de página; **sem Bootstrap e sem bibliotecas** (brief F3).
- **Cor primária:** `#003F68` como `--rp-primary-700`, rampa derivada de 50 a 950, neutros azulados, status em verde/âmbar/vermelho.
- **Glass sutil** apenas em topbar, rodapé do wizard, painéis de resumo, menus, toasts e modais; **tabelas, planilha e formulários permanecem opacos** para legibilidade.
- **Segurança de dados na UI:** unidades visíveis, números tabulares sem truncar, campos calculados distintos, sem correção silenciosa, confirmações antes de perder dados.
- **Nada foi testado.** A seção 13 lista o que precisa ser verificado no app real.
