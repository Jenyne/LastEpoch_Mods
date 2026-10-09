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
| 1 | `feat/force-drop` | Legal Force Drop / affix coverage | Limited coverage passed; final LP and combined-build regression pending |
| 2 | `feat/force-drop` | Illegal Force Drop / Primordial T8 | T8/extra Rage persistence confirmed on 5b8199f6; new scroll/cyan UI awaits testing |
| 3 | `fix/mastery-lock-ground-tooltips` | Mastery chains / combat ground-item hover | Open: allocation and F9 hover traces added; runtime pending |
| 4 | `feat/maxroll-tree-preview` | Maxroll graphical passives / skills preview | Graphical trees and item retrieval/preview confirmed; equipment view deferred |
| 5 | `feat/advanced-forge-t7` | Normal forge T6/T7 / craft options | Open: native locale gate repaired; crafting transaction incomplete |
| 6 | `feat/independent-drop-rates` | Separate natural drop rate controls | UI recovery added; native build and runtime confirmation pending |
| 7 | `test/offline-guard-diagnostics` | Offline startup / session diagnostics | Done for now; confirmed working, deeper investigation deferred |
| 8 | `feat/idol-reroll-misc` | Idol rerolling regression (already confirmed) | Confirmed working; optional restart/locale regression |
| 9 | `master` | Current main baseline | Baseline/control build |
| 10 | `feat/gold-favourites-session-stats` | Fixed gold, favourite waypoints, XP/Favour/Amber counters | Implemented; native build and gameplay pending |
| 11 | `feat/travel-anywhere` | Non-waypoint map menus, right-click, scene picker and favourites | Restoration candidate; current-game build and gameplay pending |

## 1. Legal Force Drop / affix coverage

**Where:** Items > Force Drop
**Branch:** `feat/force-drop`

- Keep Illegal mode off. Check None first, grouped families, green Set affixes and purple corruption exclusives. Verify ordinary affixes on a unique clear/disable LP; removing them re-enables LP, and dedicated variant selectors alone retain LP.
- Test ordinary equipment with Set + Champion + two suffixes, then sealed + corruption; verify item identity, set effects and save/reload.
- Test Unsated Rage, Withstand the Elements and idols. One-tier affixes should clamp rather than fail. Report missing choices with item type and affix name.

[Full branch checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/force-drop/docs/TEST_FORCE_DROP_LEGAL_AFFIXES.md)

## 2. Illegal Force Drop / Primordial T8

**Where:** Items > Force Drop > Illegal mode
**Branch:** `feat/force-drop`

- Check mode switching clears selections. Prefix/Suffix columns scroll independently with wheel/drag and scrollbar handles; either side edits the opened slot, None remains pinned above each list and search resets both scroll positions. Check cyan idol affixes in choices and selected rows.
- Test T8 in all four ordinary rows, the Primordial sealed row and corruption using real eight-tier definitions. Retest legal T8 Primordial with Illegal mode off; one-tier definitions stay T1.
- Test a unique with Set membership: unique name remains, correct set piece counting and actual set bonus. Check Unsated Rage/Withstand special modifiers.
- Test four affixes + sealed + corruption, then equip/stats and save/reload. Record rejected combinations exactly.

[Full branch checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/force-drop/docs/TEST_FORCE_DROP_ILLEGAL_MODE.md)

## 3. Mastery chains / combat ground-item hover

**Where:** Skills > Unlock Other Mastery Trees
**Branch:** `fix/mastery-lock-ground-tooltips`

- Leave Remove Node Requirements off. Enable Unlock Other Mastery Trees; allocate beyond the chain in both other mastery trees.
- Normal prerequisites, point costs and rank caps should still apply. Test turning the option off, tree page changes and restart persistence.
- Hover dropped items during combat/minion attacks with the damage meter hidden, visible but stopped, and recording. Test meter controls/controller casting too.
- Keep `[MasteryTrace]` click/spend lines. Press F9 over the same ground item idle and in combat; repeat meter states and annotate the failed `[HoverTrace]` snapshot.

[Full branch checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/fix/mastery-lock-ground-tooltips/docs/TEST_MASTERY_LOCK_AND_TOOLTIPS.md)

[Trace checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/fix/mastery-lock-ground-tooltips/docs/TEST_MASTERY_HOVER_TRACES.md)

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

- Enable Craft Affixes to T7 plus Infinite Forging Potential on expendable normal equipment. Keep `[ForgeTrace]` when selecting T5; check T5 -> T6 -> T7 and stopping at T7.
- Test Max Crafted Roll. Slot a real Hope or Despair glyph to test its guarantee; inspect actual FP, seal outcome and material consumption.
- Toggle off, swap items, reopen the forge and restart. Deselect All should leave Infinite FP and the four advanced controls alone.
- Record exact forge title, selected affix, no-shard rejection, actual shard/glyph counts and locale. The custom transaction remains incomplete.

[Rejection trace checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/advanced-forge-t7/docs/TEST_FORGE_REJECTION_TRACE.md)

## 6. Separate natural drop rate controls

**Where:** Items > Drop > Natural Drop Rates
**Branch:** `feat/independent-drop-rates`

- Check separate Unique, Set, Exalted Affix and T7 Affix rows; 100% is normal, 1000% is 10x, not a guaranteed final chance.
- Test each alone over enough ordinary drops: all off/100%, then 1000%, then 50%/0%. Exclude forced/guaranteed rewards from comparisons.
- Enable each row, drag and type 0, 50, 100 and 1000; click outside the input. Check slider/input agreement, legacy rows, close/reopen and restart. Save the bind confirmation and `[DropRates]` baseline lines.

[UI recovery checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/independent-drop-rates/docs/TEST_DROP_RATE_UI_RECOVERY.md)

## 7. Offline startup / session diagnostics

Done for now at the user's request. The following checklist is retained for optional regression; deeper investigation is deferred.

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

## 10. Gold, favourite teleports and session counters

**Where:** Character > Cheats > Currencies; Scenes > Misc (scroll)
**Branch:** `feat/gold-favourites-session-stats` (`87e5d8c4`)

- Click Spawn 1,000,000 Gold once with Auto Pickup Gold off, collect the pile, then test with auto-pickup on. Check exactly 1,000,000 per click, including with monster gold multipliers enabled; no runes should be added.
- Save favourite unlocked waypoints, travel between them, remove/re-add, fill eight slots and restart. Switching to a character without a saved waypoint must reject travel and leave its unlocks alone. Open the world map once if pins are unavailable.
- Reset counters, farm for 60 active seconds and compare XP/Favour/Memory Amber totals and hourly rates with actual gains. Test level-ups, multipliers/caps, currency spending, manual mod grants, pause/resume/reset, loading/zone continuity and character reset.
- Scroll Misc to every control, retest Safe Teleport, hide/show the HUD and restart. Check loot hover/casting below the overlay and EN > FR > KO > ZH > EN captions.

[Full branch checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/gold-favourites-session-stats/docs/TEST_GOLD_FAVOURITES_SESSION_STATS.md)

Core/locale tests: 1,024 passed, six SDK-dependent checks skipped. Native mod compilation, patch targets and all gameplay behaviours remain unconfirmed. Selection 11 is the separate Travel Anywhere restoration candidate, based on this QoL build.

## 11. Travel Anywhere / map menus and right-click

**Where:** Scenes > Misc > Travel Anywhere; world map
**Branch:** `feat/travel-anywhere` (`5744c5b7`), based on QoL `87e5d8c4`

- Leave the option off first and check ordinary waypoints, favourites and Safe Teleport. Enable it, select a campaign area without a waypoint and travel. Confirm usable spawn, movement/pathfinding, camera, enemies, loot, exits and NPCs.
- Left-click non-waypoint, locked and unlocked map nodes to open the normal area menu. Check its waypoint/Travel controls are usable while enabled, then travel using the menu. Right-click each node for direct travel. Verify one Load/Complete pair, nonzero gates, same-area rejection, every era and popups blocking direct clicks. This requires a `UIWaypoint` parent; universal map-widget coverage is not confirmed.
- Double-click, click another node while loading and disable mid-flight or with an area menu already open. A stale temporary Travel action must be blocked; reopen the menu for normal availability. Check map flags return on close/off, Unlock All Waypoints on/off and reused era widgets. Failed placement must retain the source and clean up before another request. Keep `[TravelAnywhere]` lines and check recovery position/camera/navigation.
- Save and travel to a non-waypoint favourite while enabled; disable and confirm it is blocked. Check restart, EN > FR > KO > ZH > EN and unchanged saved waypoint unlocks. The temporary waypoint only changes map controls; it does not create a physical waypoint in the world.
- Repeat trips, including leaving an echo/arena for a static area. Watch for duplicate actors/managers, broken portals, quests or memory growth. Generated echo/arena destinations remain excluded.
- This build includes selection 10's gold, favourites and session counters. Retest fixed gold and counter continuity across zones.

[Full branch checklist and historical findings](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/travel-anywhere/docs/TEST_TRAVEL_ANYWHERE.md)

Core/locale checks: 1,064 passed, six SDK-dependent checks skipped; formatting passed. Current SDK compilation, Harmony targets and all gameplay remain unconfirmed. Refresh the runner branch and copy the script to TEMP again to see entry 11.

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

Legal and Illegal Force Drop now share `feat/force-drop` at `4585e880`. Both menu entries install the same combined DLL, with different checks. The earlier legal/illegal branches remain historical checkpoints. Keep the current Force Drop UI; a full UI redesign is deferred.

Prophecy reward multiplication is confirmed working and persistent on `fix/prophecy-reward-trigger` at `cb638c70`. The old `feat/prophecy-reward-multiplier` branch is a crash-isolation checkpoint, not the working build. Prophecy currently has no numbered runner entry; track it in [DEV_TODO.md](DEV_TODO.md). Large multipliers can lag; UI relocation is deferred.

Idol rerolling and the earlier Maxroll item preview were confirmed in game. Item preview regression is included in the graphical preview selection; idol rerolling has its own optional regression entry. Other completed main-branch fixes can be compared with selection 9.

## Runner validation

PowerShell 7.4 parsing and mocked end-to-end checks passed for both modern and legacy test paths, build failure, test failure, fetch failure, partial locale copy rollback, a running game and a dirty checkout. These validate installer control flow; they do not certify the feature branches in game. The runner uses PowerShell 5.1-compatible syntax; Windows PowerShell and actual Windows build/install execution still require local use.

The queue parser now assigns `ConvertFrom-Json` directly instead of wrapping its pipeline in `@()`. Windows PowerShell 5.1 returns a JSON array as one pipeline object; the old wrapper nested it, displaying `System.Object[]` and rejecting all numbers. Menu entries and every numbered selection were checked with both normal PowerShell 7 output and simulated PowerShell 5.1 array output. After fetching this fix, copy the updated script into TEMP again before reopening the menu.

Locale installation relies on the repository's successful locale tests and copies the JSON bytes unchanged. It does not reparse the files with PowerShell's case-insensitive object parser, which rejects the existing `Forging Potential` / `Forging potential` keys. Native command output is converted to plain text before display/logging so successful Git status messages on stderr no longer look like PowerShell failures. Nonzero native exit codes still stop the runner.

## Latest test reports — 2026-10-08

Current development priorities and remaining work are tracked in [DEV_TODO.md](DEV_TODO.md).

- Force Drop legal and illegal histories are consolidated in `feat/force-drop`, commit `4585e880`. Entries 1 and 2 both install this branch.
- On `5b8199f6`, screenshots showed four ordinary T8 affixes, seals/corruption, retained item identity and additional distinct Rage modifiers. Nyk confirmed persistence. Individual gameplay effects and broader legal-mode regression remain open.
- The combined build adds cyan idol affixes in Illegal Mode and independently scrolling Prefix/Suffix columns with pinned None and scrollbar handles. These new UI changes await in-game confirmation; earlier persistence confirmation does not certify this build.
- Legal Force Drop had limited successful coverage tests. The LP correction is included in the combined branch but still needs a final regression: transferred ordinary affixes consume/disable LP; clearing them re-enables LP; dedicated ring/glove modifiers alone preserve it.
- Build 3 failed: secondary mastery remains locked and combat ground-item hover remains blocked. Comparison without the mod confirmed a mod-related issue.
- Build 4 graphical trees are much better; item retrieval/preview is confirmed. Equipment presentation improvements are deferred.
- Build 5 failed at runtime: normal crafting still stops at T5. The missing `Force Crafted Affix Roll` locale key is fixed; it was not the runtime ceiling fix.
- Build 6 has no visible Natural Drop Rates sliders. Rate behavior and persistence remain unconfirmed.
- Build 7 is confirmed working with repeated logging removed and a single confirmation retained. This is an observer/diagnostics build, not a comprehensive runtime permission guard.
- Build 8 idol rerolling is confirmed working. Restart/locale checks remain optional regressions.
- Prophecy `fix/prophecy-reward-trigger` at `cb638c70` is confirmed working and persistent. Large multipliers can lag; UI relocation is deferred.

### Validation boundary

The initial scroll/cyan implementation compiled against the recovered native references, passed 1,162 tests with zero skipped and passed formatting. After the workspace reset, the same changes were reconstructed and published with new commit IDs; formatting was rechecked across 408 C# files. Native references were no longer available to rerun that build. No unit-test or formatting result is treated as in-game confirmation.

## Follow-up — 2026-10-09

Active work is #3, #5 and #6. #7 is done for now and deferred. See [current source changes and test order](REVIEW_3_5_6.md). The previous failed runtime reports remain the latest in-game evidence; follow-up source changes are awaiting installed-SDK compilation and runtime checks.

Published follow-up source: #3 `6e835e85`, #5 `b59281d6`, #6 `75e23254`. Fetch the runner branch and copy the updated script to TEMP again to get the new statuses and checklists.

Travel Anywhere source was published on `feat/travel-anywhere` at `43ea2bc8`. Selection 11 includes QoL selection 10 and restores static-area picker/map travel as a default-off test candidate. Current SDK and gameplay validation are still pending; this is not confirmation of every map node or generated instance.

Menu/right-click follow-up: `feat/travel-anywhere` at `5744c5b7` temporarily enables eligible map waypoint flags, keeps normal left-click menus and adds guarded right-click travel. Formatting and the existing 1,064 core/locale checks passed (six SDK-dependent skipped). Native menu appearance/actions and restoration still need in-game testing; the pure tests do not cover these new native UI paths.
