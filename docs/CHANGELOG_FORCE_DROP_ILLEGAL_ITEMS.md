# Force Drop: optional illegal items

## Player changes

- Add an Allow illegal items toggle, off by default for a new game session.
- In illegal mode, allow unsealed standard affixes up to T8 where native tier data exists.
- Add an optional set-piece modifier selector on uniques, limited to source set pieces in the same equipment category.
- Use the saved set modifier to supply the source piece's set identity without changing the unique's id or consuming LP.
- Count distinct source pieces: two different ring pieces and the amulet piece can complete a three-piece set; two copies of the same source piece count once.
- Support mixed normal set gear and modified uniques. Membership continues after the creation toggle is turned off and after reload while this mod is installed.
- Filter set affixes from ordinary affix selectors. They use the dedicated illegal-mode selector instead.
- Include translations for the new controls and validation messages in English, French, Korean and Simplified Chinese.

## Developer changes

- Resolve set-affix source identity from Affix.uniqueId and UniqueList data. Reject missing or mismatched identities and more than one set-piece modifier per item.
- Supply native set recognition through grantsSetBonus, getSetItemUniqueId, cached SetId and equipped set-piece queries.
- Preserve the existing explicit remove-set-requirements feature.
- Retain valid unsealed T8 standard affixes in the repair hook, regardless of the creation toggle. Clamp tiers beyond native data.
- Validate constructor/packing results and preserve existing affixes, rarity, unique id, LP and Weaver's Will.

## Validation

C# syntax and native wrapper API checks passed. Full build and all gameplay checks remain pending.

Test: toggle off defaults; legal pool excludes set affixes; illegal T8 ordinary affixes survive pickup/save/reload; two different Invoked ring pieces on Red Rings plus the amulet piece on Omnis activate the full set; duplicate source piece counts once; mixed normal set gear works; removing a piece removes its bonus; turning off creation mode preserves equipped-item behavior; LP and native unique effects remain unchanged.

The sealed-plus-corruption fix is a separate branch. The gloves and native-name branches are also separate queued tests.
