# Force Drop catalog and rules audit

Base: Syncingoutt/master `2bc921dc` (v4.4.21).
Branch: `audit/force-drop-catalog`.
Scope: inspect existing behavior and capture live definitions. No creation-engine or legal/illegal behavior changes.

## Evidence levels

- **Code-confirmed:** observed directly in this revision's implementation.
- **API-confirmed:** members/signatures exist in the supplied generated game assembly metadata. This does not establish live values or native method behavior.
- **Live data required:** needs the catalog export from the running game.
- **Gameplay required:** needs actual construction, packing, equip/save/reload tests on a later implementation branch.

## Current pipeline

1. ForceDropBuilder selects through NativeItemNames and the old OdlForceDrop dropdowns.
2. Ordinary affix choices enumerate AffixList.singleAffixes and multiAffixes. FindAffix and MakeAffix use the same two collections.
3. AddAffixChoice excludes corruption definitions and recognized unique variants; checks prefix/suffix position, native CanRollOn, and duplicate IDs.
4. Validate repeats those checks, checks base/unique/subtype identity, variant selections and numeric input ranges.
5. Drop copies the request into old sliders/static fields; OdlForceDrop.Drop constructs ItemDataUnpacked and ordinary ItemAffix objects.
6. ApplySelectedCorruption also applies exclusive unique modifiers, then chosen corruption.
7. RefreshIDAndValues packs/refreshes, adapters verify selected special modifiers, then dropItemForPlayer emits the item.

Names are presentation; selection uses IDs. Preserve this separation throughout any rebuild.

## Affix classification map

The placement enum (PREFIX/SUFFIX/SPECIAL) is independent of SpecialAffixType. A PREFIX is not necessarily an ordinary legal affix.

| Native special type | Audit classification | Planning treatment |
| --- | --- | --- |
| Standard | Ordinary modifier | Check actual item restrictions, groups, tiers and application route. |
| Experimental | Experimental modifier | Keep separate; native outcome/type restrictions require live validation. |
| Personal | Personal modifier | Do not assume this means Champion without inspecting the outcome path. |
| Set | Set modifier | Track set identity/application route separately from prefix/suffix placement. |
| IdolEnchantment | Idol enchantment | Restrict legal application to the supported idol/crafting route. |
| IdolWeaver | Weaver idol modifier | Inspect native idol rules and definitions. |
| Corrupted | Corruption-exclusive | Distinguish the definition's category from the corruption outcome that adds it. |
| FakeUniqueMod | Unique special modifier | Inspect native unique pools; not an ordinary LP transfer by default. |
| Unknown/new value | Unclassified | Record it; Legal must not silently classify it as Standard. |

The names above are API-confirmed, not a complete legal-outcome whitelist. Champion appears as a corruption outcome in the metadata; its actual affix-category mapping remains unverified.

## Current legality matrix

| Concern | Current implementation | Missing evidence/rule |
| --- | --- | --- |
| Base/subtype | ValidSelectedItem checks both in native catalogs. | No claim that every catalog entry is obtainable; hidden/drop/crafting-only fields need inspection. |
| Unique identity | Checks ID/base/subtype and set-vs-unique selection. | Some capabilities remain identified by internal names. |
| Ordinary compatibility | FitsItem calls CanRollOn using the original subtype class requirement. | No route-specific legality proof for normal craft, LP transfer or corruption. |
| Affix category | Rejects Corrupted/FakeUniqueMod/recognized variant definitions; accepts matching PREFIX/SUFFIX. | No explicit complete special-type whitelist. Set/experimental/personal/idol eligibility needs a policy. |
| Duplicate modifiers | Rejects repeated affix IDs. | Group/conflict relationships are not checked. Distinct IDs may still conflict. |
| Definition coverage | Ordinary catalog uses single/multi; corruption and variant paths also use AllAffixes. | Need list-source coverage comparison and definition discrepancies. |
| Tiers | Screen ranges ordinary and corruption selectors T1–T7. Internal tier is UI tier minus one. | No per-definition supported-tier policy; T8/primordial route and global limits need live/gameplay proof. |
| Regular seal | Added first; Regular seal flag stored. | Need exact game-supported combinations, counts and tier restrictions. |
| Corruption seal | Native AddRandomSpecialAffix creates the slot; chosen affix replaces that slot. | Coexistence with a regular seal remains a gameplay blocker. |
| Corruption outcome | Fixed AddsCorruptedAffix outcome; only Corrupted definitions selectable. | Does not represent the full native outcome/configuration system. |
| LP/WW | Populated according to selected legendary type. | Complete legal relationship with transferred affixes, rarity and item capabilities is not validated. |
| Unique variants | Unsated Rage=1; Withstand the Elements=2; IDs from droppableLegendaryAffixes. | Generalize only after inspecting other pools; membership alone does not prove exclusivity. |
| Set effects | A Set affix may be constructed when accepted. | Set membership/bonus calculation and unique-name preservation are not established. |
| Final integrity | Corruption/variants verify IDs, roll metadata and preservation of existing special/item properties. | Ordinary/implicit/unique-roll/full-item verification is incomplete. |
| Persistence | Several individual features were tested by the user. | Packing checks alone do not prove equipment and save/reload stability for new combinations. |

## Code-confirmed creation risks

- Random corruption roll is sampled once for Apply and again for Verify; the expected byte can change. Resolve once per generated item in the future engine.
- Item creation is mediated by mutable hidden UI state. There is no immutable resolved request passed through construction and verification.
- Ordinary modifiers use object-initializer construction; corruption/variants use native constructors. Those paths have different metadata initialization and validation.
- Rarity is derived from ordinary affix count in the old builder rather than a shared item-rules policy.
- Quantity reporting delegates to a void Drop method; success counts and partial-batch failures are not reliably represented.
- Broad item-type numeric comparisons (`<100`, etc.) are used as capability checks in several paths.
- Reflection/catalog support may be unavailable while loading; "unavailable" must remain distinct from "no compatible affixes".

These observations are planning findings. This audit branch deliberately does not change them.

## Native corruption discovery

ItemList.corruptionOutcomeConfig exposes category and equipment-type configurations. Available fields include positive/negative outcomes, positive chance, forbidden combinations, weights, tier weights, replacement flags, normalized roll behavior and additional outcomes.

ItemData exposes GetCorruptableItemCategory, TryGetCorruptionConfig, CorruptionOutcomeCanApplyToItem and TryApplyCorruptionOutcome. The audit records their signatures and raw configs; it does not invoke them or infer that enum membership makes an outcome legal for every item.

The API contains outcomes for corrupted/standard/low-tier standard/champion/experimental/set additions, roll changes, subtype changes and other mutations. Exact legal pools and combinations depend on live configuration and native behavior.

## Planned engine boundary

- Shared catalog and item draft, independent of HUD controls.
- Separate placement, special type, acquisition/application route and seal origin.
- One rule evaluator shared by picker, preview and creation.
- Resolve every random choice once per item; keep exact values for final comparison.
- Legal applies verified route/type/tier/group/unique/primordial constraints.
- Illegal may relax gameplay restrictions, but still requires defined IDs and lossless representable packing. Unknown/unrepresentable data is not emitted.
- Construct, pack and verify complete item data before any ground drop.
- Return actual per-item results so batch counts and partial failures are accurate.

## Next evidence to collect

1. Export native definitions/configs with this branch.
2. Inspect restriction structures, tier ranges, affix groups and unique/set associations. The export records wrapper property names where the bounded reader cannot capture a structure fully.
3. Identify Champion's mapping and legal set-corruption paths for each relevant category.
4. Establish set membership and bonus APIs before promising unique items that function as set pieces.
5. Only then implement a creation engine and test sealed+corrupted, T8, LP/WW, variants and persistence in a separate branch.
