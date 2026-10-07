param(
    [string]$GamePath = "D:\SteamLibrary\steamapps\common\Last Epoch"
)

$ErrorActionPreference = "Stop"
$repoPath = Split-Path -Parent $PSScriptRoot
Push-Location $repoPath
try {
    if (Get-Process -Name "Last Epoch" -ErrorAction SilentlyContinue) {
        throw "Close Last Epoch before installing the testing build."
    }
    $branch = git branch --show-current
    if ($LASTEXITCODE -ne 0 -or $branch -notin @("feat/maxroll-build-preview", "feat/maxroll-tree-preview")) {
        throw "Switch to feat/maxroll-build-preview or feat/maxroll-tree-preview and pull its latest changes first."
    }
    if (!(Test-Path -LiteralPath (Join-Path $GamePath "Mods") -PathType Container)) {
        throw "The game Mods folder was not found. Check -GamePath and install MelonLoader first."
    }

    dotnet build .\LastEpoch_Hud\LastEpoch_Hud.csproj -c Release "-p:LastEpochPath=$GamePath"
    if ($LASTEXITCODE -ne 0) { throw "Build failed; installed DLL was not replaced." }

    $previousGamePath = $env:LAST_EPOCH_PATH
    $previousModDll = $env:LAST_EPOCH_MOD_DLL
    $modDll = Join-Path $repoPath "Build\Release\net6.0\LastEpoch_Hud.dll"
    try {
        $env:LAST_EPOCH_PATH = $GamePath
        $env:LAST_EPOCH_MOD_DLL = $modDll
        Write-Host "Validating $modDll"
        dotnet run --project .\LastEpoch_Hud.Tests
        if ($LASTEXITCODE -ne 0) { throw "Tests failed; installed DLL was not replaced." }
    } finally {
        $env:LAST_EPOCH_PATH = $previousGamePath
        $env:LAST_EPOCH_MOD_DLL = $previousModDll
    }

    Copy-Item $modDll (Join-Path $GamePath "Mods\LastEpoch_Hud.dll") -Force
    $localePath = Join-Path $GamePath "Mods\LastEpoch_Hud\Locales"
    New-Item -ItemType Directory -Path $localePath -Force | Out-Null
    foreach ($language in @("en", "fr", "ko", "zh")) {
        Copy-Item ".\LastEpoch_Hud\LastEpoch_Hud\Locales\$language.json" (Join-Path $localePath "$language.json") -Force
    }
    Write-Host "Installed Maxroll preview. Open Items > Force Drop > Maxroll Build Preview."
} finally {
    Pop-Location
}
