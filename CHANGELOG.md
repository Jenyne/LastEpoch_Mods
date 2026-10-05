# Changelog

## 4.4.18 — custom branch

Changes from [Syncingoutt v4.4.17](https://github.com/Syncingoutt/LastEpoch_Mods/releases/tag/v4.4.17) to [Jenyne v4.4.18](https://github.com/Jenyne/LastEpoch_Mods/tree/v4.4.18), including retained custom changes. Release publication is tracked separately.

## For players

### Confirmed fixes

- **Rune of Ascendance:** items now display the correct unique name after conversion.
- **Corrupted Force Drop:** fixed the failure that prevented the tested corrupted item from dropping.
- **Menu yellow screen:** fixed the reported issue requiring a second click to open a tab.
- **Temporalis:** restored and confirmed working.
- **Headhunter:** fixed the white/missing inventory and tooltip icon.
- **Ctrl+Q conflict:** disabled Safe Teleport so it no longer conflicts with skill bindings.

### New and improved controls

- **Force Drop redesign:** compact text-only editor, searchable items and affixes, clearer item/rarity selection, and highlighted selected items.
- **Item categories:** Weapons, Armour, Accessories, Idols, and Other are grouped together without the old extra category page.
- **Corruption selection:** choose a corrupted affix alongside the ordinary affix controls.
- **Affix filtering:** hide already-selected affixes and reject incompatible or duplicate selections before dropping.
- **Drop controls:** compact implicit, affix, unique-roll, Legendary Potential, Weaver's Will, quantity, and random-roll controls.
- **Clearer names:** Runes, Glyphs, and Lens labels; Blessings removed from Force Drop.
- **Numeric inputs:** type a value beside a slider; the field and slider stay synchronized. Values use whole numbers.
- **Crafting percentages:** restored % labels and 0–100% editing.
- **Affix tiers:** ordinary unsealed editors stop at T7; existing sealed primordial tier ranges are preserved.
- **Soul Embers:** amount field and Add button restored to Character > Data. Addition still needs an in-game test.
- **Summons:** restored God Mode, Forever, and Don't Collide controls.
- **Memory Amber:** corrected multiplier control, with values from 1 to 10,000.
- **Scenes / Monoliths:** revised layout, timeline editor, and Copy to All for timeline values.
- **AutoCast:** restored its missing runtime component; instructions now say to hold the modifier and press the skill's normal keybind.
- **Languages:** corrected English labels, English fallback for unavailable translations, and Korean locale content.

### Map, stash, and other changes

These changes are implemented; gameplay confirmation remains pending where noted below.

- **Fog of war:** revised the reveal-map option so reveal coverage follows the map's size and is reapplied as the map loads and updates.
- **Auto-store:** batches rapid material pickups and improves key/woven-echo storage and timer handling.
- **Quad Stash:** improved grid and occupied-slot handling, preserves quad status when renaming, and requires an empty tab before changing its size.
- **Damage Meter:** includes Mjolner-triggered abilities.
- **Skill points:** revised multiplier and gear-bonus handling. Automatic refresh of an already-open skill tree remains unconfirmed.
- **Settings:** restored ModUI settings initialization; the zone-level XP cap defaults off when settings are regenerated or upgraded.

### Things to know

- **Summon Forever:** turning it off does not restore the lifetime timer on summons already made permanent.
- **Memory Amber 10,000:** this is a multiplier setting, not a button that adds 10,000 Amber.
- **Primordial tiers:** preserving existing sealed tiers does not mean every legal T8 item can be created.
- **Soul Embers:** still awaiting a successful addition test.
- **Skill-tree refresh:** previously failed testing; it is not listed as a confirmed fix.
- **Skill-effect cosmetics:** this build contains no new fix for the reported loading issue.
- Fog of war, summon toggle restoration, AutoCast, Copy to All, numeric bounds, Memory Amber, Quad Stash, auto-store, and Damage Meter still need broader gameplay verification.
- Successful corrupted-drop testing does not cover every item, sealed-plus-corrupted combination, or save/reload scenario.

## For developers

### Force Drop and item handling

- Added a runtime three-column builder with searchable/paginated catalogs and integration with the existing drop backend.
- Built the corrupted-affix catalog from game data and separated special corruption classification from ordinary prefix/suffix selection.
- Used native compatibility checks for exact base type, subtype, and original class requirement, including when class restrictions are disabled.
- Added item/unique compatibility, slot-type, duplicate-affix, and corruption-support validation.
- Constructed corruption through the native ItemAffix constructor and appended it in the serialization order expected by the current game.
- Verified the requested corruption before and after packing; failed verification aborts the drop and logs the packed state.
- Restored native metadata for ordinary sealed affixes.
- Refreshed the cached full name after ChangeToUniqueOfSameItemType.
- Preserved native initialization for implicit and unique-roll overrides.
- Generated random rolls independently for repeated drops.

### Custom item rendering

- Enabled the Temporalis runtime component; revised its unique ID, dynamic subtype assignment, modifiers, tooltip entries, and current tooltip image hooks.
- Allocated Headhunter subtypes dynamically and hid its menu button when the loaded bundle lacks the corresponding page.
- Loaded Headhunter's Texture2D explicitly and created a runtime sprite, with an explicitly typed imported-Sprite fallback. The PNG path contains both asset types.
- Matched Headhunter by unique ID rather than localized display name.
- Refreshed only bound Headhunter inventory/tooltip images in LateUpdate, including comparison images; cleared masking override sprites and stopped tracking reused views.
- Added null guards, asset-error detail, and icon dimensions to diagnostics.

### UI, locales, and settings

- Added synchronized numeric inputs using native submit events and display-unit conversion.
- Applied whole-number editing, 0–100% display conversion for 0–255 roll storage, ordinary T7 caps, and preserved sealed-tier ranges.
- Changed tab actions to explicitly activate the requested content rather than toggle visibility.
- Added the three-column Monolith timeline editor and timeline-copy action.
- Routed Soul Ember balance/addition through DungeonRunManager.
- Added persistent summon settings, actual-toggle-state callbacks, and restoration of tracked damageability/NavMeshAgent radii.
- Forever removes UnsummonAfterDelay from affected summons.
- Instantiated Skills_AutoCast and corrected its UI instructions.
- Added Safe Teleport map-wrapper guards before disabling the conflicting shortcut.
- Added canonical English label aliases, per-key English fallback, invalid-locale handling, and fresh dictionary loading.
- Added Korean locale output, ModUI SaveManager bootstrap/HUD binding, and an off default for the zone-level cap during schema rewrites.

### Gameplay state and performance

- Kept native skill levels separate from multiplied point capacity; revised gear-derived additional-point recalculation and native respec/overinvestment handling.
- Attempted to refresh the active skill tree when effective capacity changes; successful live refresh remains unverified.
- Changed Memory Amber hooks to operate on the current player's pickup set with overflow protection.
- Debounced auto-store pickup requests by 0.20 seconds, combined key/echo scans, and used realtime timer scheduling.
- Kept the auto-store worker active to service queued requests even with the periodic timer disabled.
- Revised Quad Stash occupancy caching and reconstructed occupancy from existing contents; rejected invalid placement coordinates and prevented resizing populated tabs.
- Sized fog-of-war reveal distances from map data rather than int.MaxValue, and reapplied reveal handling during initialization and map updates.
- Exposed Mjolner-triggered abilities to Damage Meter.
- Added optional operation timing/allocation profiling and deferred attachment until settings initialization.

### Build and maintenance

- Updated the version to 4.4.18.
- Added the UnityEngine.AIModule reference.
- Added conditional local D: Steam game-path detection; an explicit LastEpochPath takes precedence.
- Removed the unused effectiveRespec field and dead assignments causing CS0414.
- C# syntax and relevant game-wrapper metadata were checked during implementation. The development environment did not run a fresh .NET build or the full repository test suite; gameplay confirmations came from maintainer testing.
- Comparison baseline: upstream tag commit `b2399c7c521c02a20b83531a640e2f56e3708212`.

## Inherited from upstream v4.4.17

These are already in the baseline and are not new custom-build fixes:

- Drop/Force Drop forging-potential fixes and Drop Legendary Potential fixes.
- Removal of minimap loot icons.
- French translation, Chinese translation completion, and build stamps.
- Clean-checkout build changes, existing tests, and repository housekeeping.
