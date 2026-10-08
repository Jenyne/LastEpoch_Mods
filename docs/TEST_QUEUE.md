# Last Epoch queued tests

Run one build at a time, launch the game, test it, then close the game before selecting the next. The menu stays open. Numbers match the script; they are not the old branch numbers.

## Start the menu

Close Last Epoch and run from PowerShell:

```powershell
& {
    $ErrorActionPreference = "Stop"
    Set-Location "D:\GitHub\LastEpoch_Mods"
    git fetch origin "+refs/heads/chore/test-queue-runner:refs/remotes/origin/chore/test-queue-runner"
    if ($LASTEXITCODE -ne 0) { throw "Fetch failed" }
    git switch --detach origin/chore/test-queue-runner
    if ($LASTEXITCODE -ne 0) { throw "Switch failed" }
    $runner = Join-Path $env:TEMP "Test-LastEpochBranches.ps1"
    Copy-Item .\scripts\Test-LastEpochBranches.ps1 $runner -Force
    & $runner -RepoPath "D:\GitHub\LastEpoch_Mods" `
        -GamePath "D:\SteamLibrary\steamapps\common\Last Epoch"
}
```

The cached script remains available after changing to a branch that does not contain it. To reopen the menu later:

```powershell
& (Join-Path $env:TEMP "Test-LastEpochBranches.ps1")
```

Both paths have the defaults shown above. Override `-RepoPath` / `-GamePath` if needed. Use `-Select 3` to install one selection without the menu, or `-ListOnly` to print all checks.

## Queue

| # | Branch | Main checks | Status |
|---|---|---|---|
| 1 | `fix/force-drop-legal-affixes` | Legal Force Drop / affix coverage | Pending final coverage/persistence checks |
| 2 | `feat/force-drop-illegal-mode` | Illegal Force Drop / Primordial T8 | Pending in-game validation |
| 3 | `fix/mastery-lock-ground-tooltips` | Mastery chains / combat ground-item hover | Failed: mastery chain and combat hover remain blocked |
| 4 | `feat/maxroll-tree-preview` | Maxroll graphical passives / skills preview | Tree preview improved in game; equipment view deferred |
| 5 | `feat/advanced-forge-t7` | Normal forge T6/T7 / craft options | Failed: crafting still stops at T5 |
| 6 | `feat/independent-drop-rates` | Separate natural drop rate controls | Failed: Natural Drop Rates UI missing; behavior untested |
| 7 | `test/offline-guard-diagnostics` | Offline startup / session diagnostics | Confirmed working, including quieter logging |
| 8 | `feat/idol-reroll-misc` | Idol rerolling regression (already confirmed) | Confirmed working; optional restart/locale regression |
| 9 | `master` | Current main baseline | Baseline/control build |

## 1. Legal Force Drop / affix coverage

**Where:** Items > Force Drop
**Branch:** `fix/force-drop-legal-affixes`

- Keep Illegal mode off. Check None first, grouped families, green Set affixes and purple corruption exclusives.
- Test ordinary equipment with Set + Champion + two suffixes, then sealed + corruption; verify item identity, set effects and save/reload.
- Test Unsated Rage, Withstand the Elements and idols. One-tier affixes should clamp rather than fail. Report missing choices with item type and affix name.

[Full branch checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/fix/force-drop-legal-affixes/docs/TEST_FORCE_DROP_LEGAL_AFFIXES.md)

## 2. Illegal Force Drop / Primordial T8

**Where:** Items > Force Drop > Illegal mode
**Branch:** `feat/force-drop-illegal-mode`

- Check mode switching clears selections. Prefix/Suffix columns page independently; either side edits the opened slot, None remains first and search resets both pages.
- Test T8 in all four ordinary rows, the Primordial sealed row and corruption using real eight-tier definitions. Retest legal T8 Primordial with Illegal mode off; one-tier definitions stay T1.
- Test a unique with Set membership: unique name remains, correct set piece counting and actual set bonus. Check Unsated Rage/Withstand special modifiers.
- Test four affixes + sealed + corruption, then equip/stats and save/reload. Record rejected combinations exactly.

[Full branch checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/force-drop-illegal-mode/docs/TEST_FORCE_DROP_ILLEGAL_MODE.md)

## 3. Mastery chains / combat ground-item hover

**Where:** Skills > Unlock Other Mastery Trees
**Branch:** `fix/mastery-lock-ground-tooltips`

- Leave Remove Node Requirements off. Enable Unlock Other Mastery Trees; allocate beyond the chain in both other mastery trees.
- Normal prerequisites, point costs and rank caps should still apply. Test turning the option off, tree page changes and restart persistence.
- Hover dropped items during combat/minion attacks with the damage meter hidden, visible but stopped, and recording. Test meter controls/controller casting too.

[Full branch checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/fix/mastery-lock-ground-tooltips/docs/TEST_MASTERY_LOCK_AND_TOOLTIPS.md)

## 4. Maxroll graphical passives / skills preview

**Where:** Items > Force Drop > Maxroll Build Preview
**Branch:** `feat/maxroll-tree-preview`

- Load https://maxroll.gg/last-epoch/planner/2ai4s0qh#1; inspect named passive/mastery and skill trees, connections and allocated ranks.
- Test pan, zoom, Fit, node hover/click details, variant switching and close/reopen. This is read-only; no character point allocation is expected.
- Retest gear, idols, blessings and Copy Item JSON. Empty Weaver Items is expected for the sample build. Check EN/FR/KO/EN controls.

[Full branch checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/maxroll-tree-preview/docs/TEST_MAXROLL_TREE_PREVIEW.md)

## 5. Normal forge T6/T7 / craft options

**Where:** Items > Crafting
**Branch:** `feat/advanced-forge-t7`

- Enable Craft Affixes to T7 plus Infinite Forging Potential. Use a normal T5 affix and upgrade T5 -> T6 -> T7. T7 must stop; no ordinary T8 crafting.
- Test Max Crafted Roll. Slot a real Hope or Despair glyph to test its guarantee; inspect actual FP, seal outcome and material consumption.
- Toggle off, swap items, reopen the forge and restart. Deselect All should leave Infinite FP and the four advanced controls alone.

## 6. Separate natural drop rate controls

**Where:** Items > Drop > Natural Drop Rates
**Branch:** `feat/independent-drop-rates`

- Check separate Unique, Set, Exalted Affix and T7 Affix rows; 100% is normal, 1000% is 10x, not a guaranteed final chance.
- Test each alone over enough ordinary drops: all off/100%, then 1000%, then 50%/0%. Exclude forced/guaranteed rewards from comparisons.
- Test toggling off, changing zones, slider/input synchronization and restart persistence. Save the first [DropRates] line plus errors.

## 7. Offline startup / session diagnostics

**Where:** Launch -> offline character selection; MelonLoader/Latest.log
**Branch:** `test/offline-guard-diagnostics`

- Login.Enable_AutoLoginOffline must be true. Confirm automatic offline character selection and that its online switch is hidden.
- Load an offline character, change zones, run an echo and remain playable for 30+ seconds. Switch characters and reload the first.
- Exit normally and capture the complete log with [Offline] and [OfflineGuard] lines. This observes session signals; it does not yet enforce the proposed runtime guard.

[Full branch checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/test/offline-guard-diagnostics/docs/TEST_OFFLINE_GUARD_DIAGNOSTICS.md)

## 8. Idol rerolling regression (already confirmed)

**Where:** Scenes > Misc > Idol Rerolling
**Branch:** `feat/idol-reroll-misc`

- Test No Memory Amber Cost and Unlimited Idol Altar Uses independently and together.
- Turn them off while the altar is open. Check normal amber costs/use limits return and unrelated amber purchases still cost amber.
- Confirm restart persistence and EN -> FR -> KO -> EN captions.

[Full branch checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/idol-reroll-misc/docs/TEST_IDOL_REROLL_MISC.md)

## 9. Current main baseline

**Where:** Normal mod UI
**Branch:** `master`

- Use this to compare behavior with current main after testing a feature branch.
- Each selection installs one branch DLL; this runner does not combine pending features.

## What the runner does

- Fetches the selected branch explicitly and checks out its exact remote commit in detached mode. Existing local feature branches are not rebased, merged or overwritten.
- Requires a clean tracked worktree and a closed game. A running game is checked again before installation.
- Rebuilds `LastEpoch_Hud` in Release/net6.0, then runs the repository tests against that exact DLL and your game SDK.
- For older tests that hardcode `Build/Keyboard`, temporarily places the same fresh Release DLL there, then restores the previous validation DLL. No second Keyboard build is installed.
- Installs the tested DLL into `Mods/LastEpoch_Hud.dll` plus matching `en`, `fr`, `ko`, `zh` files into `Mods/LastEpoch_Hud/Locales`. Existing v4.4.21 assets are required.
- Backs up the DLL and those locales outside Mods, verifies the installed DLL hash, and restores prior files if copying fails.
- Saves build/test logs and commit/hash metadata under `UserData/LastEpoch_Hud/TestQueue/<timestamp>-<branch>/`.
- Press **L** after testing to archive `MelonLoader/Latest.log`. Selecting the next build also archives the previous log before installing.

Each selection replaces the installed mod DLL. These branches are separate test builds; features from another selection may disappear until the branches are integrated.

Advanced Forge and independent drop rates still require compilation/runtime confirmation on the installed game version. A failed build or failed test stops before installation; retain the printed log folder.

Offline diagnostics includes the latest auto-offline startup and online-switch UI changes, so a separate startup-only test is not required in this queue. It is an observer build, not an implemented comprehensive runtime permission guard.

Legal Force Drop includes the earlier integrity/unique-modifier work. Illegal Mode builds on Legal Force Drop. Use these latest branches instead of the older corruption-pool, Withstand-only or integrity-only test builds.

Prophecy rewards remain paused after crash isolation: the current `feat/prophecy-reward-multiplier` backend has no native reward hooks and leaves vanilla reward counts. It is not queued as a functioning multiplier.

Idol rerolling and the earlier Maxroll item preview were confirmed in game. Item preview regression is included in the graphical preview selection; idol rerolling has its own optional regression entry. Other completed main-branch fixes can be compared with selection 9.

## Runner validation

PowerShell 7.4 parsing and mocked end-to-end checks passed for both modern and legacy test paths, build failure, test failure, fetch failure, partial locale copy rollback, a running game and a dirty checkout. These validate installer control flow; they do not certify the feature branches in game. The runner uses PowerShell 5.1-compatible syntax; Windows PowerShell and actual Windows build/install execution still require local use.

The queue parser now assigns `ConvertFrom-Json` directly instead of wrapping its pipeline in `@()`. Windows PowerShell 5.1 returns a JSON array as one pipeline object; the old wrapper nested it, displaying `System.Object[]` and rejecting all numbers. Menu entries and every numbered selection were checked with both normal PowerShell 7 output and simulated PowerShell 5.1 array output. After fetching this fix, copy the updated script into TEMP again before reopening the menu.

Locale installation relies on the repository's successful locale tests and copies the JSON bytes unchanged. It does not reparse the files with PowerShell's case-insensitive object parser, which rejects the existing `Forging Potential` / `Forging potential` keys. Native command output is converted to plain text before display/logging so successful Git status messages on stderr no longer look like PowerShell failures. Nonzero native exit codes still stop the runner.

## Latest test reports — 2026-10-08

- Prophecy reward multiplication (`fix/prophecy-reward-trigger`) is confirmed working and persistent. Large multipliers can lag; UI relocation is deferred until the UI rework.
- Build 3 did not unlock either non-main mastery allocation or combat ground-item hover. Both need another fix.
- Build 4 graphical tree preview is much better in game. A similar equipment view is a lower-priority follow-up.
- Build 5 compiled successfully but installation stopped on the missing `Force Crafted Affix Roll` locale key. The key has been added in all supplied languages. In-game test failed: the forge still refuses upgrades past T5.
- Build 6 has no visible Natural Drop Rates section. No rate behavior or persistence is confirmed.
- Build 7, including the quieter logging, is confirmed working by Nyk. One confirmation per loaded offline character is retained when the live signals agree; observer errors and a subsequent revocation remain visible.

### Legal Force Drop LP follow-up

Build 1 now clears and disables LP when ordinary transferred affixes are selected for a unique. Maximum/Random cannot restore LP on a Legendary. Clearing all ordinary affixes re-enables LP; the native ring/glove modifier selectors alone preserve LP. Retest creation and save/reload on `fix/force-drop-legal-affixes`.

### Illegal affix picker follow-up

Build 2 includes the LP fix from build 1 and a split Prefix/Suffix picker. Both columns select into the opened slot, with independent paging and None pinned first. Existing definition-backed T8 support covers every ordinary row, Primordial sealing and corruption; full T8 combinations still need native creation and save/reload checks.
