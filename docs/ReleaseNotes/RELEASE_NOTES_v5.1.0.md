# LastEpoch HUD v5.1.0

Updated build of Ash's HUD for Last Epoch Season 5 (Unity 6000.4.8) and MelonLoader 0.7.3 Open-Beta. Press F3 in a zone to open the mod menu.

## Changelog

### Features

- Added Mageblood and flasks, a new custom unique belt modelled on Path of Exile's Mageblood (F3 > Items > Custom Items)
- Force Drop Search receives focus and printable typing.
- Added two new buttons: Cast When Out Of Mana: once mana is short, casts are free. || Allow Negative Mana: casts still cost mana, and mana goes below 0. (F3 > Utilities > Character > Cheats) (Note: This is still a bit janky, and we'll overall look into reworking how mana features work)
- Added Always Spawn Nemesis / Blood Ritual / Lost Cache / Rift Beast / Omen Window (F3 > World > Monoliths)
- Rune Prisons Per Echo slider 0-10 (F3 > World > Monoliths)
- Favour Caches Per Echo slider 0-10 (F3 > World > Monoliths)

### Fixes

- Force Drop now separates regular sealed and T8 Primordial slots, preserving coexistence with regular and corrupted affixes.
- Idol-unlock state/control fixes
- Added HUD startup loading guards and diagnostics
- Disabled malformed rarity-only Force Legendary drops
- Reworked auto-loot (auto-pickup, auto-sell and auto-shatter on drop) as some items were getting deleted
- Removed BossEngaged probe hook that crashes boss fights (let us know if moroditas etc.. still crashes your game, during testing everything was fine)
- Fixed Crafting FP
- Search now accounts for locales
- Localization fixes
- Rewrote the way cosmetics are unlocked

## Install

- Install steps are in the repository [README](https://github.com/Syncingoutt/LastEpoch_Mods/blob/master/README.md).
- Alternatively, use the [LE Hud Installer](https://github.com/TriSSec-Lab/LE-Hud-Installer) to install the mod automatically.
