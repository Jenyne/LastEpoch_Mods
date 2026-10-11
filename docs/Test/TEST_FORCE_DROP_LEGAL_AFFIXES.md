# Force Drop legal affix coverage

Branch: `feat/force-drop`. Keep Illegal Mode off for this checklist.

The combined branch includes the legal filtering and LP fixes from `fix/force-drop-legal-affixes` and the Illegal Mode work. It retains the integrity fixes and earlier Unsated Rage results recorded in [TEST_FORCE_DROP_INTEGRITY.md](TEST_FORCE_DROP_INTEGRITY.md). This checklist covers legal behavior only; the full HUD redesign remains separate.

## Changes to test

- Search all items before choosing any category or rarity. Search checks native translated names, raw/internal names and aliases across base items, uniques and sets. Results show category/rarity; clicking one fills all three selectors by stable identity. Clearing the search returns to the selected category/rarity list. The native catalog supplies the names and IDs; no item is hardcoded.

- All ordinary affix and idol enchantment pickers now use the existing wheel/drag scrolling system rather than Previous/Next pages. Legal prefix-only and suffix-only slots use one full-width list; enchantment, sealed and corruption pools retain separate Prefix/Suffix lists. None stays pinned above each list. Search resets scroll positions and only visible rows are pooled. Legal eligibility, duplicate exclusions and family colors are unchanged.
- Selecting an ordinary affix for a unique clears LP to zero and disables its input and Fixed/Random button. Maximum/Random presets cannot restore LP while the affix remains selected. Clear every ordinary affix to re-enable LP; fixed native unique modifiers alone do not disable it. Creation requests also enforce zero LP on the resulting Legendary item.
- Normal affix selectors use the complete deduplicated native catalog and shared eligibility rules. Their log lines include choice counts and exclusion counts by reason.
- Unique equipment uses a matching normal/exalted donor's equipment type and original class restrictions, rather than requiring affixes to roll on the unique-only subtype. All selected transferred affixes must coexist on one donor subtype. Selecting a Mage modifier must not enable an incompatible Rogue modifier.
- Set shards use their referenced Set piece's equipment type and native class metadata. A matching base can show the Set affix without having the Set unique's exact subtype. Only one Set affix is allowed. Reforged Set affixes are excluded from Legendary transfers.
- Experimental affixes remain available on compatible native item types; a second Experimental affix is excluded. Champion membership comes from `ChampionDataList`, rather than treating every Personal affix as a Champion affix. Campaign Personal affixes are excluded from Legendary transfers.
- Corruption choices include native-permitted Standard, Experimental, Champion, Set and corruption-exclusive definitions, rather than only the last family. The selected item's category, equipment outcomes, positive weights/chances, native eligibility and supported tier weights decide which families appear. Duplicate selections and conflicting Set/Experimental selections are excluded.
- Set choices are green and corruption-exclusive choices are purple, including the selected affix captions. Affix and corruption pickers group choices by native family, then alphabetically within each family. `None` comes first and stays available during searches. Other special choices retain family labels. Names still come from the game's locale, with translated labels supplied in EN/FR/KO/ZH.
- Corruption tiers respect definition lengths, holes in outcome weights and native tier checks. Rune level thresholds for forgeable equipment are T5 below level requirement 35, T6 below 55 and T7 otherwise. These are not the 50/80 area thresholds for pre-corrupted drops. The preview seed uses actual native base/affix level requirements rather than forcing zero.
- Ordinary idols use Prefix 1 and Suffix 1 at T1. Weaver subtypes can use the native Weaver pool and must retain at least one Weaver affix. Heretical subtypes use the two other rows for separate enchantments at their supported tiers. Regular seals are unavailable on idols and unique equipment in this legal path.
- Fixed unique pools are detected from native metadata. Frostborn Solitude now uses the existing modifier selector alongside Unsated Rage and Withstand the Elements.
- Real drops re-evaluate the same eligibility rules and retain the complete packed-ID verification before spawning. The corruption picker uses a cached, deterministic seed built by the same constructor; it does not sample the user's random rolls or spawn anything.

## Install the test build

Close Last Epoch. Fetch and run `scripts/Test-LastEpochBranches.ps1` from `chore/test-queue-runner`, then choose **1**. Selection 1 is the single combined legal/illegal build; selection 2 is retired. The older dedicated legal script targets its historical branch.

The queue runner updates the selected branch, builds Release, runs the full suite against the just-built DLL and your supplied game assemblies, backs up the installed mod and copies the new DLL only after success. Tracked local changes or a branch that differs from the remote stop installation.

## In-game checks

| Selection | Expected result |
| --- | --- |
| Search `seed` with category/rarity unset or set to a different category | Matching native items appear across categories; choosing the desired helmet fills helmet category, Unique rarity and the exact unique. Verify preview, affix pool, LP and actual drop identity. |
| Search partial/case-insensitive names, translated names and aliases; `seed helmet`; unmatched query; clear search | Every query word matches name/category/rarity/aliases. Empty results show No matching items and do not alter selection. Clearing search shows the selected category/rarity list. Switch locale and reopen the menu; labels/search refresh. |
| Same-name items, base items and set items; choose a new result after customizing another item | Stable IDs select the right item, never a name collision. Dependent catalogs refill and old affix/unique-modifier/corruption choices clear, matching ordinary item selection. Check paged results and both legal/illegal modes. |
| Legal prefix and suffix slots; mouse wheel, drag and scrollbar | One full-width list containing only that slot's eligible family; reach the last entry and select it. No Previous/Next controls. Smooth drag movement, clipped rows and pinned None. |
| Legal enchantment, sealed and corruption pickers; search while scrolled, empty results, clear search | Separate Prefix/Suffix lists scroll independently; search resets both positions. None remains available, including zero results. Switching from a full-width picker restores both columns correctly. |
| Unique with all ordinary/sealed affixes None and no corruption affix | With Corrupted: No, LP input and Fixed/Random are editable; dedicated unique modifiers alone keep them editable. With Corrupted: Yes, LP remains disabled/zero even when every affix is None: native corruption removes LP independently of transferred affixes. Turn corruption off and confirm LP becomes editable again. |
| Ordinary helmet/body/relic with an original class requirement | Correct class-compatible ordinary affixes; restriction cheats must not change the legal pool. |
| Generic unique helmet/body/relic | Applicable class-specific transferred affixes are visible; incompatible classes cannot be combined. Choose the class modifier first, then inspect the second prefix. |
| Unique with LP 4; select one through four ordinary affixes | LP immediately becomes zero; its value and Fixed/Random button are disabled. Maximum/Random leave it disabled. Drop a Legendary with all selected affixes, zero LP and the original unique identity; check save/reload. |
| Clear every ordinary affix, then select only an Unsated Rage or Withstand native modifier | LP becomes editable again and can be set to 4. Native fixed modifiers alone preserve selected LP; check creation and save/reload. |
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
| Open an ordinary affix picker, then the corruption picker; search and clear a selection | `None` is first, including filtered searches. Set entries stay together in green; corruption-exclusive entries stay together in purple. Selecting either retains its color in the affix row; clearing restores gold. Within each family, names are alphabetical. |
| FR → EN → KO, change item/category and reopen pickers | Correct translated names/labels and refreshed pool; native family grouping and `None` ordering survive language changes; no green/purple formatting carried over to ordinary/category choices. |

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
- Illegal Mode, illegal T8 ordinary affixes and regular sealed affixes on uniques remain outside this legal-mode checklist; see [TEST_FORCE_DROP_ILLEGAL_MODE.md](TEST_FORCE_DROP_ILLEGAL_MODE.md).

## Earlier legal-branch verification

- 123 game-independent core regression cases passed, including the five base-item socket regressions, outcome tier holes, actual definition bounds, Rune level thresholds, Set versus Legendary routes, Champion versus Personal routes and ordinary/Weaver/Heretical idol slots.
- 13 managed adapter fixtures passed for donor subtype/class intersections, the distinction between the two class enums, matching-type Set shards, idol routes and metadata-based fixed pools. These simulate native responses; they are not game execution.
- The new core compiles against .NET 6 references. Changed HUD/adapters compile against the supplied Unity/TMP/Harmony assemblies with game context stubs.
- JSON parsing/key parity, formatting and diff checks pass. No Harmony patch was added or removed.

The supplied `Il2CppLE.dll` has unreadable metadata in this environment, so a full game SDK build cannot be repeated here. The user's Windows builds have reached in-game startup at `0805d229` and `33fedcf1`. Remaining native creation/effect/persistence checks require the guarded Windows script and in-game tests above.

## Current legal scrolling follow-up

The selector presentation change does not change eligibility, LP rules or item packing. Formatting and diff checks pass. The game-independent suite reports 1,156 passed and six SDK-dependent skips. The current workspace has no game SDK for a native mod build; scrolling, full-width/split layout transitions and the LP state checks above still require the guarded Windows build and in-game confirmation.

## Rule references

- [EHG: Corrupted Items](https://support.lastepoch.com/hc/en-us/articles/52977667498267-Corrupted-Items): corruption removes LP independently of added affixes.
- [EHG: Legendary Items](https://support.lastepoch.com/hc/en-us/articles/46361924310555-Legendary-Items)
- [EHG: Set crafting and Legendary restrictions](https://forum.lastepoch.com/t/endgame-balance-and-itemization-updates-coming-to-last-epoch-april-17th/75189/1)
- [EHG: matching-type Set shards and sealing](https://forum.lastepoch.com/t/last-epoch-tombs-of-the-erased-patch-notes/75247/2)
- [EHG: Weaver and Heretical idol affix layouts](https://forum.lastepoch.com/t/the-woven-faction-monolith-updates-coming-to-last-epoch-april-17th/75147/1)
- [EHG: 1.5 corruption tier restrictions](https://lastepoch.com/patchnotes/)
- Runtime catalog `ForceDropCatalog_20261007_063145.json`, provided by the user: 1,156 definitions and native category/type corruption outcomes.
