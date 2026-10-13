# Item fixes test branch

Based on Syncingoutt/LastEpoch_Mods master `15ab56db614e3a7874a6c91babafc5050f769d77`.

Run `scripts/Test-ItemFixes.ps1 -GamePath "D:\SteamLibrary\steamapps\common\Last Epoch"` with the game closed. It updates this branch, builds, runs tests, backs up the installed DLL, and copies the new DLL into Mods. When the native game and Cpp2IL metadata are available, it also exports `Build/ItemNativeProbe.zip`.

## Confirmed in-game

The user confirmed the Set display/count fix and T8 seal controls work in d90b954f. Legal Unique/Set seal restrictions remain in place. Retest distinct-piece counting with Remove Set Requirements off, duplicates, item removal, and reload if changing equipment.

## Unique regular seals — native-based candidate

The native export established that Legendary corruption belongs to the last affix
and that its saved format lacks a regular-seal flag. The current candidate orders
corruption accordingly and persists explicitly created illegal regular seals in
`Mods/LastEpoch_Hud/IllegalUniqueSeals.json`, tied to the full saved item ID. The
base seven-T8 socket-count rejection is also corrected without weakening affix
round-trip checks. See [native-unique-seals.md](native-unique-seals.md) for exact
native evidence, storage behavior, limits and tests.

Test Unique and base seven-slot setups at T7/T8, including pickup, equip, area
change, Illegal off and a full restart. Keep the JSON file with the mod's data.
Native item creation/persistence still need in-game confirmation.

The native exporter remains available through `scripts/Export-ItemNativeProbe.ps1`
or the optional `-ExportNativeProbe` switch on the build/install script. A normal
test install no longer repeats this export.

## Ground tooltip test

The trace showed ground tooltips opening successfully and closing during initialization with neither mouse button held. A highlighted TooltipItem alone is insufficient proof that the pointer remains over it.

This candidate guard keeps an active ground tooltip only when a fresh UI raycast places its opener/highlighted label at the front, the item and label are valid, the application is focused, no mouse button or Escape is held, no blocking UI is present, and no replacement tooltip is opening. It allows native closes for everything else. It neither forces a tooltip open nor suppresses native exceptions.

Hover ground loot in combat, then test leaving the label, left/right click, pickup, Escape, opening inventory/the HUD, switching to another item, and changing areas. Send `[Ground Tooltip]` entries if a failure remains. Active-close entries report `kept` or `allowed`, the native pointer-exit source, the opener/highlight instance paths, cursor and label positions, and the first three UI raycast hits. Pointer-exit entries also include Unity's event hit. These diagnostics distinguish label movement, an intercepting UI element, and a cleared/inactive highlight without changing the guard. Output is bounded to 120 entries per scene; inactive close spam is omitted.

## Validation

Full mod source compiled against .NET 6, Harmony 2.3.1.1, MelonLoader 0.7.3 and supplied game interop assemblies. Available unit/interop/patch checks pass; native Cpp2IL checks are skipped locally. The native exporter also passes a synthetic fixture check of the binary hash, addresses, and exact code slices. In-game tooltip behavior and the remaining unique-seal repair still require verification.

The b642173b runtime trace reaches Completed while the user reports most tooltips never appear. Blocking the legacy manager close does not establish visible output. The native exporter now includes TooltipSystem, ItemTooltip, UITooltipItem, their nested helpers, and ground-label update code to trace the opening/visibility path. Run scripts/Export-ItemNativeProbe.ps1 and attach Build/ItemNativeProbe.zip; this exporter reads game files and does not replace the installed mod.
