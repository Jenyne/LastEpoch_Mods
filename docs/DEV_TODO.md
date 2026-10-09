# Last Epoch development todo

Updated 2026-10-09 (UTC). Repository: `Jenyne/LastEpoch_Mods`; upstream: `Syncingoutt/LastEpoch_Mods`.

Keep changes on separate topic branches. Force Drop legal and illegal work is now one topic: `feat/force-drop`. Preserve the current Force Drop layout until the planned full UI redesign. Runtime confirmation and compilation/unit-test results are separate evidence.

## Active work and current statuses

| Queue | Topic / branch | Current status | Remaining work |
|---|---|---|---|
| 1 | Force Drop — `feat/force-drop` (`2c04bdd0`) | Legal/illegal histories combined and published. Four ordinary T8s, seals/corruption, extra distinct Rage modifiers and persistence confirmed on earlier `5b8199f6`. Global cross-category base/unique/set search and exact-identity category/rarity autofill added. Selection 2 is retired; scrolling/cyan/search UI awaits testing. | Search seed before selecting category/rarity; check aliases/locales, auto-filled helmet/Unique identity, duplicate names, empty results and clear-query behavior. Test full-width legal prefix/suffix lists, split legal enchantment/sealed/corruption pools, layout transitions, independent scrolling, scrollbar handles, pinned None, search reset, row clipping and menu rebuilding. Check cyan ordinary idol/Weaver/enchantment affixes in choices and selected rows. Retest legal restrictions and LP clearing/re-enabling, including dedicated modifier-only LP retention. Verify individual gameplay effects. |
| 3 | Mastery / ground tooltips — `fix/mastery-lock-ground-tooltips` (`764ea599`) | Remove Node Requirements works/persists. Two 7a91 runs stop at the native 22→45 cap-write boundary. Latest candidate removes all global-cap writes; 999 tests passed, six SDK checks skipped. User confirms clicking the checkbox no longer crashes; allocation is still not functional and needs a targeted bypass. Hover is on hold. | Native-build and test checkbox closed/open, off/on, page changes and restart. Keep `[MasteryTrace]` state/click/spend lines and `[MasteryApi]` signatures for targeting the allocation check. Check actual point spending and mastery/innate preservation. See [follow-up](REVIEW_3_5_6.md). |
| 4 | Maxroll preview — `feat/maxroll-tree-preview` | Graphical trees improved; item retrieval/preview confirmed. | Equipment presentation improvements deferred. Keep existing preview functionality in regression checks. |
| 5 | Advanced Forge — `feat/advanced-forge-t7` (`7575671e`) | **Done — user confirmed #5 is good on 2026-10-09.** Includes the crafting options and existing-seal guard for Guaranteed Despair. | No active work. Retain selection 5 for optional regression; reopen only for a new reported issue. |
| 6 | Drop rates — `feat/independent-drop-rates` (`75e23254`) | UI recovery implemented through the live resolver, deferred layout and native input hooks. Runtime behavior/persistence unconfirmed. | Compile and confirm four rows, legacy layout, slider/input synchronization and restart; then measure independent Unique/Set/Exalted/T7 behavior. See [follow-up](REVIEW_3_5_6.md). |
| 7 | Offline diagnostics — `test/offline-guard-diagnostics` | Done for now: confirmed working; spam removed, one confirmation retained. | Deferred at the user's request. No further #7 changes in this pass. |
| 8 | Idol rerolling — `feat/idol-reroll-misc` | Confirmed working. | Optional restart/locale regression; no immediate fix. |
| — | Prophecy — `fix/prophecy-reward-trigger` (`cb638c70`) | Confirmed working and persistent. | Large multipliers can lag. UI relocation deferred until redesign. Old multiplier branch is an isolation checkpoint. |
| 9 | `master` baseline | Control build. | Use for comparisons; topic-branch confirmation does not mean integration into main or upstream. |
| 10 | Gold / favourite teleports / session counters — `feat/gold-favourites-session-stats` (`638504c0`) | Key teleport buttons, Alt-drag/saved/clamped overlay and live favour reconciliation added after the user reported favour did not count. 1,029 tests passed; six SDK-dependent checks skipped. | Rebuild/retest dungeon entrance resolution and ordinary waypoint locks, overlay movement/persistence, natural favour/Amber and no hook/poll doubles or manual grant credit. Retest fixed gold, pause/reset and zone continuity. |
| 11 | Travel Anywhere — `feat/travel-anywhere` (`2466cc0a`) | Windows build of `5744c5b7` failed on native IList.Count before installation. Source now uses explicit ICollection.Count and includes QoL follow-up `638504c0`. 1,069 tests passed; six SDK-dependent checks skipped. | Compile on the current SDK and test the default-off picker, non-waypoint menu Travel, right-click, map flag restoration/stale menus, spawn gates, recovery/cleanup, non-waypoint favourites and parent QoL regression. All gameplay remains unconfirmed. |

## Next development order

1. Test Travel Anywhere with selection #11; it includes the three QoL additions from #10. Enable it under Scenes > Misc, then test the picker, normal left-click area menus and right-click travel. Selection #10 is the QoL-only comparison build and also has the key teleport/drag/favour follow-up. Rebuild #11 first to check the reported CS1061 fix; native compilation and new gameplay checks remain pending.
2. Native-build and runtime-check the Drop Rates UI recovery (#6).
3. Test #3 with the global-cap write removed, then collect allocation-check signatures and real spend results. Remove Node Requirements is working/persistent. Hover is on hold; no hover work or test requested in this pass.
4. Obtain in-game results for the combined Force Drop UI and legal LP regression (#1). Do not mark the new UI confirmed from the earlier persistence report.

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

- Runner: `chore/test-queue-runner`. Selection 1 is the single combined Force Drop build. Selection 2 is retired; IDs 3–11 remain unchanged.
- Close the game before switching builds. Each selection replaces the installed DLL; it does not combine every pending feature branch.
- Follow [TEST_QUEUE.md](TEST_QUEUE.md) for the runner and per-feature checks. Force Drop's detailed checklist lives on its feature branch in `docs/TEST_FORCE_DROP_ILLEGAL_MODE.md`.
- Latest published Force Drop source: `2c04bdd0`. Global item-search matching/identity tests and the existing core/source suite passed: 1,164 passed, six SDK-dependent skips. Formatting and diff checks passed. Current native build, search/scrolling/cyan and combined-build regressions remain pending.

- QoL source: `638504c0`, based on `master` `92fb33de`. All 18 changed C# files passed CSharpier; 25 new core test cases cover counters and favourite persistence. The native SDK is unavailable here, so compilation/Harmony/UI/gameplay checks remain open. See the [QoL checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/gold-favourites-session-stats/docs/TEST_GOLD_FAVOURITES_SESSION_STATS.md).
- Travel source: `2466cc0a`, based on QoL `87e5d8c4` with follow-up changes from `638504c0`. The `5744c5b7` Windows build failed on native IList.Count; the explicit ICollection.Count fix was published separately as `ee16ef05`. Native rebuild remains pending. Historical source and the old compiled release were inspected; the old DLL was never executed. Forty new core cases cover destination exclusions, generated-area sources and transition ordering/recovery. The initial 13 C# files and four menu/right-click follow-up files passed CSharpier. Current SDK signatures, Harmony targets, UI/map behaviour and native recovery remain pending. See the [travel checklist](https://github.com/Jenyne/LastEpoch_Mods/blob/feat/travel-anywhere/docs/TEST_TRAVEL_ANYWHERE.md).
- Keep failed runtime issues open even when compilation or unit tests pass. Record the tested commit, screenshots/logs and persistence results separately.

- LP report clarification: the screenshot had Corrupted: Yes, which independently removes LP with all affixes None. No LP rule change was made. Test Corrupted: No with all ordinary affixes cleared, then toggle back; unlock confirmation remains pending.

- Branch cleanup: `feat/force-drop-illegal-mode` at `5b8199f6` is fully merged into `feat/force-drop` and retired from the active queue. Remote-ref deletion is deferred at the user's request; leave this branch untouched and ignore it going forward.
