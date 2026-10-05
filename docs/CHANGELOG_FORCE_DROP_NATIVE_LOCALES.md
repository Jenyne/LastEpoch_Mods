# Force Drop: native item localization

## Player changes

- Show the game's translated category, base-item, unique/set and affix names in Force Drop.
- Use native translated names for unique/set rarity labels, corruption and Unsated Rage's modifier choices.
- Search by the displayed translation or the original catalog/English name.
- Refresh names when the game language changes, without clearing selected items or affix ids.
- Keep the mod's Runes/Glyphs category labels and grouped category layout.
- Fall back to existing catalog names when native localization is unavailable.

## Developer changes

- Add NativeItemNames using Localization.Items.GetBaseTypeName, GetSubTypeName, GetUniqueName, GetRarityName and GetAffixDisplayName.
- Keep translated strings out of item creation: use catalog indices, base type, subtype and unique id for selections.
- Validate native-to-dropdown catalog alignment before enabling the id map; leave legacy dropdown strings untouched.
- Cache translated item choices and rebuild on catalog/language changes. Reopen an active picker after a language change to rebuild its names.
- Read translations from the game instead of hardcoding Korean item lists or adding item names to locale JSON.

## Validation

- C# syntax checks passed.
- Native localization method signatures checked against the supplied Il2CppLE.dll.
- Full build and in-game tests pending.

### In-game checks

1. Set the game's interface language to Korean; verify category names, base items, unique/set names, ordinary affixes, corruption and the ring modifier.
2. Search for an item in Korean and by its original English name; both should show the same item.
3. Select an item and change language; verify its selected state, category, subtype and unique remain correct.
4. Check items with identical displayed names still select the intended catalog entry.
5. Verify custom items without native translations remain usable with their existing names.
6. Verify an English-language session retains normal selection and dropping.

This branch is independently based on Sync's master. It does not include the queued Withstand the Elements branch. Integrate and retest both after their individual tests.
