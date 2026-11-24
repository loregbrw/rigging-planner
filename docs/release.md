# Releases

## Gerar localmente
Requisitos: .NET conforme `global.json`, workload MAUI, Windows x64 e **Inno Setup 6** (`choco install innosetup -y`).

```powershell
./scripts/publish.ps1 -Version 1.2.3
```

O script publica o app self-contained, compila `scripts/setup.iss` e gera:

- `artifacts/RiggingPlanner-1.2.3-win-x64-setup.exe` — instalador;
- `artifacts/RiggingPlanner-1.2.3-win-x64-setup.exe.sha256` — hash SHA-256.

Padrão sem versão: `0.0.0-dev`. `artifacts/` não é versionado. O instalador exclui os `.pdb` do publish, o que reduz o pacote de ~358 MB para ~80 MB.

## Instalador
O assistente (em português) mostra uma tela inicial de informações, pede a pasta de destino (padrão: `Arquivos de Programas\Rigging Planner`), permite escolher atalho na área de trabalho e exige confirmação antes de instalar. Registra entrada em "Aplicativos e recursos" com desinstalador.

## Lançar uma versão
Crie e envie uma tag `vX.Y.Z` (ex.: `git tag v1.2.3 && git push origin v1.2.3`). O workflow `release.yml` instala o Inno Setup, roda build, testes, publica o instalador e o SHA-256 e cria o Release com notas automáticas. Também pode ser disparado manualmente informando a versão.

## Dados do usuário
Banco: `%LOCALAPPDATA%\RiggingPlanner\rigging-planner.db`.
Anexos: pasta de dados do aplicativo no perfil do usuário.
Nada fica na pasta de instalação. A instalação de uma versão nova sobre a anterior **preserva** banco e anexos; desinstalar remove só o aplicativo.

O instalador é self-contained (não instala .NET, SQLite ou Windows App SDK) e não é assinado digitalmente — o Windows pode exibir o aviso do SmartScreen na primeira execução. Requer Edge WebView2 Runtime.

## Atualizar
Execute o instalador da versão nova. Não é necessário desinstalar a anterior; planos e anexos são preservados.
