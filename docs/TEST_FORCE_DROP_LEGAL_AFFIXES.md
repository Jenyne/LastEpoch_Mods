# Force Drop legal affix coverage

Branch: `fix/force-drop-legal-affixes`.

This branch contains the integrity fixes from `fix/force-drop-integrity`, including the Unsated Rage creation and persistence results recorded in [TEST_FORCE_DROP_INTEGRITY.md](TEST_FORCE_DROP_INTEGRITY.md). It changes filtering and the existing selector's slot labels; it does not introduce Illegal Mode or rebuild the HUD. Upstream was checked at `92fb33de`; its change since the runtime code baseline is README-only.

## Changes to test

- Normal affix selectors use the complete deduplicated native catalog and shared eligibility rules. Their log lines include choice counts and exclusion counts by reason.
- Unique equipment uses a matching normal/exalted donor's equipment type and original class restrictions, rather than requiring affixes to roll on the unique-only subtype. All selected transferred affixes must coexist on one donor subtype. Selecting a Mage modifier must not enable an incompatible Rogue modifier.
- Set shards use their referenced Set piece's equipment type and native class metadata. A matching base can show the Set affix without having the Set unique's exact subtype. Only one Set affix is allowed. Reforged Set affixes are excluded from Legendary transfers.
- Experimental affixes remain available on compatible native item types; a second Experimental affix is excluded. Champion membership comes from `ChampionDataList`, rather than treating every Personal affix as a Champion affix. Campaign Personal affixes are excluded from Legendary transfers.
- Corruption choices include native-permitted Standard, Experimental, Champion, Set and corruption-exclusive definitions, rather than only the last family. The selected item's category, equipment outcomes, positive weights/chances, native eligibility and supported tier weights decide which families appear. Duplicate selections and conflicting Set/Experimental selections are excluded.
- Corruption-exclusive choices are purple. Other special choices have family labels. Names still come from the game's locale, with translated labels supplied in EN/FR/KO/ZH.
- Corruption tiers respect definition lengths, holes in outcome weights and native tier checks. Rune level thresholds for forgeable equipment are T5 below level requirement 35, T6 below 55 and T7 otherwise. These are not the 50/80 area thresholds for pre-corrupted drops. The preview seed uses actual native base/affix level requirements rather than forcing zero.
- Ordinary idols use Prefix 1 and Suffix 1 at T1. Weaver subtypes can use the native Weaver pool and must retain at least one Weaver affix. Heretical subtypes use the two other rows for separate enchantments at their supported tiers. Regular seals are unavailable on idols and unique equipment in this legal path.
- Fixed unique pools are detected from native metadata. Frostborn Solitude now uses the existing modifier selector alongside Unsated Rage and Withstand the Elements.
- Real drops re-evaluate the same eligibility rules and retain the complete packed-ID verification before spawning. The corruption picker uses a cached, deterministic seed built by the same constructor; it does not sample the user's random rolls or spawn anything.

## Install the test build

Close Last Epoch. Run from your checkout after fetching/switching to this branch:

```powershell
.\scripts\Test-ForceDropLegalAffixes.ps1 -GamePath "D:\SteamLibrary\steamapps\common\Last Epoch"
```

The script updates this branch, builds Release, runs the full suite against the just-built DLL and your supplied game assemblies, backs up the installed mod and copies the new DLL only after success. Tracked local changes or a branch that differs from the remote stop installation.

## In-game checks

| Selection | Expected result |
| --- | --- |
| Ordinary helmet/body/relic with an original class requirement | Correct class-compatible ordinary affixes; restriction cheats must not change the legal pool. |
| Generic unique helmet/body/relic | Applicable class-specific transferred affixes are visible; incompatible classes cannot be combined. Choose the class modifier first, then inspect the second prefix. |
| Base ring, staff and body armor; inspect Set choices | Matching Set piece's affix is available; wrong equipment type is absent. A second Set affix is excluded. Check actual Set bonus/name and save/reload after dropping. |
| Unique ring/body armor; inspect normal and corruption choices | No normal Set transfer; the corruption row follows that item's native outcomes rather than borrowing the base-item outcome list. |
| Base gloves/boots/belt | Compatible Experimental choices; after selecting one, a second is excluded in both ordinary and corruption rows. |
| Forgeable base equipment; inspect corruption | Standard, compatible Champion/Experimental/Set and corruption-exclusive choices appear where the native outcome permits. Try a corruption from each available family. |
| Low-level base equipment, then add a high-level ordinary affix | Corruption tier limit reflects the actual resulting level requirement. A one-tier definition remains T1, even under Maximum. |
| Base ring, helmet or amulet with four ordinary affixes + regular seal + corruption | Both seals retain their own modifier. This legal base-item combination still needs runtime confirmation. |
| Unsated Rage with its modifier + zero through four ordinary affixes + corruption | Earlier confirmed creation/persistence remains working under the new filters. |
| Withstand the Elements with two modifiers + ordinary affixes + corruption | Correct independent fixed modifiers and selected corruption; verify effects and save/reload. |
| Small/Minor/Humble/Stout Weaver idol | Weaver choices appear only on Weaver subtypes; one prefix and one suffix, at least one Weaver affix, all T1. |
| Heretical class idol | Ordinary prefix/suffix stay T1; Enchantment 1/2 select distinct native enchantments through T7. Regular class idols must not offer those extra enchantment slots. |
| Frostborn Solitude | Correct native fixed modifier, unique identity, tooltip/effects and persistence. |
| Idol Altar | Native-compatible ordinary/corruption affixes remain visible; verify creation, grid effects and persistence. |
| FR → EN → KO, change item/category and reopen pickers | Correct translated names/labels and refreshed pool; no purple formatting carried over to ordinary/category choices. |

Do not certify a family from its list alone. Check creation, tooltip/equip effects, native item identity and save/reload for the selected combinations. Send `Latest.log` with the exact item, affix names, slot and tier if a choice is missing or rejected. Exclusion summaries help distinguish wrong type/class from a missing native route.

## Corrupted base-item socket regression

The user's `0805d229` test log demonstrates two false packing rejections: a Heretical idol with two ordinary affixes, two enchantments and corruption, and a base ring with four ordinary affixes (including a Set affix), a regular seal and corruption. The live socket counts were 4/5 for 5/6 affixes respectively. Both decoded items reported zero sockets and retained every affix ID, tier, roll, family, placement and seal, along with the remaining item fields.

The initial correction accepted these socket representations only when the item was corrupted and had a corruption seal. The follow-up `33fedcf1` log demonstrates zero decoded sockets on three more base rings: corrupted with a regular seal but no corruption affix; uncorrupted with a regular seal; and uncorrupted without a seal. All three contain a Set affix, and every saved affix and item field matches the live item.

Validation now distinguishes the live item from the decoded item. Base-item live counts remain strict, allowing the observed native corruption insertion count; decoded zero sockets are accepted independently of rarity, Set membership or corruption. The complete saved affix list and every other field still have to match, and unrelated nonzero socket changes remain rejected. No construction, filtering or HUD behavior changed in this correction.

The user confirmed creation and supplied a tooltip for a base ring with a Set affix, Champion affix, two suffixes, regular seal and corruption. All six modifiers and the Set name/bonus text are visible. Equipped effects, Set bonus activation and save/reload remain unconfirmed. The Heretical idol combination still needs successful creation confirmation.

Retest the uncorrupted Set ring from the latest screenshot first, then the same item with a regular seal, and with corruption toggled on but no corruption affix. Inspect tooltip/effects and save/reload. Regression tests include all five exact rejected snapshots, checks for changed/missing affixes and lost seals, and rejection of invalid live base-item counts.

## Known boundaries

- This is an additive-affix selector. Replacement outcomes (such as Oculus of Ruin's replacement routes), subtype transformations and compound Set conversion/corruption outcomes are not reproduced by selecting a single extra affix. They remain separate work; this branch does not advertise those outcomes as supported.
- Exulis has tiered Corrupted modifiers in its own explicit pool, not fixed `FakeUniqueMod` modifiers. Its dedicated two-modifier path is not added here. Those definitions are not globally hidden from other items' legal corruption pools.
- Weaver/Heretical subtype classification currently uses the native internal subtype name from the supplied catalog, independently of translated display names. Native `CanRollOn` still checks the actual subtype. New or renamed native subtypes will need classification review.
- Personal items can have special drop-source/predetermined-modifier rules beyond `CanRollOn`. Base Personal choices retain native compatibility checks; this pass does not certify arbitrary campaign Personal item combinations.
- Illegal Mode, primordial storage, illegal T8 ordinary affixes and regular sealed affixes on uniques remain outside this change.

## Verification performed here

- 123 game-independent core regression cases passed, including the five base-item socket regressions, outcome tier holes, actual definition bounds, Rune level thresholds, Set versus Legendary routes, Champion versus Personal routes and ordinary/Weaver/Heretical idol slots.
- 13 managed adapter fixtures passed for donor subtype/class intersections, the distinction between the two class enums, matching-type Set shards, idol routes and metadata-based fixed pools. These simulate native responses; they are not game execution.
- The new core compiles against .NET 6 references. Changed HUD/adapters compile against the supplied Unity/TMP/Harmony assemblies with game context stubs.
- JSON parsing/key parity, formatting and diff checks pass. No Harmony patch was added or removed.

The supplied `Il2CppLE.dll` has unreadable metadata in this environment, so a full game SDK build cannot be repeated here. The user's Windows builds have reached in-game startup at `0805d229` and `33fedcf1`. Remaining native creation/effect/persistence checks require the guarded Windows script and in-game tests above.

## Rule references

- [EHG: Legendary Items](https://support.lastepoch.com/hc/en-us/articles/46361924310555-Legendary-Items)
- [EHG: Set crafting and Legendary restrictions](https://forum.lastepoch.com/t/endgame-balance-and-itemization-updates-coming-to-last-epoch-april-17th/75189/1)
- [EHG: matching-type Set shards and sealing](https://forum.lastepoch.com/t/last-epoch-tombs-of-the-erased-patch-notes/75247/2)
- [EHG: Weaver and Heretical idol affix layouts](https://forum.lastepoch.com/t/the-woven-faction-monolith-updates-coming-to-last-epoch-april-17th/75147/1)
- [EHG: 1.5 corruption tier restrictions](https://lastepoch.com/patchnotes/)
- Runtime catalog `ForceDropCatalog_20261007_063145.json`, provided by the user: 1,156 definitions and native category/type corruption outcomes.
