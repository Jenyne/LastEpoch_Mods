param([string]$GamePath = "D:\SteamLibrary\steamapps\common\Last Epoch")

$ErrorActionPreference = "Stop"
$branch = "fix/force-drop-integrity"
$repoPath = Split-Path -Parent $PSScriptRoot
Push-Location $repoPath
try {
    if (Get-Process -Name "Last Epoch" -ErrorAction SilentlyContinue) {
        throw "Close Last Epoch before installing the testing build."
    }
    if (!(Test-Path -LiteralPath (Join-Path $GamePath "Mods") -PathType Container)) {
        throw "The game Mods folder was not found. Check -GamePath."
    }
    $changes = git status --porcelain --untracked-files=no
    if ($LASTEXITCODE -ne 0) { throw "Cannot read repository status." }
    if ($changes) { throw "Commit or stash tracked changes before changing branches." }
    git fetch origin
    if ($LASTEXITCODE -ne 0) { throw "Fetch failed." }
    git show-ref --verify --quiet "refs/heads/$branch"
    if ($LASTEXITCODE -eq 0) {
        git switch $branch
    } else {
        git switch --track "origin/$branch"
    }
    if ($LASTEXITCODE -ne 0) { throw "Branch switch failed." }
    git pull --ff-only origin $branch
    if ($LASTEXITCODE -ne 0) { throw "Update failed." }
    $localHead = git rev-parse HEAD
    if ($LASTEXITCODE -ne 0) { throw "Cannot read the local revision." }
    $remoteHead = git rev-parse "origin/$branch"
    if ($LASTEXITCODE -ne 0 -or $localHead -ne $remoteHead) {
        throw "The local branch differs from origin; nothing was installed."
    }

    dotnet build .\LastEpoch_Hud\LastEpoch_Hud.csproj -c Release "-p:LastEpochPath=$GamePath"
    if ($LASTEXITCODE -ne 0) { throw "Build failed; installed DLL was not replaced." }
    $modDll = Join-Path $repoPath "Build\Release\net6.0\LastEpoch_Hud.dll"
    $previousGamePath = $env:LAST_EPOCH_PATH
    $previousModDll = $env:LAST_EPOCH_MOD_DLL
    try {
        $env:LAST_EPOCH_PATH = $GamePath
        $env:LAST_EPOCH_MOD_DLL = $modDll
        dotnet run --project .\LastEpoch_Hud.Tests
        if ($LASTEXITCODE -ne 0) { throw "Tests failed; installed DLL was not replaced." }
    } finally {
        $env:LAST_EPOCH_PATH = $previousGamePath
        $env:LAST_EPOCH_MOD_DLL = $previousModDll
    }
    $installedDll = Join-Path $GamePath "Mods\LastEpoch_Hud.dll"
    if (Test-Path -LiteralPath $installedDll) {
        $stamp = Get-Date -Format "yyyyMMdd_HHmmss"
        Copy-Item -LiteralPath $installedDll -Destination "$installedDll.before-force-drop-$stamp"
    }
    Copy-Item -LiteralPath $modDll -Destination $installedDll -Force
    Write-Host "Installed Force Drop integrity test build $localHead."
    Write-Host "Open Items > Force Drop. See docs/TEST_FORCE_DROP_INTEGRITY.md for the tests."
} finally {
    Pop-Location
}
