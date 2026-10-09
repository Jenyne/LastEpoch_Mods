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
| 1 | `feat/force-drop` | Force Drop / legal + illegal / global item search | Single combined branch; global search/scrolling/cyan and LP regressions await testing |
| 3 | `fix/mastery-lock-ground-tooltips` | Mastery chains / combat ground-item hover | Checkbox no longer crashes (user-confirmed); allocation blocked; hover on hold |
| 4 | `feat/maxroll-tree-preview` | Maxroll graphical passives / skills preview | Graphical trees and item retrieval/preview confirmed; equipment view deferred |
| 5 | `feat/advanced-forge-t7` | Normal forge T6/T7 / craft options | Done — user confirmed 2026-10-09 |
| 6 | `feat/independent-drop-rates` | Separate natural drop rate controls | UI recovery added; native build and runtime confirmation pending |
| 7 | `test/offline-guard-diagnostics` | Offline startup / session diagnostics | Done for now; confirmed working, deeper investigation deferred |
| 8 | `feat/idol-reroll-misc` | Idol rerolling regression (already confirmed) | Confirmed working; optional restart/locale regression |
| 9 | `master` | Current main baseline | Baseline/control build |
| 11 | `feat/travel-anywhere` | Combined gold, key teleports, counters and map travel | Map travel confirmed; picker removed / busy guard fix pending retest |

## 1. Force Drop / legal + illegal / global item search

**Where:** Items > Force Drop (Illegal mode toggles both workflows)
**Branch:** `feat/force-drop` at `2c04bdd0`

Selection 2 is retired; other test numbers remain unchanged.

- Search all items before choosing category/rarity: try seed, partial names, aliases and seed helmet. Choosing a result fills category/rarity/exact item; clearing search returns to that category list. Check base/Unique/Set items, duplicate names, empty results, paging and locale changes, with Illegal mode off/on. Native search UI and actual drop identity are not yet confirmed.
- Keep Illegal mode off. Prefix-only/suffix-only slots use full-width scrolling; enchantment/sealed/corruption pools use independent Prefix/Suffix lists. Check wheel/drag/handles, last entries, layout transitions, pinned None and search reset; legal restrictions and green Set/purple corruption colors stay intact.
- With Corrupted: No and every ordinary affix None, LP and Fixed/Random must be editable; dedicated unique modifiers alone preserve LP. Adding ordinary affixes clears/disables LP; clearing them unlocks it. Corrupted: Yes independently locks LP at zero even with every affix None. Turn corruption off and confirm unlock; this report is not yet confirmed.
- Test ordinary equipment with Set + Champion + two suffixes, then sealed + corruption; verify item identity, set effects and save/reload.
- Test Unsated Rage, Withstand the Elements and idols. One-tier affixes should clamp rather than fail. Report missing choices with item type and affix name.
- Check mode switching clears selections. Illegal picker has independently scrolling Prefix/Suffix columns and scrollbar handles; either side edits the opened slot. None stays pinned. Search resets both scroll positions. Check cyan idol affixes in choices and selected rows.
- Test T8 in all four ordinary rows, the Primordial sealed row and corruption using real eight-tier definitions. Retest legal T8 Primordial with Illegal mode off; one-tier definitions stay T1.
- Test a unique with Set membership: unique name remains, correct set piece counting and actual set bonus. Check Unsated Rage/Withstand special modifiers.
- Test four affixes + sealed + corruption, then equip/stats and save/reload. Record rejected combinations exactly.

[Legal and global search checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/force-drop/docs/TEST_FORCE_DROP_LEGAL_AFFIXES.md)

[Illegal checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/force-drop/docs/TEST_FORCE_DROP_ILLEGAL_MODE.md)

## 3. Mastery chains / combat ground-item hover

**Where:** Skills > Unlock Other Mastery Trees
**Branch:** `fix/mastery-lock-ground-tooltips`

- User confirms 764ea599 no longer crashes when clicking Unlock Other Mastery Trees; the option is still not functional for allocation. Keep Remove Node Requirements off (it works/persists). Retest off/on, panel closed/open, page changes and restart before extending crash confirmation beyond the reported checkbox click.
- The global cap stays unchanged. This is a crash-isolation candidate, not a completed allocation bypass. Try beyond-chain nodes with adequate prerequisites and points, and report whether a real point is spent. Check selected mastery, innate bonus, point costs and rank caps.
- Keep [MasteryTrace] toggle, Visual unlock active, click/spend lines and the once-per-run [MasteryApi] method signatures. If it crashes again, keep the matching MelonLoader log and native crash stack.
- Hover investigation is on hold at the user's request. Existing hover implementation is unchanged; no F9 test is requested in this pass.

[Full branch checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/fix/mastery-lock-ground-tooltips/docs/TEST_MASTERY_LOCK_AND_TOOLTIPS.md)

[Trace checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/fix/mastery-lock-ground-tooltips/docs/TEST_MASTERY_LOCK_AND_TOOLTIPS.md#latest-report-and-follow-up)

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
**Status:** Done — Nyk confirmed selection 5 is good and called it done on 2026-10-09. The checks below are retained for optional regression.

- Enable Craft Affixes to T7 plus Infinite Forging Potential on expendable normal equipment. Keep `[ForgeTrace]` when selecting T5; check T5 -> T6 -> T7 and stopping at T7.
- Test Max Crafted Roll. Slot a real Hope or Despair glyph to test its guarantee; inspect actual FP, seal outcome and material consumption.
- Toggle off, swap items, reopen the forge and restart. Deselect All should leave Infinite FP and the four advanced controls alone.
- Record exact forge title, selected affix, no-shard rejection, actual shard/glyph counts and locale. These are optional regression observations if a new issue appears.

[Rejection trace checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/advanced-forge-t7/docs/TEST_FORGE_REJECTION_TRACE.md)

Latest source: `7575671e`. With Guarantee Despair enabled and a real Despair glyph slotted, any existing sealed affix blocks the craft, including explicit Primordial/corruption flags. Rejection happens before native/custom Forge execution. Test immediate second attempts, item swaps and T7 on/off; item/tier/roll/FP/materials must stay unchanged. Switching to an unsealed item retains the first-seal option. Other glyphs and the guarantee-off native path retain native eligibility. Nyk’s 2026-10-09 completion report closes selection 5; earlier T5-ceiling and pending-test notes are superseded. The report does not enumerate individual checks. Existing suite: 51 passed, three SDK-dependent checks skipped.

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

## 10. Retired — merged into 11

The full `feat/gold-favourites-session-stats` history and features are merged into `feat/travel-anywhere`. Use selection **11** for gold, key teleport buttons, saved favourites, movable counters and Travel Anywhere. The old branch remains a historical reference.

## 11. Combined QoL and Travel Anywhere

**Where:** Character > Cheats > Currencies; Scenes > Misc; world map
**Branch:** `feat/travel-anywhere` (`f53998db`), fully merged with QoL `638504c0`

User confirmed map-menu travel on `2466cc0a`. The supplied log confirms the native build loaded and key destinations resolved as End of Time, Temporal Sanctum, Lightless Arbor, Soulfire Bastion, Bazaar and Observatory. A full-picker attempt `EoT -> WE502` failed to start loading, then stayed busy and blocked their travel. The picker is removed; only the Travel Anywhere toggle/status and key teleport list remain. Rejected loads release the guard only after verifying the source/player are intact and the target is absent. Real pending loads/cleanup keep the guard.

- Combined build: selection 10 is retired. Check Spawn 1,000,000 Gold with auto-pickup off/on and multipliers; test Alt-left-drag, saved/clamped counter position, Reset counter position, natural XP/Favour/Amber, no double counting or manual grant credit, pause/reset and zone continuity.
- Only the Travel Anywhere toggle/status should remain; no full scene picker. Open the map and refresh key teleports, then test End of Time, Temporal Sanctum, Lightless Arbor, Soulfire Bastion, Bazaar and Observatory. Preserve normal key/tier entry and current-character waypoint locks. Keep [KeyTeleports] lines. Test extra saved waypoint favourites and restart.
- Map-menu travel is user-confirmed on 2466cc0a. Retest non-waypoint left-click menus and Travel with the option on, plus spawn, movement, camera, enemies, loot, exits and NPCs. Non-waypoint right-click is a known limitation accepted for now; check supported right-click nodes and popups without expanding that scope.
- The old full-picker attempt EoT -> WE502 failed to start loading and left the busy guard set. The current build removes the picker and immediately releases rejected loads only with the original source/player intact and no target loaded. If a direct attempt fails, key and ordinary waypoint travel must resume after cleanup. Real pending async loads/cleanup must still block overlapping requests; keep [TravelAnywhere] lines.
- Double-click and disable during loading or with an area menu open. Block stale temporary actions; reopen for normal availability. Check map flags return on close/off, Unlock All Waypoints combinations, gates, era widgets, failed placement/source retention and non-waypoint favourites enabled versus disabled.
- Repeat trips and leave an echo/arena for a static area; check actors, portals, quests and memory. Retest Safe Teleport, Misc scrolling, loot hover/casting and locales. See docs/TEST_TRAVEL_ANYWHERE.md and docs/TEST_GOLD_FAVOURITES_SESSION_STATS.md.

[Travel checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/travel-anywhere/docs/TEST_TRAVEL_ANYWHERE.md) · [QoL checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/travel-anywhere/docs/TEST_GOLD_FAVOURITES_SESSION_STATS.md)

Core checks: **1,074 passed, six SDK-dependent checks skipped**. Formatting, locale preservation and queue checks passed. The new rejected-load path and combined follow-up still need native-build/runtime confirmation. Non-waypoint right-click is a known limitation accepted for now. Refresh the runner and use **11**; selection 10 is retired.

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

Advanced Forge is user-confirmed done. Independent drop rates still require compilation/runtime confirmation on the installed game version. A failed build or failed test stops before installation; retain the printed log folder.

Offline diagnostics includes the latest auto-offline startup and online-switch UI changes, so a separate startup-only test is not required in this queue. It is an observer build, not an implemented comprehensive runtime permission guard.

Legal and Illegal Force Drop now use only selection 1 and `feat/force-drop` at `2c04bdd0`. Selection 2 is removed; both earlier histories are preserved in the combined branch. Global search now fills the category, rarity and exact native item when selected. Keep the current Force Drop UI; a full UI redesign is deferred.

Prophecy reward multiplication is confirmed working and persistent on `fix/prophecy-reward-trigger` at `cb638c70`. The old `feat/prophecy-reward-multiplier` branch is a crash-isolation checkpoint, not the working build. Prophecy currently has no numbered runner entry; track it in [DEV_TODO.md](DEV_TODO.md). Large multipliers can lag; UI relocation is deferred.

Idol rerolling and the earlier Maxroll item preview were confirmed in game. Item preview regression is included in the graphical preview selection; idol rerolling has its own optional regression entry. Other completed main-branch fixes can be compared with selection 9.

## Runner validation

PowerShell 7.4 parsing and mocked end-to-end checks passed for both modern and legacy test paths, build failure, test failure, fetch failure, partial locale copy rollback, a running game and a dirty checkout. These validate installer control flow; they do not certify the feature branches in game. The runner uses PowerShell 5.1-compatible syntax; Windows PowerShell and actual Windows build/install execution still require local use.

The queue parser now assigns `ConvertFrom-Json` directly instead of wrapping its pipeline in `@()`. Windows PowerShell 5.1 returns a JSON array as one pipeline object; the old wrapper nested it, displaying `System.Object[]` and rejecting all numbers. Menu entries and every numbered selection were checked with both normal PowerShell 7 output and simulated PowerShell 5.1 array output. After fetching this fix, copy the updated script into TEMP again before reopening the menu.

Locale installation relies on the repository's successful locale tests and copies the JSON bytes unchanged. It does not reparse the files with PowerShell's case-insensitive object parser, which rejects the existing `Forging Potential` / `Forging potential` keys. Native command output is converted to plain text before display/logging so successful Git status messages on stderr no longer look like PowerShell failures. Nonzero native exit codes still stop the runner.

## Latest test reports — 2026-10-09 (UTC)

Current development priorities and remaining work are tracked in [DEV_TODO.md](DEV_TODO.md).

- Force Drop legal/illegal histories and global search are consolidated in `feat/force-drop`, commit `2c04bdd0`. Selection 1 is the single build; selection 2 is retired. Search matches aliases/translated names, fills category/rarity/exact item and restores the filtered list when cleared. 1,164 tests passed; six SDK-dependent checks skipped; native/runtime confirmation remains pending.
- On `5b8199f6`, screenshots showed four ordinary T8 affixes, seals/corruption, retained item identity and additional distinct Rage modifiers. Nyk confirmed persistence. Individual gameplay effects and broader legal-mode regression remain open.
- The combined build adds cyan idol affixes in Illegal Mode, independent Prefix/Suffix scrolling and full-width legal prefix/suffix scrolling, with pinned None and scrollbar handles. These new UI changes await in-game confirmation; earlier persistence confirmation does not certify this build.
- Legal Force Drop had limited successful coverage tests. The LP correction is included in the combined branch but still needs a final regression: transferred ordinary affixes consume/disable LP; clearing them re-enables LP; dedicated ring/glove modifiers alone preserve it.
- Build 3: Remove Node Requirements works/persists. Two 7a91 runs stop immediately after requesting the native cap write 22→45. Published `764ea599` removes that write and logs read-only allocation-check signatures. User confirms the mastery checkbox no longer crashes on this candidate; allocation is still not functional. Broader reopen/restart behavior needs checking. Hover is on hold. 999 core/source tests passed; six SDK checks skipped. Native compilation/runtime remain pending.
- Build 4 graphical trees are much better; item retrieval/preview is confirmed. Equipment presentation improvements are deferred.
- Build 5: done — Nyk confirmed it is good on 2026-10-09. The earlier T5 runtime failure is historical and no longer an active task.
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

QoL follow-up: `638504c0` adds key teleport buttons and Alt-drag position persistence. The reported missing favour count is addressed using the fresh local faction tracker plus balance/event reconciliation, including paused/manual baselines. Source checks: 1,029 passed, six native SDK checks skipped. Travel `2466cc0a` includes these changes and the CS1061 fix (1,069 passed, six skips). All new native behaviour remains pending.

Combined-build update (2026-10-09): `f53998db` merges both topic histories, removes the full scene picker and corrects the rejected-load busy state. Selection 10 is retired; its remote branch is retained as a reference. The preceding QoL/travel build notes are historical. Map-menu travel and destination resolution on `2466cc0a` are user/log-confirmed; new recovery/key travel checks remain pending.
