# Last Epoch development todo

Updated 2026-10-09 (UTC). Repository: `Jenyne/LastEpoch_Mods`; upstream: `Syncingoutt/LastEpoch_Mods`.

Keep changes on separate topic branches. Force Drop legal and illegal work is now one topic: `feat/force-drop`. Preserve the current Force Drop layout until the planned full UI redesign. Runtime confirmation and compilation/unit-test results are separate evidence.

## Active work and current statuses

| Queue | Topic / branch | Current status | Remaining work |
|---|---|---|---|
| 1 | Force Drop — `feat/force-drop` (`2c04bdd0`) | **User confirmed #1 works great on 2026-10-09.** Combined legal/illegal branch includes global item search, scrolling and cyan affixes. | No active work; leave it unchanged until the updated HUD arrives, as requested. Reopen only for a new issue. |
| 3 | Mastery / ground tooltips — `fix/mastery-lock-ground-tooltips` (`764ea599`) | Remove Node Requirements works/persists. Two 7a91 runs stop at the native 22→45 cap-write boundary. Latest candidate removes all global-cap writes; 999 tests passed, six SDK checks skipped. User confirms clicking the checkbox no longer crashes; allocation is still not functional and needs a targeted bypass. Hover is on hold. | Native-build and test checkbox closed/open, off/on, page changes and restart. Keep `[MasteryTrace]` state/click/spend lines and `[MasteryApi]` signatures for targeting the allocation check. Check actual point spending and mastery/innate preservation. See [follow-up](REVIEW_3_5_6.md). |
| 4 | Maxroll preview — `feat/maxroll-tree-preview` | Graphical trees improved; item retrieval/preview confirmed. | Equipment presentation improvements deferred. Keep existing preview functionality in regression checks. |
| 5 | Advanced Forge — `feat/advanced-forge-t7` (`7575671e`) | **Done — user confirmed #5 is good on 2026-10-09.** Includes the crafting options and existing-seal guard for Guaranteed Despair. | No active work. Retain selection 5 for optional regression; reopen only for a new reported issue. |
| 6 | Drop rates — `feat/independent-drop-rates` (`e1b3f857`) | Native build `75e23254` loaded and binding completed, but controls were invisible. Fixed the zero-height section caused by the legacy layout ignoring preferred height. 45 core tests passed; three native checks skipped. | Rebuild selection 6; scroll below Weaver Will and confirm four rows, slider/input synchronization and restart. Retain section/content dimensions from `[DropRates]`. Rate behavior remains unconfirmed. |
| 7 | Offline diagnostics — `test/offline-guard-diagnostics` | Done for now: confirmed working; spam removed, one confirmation retained. | Deferred at the user's request. No further #7 changes in this pass. |
| 8 | Idol rerolling — `feat/idol-reroll-misc` | Confirmed working. | Optional restart/locale regression; no immediate fix. |
| — | Prophecy — `fix/prophecy-reward-trigger` (`cb638c70`) | Confirmed working and persistent. | Large multipliers can lag. UI relocation deferred until redesign. Old multiplier branch is an isolation checkpoint. |
| 9 | `master` baseline | Control build. | Use for comparisons; topic-branch confirmation does not mean integration into main or upstream. |
| 10 (retired) | QoL — `feat/gold-favourites-session-stats` (`638504c0`) | Fully merged into selection 11, including branch history. | Use selection 11. Old branch retained as a historical reference. |
| 11 | Combined QoL / Travel Anywhere — `feat/travel-anywhere` (`f53998db`) | User confirmed map travel on `2466cc0a`; log confirms the native build loaded and all six key destination IDs resolved. Full picker removed at user request. Its rejected-load busy state is corrected; ordinary waypoint favourites use the native route. QoL history merged; 1,074 core tests passed, six SDK checks skipped. | Rebuild/retest key teleports after failed travel, gold, movable counter and natural favour. Detailed load/recovery and map regressions remain pending. Non-waypoint right-click is accepted as a known limitation for now. |

## Next development order

1. Rebuild combined selection #11. Selection #10 is retired and fully merged here. Map-menu travel is user-confirmed; retest key teleports with the full picker removed and failed-load busy state fixed, then the gold/counter/favour additions.
2. Native-build and runtime-check the Drop Rates UI recovery (#6).
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
- #7 is done for now; any deeper offline investigation is deferred. No offline-diagnostics patches changed in this pass.

## Testing and evidence

- Runner: `chore/test-queue-runner`. Selection 1 is the combined Force Drop build; selection 11 is the combined QoL/travel build. Selections 2 and 10 are retired; other IDs remain unchanged.
- Close the game before switching builds. Each selection replaces the installed DLL; it does not combine every pending feature branch.
- Follow [TEST_QUEUE.md](TEST_QUEUE.md) for the runner and per-feature checks. Force Drop's detailed checklist lives on its feature branch in `docs/TEST_FORCE_DROP_ILLEGAL_MODE.md`.
- Latest published Force Drop source: `2c04bdd0`. Global item-search matching/identity tests and the existing core/source suite passed: 1,164 passed, six SDK-dependent skips. Formatting and diff checks passed. Current native build, search/scrolling/cyan and combined-build regressions remain pending.

- QoL source `638504c0` is fully merged into combined travel `f53998db`; selection 10 is retired. Key teleport resolution on `2466cc0a` is confirmed by the supplied log, but travel was blocked by a failed full-picker load. Gold, movable counter and favour regression checks remain open. See the [QoL checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/travel-anywhere/docs/TEST_GOLD_FAVOURITES_SESSION_STATS.md).
- Combined travel source: `f53998db`. User reports map-menu travel works on `2466cc0a`; its log confirms the native build loaded. The picker attempted `EoT -> WE502`, returned no load and kept the busy guard set. The picker is removed; verified rejected loads release the guard immediately, while real asynchronous cleanup remains protected. Five new core tests cover this distinction. This follow-up’s native build and recovery/key-teleport checks remain pending. See the [travel checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/travel-anywhere/docs/TEST_TRAVEL_ANYWHERE.md).
- Keep failed runtime issues open even when compilation or unit tests pass. Record the tested commit, screenshots/logs and persistence results separately.

- LP report clarification: the screenshot had Corrupted: Yes, which independently removes LP with all affixes None. No LP rule change was made. Test Corrupted: No with all ordinary affixes cleared, then toggle back; unlock confirmation remains pending.

- Branch cleanup: `feat/force-drop-illegal-mode` at `5b8199f6` is fully merged into `feat/force-drop` and retired from the active queue. Remote-ref deletion is deferred at the user's request; leave this branch untouched and ignore it going forward.

- QoL branch cleanup: `feat/gold-favourites-session-stats` is fully merged into `feat/travel-anywhere` and retired from the active queue. Leave its remote ref as a historical reference; future QoL/travel changes go on the combined branch.
