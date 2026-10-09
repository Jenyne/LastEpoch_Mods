# Last Epoch development todo

Updated 2026-10-09 (UTC). Repository: `Jenyne/LastEpoch_Mods`; upstream: `Syncingoutt/LastEpoch_Mods`.

Keep changes on separate topic branches. Force Drop legal and illegal work is now one topic: `feat/force-drop`. Preserve the current Force Drop layout until the planned full UI redesign. Runtime confirmation and compilation/unit-test results are separate evidence.

## Active work and current statuses

| Queue | Topic / branch | Current status | Remaining work |
|---|---|---|---|
| 1 + 2 | Force Drop — `feat/force-drop` (`4585e880`) | Legal/illegal histories combined and published. Four ordinary T8s, seals/corruption, extra distinct Rage modifiers and persistence confirmed on earlier `5b8199f6`. New scrolling/cyan UI awaits testing. | Test independent scrolling, scrollbar handles, pinned None, search reset, row clipping and menu rebuilding. Check cyan ordinary idol/Weaver/enchantment affixes in choices and selected rows. Retest legal restrictions and LP clearing/re-enabling, including dedicated modifier-only LP retention. Verify individual gameplay effects. |
| 3 | Mastery / ground tooltips — `fix/mastery-lock-ground-tooltips` (`6e835e85`) | Runtime failures remain open. Bounded mastery click/spend traces and manual F9 hover snapshots added. | Compile on the installed SDK. Leave Remove Node Requirements off and capture `[MasteryTrace]`; press F9 over the same loot idle/in combat with the meter hidden/visible. Use the native rejection/raycast evidence for the next fix. See [follow-up](REVIEW_3_5_6.md). |
| 4 | Maxroll preview — `feat/maxroll-tree-preview` | Graphical trees improved; item retrieval/preview confirmed. | Equipment presentation improvements deferred. Keep existing preview functionality in regression checks. |
| 5 | Advanced Forge — `feat/advanced-forge-t7` (`b59281d6`) | Native max-tier label matching repaired; bounded rejection trace added. In-game T5 ceiling remains unconfirmed. | Inspect `[ForgeTrace]`, cached item and native keys. Complete material consumption, glyph semantics and definition-backed tier checks before closing the crafting issue. See [follow-up](REVIEW_3_5_6.md). |
| 6 | Drop rates — `feat/independent-drop-rates` (`75e23254`) | UI recovery implemented through the live resolver, deferred layout and native input hooks. Runtime behavior/persistence unconfirmed. | Compile and confirm four rows, legacy layout, slider/input synchronization and restart; then measure independent Unique/Set/Exalted/T7 behavior. See [follow-up](REVIEW_3_5_6.md). |
| 7 | Offline diagnostics — `test/offline-guard-diagnostics` | Done for now: confirmed working; spam removed, one confirmation retained. | Deferred at the user's request. No further #7 changes in this pass. |
| 8 | Idol rerolling — `feat/idol-reroll-misc` | Confirmed working. | Optional restart/locale regression; no immediate fix. |
| — | Prophecy — `fix/prophecy-reward-trigger` (`cb638c70`) | Confirmed working and persistent. | Large multipliers can lag. UI relocation deferred until redesign. Old multiplier branch is an isolation checkpoint. |
| 9 | `master` baseline | Control build. | Use for comparisons; topic-branch confirmation does not mean integration into main or upstream. |

## Next development order

1. Native-build and runtime-check the Drop Rates UI recovery (#6).
2. Use the new rejection traces to target the T5 crafting ceiling and complete the craft transaction (#5).
3. Use allocation and F9 hover evidence to resolve both runtime failures (#3).
4. In parallel with those priorities, obtain in-game results for the combined Force Drop UI and legal LP regression (#1/#2). Do not mark the new UI confirmed from the earlier persistence report.

## Backlog and deferred work

- Cosmetics/skill effects: investigation requested for missing menu choices, effects that will not apply and effects resetting. No confirmed diagnosis or fix is recorded in this tracker; inspect the current fork before proposing changes. Use a separate topic branch.
- Full Force Drop UI redesign: deferred; only the requested scrolling and color changes are in the current combined branch.
- Maxroll equipment presentation: deferred.
- Prophecy UI relocation: deferred.
- #7 is done for now; any deeper offline investigation is deferred. No offline-diagnostics patches changed in this pass.

## Testing and evidence

- Runner: `chore/test-queue-runner`. Entries 1 and 2 target the same combined Force Drop branch; they keep their existing IDs for legal and illegal checklists.
- Close the game before switching builds. Each selection replaces the installed DLL; it does not combine every pending feature branch.
- Follow [TEST_QUEUE.md](TEST_QUEUE.md) for the runner and per-feature checks. Force Drop's detailed checklist lives on its feature branch in `docs/TEST_FORCE_DROP_ILLEGAL_MODE.md`.
- Latest published Force Drop source: `4585e880`. Initial scroll/cyan implementation compiled and passed 1,162 tests (zero skipped); after workspace recovery, formatting passed across 408 C# files. In-game scroll/cyan and combined-build regressions remain pending.
- Keep failed runtime issues open even when compilation or unit tests pass. Record the tested commit, screenshots/logs and persistence results separately.
