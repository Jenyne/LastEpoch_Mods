# Force Drop corruption packing

## Player changes

- Use the game's special-affix operation to create the corruption slot before applying the selected corruption, tier and roll.
- Keep the native sealed-affix order and packing flags instead of appending corruption after ordinary affixes.
- Keep invalid items from dropping: check the selected corruption again after the final item refresh.

## Developer changes

- Replace manual affix insertion and socket-count assignment with ItemData.AddRandomSpecialAffix, using the live corruption catalog.
- Replace only the native-created corruption slot. Keep the existing base-type/subtype/class compatibility checks.
- Verify that regular, sealed and primordial affix ids, tiers, rolls and seal types survive packing unchanged. Also check unique id, legendary potential and Weaver's Will.
- Check corruption id, seal, special-affix type, tier and roll after packing.

## Validation

- Wrapper metadata checked against the supplied Il2CppLE.dll and Il2Cppmscorlib.dll.
- C# syntax checks performed. A full build and in-game validation are still required.
- Retest Death Mask with one ordinary affix plus corruption 1020; then four ordinary affixes, regular sealed affix, and unique items with LP.
- Verify the selected corruption and all original affixes after pickup and save/reload.
