# Independent natural drop rate controls

Status: queued for in-game testing; full game-linked build still required.

## Player changes

- Added separate Unique, Set, Exalted Affix and T7 Affix rate controls under **Items → Drop → Natural Drop Rates**.
- Each has its own checkbox, whole-number slider and matching input box. All default off at 100%.
- 100% means the native rate, 50% means half, and 300% means 3x. Range: 0–1000%.
- Settings persist in SaveModUI.json. New UI labels are included in English, French, Korean and Chinese locale files.
- Explicit Force Unique/Set/Legendary modes take priority.

These are multipliers of native roll inputs, not final percentage guarantees or item-quantity multipliers. For example, entering 300% Unique does not mean every item is unique. Rarity outcomes compete, and native modifiers and caps still affect results.

Exalted scales both native rare-to-exalted conversion and exalted-affix chance. T7 has its own control. Forced or guaranteed rewards can still produce a category even if its random rate is set to zero. These controls do not change unique-specific rarity weights, boss-only loot tables, Legendary Potential, or Weaver’s Will rolls.

## Scope and implementation

- Wrapped the local generator’s synchronous SpawnItemAtPoint and spawnRandomItem calls. Nested entry calls do not multiply twice.
- Temporarily scale the native uniqueDropRate, lowLevelUniqueDropRate and setDropRate fields, plus the generator’s exalted conversion/chance and T7 fields. Restore original values in a finalizer, including on failure.
- Native item/affix selection and tier eligibility remain in use. No post-generation rarity relabeling or affix replacement.
- Direct shop, gambler, Nemesis and Morditas generation methods are not patched. Reward paths that use normal spawning may inherit these controls; category-forced rewards keep their native category.
- Gameplay is skipped while the mod HUD/pause screen is open, including its Force Drop operation.
- On the first affected spawn, one [DropRates] line prints configured factors and native baselines for validation.

Interop metadata confirms these fields and entry points, but does not provide native method bodies. Live testing must verify field consumption, any rarity-boundary caching, and reward paths that use separate generation methods. Other native threads reading shared rarity fields during a scoped override have not been evaluated; normal Unity item spawning is the intended path.

## Test checklist

1. Build against the installed game; verify four rows and slider/input synchronization, clamping, and persistence.
2. All controls off, then all at 100%: compare ordinary drops with the baseline build.
3. Enable only Unique, then only Set at 1000%; collect enough normal drops to see a distribution change. Keep other values at 100%.
4. Test Exalted at 1000% and T7 at 100%; then reverse them. Verify T6/T7 generation and item tooltips.
5. Test reductions such as 50% and 0% on ordinary random drops; exclude forced/guaranteed rewards from comparisons.
6. Switch off mid-zone and change zones. Check the original distribution returns and no values accumulate between drops.
7. Check vendors, gamblers, Force Drop, forced-rarity modes and boss/reward generation. Send the first [DropRates] line and any errors.
