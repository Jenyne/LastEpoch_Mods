# Changelog

## 4.4.18 — custom branch

This changelog describes the source differences between
[Syncingoutt v4.4.17](https://github.com/Syncingoutt/LastEpoch_Mods/releases/tag/v4.4.17)
(tag commit `b2399c7c521c02a20b83531a640e2f56e3708212`) and
[Jenyne v4.4.18](https://github.com/Jenyne/LastEpoch_Mods/tree/v4.4.18).
It includes earlier custom changes retained in this branch. It does not imply that a
4.4.18 release archive has been published.

### Confirmed in game

The maintainer has confirmed the following behaviors as of October 5, 2026:

- **Rune of Ascendance names:** converted items now display the resulting unique's name instead of retaining the original base item's cached name.
- **Corrupted Force Drop:** the previously failing test item now drops successfully after correcting corrupted-affix serialization order.
- **Menu yellow screen:** the menu-tab fix resolves the reported yellow-screen/double-click problem.
- **Temporalis:** reported working.
- **Safe Teleport disabled:** the change removing the Ctrl+Q shortcut conflict was reported working.

These confirmations cover the reported tests. They do not establish coverage of every item, affix combination, language, or gameplay situation.

### Force Drop

- Replaced the legacy editor with a text-only, three-column item builder: item selection, customization, and preview/drop controls.
- Added searchable, paginated item and affix lists.
- Grouped item categories into Weapons, Armour, Accessories, Idols, and Other. Category choices appear together without the old extra category page.
- Added a gold tint, stronger border, and a "Selected" label to the currently selected item.
- Compacted the affix, implicit, unique-roll, Legendary Potential, and Weaver's Will controls.
- Renamed Crafting Modifier to Runes and Crafting Support to Glyphs; clarified Lens labels.
- Removed Blessings from the selection list.
- Added a separate corrupted-affix selector alongside the ordinary affix rows, populated from game data.
- Excluded corrupted affixes from ordinary prefix/suffix selection and excluded affixes already selected in another row.
- Filtered affixes using native compatibility checks against the exact base type, subtype, and original class requirement, including when class restrictions are disabled.
- Added validation for item/unique compatibility, affix slot type, duplicate affixes, and corrupted-affix support before dropping.
- Constructed corrupted affixes through the game's ItemAffix constructor and appended corruption in the order expected by native packing.
- Verified the requested corrupted affix before and after packing. Failed verification stops the drop and records the affix state in the log.
- Restored native metadata when constructing ordinary sealed affixes.
- Added quantity and roll controls, including random roll generation for repeated drops.

### Item names and custom items

- Refreshed the native full-name cache after Rune of Ascendance conversion.
- Preserved native initialization when overriding implicit and unique-mod rolls.
- Enabled the Temporalis runtime component; updated its unique ID, dynamic base subtype assignment, tooltip images, and modifier/tooltip entries.
- Updated Headhunter to allocate its base subtype dynamically and use the current tooltip image API.
- Hid the unfinished Headhunter menu button when the loaded HUD bundle has no corresponding page.
- Changed Headhunter icon loading to request an explicit Sprite, with an explicit Texture2D fallback. The bundle contains both types under the same PNG path.
- Changed Headhunter inventory/tooltip icon matching to use the unique ID instead of the displayed name; added null checks and more useful asset-error logging.

### Numeric inputs and crafting

- Added compact editable input boxes synchronized with HUD sliders.
- Styled the fields and used the native input-submit event.
- Enforced whole-number slider editing and synchronized fields using the displayed units.
- Restored percentage signs and percentage input conversion, including 0–100% editing for crafting-affix rolls backed by 0–255 values.
- Capped ordinary unsealed affix tier editors at T7.
- Preserved existing sealed primordial tier ranges rather than offering unsealed T8 affixes.

### Character and summons

- Restored Soul Embers to Character > Data, with an amount field and an adjacent Add Soul Embers button.
- Routed Soul Ember balance reads and additions through the dungeon manager instead of directly assigning the old player-data field.
- Restored summon God Mode, Forever, and Don't Collide controls and saved settings.
- Read the actual summon toggle states when settings change.
- Added restoration of tracked damageability and navigation-agent radii when God Mode or Don't Collide is disabled.
- Forever removes the lifetime component from affected summons. Disabling it does not recreate that component on existing summons.
- Revised Memory Amber pickup and multiplier hooks to act on the current player's pickup set, with overflow protection.
- Corrected the Memory Amber multiplier toggle binding and exposed a whole-number multiplier range of 1–10,000. This is a multiplier, not an Add 10,000 Amber action.

### Scenes, skills, and shortcuts

- Changed menu clicks to explicitly open the requested tab instead of toggling its visibility.
- Revised the Scenes panel layout and added a three-column Monolith timeline editor.
- Moved timeline selection, stability, corruption, and gaze editing into the dedicated Scenes controls.
- Added Copy to All for the selected timeline values.
- Instantiated the missing AutoCast runtime component.
- Corrected AutoCast instructions to hold the modifier and press the skill's normal keybind.
- Added guards for Safe Teleport map-wrapper errors, then disabled Safe Teleport to remove the Ctrl+Q skill-binding conflict.
- Revised skill-point multiplier arithmetic to keep native skill levels separate from multiplied point capacity.
- Revised gear-derived skill-point recalculation and native overinvestment/respec handling.
- Added an attempt to refresh the active skill tree when effective point capacity changes.
- Removed the unused effectiveRespec field and dead assignments responsible for warning CS0414.

### Stash, pickup, and map handling

- Debounced bursts of auto-store pickup requests into a single store pass.
- Combined key and woven-echo inventory scans and revised periodic auto-store scheduling.
- Kept the auto-store worker available to service queued requests when the periodic timer is disabled.
- Revised Quad Stash occupancy tracking, tab/grid handling, and configuration binding.
- Reconstructed occupancy from existing contents, rejected invalid placement coordinates, required empty tabs before resizing, and preserved quad status during renames.
- Reworked fog-of-war reveal distances using map dimensions and applied reveal handling during map initialization and updates.
- Included Mjolner-triggered abilities in Damage Meter ability tracking.

### Locales, settings, and build

- Added corrected English label aliases, including Strength, Mastery, Forging Potential, Legendary Potential, Wolves, and Affixes.
- Added English fallback for missing, invalid, or incomplete selected locales; prevented a failed load from retaining the previous language dictionary.
- Added Korean locale content and copied it into build output.
- Added ModUI SaveManager initialization and HUD binding.
- Defaulted the zone-level XP cap off when settings are regenerated or upgraded.
- Expanded optional operation profiling and deferred profiler attachment until ModUI settings initialization.
- Updated the version to 4.4.18, added the AI module assembly reference, and added detection of the local D: Steam game path.

### Validation status and remaining issues

- **Soul Embers:** implementation present; in-game addition still needs testing.
- **Headhunter icon:** reported broken before this patch; the explicit asset-type loading fix needs an in-game retest.
- **Live skill-tree refresh:** previously failed testing; do not describe it as confirmed fixed.
- **Skill-effect cosmetics:** no new cosmetics-loading fix in this comparison; affected users still need investigation.
- **Summon toggle restoration, AutoCast, timeline Copy to All, numeric bounds, Memory Amber, Quad Stash, auto-store, fog of war, and Damage Meter changes:** present in source, but not all have explicit successful gameplay confirmation.
- **Force Drop:** corruption's reported failure is fixed; broad item/affix coverage, sealed-plus-corrupted combinations, and save/reload behavior still need testing.
- **Primordial creation:** preserving existing sealed tier ranges does not establish support for creating every legal T8 combination.

### Already included in upstream 4.4.17

The following are inherited upstream changes, not new 4.4.18 fixes:

- Drop/Force Drop forging-potential fixes and Drop Legendary Potential fixes.
- Removal of minimap loot icons.
- French translation, Chinese translation completion, and build stamps.
- Clean-checkout build work, the existing test project, and related repository housekeeping.
