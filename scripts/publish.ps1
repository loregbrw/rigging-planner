param(
    [string]$Version = "0.0.0-dev"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src/RiggingPlanner.App/RiggingPlanner.App.csproj"
$artifacts = Join-Path $root "artifacts"
$publishDir = Join-Path $artifacts "publish"
$innoScript = Join-Path $PSScriptRoot "setup.iss"

function Find-InnoCompiler {
    $command = Get-Command iscc -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $roots = @()
    if ($env:ProgramFiles) { $roots += $env:ProgramFiles }
    if (${env:ProgramFiles(x86)}) { $roots += ${env:ProgramFiles(x86)} }
    if ($env:LOCALAPPDATA) { $roots += (Join-Path $env:LOCALAPPDATA "Programs") }

    foreach ($base in $roots) {
        $candidate = Join-Path $base "Inno Setup 6\ISCC.exe"
        if (Test-Path -LiteralPath $candidate) {
            return $candidate
        }
    }

    throw "Inno Setup 6 não encontrado. Instale com: choco install innosetup -y"
}

if (Test-Path -LiteralPath $publishDir) {
    Remove-Item -LiteralPath $publishDir -Recurse -Force
}

Write-Host "Publicando Rigging Planner $Version (win-x64, self-contained)..."
dotnet publish $project -c Release -r win-x64 --self-contained true -p:Version=$Version -o $publishDir
if ($LASTEXITCODE -ne 0) {
    throw "Falha no dotnet publish."
}

$versionMatch = [regex]::Match($Version, '(\d+)\.(\d+)\.(\d+)')
if ($versionMatch.Success) {
    $fileVersion = "$($versionMatch.Groups[1].Value).$($versionMatch.Groups[2].Value).$($versionMatch.Groups[3].Value).0"
}
else {
    $fileVersion = "0.0.0.0"
}

$compiler = Find-InnoCompiler
Write-Host "Gerando instalador com $compiler..."
& $compiler "/q" "/dAppVersion=$Version" "/dVersionInfoVersion=$fileVersion" $innoScript
if ($LASTEXITCODE -ne 0) {
    throw "Falha na compilação do instalador."
}

$installer = Join-Path $artifacts "RiggingPlanner-$Version-win-x64-setup.exe"
if (-not (Test-Path -LiteralPath $installer)) {
    throw "Instalador não foi gerado: $installer"
}

foreach ($old in @("RiggingPlanner-*-win-x64.zip", "RiggingPlanner-*-win-x64.zip.sha256")) {
    Get-ChildItem -LiteralPath $artifacts -Filter $old -File -ErrorAction SilentlyContinue |
        Remove-Item -Force
}

$hash = (Get-FileHash -Algorithm SHA256 -Path $installer).Hash.ToLowerInvariant()
$sha = "$installer.sha256"
[System.IO.File]::WriteAllText(
    $sha,
    "$hash  $([System.IO.Path]::GetFileName($installer))`r`n",
    [System.Text.UTF8Encoding]::new($false))

$size = [math]::Round((Get-Item -LiteralPath $installer).Length / 1MB, 1)
Write-Host "Instalador: $installer"
Write-Host "Tamanho: $size MB"
Write-Host "SHA256: $hash"
