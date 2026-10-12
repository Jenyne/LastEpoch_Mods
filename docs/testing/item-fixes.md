# Item fixes test branch

Based on Syncingoutt/LastEpoch_Mods master `15ab56db614e3a7874a6c91babafc5050f769d77`.

Run `scripts/Test-ItemFixes.ps1 -GamePath "D:\SteamLibrary\steamapps\common\Last Epoch"` with the game closed. It updates this branch, builds, runs the tests, backs up the installed DLL, and copies the new DLL into Mods.

## Unique seals

- With Illegal off, select a Unique: Sealed and Primordial must stay disabled. Repeat after changing Fixed/Random and selecting a different item. Set items retain their existing legal affix restrictions.
- With Illegal on, select a Unique and four distinct ordinary T7 affixes, a distinct regular sealed T7 affix, a distinct Primordial T8 affix, and a distinct corrupted affix. Drop, pick up, equip, change area and reload. All seven affixes and their seal types must remain intact.
- Also test only a regular seal, only a Primordial seal, and both without ordinary affixes. These still require Legendary storage and zero LP.
- Retest a base item with the same seven slots. Its seal construction path is unchanged.
- Item creation retains final field/affix verification and a native pack/unpack comparison. A failed check rejects the drop; send the exact error.

## Illegal Set pieces

Turn Remove Set Requirements off. Add affixes to two different pieces from one set, equip them, and check both the set-piece indicators and the actual bonus. Remove one and confirm the bonus is removed. Reload with Illegal mode off and retest. A duplicate of the same set piece must not count as another distinct piece. Also retest ordinary unmodified sets and a unique carrying a foreign set affix.

Affixed Set pieces retain Legendary serialization and their original unique ID. Recognition uses the saved identity, not the creation toggle. The patch updates the cached set ID, set membership and distinct equipped-piece count; it does not change native set definitions or required piece counts.

## Combat ground tooltips — investigation, not a confirmed fix

Hover a ground item outside combat, then reproduce the missing tooltip during combat in the same scene. Send the `[Ground Tooltip]` lines and any exception nearby. Record whether autocast, the damage meter and the Headhunter/Mageblood bars were active, and whether disabling each changes the result.

Logging observes native ground-item pointer entry, tooltip opening and closing, with frame numbers and a limit of 60 events per scene. Exceptions are logged and propagated. No tooltip-open or combat restriction is overridden. No hover lines during failure is useful evidence too: the event may be blocked before reaching TooltipItem.

## Local validation

Full source compiled with the .NET 6 reference pack, Harmony 2.3.1.1, MelonLoader 0.7.3 references and the supplied game interop assemblies. Unit and available interop/patch checks: 2,370 passed, 10 skipped (missing native Cpp2IL output/GameAssembly). Changed C# files pass CSharpier. Native item creation, equipped bonuses, persistence and the combat reproduction still require the game.
