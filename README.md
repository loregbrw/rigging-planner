# Rigging Planner

Aplicativo Windows para montar o **Plano de Rigging** da Guindastes Ribas: os dados da operação são informados uma única vez e reaproveitados em todas as etapas, até a exportação do PDF final (folha-resumo, desenhos, ART, procedimento operacional e laudo do equipamento).

> **Download mais recente:** [v1.0.0 — instalador Windows](releases/v1.0.0/RiggingPlanner-1.0.0-win-x64-setup.exe) · [SHA-256](releases/v1.0.0/RiggingPlanner-1.0.0-win-x64-setup.exe.sha256)

Projeto da disciplina *Jornada de Aprendizagem — Sistemas Inteligentes Conectados* (equipe XHG).

## O que o sistema faz

- Cadastro de clientes, equipamentos, responsáveis técnicos e ART/laudos.
- Plano numerado automaticamente no formato `AAMMNNN`, com mês tirado da data do plano.
- Cálculo de pesos, lingadas/dispositivos e taxa de utilização do guindaste.
- Anexos: ART do plano e folhas de desenho (imagens), com hash SHA-256.
- Pré-visualização e exportação do PDF completo.
- Finalização do plano: congela o texto do procedimento operacional em um snapshot para que alterações futuras nos cadastros não mudem planos já emitidos.

## Como usar

1. **Instale** o executável da versão desejada (seção abaixo) e abra o Rigging Planner.
2. Na tela **Planos**, clique em **Novo plano**.
3. Preencha a **entrada**: operação, local, data, cliente, equipamento, responsável e carga. valores decimais usam vírgula (`RpDecimalInput`).
4. Avance para **Capacidade**: informe lingadas/dispositivos e os parâmetros do guindaste. Os totais são calculados automaticamente.
5. Em **Anexos**, envie a ART do plano e as imagens dos desenhos.
6. Em **Pré-visualização**, confira as páginas do PDF que será gerado.
7. Em **Exportação**, use **Salvar e exportar** para gerar o PDF em `Downloads`, ou **Finalizar plano** para torná-lo somente leitura.

O botão **Salvar** grava o rascunho; sair da tela com alterações sem salvar dispara um aviso. O título da página mostra `Plano {número} — {operação}`.

### Onde ficam os dados

- Banco SQLite: `%LOCALAPPDATA%\RiggingPlanner\rigging-planner.db`.
- Anexos: mesma pasta de dados do aplicativo, nunca na pasta de instalação.
- Atualizar o app preserva banco e anexos; desinstalar remove apenas o aplicativo.

## Versões e downloads

As versões seguem `vX.Y.Z`. Cada tag gera um Release no GitHub e uma pasta `releases/vX.Y.Z/` com o instalador e seu hash, ambos versionados no repositório.

| Versão | Instalador | SHA-256 | Tamanho |
|---|---|---|---|
| v1.0.0 | [setup.exe](releases/v1.0.0/RiggingPlanner-1.0.0-win-x64-setup.exe) | [.sha256](releases/v1.0.0/RiggingPlanner-1.0.0-win-x64-setup.exe.sha256) | 79,6 MB |

O instalador é autocontido (`win-x64`), não assinado digitalmente e exige **Microsoft Edge WebView2 Runtime**. No primeiro uso o Windows pode exibir o aviso do SmartScreen.

### Gerar uma nova versão

```powershell
git tag v1.1.0
git push origin v1.1.0
```

O workflow `.github/workflows/release.yml` instala MAUI e Inno Setup, roda build e testes, gera o instalador, salva-o em `releases/v1.1.0/`, comita essa pasta em `main`, cria o Release e anexa `.exe` + `.sha256`. Também é possível disparar manualmente em **Actions → Release → Run workflow**.

Para gerar localmente:

```powershell
choco install innosetup -y
./scripts/publish.ps1 -Version 1.1.0
./scripts/store-installer.ps1 -Version 1.1.0
```

## Desenvolvimento

Requisitos: Windows 10/11, .NET 10 SDK (`global.json`) e workload MAUI (`dotnet workload install maui`).

```powershell
dotnet build RiggingPlanner.slnx -c Debug
dotnet test RiggingPlanner.slnx -c Debug
dotnet run --project src/RiggingPlanner.App
```

O aplicativo tem interface gráfica; não execute `dotnet run` em terminal não interativo.

```text
src/RiggingPlanner.Domain          entidades, regras e value objects
src/RiggingPlanner.Application     portas, DTOs, validação e builders de documento
src/RiggingPlanner.Infrastructure  EF Core/SQLite, arquivos e renderização de PDF
src/RiggingPlanner.App             interface Blazor Hybrid e estado de apresentação
tests/                             testes xUnit de Domain e Infrastructure
releases/                          instaladores versionados por release
scripts/                           publicação, instalador(versionamento) e setup.iss
```

## Documentação

- [Arquitetura](docs/architecture.md) — camadas, composição e estado.
- [Regras de domínio](docs/domain-rules.md) — numeração, snapshot, valores derivados e ciclo de vida.
- [Design system](docs/design-system.md) — tokens CSS e componentes.
- [Procedimento operacional](docs/procedure-spec.md) — texto fixo e variáveis.
- [Exportação PDF](docs/pdf-export.md) — montagem do documento e fontes.
- [Releases](docs/release.md) — detalhes do instalador e atualização.

Convenções: identificadores em inglês, textos de interface em português, a UI nunca acessa EF Core ou disco diretamente e as mensagens de commit seguem Conventional Commits em inglês minúsculo.
