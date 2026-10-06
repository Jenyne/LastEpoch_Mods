$ErrorActionPreference = "Stop"
$branch = "feat/idol-reroll-misc"
$gamePath = "D:\SteamLibrary\steamapps\common\Last Epoch"

if (Get-Process -Name "Last Epoch" -ErrorAction SilentlyContinue) {
    throw "Close Last Epoch before installing the testing build."
}
git fetch origin
if ($LASTEXITCODE -ne 0) { throw "Fetch failed" }
git show-ref --verify --quiet "refs/heads/$branch"
if ($LASTEXITCODE -eq 0) {
    git switch $branch
} else {
    git switch --track "origin/$branch"
}
if ($LASTEXITCODE -ne 0) { throw "Branch switch failed" }
git pull --ff-only origin $branch
if ($LASTEXITCODE -ne 0) { throw "Update failed" }
$localHead = git rev-parse HEAD
$remoteHead = git rev-parse "origin/$branch"
if ($localHead -ne $remoteHead) { throw "Local branch differs from origin" }
if (git status --porcelain --untracked-files=no) { throw "Commit or stash tracked changes first" }

dotnet build .\LastEpoch_Hud\LastEpoch_Hud.csproj -c Keyboard -p:LastEpochPath="$gamePath"
if ($LASTEXITCODE -ne 0) { throw "Build failed; installed DLL was not replaced" }
$env:LAST_EPOCH_PATH = $gamePath
dotnet run --project .\LastEpoch_Hud.Tests
if ($LASTEXITCODE -ne 0) { throw "Tests failed; installed DLL was not replaced" }
Copy-Item .\Build\Keyboard\net6.0\LastEpoch_Hud.dll "$gamePath\Mods\LastEpoch_Hud.dll" -Force
$localePath = "$gamePath\Mods\LastEpoch_Hud\Locales"
New-Item -ItemType Directory -Path $localePath -Force | Out-Null
foreach ($language in @("en", "fr", "ko", "zh")) {
    Copy-Item ".\LastEpoch_Hud\LastEpoch_Hud\Locales\$language.json" "$localePath\$language.json" -Force
}
Write-Host "Installed $localHead. Test Scenes > Misc > Idol Rerolling."
