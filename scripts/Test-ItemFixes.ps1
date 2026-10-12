param([string]$GamePath = "D:\SteamLibrary\steamapps\common\Last Epoch")

$ErrorActionPreference = "Stop"
$branch = "fix/illegal-unique-set-tooltips"
$repoPath = Split-Path -Parent $PSScriptRoot

function Assert-LastEpochExited {
    foreach ($gameProcess in @(Get-Process -Name "Last Epoch" -ErrorAction SilentlyContinue)) {
        try {
            $gameProcess.Refresh()
            # Windows can retain a process entry after it has exited.
            # Check termination state instead of treating the name as proof of life.
            if (!$gameProcess.HasExited) {
                throw "Close Last Epoch before installing the testing build. Active PID: $($gameProcess.Id)."
            }
        } finally {
            $gameProcess.Dispose()
        }
    }
}

Push-Location $repoPath
try {
    Assert-LastEpochExited
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
    if (!(Test-Path -LiteralPath $modDll -PathType Leaf)) {
        throw "Built DLL was not found; nothing was installed."
    }
    Assert-LastEpochExited
    $installedDll = Join-Path $GamePath "Mods\LastEpoch_Hud.dll"
    if (Test-Path -LiteralPath $installedDll) {
        $stamp = Get-Date -Format "yyyyMMdd_HHmmss"
        Copy-Item -LiteralPath $installedDll -Destination "$installedDll.before-item-fixes-$stamp"
    }
    Copy-Item -LiteralPath $modDll -Destination $installedDll -Force
    Write-Host "Installed item-fix test build $localHead."
    Write-Host "Illegal On: drop a Unique with four affixes + sealed + Primordial + corrupted; pick up and reload."
    Write-Host "Illegal Off: confirm Unique/Set sealed and Primordial slots stay locked, including after Fixed/Random changes."
    Write-Host "Equip distinct illegal Set pieces with Remove Set Requirements OFF; check bonuses, duplicates and reload."
    Write-Host "Hover ground loot before/during combat; send the [Ground Tooltip] lines if the tooltip fails."
} finally {
    Pop-Location
}
