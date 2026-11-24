param(
    [string]$Version = "0.0.0-dev"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$artifacts = Join-Path $root "artifacts"
$installer = Join-Path $artifacts "RiggingPlanner-$Version-win-x64-setup.exe"
$checksum = "$installer.sha256"
$target = Join-Path $root "releases/v$Version"

if (-not (Test-Path -LiteralPath $installer)) {
    throw "Instalador não encontrado: $installer"
}
if (-not (Test-Path -LiteralPath $checksum)) {
    throw "Arquivo SHA-256 não encontrado: $checksum"
}

New-Item -ItemType Directory -Path $target -Force | Out-Null
Copy-Item -LiteralPath $installer -Destination $target -Force
Copy-Item -LiteralPath $checksum -Destination $target -Force

$hash = (Get-Content -LiteralPath $checksum).Split(" ")[0]
$size = [math]::Round((Get-Item -LiteralPath $installer).Length / 1MB, 1)
$fileName = [System.IO.Path]::GetFileName($installer)

$notes = @"
# Rigging Planner v$Version

- Instalador: [$fileName]($fileName)
- SHA-256: ``$hash``
- Tamanho: $size MB
- Publicado em: $(Get-Date -Format "yyyy-MM-dd")
"@
[System.IO.File]::WriteAllText(
    (Join-Path $target "README.md"),
    "$notes`r`n",
    [System.Text.UTF8Encoding]::new($false))

Write-Host "Instalador versionado em: $target"
