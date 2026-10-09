# Gold, favourite teleports and session counters

Branch: `feat/gold-favourites-session-stats`, based on `master` at `92fb33de`.

These three additions share one test build. Other pending feature branches remain separate.

## Gold button

Character > Cheats > Currencies contains **Spawn 1,000,000 Gold**. It calls the native local-player gold drop method at the player's position. The existing Auto Pickup Gold option can intercept the drop and credit the wallet immediately.

1. Disable Auto Pickup Gold and click once in an open area. Check a gold pile appears and collecting it adds exactly 1,000,000 gold. Use a character below the native currency cap.
2. Enable Auto Pickup Gold and click once. Check the wallet receives 1,000,000 without leaving a pile. Repeat with the normal monster gold multiplier enabled; the button should still grant the fixed amount.
3. Check the button adds no runes or other currencies. Close/reopen the HUD, change zones, restart and check one click still triggers one grant.

## Favourite teleports

Scenes > Misc now scrolls. It contains Safe Teleport, Session Gains and Favourite Teleports. The list has buttons for End of Time, Temporal Sanctum, Lightless Arbor, Soulfire Bastion, the Bazaar and the Observatory. Open the world map once, then click **Refresh key teleports**. The buttons resolve real native map waypoint scenes by scene/localized name; missing or ambiguous matches are rejected with an explanation. They use unlocked waypoint travel to dungeon entrances, rather than loading dungeon interiors. Visit another waypoint area, unlock its waypoint and use **Favourite current waypoint**. Eight destinations can be saved. Click a saved destination to travel, or **Remove** to delete it.

Saved favourites are shared mod preferences in `Mods/LastEpoch_Hud/FavouriteTeleports.json`. Travel checks the current character's unlocked waypoints and native map pins each time. Saving or using a favourite does not unlock a waypoint. If map pins have not been loaded, open the world map once and retry.

1. Test each key button, including all three dungeon entrances. Check keys/tier entry still operate normally, ambiguous/unavailable and locked waypoints are rejected, and no unlock is added. Then save two different unlocked areas, including End of Time. Test both return trips and travelling to the current area. Check the same area cannot occupy two slots.
2. Fill eight slots. Check a ninth is rejected; remove one and save a replacement. All rows and Remove buttons should be accessible by wheel and drag inside Misc.
3. Restart and check order, names and saved destinations. Switch to a character that lacks a saved waypoint; travel must be rejected and its waypoint list must stay unchanged.
4. Try saving an area without a waypoint. It should show an explanation and leave the file unchanged. Test the recovery message before/after opening the world map.
5. Check Safe Teleport still captures and retains its binding, Dungeons/Minimap remain visible, and the Misc viewport clips scrolled controls. Rebuilding the menu must not create duplicate buttons.

## Session counters

The HUD shows session time, positive XP/Favour/Memory Amber gains and average rates per active hour. It also appears under Scenes > Misc > Session Gains, with **Hide/Show counter HUD**, **Pause/Resume**, **Reset** and **Reset counter position**. Hold **Alt** and left-drag the overlay to move it. Its position persists and is clamped to the screen; it adds no canvas/raycast surface.

- Favour includes Circle of Fortune and Merchant's Guild. Memory Amber uses the Weaver's native favour balance. Forgotten Knights reputation is excluded.
- The current local actor’s faction balances are sampled alongside the native gain hook. Wallet baselines prevent hook/poll double counting and are refreshed during pause/loading/manual grants. Positive native balance changes count; spending and unchanged balances do not. Nested XP calls are observed once. The mod's manual Favour and 10,000 Amber buttons are excluded.
- The timer excludes unavailable-player/menu/paused frames and long loading frames. Zone transitions preserve totals. Returning to character selection/login starts a new session. Reset clears totals/time and resumes recording.
- Session totals are intentionally temporary. The ShowOverlay preference persists in `SaveModUI.json`.

1. Reset, farm for 60 active seconds and compare all three totals with actual positive gains. At 60 seconds each displayed hourly rate should be approximately 60 times its total.
2. Check enemy XP, quest/direct XP, XP tomes, level-up and XP cap cases. Compare against actual granted XP, including configured multipliers. Verify nested calls do not double count and a level-up does not lose XP.
3. Test both trade factions and a Weaver Amber pickup, with Amber auto-pickup on/off and multipliers on/off. Spend Favour/Amber; the cumulative totals must not decrease. Test gains near the currency cap.
4. Use Add Favour, Set Favour and Add 10,000 Memory Amber; these manual grants must not increase farming totals.
5. Pause, farm, then resume. Time and gains should stay frozen while paused and resume from their previous totals. Reset should clear every counter.
6. Open a pausing menu, change zones/run an echo, return to character selection, then load another character. Check pause/loading time, zone continuity and character reset separately.
7. Alt-drag the overlay to every screen edge, hide/show it, restart and check visibility and position. Use Reset counter position and change resolution. Hover ground loot and cast skills under the overlay; labels must not capture mouse/controller input. Check 1080p/1440p/4K readability and overlap with native UI.
8. Switch EN > FR > KO > ZH > EN. Check captions, statuses and number formatting, then close/reopen the menu.

## Verification recorded here

- User reported that favour did not count in the initial build. The current fix uses the live local tracker and reconciles wallet samples with gain events; native success is pending. A gain and spend that both occur between samples on an unhooked route can only be observed as a net balance change.
- Preset destination matching, current SDK compilation and Alt-drag persistence require runtime testing. Retain `[KeyTeleports]` lines if a dungeon button cannot resolve its waypoint; no scene IDs are guessed.

- Direct Roslyn/.NET 8 test execution: **1,029 passed; six game-dependent checks skipped**, zero failures. This includes 25 new cases for gain arithmetic, pause/reset, spending, saturation, favourite validation/capacity, JSON round trips and disk replacement.
- Formatting and whitespace checks run on the changed C# source.
- Native mod compilation, Harmony target resolution, HUD layout and gameplay checks require the installed Last Epoch/MelonLoader SDK. None of these runtime behaviours is confirmed by the pure tests.

## Travel beyond waypoints: follow-up investigation

Upstream commit `65b8e77b348ed0f2c1420ff6bdfc118386df0bea` (2026-10-04) deliberately hides the old scene picker and replaces direct additive scene loading/player placement with the native waypoint transition service. The previous helper called `LoadSceneAsync`, `TryPlacePlayerAtSpawn(..., 0, ...)` and unloaded the old scene. The current helper resolves a map waypoint gate, with gate zero as its fallback.

Restoring a general scene selector is a contained UI task. Supporting click-to-travel on every world-map node additionally needs native map-node/gate and scene-entry validation, especially locations without a waypoint and progression/instance-specific areas. The old direct-loader path needs a current-game regression before reuse. This build adds favourite unlocked waypoints; general map-node travel remains a separate follow-up, not a confirmed restored feature.
