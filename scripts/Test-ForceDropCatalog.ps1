param([string]$GamePath = "D:\SteamLibrary\steamapps\common\Last Epoch")
$ErrorActionPreference = "Stop"
$branch = "audit/force-drop-catalog"
Set-Location (Split-Path $PSScriptRoot -Parent)
if (Get-Process -Name "Last Epoch" -ErrorAction SilentlyContinue) {
    throw "Close Last Epoch before installing the audit build."
}
if (git status --porcelain --untracked-files=no) { throw "Commit or stash tracked changes first" }
git fetch origin
if ($LASTEXITCODE -ne 0) { throw "Fetch failed" }
git show-ref --verify --quiet "refs/heads/$branch"
if ($LASTEXITCODE -eq 0) { git switch $branch } else { git switch --track "origin/$branch" }
if ($LASTEXITCODE -ne 0) { throw "Branch switch failed" }
git pull --ff-only origin $branch
if ($LASTEXITCODE -ne 0) { throw "Update failed" }
$localHead = git rev-parse HEAD
$remoteHead = git rev-parse "origin/$branch"
if ($localHead -ne $remoteHead) { throw "Local branch differs from origin" }

$settingsPath = Join-Path $GamePath "Mods\LastEpoch_Hud\SaveModUI.json"
if (!(Test-Path $settingsPath)) { throw "Launch your existing mod once, then close the game to create SaveModUI.json" }
$settings = Get-Content -Raw $settingsPath | ConvertFrom-Json
if (!$settings.PSObject.Properties["ForceDropAudit"]) {
    $settings | Add-Member -NotePropertyName ForceDropAudit -NotePropertyValue ([pscustomobject]@{ DumpOnNextLaunch = $true })
} else {
    $settings.ForceDropAudit | Add-Member -NotePropertyName DumpOnNextLaunch -NotePropertyValue $true -Force
}
$settingsJson = $settings | ConvertTo-Json -Depth 100

dotnet build .\LastEpoch_Hud\LastEpoch_Hud.csproj -c Keyboard -p:LastEpochPath="$GamePath"
if ($LASTEXITCODE -ne 0) { throw "Build failed; installed DLL was not replaced" }
$env:LAST_EPOCH_PATH = $GamePath
dotnet run --project .\LastEpoch_Hud.Tests
if ($LASTEXITCODE -ne 0) { throw "Tests failed; installed DLL was not replaced" }

$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$dllPath = Join-Path $GamePath "Mods\LastEpoch_Hud.dll"
if (Test-Path $dllPath) { Copy-Item $dllPath "$dllPath.before-audit-$stamp" }
Copy-Item $settingsPath "$settingsPath.before-audit-$stamp"
Copy-Item .\Build\Keyboard\net6.0\LastEpoch_Hud.dll $dllPath -Force
[System.IO.File]::WriteAllText($settingsPath, $settingsJson, [System.Text.UTF8Encoding]::new($false))
Write-Host "Installed $localHead. Enter a character in town; the catalog exports once."
Write-Host "Output: $GamePath\Mods\LastEpoch_Hud\ForceDropCatalog_*.json"
Write-Host "The audit request turns off after a successful export. See docs/TEST_FORCE_DROP_CATALOG.md."
