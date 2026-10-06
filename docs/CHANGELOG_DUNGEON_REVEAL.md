# Dungeon Objective Reveal — pending game testing

- Scenes > Dungeons: new Reveal Dungeon Objectives toggle, separate from Monolith reveal.
- Default off; persists in SaveModUI.json.
- Apply the existing Monolith reveal-threshold approach to DungeonZoneManager.initialise.
- Capture each dungeon manager's original threshold and restore it when disabled. Reinitialisation restores the previous override before taking a new baseline.
- English, French, Korean and Simplified Chinese labels supplied.

Validation: native initialise and threshold APIs verified against supplied Il2CppLE.dll; syntax checked. Full compilation and gameplay pending.

Test each dungeon/floor, enabled before entry and toggled during a run. Check objective/exit markers, disabling, zone changes and saved settings after restarting. Actual objective-pulse behavior requires gameplay validation; this does not complete objectives or unlock doors. Enter Without Key and Herald of Ice remain separate investigations.
