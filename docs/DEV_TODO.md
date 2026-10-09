# Last Epoch development todo

Updated 2026-10-09 (UTC). Repository: `Jenyne/LastEpoch_Mods`; upstream: `Syncingoutt/LastEpoch_Mods`.

Keep changes on separate topic branches. Force Drop legal and illegal work is now one topic: `feat/force-drop`. Preserve the current Force Drop layout until the planned full UI redesign. Runtime confirmation and compilation/unit-test results are separate evidence.

## Status overview — 2026-10-09

### Done / confirmed

- **#1 Force Drop:** combined legal/illegal branch, search, scrolling and cyan affixes; user says it works great. Leave unchanged until the updated HUD.
- **#5 Advanced Forge:** user confirmed done; includes Guaranteed Despair's existing-seal guard.
- **#7 Offline startup:** reopened after character-load crashes; see in-progress status.
- **#8 Idol rerolls:** confirmed working.
- **Prophecy rewards:** multiplier works and persists; very large values can lag.
- **#3 Remove Node Requirements:** works and persists. This does not complete mastery-chain unlocking.
- **#11 map-menu Travel Anywhere:** confirmed on the earlier tested build. Non-waypoint right-click remains an accepted limitation.
- **Branch consolidation:** Force Drop #1/#2 and QoL #10/#11 histories merged; use selections 1 and 11.

### Working features waiting on HUD integration / presentation

- **Force Drop:** functionality confirmed; port its current logic and controls into the updated HUD when available.
- **Prophecy multiplier:** functionality confirmed; UI relocation deferred until the redesign.
- **Maxroll (#4):** graphical trees and item retrieval/preview confirmed; equipment presentation remains deferred. HUD arrival does not by itself complete it.
- **Full HUD redesign:** recovered runtime source reviewed, but integration remains pending; do not mark the redesign done.

### In progress / testing or fixes required

- **#7 offline startup:** candidate `951f4e01` removes session observer hooks/polling after crashes on `99eebd98`; same-character baseline succeeded. Character entry/combat retest pending; upstream PR on hold.

- **#6 Drop rates:** build `e1b3f857` loaded. Section/content dimensions are `(699.07, 288.00)` / `(699.07, 1000.00)`, confirming the zero-height issue is corrected. User reports a combat crash. Last MelonLoader line at 08:13:05 is the first normal-spawn override (`set=6x`, others `1x`). Neither supplied log contains a fatal crash stack. Isolate the rate hook before marking safe; visible controls, synchronization, persistence and independent drop behavior still need confirmation.
- **#3 mastery-chain unlock:** checkbox no longer crashes, but allocation past the chain remains blocked; targeted bypass needed.
- **#11 combined QoL/travel:** key teleports/recovery, fixed gold, movable counter, natural favour/amber counting and persistence still require confirmation on the merged candidate.
- **Cosmetics / skill effects:** missing choices, failed application and resetting remain undiagnosed.
- **Hover tooltips:** on hold at the user's request.

## Active work and current statuses

| Queue | Topic / branch | Current status | Remaining work |
|---|---|---|---|
| 1 | Force Drop — `feat/force-drop` (`2c04bdd0`) | **User confirmed #1 works great on 2026-10-09.** Combined legal/illegal branch includes global item search, scrolling and cyan affixes. | No active work; leave it unchanged until the updated HUD arrives, as requested. Reopen only for a new issue. |
| 3 | Mastery / ground tooltips — `fix/mastery-lock-ground-tooltips` (`764ea599`) | Remove Node Requirements works/persists. Two 7a91 runs stop at the native 22→45 cap-write boundary. Latest candidate removes all global-cap writes; 999 tests passed, six SDK checks skipped. User confirms clicking the checkbox no longer crashes; allocation is still not functional and needs a targeted bypass. Hover is on hold. | Native-build and test checkbox closed/open, off/on, page changes and restart. Keep `[MasteryTrace]` state/click/spend lines and `[MasteryApi]` signatures for targeting the allocation check. Check actual point spending and mastery/innate preservation. See [follow-up](REVIEW_3_5_6.md). |
| 4 | Maxroll preview — `feat/maxroll-tree-preview` | Graphical trees improved; item retrieval/preview confirmed. | Equipment presentation improvements deferred. Keep existing preview functionality in regression checks. |
| 5 | Advanced Forge — `feat/advanced-forge-t7` (`7575671e`) | **Done — user confirmed #5 is good on 2026-10-09.** Includes the crafting options and existing-seal guard for Guaranteed Despair. | No active work. Retain selection 5 for optional regression; reopen only for a new reported issue. |
| 6 | Drop rates — `feat/independent-drop-rates` (`e1b3f857`) | Native build loaded; post-layout dimensions confirm a 288-unit section. User reports combat crash; last log line is the first normal-spawn override with Set at 6×. Fatal cause not captured. | Isolate combat/drop hook with rates disabled versus enabled before further effectiveness testing. Confirm visible controls, slider/input synchronization, persistence and independent behavior. |
| 7 | Offline startup — `test/offline-guard-diagnostics` (`951f4e01`) | Reopened: `99eebd98` crashed on character entry; baseline `92fb33de` loaded the same character and exited normally. Observer/native hooks removed from candidate, preserving startup UI behavior. 1,005 core tests passed, six SDK checks skipped; formatting passed. | Rerun selection 7 and confirm build, same-character entry, combat, zone changes and switching characters. Keep full logs. Not a confirmed crash fix; upstream PR on hold. |
| 8 | Idol rerolling — `feat/idol-reroll-misc` | Confirmed working. | Optional restart/locale regression; no immediate fix. |
| — | Prophecy — `fix/prophecy-reward-trigger` (`cb638c70`) | Confirmed working and persistent. | Large multipliers can lag. UI relocation deferred until redesign. Old multiplier branch is an isolation checkpoint. |
| 9 | `master` baseline | Control build. | Use for comparisons; topic-branch confirmation does not mean integration into main or upstream. |
| 10 (retired) | QoL — `feat/gold-favourites-session-stats` (`638504c0`) | Fully merged into selection 11, including branch history. | Use selection 11. Old branch retained as a historical reference. |
| 11 | Combined QoL / Travel Anywhere — `feat/travel-anywhere` (`f53998db`) | User confirmed map travel on `2466cc0a`; log confirms the native build loaded and all six key destination IDs resolved. Full picker removed at user request. Its rejected-load busy state is corrected; ordinary waypoint favourites use the native route. QoL history merged; 1,074 core tests passed, six SDK checks skipped. | Rebuild/retest key teleports after failed travel, gold, movable counter and natural favour. Detailed load/recovery and map regressions remain pending. Non-waypoint right-click is accepted as a known limitation for now. |

## Next development order

1. Investigate #6 combat crash first: compare rates disabled/enabled on the same build, then isolate the drop hook if needed. The latest logs show corrected layout dimensions but do not identify the fatal cause.
2. Retest combined selection #11 key teleports/recovery, gold and counters. Selection #10 is fully merged and retired.
3. Test #3 with the global-cap write removed, then collect allocation-check signatures and real spend results. Remove Node Requirements is working/persistent. Hover is on hold; no hover work or test requested in this pass.
4. Force Drop (#1) is user-confirmed working. Leave it unchanged until the updated HUD arrives.

## Backlog and deferred work

- Cosmetics/skill effects: investigation requested for missing menu choices, effects that will not apply and effects resetting. No confirmed diagnosis or fix is recorded in this tracker; inspect the current fork before proposing changes. Use a separate topic branch.
- Full HUD UI redesign: runtime source recovered from the uploaded test DLL for review, including navigation, cards, themes and font scaling. The bundle matches ours; no new public Syncingoutt branch/PR exists. Recovery is not integrated or an original source project. Keep our newer Force Drop logic when porting; full rollout remains deferred.
- Maxroll equipment presentation: deferred.
- Prophecy UI relocation: deferred.
- Broader travel coverage: static-area restoration is now the #11 test candidate. Generated echo/arena destinations remain excluded, as in the old picker. Map widgets without a `UIWaypoint` parent need an adapter; universal map-node coverage is not established. With Travel Anywhere off, favourites still require unlocked waypoints.
- Guaranteed mechanic spawns (Moroditas/Omen etc.): highest-priority backlog investigation; a universal 100% spawn hook has not been established.
- Dedicated boss/lizard/champion spawn controls and general boss-drop distribution: investigation pending. Existing density/drop controls do not confirm these features.
- #7 observer investigation remains deferred; its runtime hooks/polling are removed from the startup-only candidate. Hold upstream submission until character-load and combat retest succeeds.

## Testing and evidence

- Runner: `chore/test-queue-runner`. Selection 1 is the combined Force Drop build; selection 11 is the combined QoL/travel build. Selections 2 and 10 are retired; other IDs remain unchanged.
- Close the game before switching builds. Each selection replaces the installed DLL; it does not combine every pending feature branch.
- Follow [TEST_QUEUE.md](TEST_QUEUE.md) for the runner and per-feature checks. Force Drop's detailed checklist lives on its feature branch in `docs/TEST_FORCE_DROP_ILLEGAL_MODE.md`.
- Latest published Force Drop source: `2c04bdd0`. User confirmed selection #1 works great on 2026-10-09; retain existing functionality until HUD integration. Earlier core/source checks passed (1,164 passed, six SDK skips); this is separate from the user confirmation.

- QoL source `638504c0` is fully merged into combined travel `f53998db`; selection 10 is retired. Key teleport resolution on `2466cc0a` is confirmed by the supplied log, but travel was blocked by a failed full-picker load. Gold, movable counter and favour regression checks remain open. See the [QoL checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/travel-anywhere/docs/TEST_GOLD_FAVOURITES_SESSION_STATS.md).
- Combined travel source: `f53998db`. User reports map-menu travel works on `2466cc0a`; its log confirms the native build loaded. The picker attempted `EoT -> WE502`, returned no load and kept the busy guard set. The picker is removed; verified rejected loads release the guard immediately, while real asynchronous cleanup remains protected. Five new core tests cover this distinction. This follow-up’s native build and recovery/key-teleport checks remain pending. See the [travel checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/travel-anywhere/docs/TEST_TRAVEL_ANYWHERE.md).
- Keep failed runtime issues open even when compilation or unit tests pass. Record the tested commit, screenshots/logs and persistence results separately.

- LP report clarification: Corrupted: Yes independently removes LP with all affixes None. No LP rule change was made. User later confirmed #1 overall working; no separate detailed LP retest result was supplied.

- Branch cleanup: `feat/force-drop-illegal-mode` at `5b8199f6` is fully merged into `feat/force-drop` and retired from the active queue. Remote-ref deletion is deferred at the user's request; leave this branch untouched and ignore it going forward.

- QoL branch cleanup: `feat/gold-favourites-session-stats` is fully merged into `feat/travel-anywhere` and retired from the active queue. Leave its remote ref as a historical reference; future QoL/travel changes go on the combined branch.
