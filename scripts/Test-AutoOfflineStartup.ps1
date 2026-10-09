[CmdletBinding()]
param(
    [string]$GamePath = 'D:\SteamLibrary\steamapps\common\Last Epoch',
    [switch]$BuildOnly
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot 'LastEpoch_Hud\LastEpoch_Hud.csproj'
$gameAssembly = Join-Path $GamePath 'MelonLoader\Il2CppAssemblies\Il2CppLE.dll'
if (-not (Test-Path -LiteralPath $gameAssembly)) {
    throw "Game assemblies not found at $gameAssembly. Launch once with MelonLoader first."
}
if (-not $BuildOnly -and (Get-Process -Name 'Last Epoch' -ErrorAction SilentlyContinue)) {
    throw 'Close Last Epoch before installing the test DLL.'
}

& dotnet build $project -c Release "-p:LastEpochPath=$GamePath"
if ($LASTEXITCODE -ne 0) { throw 'Build failed; the installed mod was not changed.' }
$builtDll = Join-Path $repoRoot 'Build\Release\net6.0\LastEpoch_Hud.dll'
if (-not (Test-Path -LiteralPath $builtDll)) { throw "Build output not found: $builtDll" }
if ($BuildOnly) {
    Write-Host "Built: $builtDll"
    return
}

$installedDll = Join-Path $GamePath 'Mods\LastEpoch_Hud.dll'
if (Test-Path -LiteralPath $installedDll) {
    $backupDir = Join-Path $GamePath ('UserData\LastEpoch_Hud\test-backups\auto-offline-' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    $backupDll = Join-Path $backupDir 'LastEpoch_Hud.dll'
    Copy-Item -LiteralPath $installedDll -Destination $backupDll
    Write-Host "Previous DLL backed up to: $backupDll"
}
New-Item -ItemType Directory -Path (Split-Path -Parent $installedDll) -Force | Out-Null
Copy-Item -LiteralPath $builtDll -Destination $installedDll -Force
Write-Host 'Installed the auto-offline test DLL. Existing assets and saves were left in place.'
Write-Host 'Launch normally. Expect offline character selection without clicking Play Offline.'
Write-Host 'Look for [Offline] messages in MelonLoader\Latest.log.'
