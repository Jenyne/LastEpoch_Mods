# 4.4.18: Force Drop redesign, corruption and Ascendance fixes, HUD updates

This brings the custom 4.4.18 changes onto the v4.4.17 baseline. Rune of Ascendance now refreshes the converted item's cached name, the redesigned Force Drop editor supports native corrupted-affix packing with compatibility checks, and menu clicks open the requested tab instead of producing the yellow-screen/double-click behavior.

## Main changes

- Text-only Force Drop builder with searchable catalogs, grouped categories including a dedicated Idol section, selected-item highlighting, compact roll controls, and Blessings removed.
- Native corrupted-affix catalog/construction/serialization; duplicate and exact item-compatibility validation; packing verification stops failed drops and logs diagnostics.
- Ascendance name-cache refresh; native sealed-affix metadata; native initialization preserved for implicit and unique-roll overrides.
- Synchronized integer slider inputs, percentage display/conversion, ordinary T7 caps, and preservation of existing sealed primordial ranges.
- Soul Ember amount/add controls restored to Character using the dungeon manager.
- Summon God Mode/Forever/Don't Collide controls and settings; AutoCast runtime instantiation and corrected keybind instructions; Safe Teleport disabled.
- Scenes/Monolith timeline layout, Copy to All, English locale fallback/corrected labels, and Korean locale output.
- Retained custom skill-cap/respec, Memory Amber, auto-store debounce, Quad Stash, fog-of-war, Temporalis, Headhunter icon, Damage Meter, settings initialization, and optional profiling changes.

The complete source comparison and validation notes are in [CHANGELOG.md](https://github.com/Jenyne/LastEpoch_Mods/blob/88cc139f03d3e8523a49032d45f7c29e9a816154/CHANGELOG.md).

## Validation

The maintainer confirmed these behaviors in game:
- Correct names after Rune of Ascendance conversion.
- Successful corrupted Force Drop for the previously failing test item.
- Yellow-screen/double-click menu issue resolved.
- Temporalis working.
- Headhunter inventory/tooltip icon working after the final runtime-sprite and targeted refresh patch.
- Safe Teleport shortcut conflict resolved by disabling it.

C# syntax and relevant game-wrapper metadata were checked during implementation. This review environment has not run a fresh .NET build or the repository's entire test suite.

## Remaining validation

Soul Ember addition still needs gameplay testing. Live skill-tree refresh previously failed testing and is not claimed fixed. Other retained changes need broader regression testing; successful corrupted-drop testing does not establish coverage of all items, sealed-plus-corrupted combinations, or save/reload behavior. This PR does not add the separate offline skill-cosmetics fix proposed in #8.

The local D: Steam path detection in the project file is conditional; an explicit LastEpochPath still takes precedence. The version is 4.4.18. Upstream 4.4.17 LP/forging-potential fixes and translation/test-project work are inherited, not claimed as new fixes.
