# Gold, Session Gains and Prophecy — modern HUD test

Base: Syncingoutt/LastEpoch_Mods master at c3d399c (includes merged Force Drop and Advanced Crafting integrations).

## User controls
- Utilities > Currency: Spawn 1,000,000 Gold. Uses native local-player gold drop; Auto Pickup Gold can collect it.
- World > Misc > Session Gains: session time, XP, Favour and Memory Amber totals and rates. Toggle the HUD overlay, pause/resume, reset totals, or reset overlay position. Hold Alt and drag the overlay to move it.
- Utilities > Cheats: Prophecy Reward Multiplier, enable and choose whole-number x1–x10.

This branch deliberately excludes saved/favourite/key/dungeon teleport shortcuts. No native scene travel overrides are imported.

## Accounting
- Session statistics use a persistent runtime worker, independent of the configuration HUD.
- Favour covers Circle of Fortune and Merchant's Guild. Memory Amber is Woven/Weaver favour. Spending does not count as positive gains.
- The mod's Add Memory Amber action is excluded from session gains.
- Manual faction-grant actions must also be excluded; verify this after integration.
- Prophecy patch changes the temporary reward itemsDropped count only within the trigger and restores it in a Harmony finalizer; does not detour the nullable SpawnRewardForPlayer path.

## In-game test checklist
1. Build and install only this DLL and latest locales. Confirm the three feature sections appear and prior merged Force Drop and Advanced Crafting still work.
2. Spawn 1,000,000 Gold with Auto Pickup Gold off, then on; verify exactly 1,000,000 in each case.
3. Kill a mob and gain XP, CoF/MG favour and Weaver Amber. Confirm totals/rates increase once, pause/resume works, and 10,000 Amber cheat doesn't count.
4. Test session overlay display, Alt-dragging, reset position, reset totals, HUD hiding and character switching.
5. Prophecy: confirm baseline with setting disabled; enable x2, then x5 for equipment/material rewards. Verify correct quantities, charges, lenses and native bonus; turn off and confirm baseline returns. Test with a scene transition.
6. Confirm no favourite or dungeon teleport shortcut UI appears.
7. Run csharpier check and dotnet run --project LastEpoch_Hud.Tests against the same source before PR.
