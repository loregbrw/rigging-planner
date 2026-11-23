param(
    [string]$Version = "0.0.0-dev"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src/RiggingPlanner.App/RiggingPlanner.App.csproj"
$artifacts = Join-Path $root "artifacts"
$publishDir = Join-Path $artifacts "publish"
$staging = Join-Path $artifacts "RiggingPlanner"
$zip = Join-Path $artifacts "RiggingPlanner-$Version-win-x64.zip"
$sha = "$zip.sha256"

foreach ($path in @($publishDir, $staging)) {
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Recurse -Force
    }
}

Write-Host "Publicando RiggingPlanner $Version (win-x64, self-contained)..."
dotnet publish $project -c Release -r win-x64 --self-contained true -p:Version=$Version -o $publishDir
if ($LASTEXITCODE -ne 0) {
    throw "Falha no dotnet publish."
}

New-Item -ItemType Directory -Path $staging -Force | Out-Null
Copy-Item -Path (Join-Path $publishDir "*") -Destination $staging -Recurse -Force

$readme = @"
Rigging Planner $Version

COMO ABRIR
Extraia o ZIP em uma pasta de sua escolha e execute RiggingPlanner\RiggingPlanner.App.exe.

NADA PRECISA SER INSTALADO
O pacote já contém o .NET, o Windows App SDK, o SQLite e as bibliotecas gráficas.

ONDE FICAM OS DADOS
Banco: %LOCALAPPDATA%\RiggingPlanner
Anexos: %LOCALAPPDATA%\Guindastes Ribas\Rigging Planner\Data
Os dados ficam no perfil do usuário, nunca na pasta do aplicativo.

ATUALIZANDO
Descompacte a nova versão sobre a pasta antiga. Os dados são preservados.

REQUISITO
Microsoft Edge WebView2 Runtime (já vem no Windows 11 e na maioria dos Windows 10 atualizados).

SMARTSCREEN
O executável não é assinado: se o Windows avisar, clique em "Mais informações" > "Executar mesmo assim".
"@
[System.IO.File]::WriteAllText(
    (Join-Path $staging "LEIA-ME.txt"),
    $readme.Replace("`r`n", "`n").Replace("`n", "`r`n"),
    [System.Text.UTF8Encoding]::new($false))

if (Test-Path -LiteralPath $zip) {
    Remove-Item -LiteralPath $zip -Force
}
Compress-Archive -Path $staging -DestinationPath $zip -CompressionLevel Optimal

$hash = (Get-FileHash -Algorithm SHA256 -Path $zip).Hash.ToLowerInvariant()
[System.IO.File]::WriteAllText(
    $sha,
    "$hash  $([System.IO.Path]::GetFileName($zip))`r`n",
    [System.Text.UTF8Encoding]::new($false))

$size = [math]::Round((Get-Item -LiteralPath $zip).Length / 1MB, 1)
Write-Host "Pacote: $zip"
Write-Host "Tamanho: $size MB"
Write-Host "SHA256: $hash"
