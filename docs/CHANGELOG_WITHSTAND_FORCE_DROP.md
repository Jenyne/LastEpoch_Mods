# Force Drop: Withstand the Elements

## Player changes

- Add two exclusive modifier selectors when Withstand the Elements is selected.
- Each selector uses the selected gloves' native fixed affix pool and excludes the modifier selected in the other slot.
- Require two distinct modifiers before dropping the gloves. Keep these modifiers separate from regular/sealed affixes and Legendary Potential.
- Keep Unsated Rage's existing single modifier selector.
- Include the new controls and validation messages in English, French, Korean and Simplified Chinese locale files.

## Developer changes

- Generalize the unique variant adapter to one modifier for Unsated Rage and two for Withstand the Elements.
- Construct and validate all selected variants before adding them, then pack them together.
- Verify variant id, tier, roll, special type, seal and count after packing and after the final Force Drop refresh.
- Verify that original affixes, rarity, unique id, LP and Weaver's Will remain unchanged.
- Exclude the gloves' native variant pool from ordinary affix selection.

## Validation

- C# syntax and locale JSON/coverage checks passed.
- Uses existing wrapper fields and constructors already used by the tested Unsated Rage implementation.
- Full build and in-game validation pending: two distinct glove modifiers, 4 LP, ordinary affixes, corruption, save/reload persistence and Unsated Rage regression.
