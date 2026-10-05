# Force Drop corruption pools — pending in-game testing

## Player changes
- Legal mode offers corruption-exclusive, Standard, Champion, Experimental and Set affixes when the running game's corruption configuration permits that outcome for the selected item.
- Illegal mode exposes the full live affix catalog in the corruption selector, bypassing normal item-category and tier restrictions, with tiers up to T8.
- Corruption-exclusive entries and the selected entry appear purple. All entries show a category label.
- Affixes already selected elsewhere are excluded from the corruption picker.
- Changing Illegal mode clears the selected corruption so selections cannot carry over across modes.
- Includes the earlier Illegal-mode normal T8 / set-piece controls and the fix allowing an ordinary sealed affix alongside a corruption seal.
- Category labels supplied for English, French, Korean and Simplified Chinese.

## Developer changes
- Read AllAffixes and native Champion metadata; no hardcoded affix IDs.
- Legal filtering uses category/type corruption configurations, positive outcome weights, native outcome eligibility, native item compatibility and configured tier weights. Categories marked cannotCombineWithType omit equipment-type outcomes.
- Native AddRandomSpecialAffix reserves the corruption slot with the selected legal outcome and special-affix category. Illegal selections use a native corruption-exclusive slot before replacing its definition.
- Preserve existing affix signatures, regular/primordial seal flags, unique identity, Legendary Potential and Weaver's Will.
- Decode the packed item ID and verify the chosen corruption, prior affixes and item properties before dropping. Any mismatch aborts the drop.
- Cache the immutable HUD preview item and its eligibility result to avoid repeated native allocation every frame.

## Validation and pending tests
- C# syntax and referenced APIs checked against the supplied Il2CppLE.dll; no complete build or gameplay validation in this environment.
- Build against the current game assemblies.
- Test Standard, Champion, Set and exclusive corruptions on applicable bases/rarities; verify legal type and tier filtering.
- Test an ordinary sealed affix plus corruption, including save/reload.
- Test Illegal-mode T8 and cross-category selections, including save/reload and tooltips.
- Verify unique identity, LP/WW and all original affixes remain unchanged.
- Verify purple picker rows reset when opening other selectors and category labels display in each supplied locale.
- Illegal choices still depend on the game being able to reserve and serialize a native corruption slot; otherwise no item is dropped.
