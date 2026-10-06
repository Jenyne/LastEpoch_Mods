# Completed fixes integration test

Branch: `test/completed-fixes-integration`

Base: Syncingoutt/master at `35c6a14efd3afa77ebd57a892bb0db15c46673e2`.

This is a combined regression build. The individual completed features were tested in game; their interaction in this build still needs testing. The original task branches remain available.

## Included changes and checks

| Feature | Where | Regression check |
| --- | --- | --- |
| Dungeon objective reveal | Scenes → Dungeons | Reveal on multiple floors, complete a run, and leave enabled for several minutes without a crash. Toggle on while already on a floor. |
| Enter Without Key | Scenes → Dungeons | Enter without a key; select an optional portal charm before continuing; confirm the charm modifier applies. |
| Ignore Weapon Restrictions | Character → Cheats | Equip 2H + 2H, 2H + shield, and 2H + 1H; confirm both items contribute stats and survive save/reload. |
| Safe Teleport | Scenes → Minimap | Capture Shift+Q or Ctrl+Q, teleport, restart and confirm the binding persists. Verify AutoCast and Safe Teleport reject conflicting bindings. |
| Infinite Forging Potential | Items → crafting controls | Craft with the option enabled and confirm potential stays unchanged. Disable it and confirm normal cost resumes. Deselect All should clear other crafting cheats while leaving Infinite untouched. |
| Add 10,000 Memory Amber | Character → Cheats | As a Woven member, click once and confirm exactly +10,000, including with the multiplier enabled. Check save/reload. |
| Soul Embers amount fix | Character | Enter a known amount and confirm a single click grants exactly that amount. Reopen the HUD and repeat. This fix still needs explicit gameplay confirmation. |
| Withstand the Elements | Force Drop → unique gloves | Choose two distinct exclusive modifiers and LP, drop, equip, and save/reload. Confirm Unsated Rage still uses one separate modifier. |
| Native item translations | Force Drop | In Korean or another supported game language, check categories, base items, uniques, affixes, both glove modifier labels, and search using translated and English names. |

Blessings, Unsated Rage, prior icon fixes, locale shipping, and the earlier corruption packing fix are already inherited from master. Spot-check them after the integration. This branch does not include the unfinished Illegal mode/corruption pool changes, prophecy multiplier, idol rerolls, tooltip diagnostics, independent drop rates, or custom idol diagnostics.

## Build and install on Windows

Close Last Epoch first. Run from `D:\GitHub\LastEpoch_Mods`:

```powershell
$branch = "test/completed-fixes-integration"
$gamePath = "D:\SteamLibrary\steamapps\common\Last Epoch"

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
if ($localHead -ne $remoteHead) { throw "Local branch differs from the testing branch" }
$trackedChanges = git status --porcelain --untracked-files=no
if ($trackedChanges) { throw "Commit or stash tracked changes before testing" }

dotnet build .\LastEpoch_Hud\LastEpoch_Hud.csproj -c Keyboard -p:LastEpochPath="$gamePath"
if ($LASTEXITCODE -ne 0) { throw "Build failed; installed DLL was not replaced" }
Copy-Item .\Build\Keyboard\net6.0\LastEpoch_Hud.dll "$gamePath\Mods\LastEpoch_Hud.dll" -Force
Write-Host "Installed commit $localHead"
```

For translated HUD labels, manually copy the updated `en.json` and your language's JSON from `LastEpoch_Hud\LastEpoch_Hud\Locales` into `Last Epoch\Mods\LastEpoch_Hud\Locales`. Keep the filenames unchanged. See `docs/LOCALE_INSTALLATION.md` for each language's instructions.

Optional repository checks after building:

```powershell
$env:LAST_EPOCH_PATH = $gamePath
dotnet run --project .\LastEpoch_Hud.Tests
```

## Sources

- Dungeons: `d80e068765ead58ef0904442a441a7c31b22ae31`
- Weapon restrictions: `ccfe4c059d38b82d76a12b7e665136f5da043fed`
- Safe Teleport: `ae7cf0ff8ab77b433b5e1edec560208182a2b064`
- Infinite Forging Potential: `3c4f56251f25fd9a68a111ca515d22669572bea9`
- Memory Amber / Soul Embers: `f5bfa5956e319a204a49751c554e1b0b6e9520c1`
- Withstand the Elements: `bfe611d9ce6f5f75c79c0ed1b8999a3d356243ec`
- Native item locales: `de04feee2a5dc5cb26dc3adbd310a32a39fe146a`

The historical changelogs describe each task separately. This document is the current combined testing checklist.

## Integration validation

CSharpier formatting, whitespace checks, and locale JSON/key checks were run locally. MSBuild cannot initialize process-information APIs in this execution environment, so the full build and automated .NET suite remain unverified here. The combined build requires a Windows build and in-game testing.
