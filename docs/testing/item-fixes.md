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

## Set tooltip layouts

The legacy CreateTooltipContent dispatcher inlines default content in ordinary display mode, so a CreateTooltipDefaultContent hook alone cannot cover it. The final dispatcher result now receives the same native set bonus formatting as the default/full paths, with exact-string deduplication. Native UpdateAffixesText iterates only the prefab row pool. For recognized illegal set items, that pool now grows to fit the complete unique/set text list, preventing trailing set-bonus text from being omitted when T8 combinations produce more entries. Extra rows are registered in the native SimpleLayoutGroup with the template's order and sizing flags, then measured after text initialization. They are reused and hidden by the native renderer when unused. Recognition and equipment counting are unchanged. Check gloves and amulets in ordinary, expanded, comparison, ground and inventory tooltips, then equip/relog.

## Ground tooltip test

The native CloseAllTooltips operation closes the manager and then independently clears the legacy visible view. The guard protects only the matching view when that same operation's manager close was rejected for a valid hovered ground item. Normal pointer exit, pickup, Escape, menus, item replacement and unrelated tooltip cleanup remain native.

Nyk confirmed combat hovers, zone changes, inventory drops and re-pickup without stuck tooltips. Temporary hover/close traces and Force Drop pool/metadata diagnostics have been removed; real action and persistence errors remain visible.

Retest fresh combat hovers, leaving labels, switching items, pickup, Escape, inventory/HUD, Alt comparison and area changes after this cleanup.

## Validation

Previous combined build c8328c22 passed all 2,716 tests on the user's Windows installation, including native checks. This follow-up needs an in-game check of the gloves' bonus text in each layout. Run scripts/Test-ItemFixes.ps1 to build, test, back up and transfer the DLL.
