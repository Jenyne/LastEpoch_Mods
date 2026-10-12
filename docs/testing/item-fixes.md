# Item fixes test branch

Based on Syncingoutt/LastEpoch_Mods master `15ab56db614e3a7874a6c91babafc5050f769d77`.

Run `scripts/Test-ItemFixes.ps1 -GamePath "D:\SteamLibrary\steamapps\common\Last Epoch"` with the game closed. It updates this branch, builds, runs tests, backs up the installed DLL, and copies the new DLL into Mods. When the native game and Cpp2IL metadata are available, it also exports `Build/ItemNativeProbe.zip`.

## Confirmed in-game

The user confirmed the Set display/count fix and T8 seal controls work in d90b954f. Legal Unique/Set seal restrictions remain in place. Retest distinct-piece counting with Remove Set Requirements off, duplicates, item removal, and reload if changing equipment.

## Unique regular seals — still blocked

The latest test failed during corruption packing: requested corruption ID 1016 decoded as Primordial, while ID 572 received FromCorruption. That is a serialized-slot ownership mismatch. The earlier repair that guessed the regular slot after fixed modifiers has been removed. Request verification and native pack/unpack checks continue to reject malformed drops.

Native interop DLLs expose signatures and fields, not the game's method implementations. Attach `Build/ItemNativeProbe.zip` so the current native packing logic can be inspected before choosing a replacement layout. The exporter reads GameAssembly.dll and cpp2il_out; it produces method addresses, bounded machine-code slices for item/tooltip methods, and a game-binary hash. It does not modify the game. It can also be run separately:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Export-ItemNativeProbe.ps1 `
    -GamePath "D:\SteamLibrary\steamapps\common\Last Epoch"
```

## Ground tooltip test

The trace showed ground tooltips opening successfully and closing during initialization with neither mouse button held. A highlighted TooltipItem alone is insufficient proof that the pointer remains over it.

This candidate guard keeps an active ground tooltip only when a fresh UI raycast places its opener/highlighted label at the front, the item and label are valid, the application is focused, no mouse button or Escape is held, no blocking UI is present, and no replacement tooltip is opening. It allows native closes for everything else. It neither forces a tooltip open nor suppresses native exceptions.

Hover ground loot in combat, then test leaving the label, left/right click, pickup, Escape, opening inventory/the HUD, switching to another item, and changing areas. Send `[Ground Tooltip]` entries if a failure remains. Active-close entries report `kept` or `allowed` and `pointerOnOpener`; inactive close spam is omitted.

## Validation

Full mod source compiled against .NET 6, Harmony 2.3.1.1, MelonLoader 0.7.3 and supplied game interop assemblies. Available unit/interop/patch checks pass; native Cpp2IL checks are skipped locally. The native exporter also passes a synthetic fixture check of the binary hash, addresses, and exact code slices. In-game tooltip behavior and the remaining unique-seal repair still require verification.
