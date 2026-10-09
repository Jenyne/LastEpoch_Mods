# Force Drop Illegal Mode and Primordial sealed tiers

Branch: `feat/force-drop`, combining `feat/force-drop-illegal-mode` and `fix/force-drop-legal-affixes` (including the final LP correction). Previous testing branches remain available.

The current Force Drop layout stays in use. Its new themed **Illegal mode** checkbox starts off. Changing mode clears ordinary/sealed/corruption selections so an illegal selection cannot leak into Legal Mode. Item identity and the exclusive unique modifier controls remain separate.

## Changes

- Illegal affix pickers split Prefix and Suffix into separate independently scrolling columns (mouse wheel or drag). Either column fills the same slot that opened the picker. None stays pinned above each list; search applies to both columns and resets both scroll positions. Only visible rows are instantiated and reused. Sealed and corruption pickers also use this split without relaxing their legal eligibility when Illegal Mode is off.
- Legal ordinary prefix/suffix slots now use one full-width scrolling list; legal idol enchantments use the same two-column scrolling system. Switching between these layouts restores widths and scrollbar positions. Legal restrictions and colors remain unchanged.
- Includes the legal branch's LP correction: ordinary affixes on a unique clear LP to zero and disable its controls; native fixed unique modifiers alone preserve LP.
- Illegal Mode exposes all defined native affix families in the ordinary and corruption pickers, without item type, subtype, class, prefix/suffix placement or legal outcome-weight filtering. The four ordinary rows become Affix 1–4. Grouping, translated names, green Set affixes, purple corruption exclusives and None-first selection remain. In Illegal Mode, native idol-compatible affixes (including ordinary idol, Weaver and enchantment definitions) use cyan in pickers and selected rows. Colors do not modify the native item tooltip.
- Illegal ordinary slots can reach T8 when that definition has eight tiers. Runtime logs showed native refresh representing those T8 affixes as Primordial seals; illegal verification now accepts that specific conversion and multiple T8 Primordial affixes. Legal verification retains its single-Primordial restriction. A one-tier modifier stays T1; seven-tier definitions stay capped at T7. Saved T8 is preserved independently of the creation checkbox.
- Illegal Mode permits extra fixed unique modifiers, such as additional distinct Unsated Rage modifiers, in ordinary/sealed/corruption slots. Only the modifier chosen in the dedicated ring control (or two dedicated glove modifiers) occupies the native fixed unique prefix. Extra modifiers remain separate and must all survive complete save-ID verification. The dedicated picker also excludes IDs already selected in other slots. Legal Mode keeps its existing fixed-pool limits.
- Selecting T8 in the sealed row creates a **Primordial seal** through the native Evolution method, using a T7 seed and preserving the selected roll. T1–T7 in that row still create a regular seal. In Legal Mode, Primordial sealing is limited to supported Standard affixes on non-unique equipment; it is not a sealed Legendary transfer. Illegal Mode relaxes that eligibility but still requires a real eighth tier.
- Regular, Primordial and corruption seals are distinct in the resolved request and packing verifier. The current single sealed UI row selects regular **or** Primordial; a second independent sealed selection is not added in this UI pass.
- Illegal corruption insertion uses the game's unchecked insertion method with its corruption-slot argument, rather than manually assigning presence flags.
- A unique with one Set-piece modifier gains that piece's Set identity and equipped-set counting while retaining its own unique ID/name and unique properties. Recognition derives from saved affixes, so it does not depend on the creation checkbox. Repeated copies of the same Set piece do not count as different members.
- Added English/French/Korean/Chinese labels for the new controls.

Illegal means gameplay eligibility is bypassed. It does not promise the game can represent every combination. Known item identity, real tiers, distinct IDs, valid rolls, native seal ownership and full save-ID round trips remain required. The native Set identity is singular: multiple Set-piece modifiers on one unique are rejected rather than silently reporting an incomplete bonus. Fixed unique modifiers still require a native format that can preserve them. No rejected item is spawned.

## Install the testing build

Close Last Epoch. Fetch and run `scripts/Test-LastEpochBranches.ps1` from `chore/test-queue-runner`, then choose **2**. Entries 1 and 2 install the same current `feat/force-drop` build with different checklists. The older dedicated illegal script targets its historical branch.

The script switches/updates the named branch, requires its revision to match origin, builds and runs tests against the fresh DLL, backs up the installed DLL, then installs. A build/test/update failure stops installation.

## In-game checks

| Check | Expected result |
|---|---|
| Legal Mode, compatible eight-tier Standard equipment affix in the sealed row, set T8 | Input stays at 8; label/preview say Primordial; item has T8 and a Primordial seal. Check exact roll and save/reload. |
| Same sealed row at T7; one-tier modifier; incompatible item/family in Legal Mode | T7 is a regular seal. A one-tier modifier stays T1. Primordial is not offered for an unsupported legal route. |
| Primordial seal plus a legal corruption; corruption toggled without an affix | Both modifiers/flags survive where the native game permits them. A changed tier/seal must reject the drop and log details. |
| Equip Primordial gear, then try a second Primordial item | The game's equip limit remains in effect. This change does not remove that limit. |
| Illegal Mode: wrong-type affix, conflicting class modifiers, four prefixes or four suffixes | Choices appear in Affix 1–4. Create/equip/tooltip and save/reload preserve the chosen values if native packing supports them. |
| Illegal Mode: open any ordinary slot, sealed slot and corruption selector | Prefix choices appear on the left, Suffix choices on the right. Wheel/drag/scrollbar movement on one side leaves the other side's position unchanged. Selecting either side fills only the slot that opened the picker. None clears it from either column. |
| Split picker search, clear search, last entry, FR → EN → KO and item/category selector | Both columns search together and reset to the top. Each side stops at its own last entry; existing native family colors/grouping remain. Category, rarity and native unique modifier selectors retain their existing layouts. |
| Illegal Mode: four ordinary T8 affixes plus T8 sealed and T8 corruption, using definitions with eight tiers | All six inputs retain T8. Creation/packing must preserve every tier and seal or explicitly reject an unsupported native combination before spawning; confirm save/reload. There is no ordinary T7 gameplay cap in Illegal Mode, but missing native definition tiers are never invented. |
| Illegal Mode: ordinary T8, one-tier modifier and an arbitrary corruption | Real T8 survives, one-tier modifier stays T1, corruption uses its own seal. Repeat after turning Illegal Mode off and restarting. |
| Unique with four ordinary affixes, sealed affix and corruption | Native packing preserves every ID/tier/roll/seal or blocks the drop with an explicit error; it must never silently replace another affix. |
| Two Red Rings plus Omnis, each assigned a different matching Invoked Set-piece modifier | Unique names/properties remain; the three different pieces activate the full Set bonus. Repeating the same ring Set piece must not produce a third distinct member. Test with Remove Set Requirements off. |
| Above Set items after mode off, save/reload, unequip/re-equip | Set identity/bonus and original unique name persist. Ordinary legal Set items retain native behavior. |
| Switching modes, categories, FR → EN → KO; clear/search/scroll selectors | Selections clear on mode changes; native grouping/colors/None-first and locale changes remain correct. No overlap or leaked captions. |
| Illegal Unsated Rage: choose its dedicated Rage modifier, then different Rage modifiers in ordinary rows | Extra modifiers no longer produce the "already has a variant modifier" error. Inspect every effect, equip/unequip, turn Illegal Mode off, save/reload and restart; every selected ID/value must survive. Selecting the same modifier twice remains disallowed. |
| Illegal Unsated Rage / Withstand the Elements: additional fixed modifiers with a sealed affix and corruption | Dedicated native modifier(s) remain at the unique prefix; extra modifiers and independent seals survive native packing or the drop is rejected with exact evidence. Repeat on a unique without a dedicated modifier pool. |
| Legal Unsated Rage / Withstand the Elements | Dedicated native modifier paths continue working with ordinary affixes/seals/corruption. Additional fixed modifiers remain outside the legal pool. |

Send `Latest.log` with the mode, item, modifier names, tiers and seal/corruption selections for any failure. Set bonus activation, Primordial native construction and every new illegal packing combination require game execution; a successful build alone does not confirm them.

## Earlier illegal-branch verification

- Full mod source compiled against .NET 6 references, the project's Harmony 2.3.1.1 and the supplied native game/Unity/TMP assemblies.
- All 1,162 xUnit cases passed without skips, including the native Harmony target-resolution/source-scanner checks, locale checks, extra fixed modifier packing order/integrity and illegal native T8 normalization regressions.
- CSharpier formatting and `git diff --check` passed.

The container's `dotnet` CLI cannot initialize its process-information API, so compilation used the SDK's Roslyn compiler directly and tests used xUnit's in-process runner. The guarded Windows script still performs the normal Release build, test run and installation. In-game results above are pending.

## Primordial rule references

- [EHG Primordial Items](https://support.lastepoch.com/hc/en-us/articles/46361924471067-Primordial-Items): one Primordial item equipped at a time across the Unique and Exalted routes.
- [EHG Runes and Glyphs](https://support.lastepoch.com/hc/en-us/articles/46361877750043-Runes-and-Glyphs): Rune of Evolution upgrades T7 into a separate T8 sealed affix.
- [EHG Primal Hunt announcement](https://forum.lastepoch.com/t/primal-hunt-coming-to-last-epoch-august-21st/78569): Primordial sealing does not consume the regular sealed slot; Idol modifiers and sealed Legendary transfers are excluded.

## Combined branch UI checks (awaiting in-game confirmation)

- Wheel/drag each column independently, including the last entries; select from either side and confirm the opened slot changes.
- Search while scrolled to the bottom: both lists reset to the top; None remains pinned, including zero-result searches. Clear search and reopen another slot.
- Illegal Mode: ordinary idol-only, Weaver and enchantment affixes appear cyan in choices and selected rows; ordinary equipment affixes remain gold, Set green and corruption purple unless also idol-compatible. Legal Mode retains its original colors.
- Check clipping, readable row heights and scrolling at different menu sizes; close/reopen the menu and change scenes to check pooled row rebuilding.
- Repeat legal LP clearing and unique variant-only LP retention on this combined build. Prior in-game confirmation applies to `5b8199f6`, not these new UI changes.
- Legal ordinary prefix/suffix slots use one full-width list; legal enchantment, sealed and corruption pools use two columns. Reopen between layouts and check search resets, clipping, pinned None and exact legal eligibility. Corrupted: Yes still disables LP with every affix None; Corrupted: No unlocks LP once every ordinary affix is cleared.

For this legal scrolling follow-up, the game-independent suite reports 1,156 passed with six SDK-dependent skips. Formatting and diff checks pass. Native compilation and in-game UI checks remain pending in the current workspace.
