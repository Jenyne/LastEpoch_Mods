# HUD rework integration changelog

Reviewed 2026-10-09. This is an integration plan, not a completed merge or runtime test.

Upstream source: [Syncingoutt/LastEpoch_Mods — feat/hud-ui-rework](https://github.com/Syncingoutt/LastEpoch_Mods/tree/feat/hud-ui-rework), reviewed at `a69b62b30e99564ec12baef3e6d1a4aa2c368d88`.

## What upstream changed

The redesign composes the UI at runtime over the existing prefab. The reviewed diff adds navigation, theme/font scaling, form cards and 15 page classes; it does not replace the asset bundle. `Hud_Manager` initializes `HudLayout` after existing settings bindings.

`HudLayout` reparents the menu/content and hides the old panels for converted pages. Consequently, merging our old control-injection code can compile cleanly while leaving the controls invisible. Port controls to the new `HudFormPage` methods and retain their settings keys and feature backends.

Use `HudTheme` for ordinary surfaces, text and spacing, while preserving semantic colors such as cyan idol affixes and item rarity. Preserve the updated `SliderHook`, which reads the slider's final value. New pages need construction, navigation, hide/show and refresh wiring; controls added to an existing page need its settings refresh handling.

One functional change requires an explicit decision: upstream reduces the Memory Amber multiplier maximum from 10,000 to 255 across its UI/bindings. Keep UI and backend limits aligned rather than restoring an older file wholesale.

## Per-feature changes required

Dry merges below compare each individual feature branch against the reviewed HUD head using `git merge-tree --write-tree`. They do not establish that a combined integration merge is clean or safe.

| Feature and reviewed source | Required changes for the new HUD | Individual merge result / status |
|---|---|---|
| Force Drop — `feat/force-drop`, `2c04bdd0` | Keep our builder logic: global item search, automatic category/rarity selection, legal and illegal affix scrolling, cyan idol affixes and current LP rules. Combine upstream theme changes into our builder. Preserve all-None LP availability for non-corrupted items. | Conflict in `ForceDropBuilder.cs`. Functionality user-confirmed; integration untested. |
| Advanced Forge — `feat/advanced-forge-t7`, `7575671e` | Add T7 crafting, affix-roll override, Guaranteed Hope and Guaranteed Despair to `Items_CraftingSlot`. Keep the existing-sealed-affix guard for Despair. Reconcile with upstream's existing Infinite Forging Potential implementation; do not overwrite it with the older branch version. | Conflicts in five locale JSONs, `Hud_Manager.cs`, `InfiniteForgingPotentialControls.cs`, `ModSettings.cs` and `Items_InfiniteForgingPotential.cs`. User-confirmed feature. |
| Independent drop rates — `feat/independent-drop-rates`, `e1b3f857` | Replace legacy injected geometry with four independently enabled controls on `Items_Drop`: Unique, Set, Exalted and T7. Retain 100% baseline, 0–1,000% range, persistence and forced-rarity priority. | Conflicts in five locale JSONs and `ModSettings.cs`. Combat crash remains open; UI integration is not a runtime fix. |
| Combined travel/QoL — `feat/travel-anywhere`, `f53998db` | Add Travel Anywhere and the six key destination actions to `World_Misc`; keep the full scene picker removed. Preserve optional saved favourites and existing Safe Teleport without duplication. Put the fixed million-gold action on `Utilities_Currency`. Add counter controls to an appropriate utilities/world page and retain its independent movable overlay and position persistence. Route new manual currency grants through the counter exclusions. | Conflict in `CharacterActionControls.cs`. Map-menu travel confirmed; key travel/recovery, gold and counter regression pending. Non-waypoint right-click is an accepted limitation. |
| Idol rerolls — `feat/idol-reroll-misc`, `9021133a` | Add separate unlimited-use and free-Memory-Amber toggles to `World_Misc`. Replace hidden legacy Scene/Misc row cloning with settings-bound controls. Preserve the two independent options and one-time event registration. | Textually clean. Feature user-confirmed. |
| Prophecy rewards — `fix/prophecy-reward-trigger`, `cb638c70` | Move the reward multiplier into `Utilities_Multipliers` using the new form controls; retain default 1× and 1–10× bounds. Keep the working reward-trigger fix and persistence. | Textually clean. Feature user-confirmed; very large rewards can lag. |
| Maxroll preview — `feat/maxroll-tree-preview`, `70232710` | Preserve the Force Drop header entry point. Adapt popup parent, sibling order, fonts and input handling to the rebuilt window so navigation does not hide or cover it. Keep parser, retrieval and graphical trees. | Textually clean against HUD alone; combined Force Drop changes may conflict. Equipment presentation remains deferred. |
| Mastery — `fix/mastery-lock-ground-tooltips`, `764ea599` | Keep working Remove Node Requirements on `Utilities_Character`. If retaining the unfinished mastery control, place it alongside that option and make its incomplete status clear. Do not revive global-cap writes. | Textually clean. Allocation past the mastery chain remains unimplemented; hover work stays on hold. |
| Mandatory offline — `test/offline-guard-diagnostics`, `fd79f606` | Use the unpatched native Play Offline handler with manual selection. Remove automatic dispatch and the failed log-readiness gate/listener. Keep online-action patches independent of HUD layout. No optional setting: old false/missing Login configuration must not enable online play. Online requires uninstalling the mod. Keep gameplay observer removed. | Updated local revision merges textually clean with the HUD head. Core checks pass; native build and mandatory-mode regression pending. |

Locale conflicts refer to `LastEpoch_Hud/LastEpoch_Hud/Locales/{base,en,fr,ko,zh}.json`. Merge keys rather than replacing these files. Existing upstream fixes and baseline features should remain; older branch ancestry is not a reason to reapply every old implementation.

## Recommended integration order

1. Send the standalone mandatory-offline PR to **Syncingoutt's master** first. It does not edit HUD layout. [PR text](https://github.com/Jenyne/LastEpoch_Mods/blob/test/offline-guard-diagnostics/docs/PR_OFFLINE.md).
2. Start a separate integration topic from the HUD branch. Preserve upstream navigation, theme, lifecycle and existing baseline fixes.
3. Integrate Force Drop, Advanced Forge, idol rerolls and Prophecy, moving each set of controls into its new page.
4. Integrate Maxroll and combined travel/QoL, checking overlays, input focus, manual currency exclusions and settings refresh.
5. Isolate the drop-rate combat crash before enabling that backend in the integrated candidate. Leave unfinished mastery allocation and hover out of a completed-feature claim.

Selections 2 and 10 are retired histories already consolidated into Force Drop and combined travel respectively; do not merge them again as separate features. A HUD merge does not complete Maxroll equipment presentation or mastery allocation.

## Acceptance checks

Build against the user's current native assemblies after conflict resolution. Run core and patch-target checks, then test every new page and setting with restart persistence. Check slider/text synchronization, scrolling, locale changes, font scaling, page switches and reopening the HUD.

Regress Force Drop search/category autofill, legal/illegal affixes, cyan styling and LP; both independent idol options; forge Despair with an already-sealed item; Prophecy rewards; key travel after rejected loads; the million-gold action; and counter dragging, zone continuity and natural favour/amber accounting. Check Maxroll popup visibility and input focus over the new window.

For mandatory offline, test the old false flag and missing Login section, controller online actions, two character entries, combat and echoes. The successful observer-free predecessor retest is evidence for that predecessor, not proof of this new revision or the integrated HUD.

Follow-up source for combined travel/QoL: `811f6bed` replaces unsafe dungeon-lobby presets with campaign approaches and redirects/blocks legacy dungeon favourites. Carry this correction into the HUD port; `f53998db` remains the head used for the original dry-merge comparison. New native/in-game confirmation is pending.
