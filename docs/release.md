# Releases

## Gerar localmente
Requisitos: .NET conforme `global.json`, workload MAUI e Windows x64.

```powershell
./scripts/publish.ps1 -Version 1.2.3
```

Gera `artifacts/RiggingPlanner-1.2.3-win-x64.zip` (pasta `RiggingPlanner` com o `.exe` e o `LEIA-ME.txt`) e o `.sha256`. Padrão sem versão: `0.0.0-dev`. `artifacts/` não é versionado.

## Lançar uma versão
Crie e envie uma tag `vX.Y.Z` (ex.: `git tag v1.2.3 && git push origin v1.2.3`). O workflow `release.yml` roda build, testes, publica o ZIP e o SHA-256 e cria o Release com notas automáticas. Também pode ser disparado manualmente informando a versão.

## Dados do usuário
Banco: `%LOCALAPPDATA%\RiggingPlanner\rigging-planner.db`.
Anexos: `%LOCALAPPDATA%\Guindastes Ribas\RiggingPlanner.App\Data`.
Nada fica na pasta do executável; o pacote é self-contained (não instala .NET, SQLite ou Windows App SDK). Requer Edge WebView2 Runtime.

## Atualizar
Descompacte o ZIP novo sobre a pasta antiga; banco e anexos são preservados.
