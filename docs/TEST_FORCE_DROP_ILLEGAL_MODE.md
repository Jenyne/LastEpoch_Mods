# Force Drop Illegal Mode and Primordial sealed tiers

Branch: `feat/force-drop-illegal-mode`, based on `fix/force-drop-legal-affixes` at `745dd1d5`.

The current Force Drop layout stays in use. Its new themed **Illegal mode** checkbox starts off. Changing mode clears ordinary/sealed/corruption selections so an illegal selection cannot leak into Legal Mode. Item identity and the exclusive unique modifier controls remain separate.

## Changes

- Illegal affix pickers split Prefix and Suffix into separate columns with independent Previous/Next buttons and page counters. Either column fills the same slot that opened the picker. None stays first on every page; search applies to both columns and resets both pages. Sealed and corruption pickers also use this split without relaxing their legal eligibility when Illegal Mode is off.
- Includes the legal branch's LP correction: ordinary affixes on a unique clear LP to zero and disable its controls; native fixed unique modifiers alone preserve LP.
- Illegal Mode exposes all defined native affix families in the ordinary and corruption pickers, without item type, subtype, class, prefix/suffix placement or legal outcome-weight filtering. The four ordinary rows become Affix 1–4. Grouping, translated names, green Set affixes, purple corruption exclusives and None-first selection remain.
- Illegal unsealed affixes can reach T8 when that definition has eight tiers. A one-tier modifier stays T1; seven-tier definitions stay capped at T7. Saved T8 is preserved independently of the creation checkbox.
- Selecting T8 in the sealed row creates a **Primordial seal** through the native Evolution method, using a T7 seed and preserving the selected roll. T1–T7 in that row still create a regular seal. In Legal Mode, Primordial sealing is limited to supported Standard affixes on non-unique equipment; it is not a sealed Legendary transfer. Illegal Mode relaxes that eligibility but still requires a real eighth tier.
- Regular, Primordial and corruption seals are distinct in the resolved request and packing verifier. The current single sealed UI row selects regular **or** Primordial; a second independent sealed selection is not added in this UI pass.
- Illegal corruption insertion uses the game's unchecked insertion method with its corruption-slot argument, rather than manually assigning presence flags.
- A unique with one Set-piece modifier gains that piece's Set identity and equipped-set counting while retaining its own unique ID/name and unique properties. Recognition derives from saved affixes, so it does not depend on the creation checkbox. Repeated copies of the same Set piece do not count as different members.
- Added English/French/Korean/Chinese labels for the new controls.

Illegal means gameplay eligibility is bypassed. It does not promise the game can represent every combination. Known item identity, real tiers, distinct IDs, valid rolls, native seal ownership and full save-ID round trips remain required. The native Set identity is singular: multiple Set-piece modifiers on one unique are rejected rather than silently reporting an incomplete bonus. Fixed unique modifiers still require a native format that can preserve them. No rejected item is spawned.

## Install the testing build

Close the game. From the repository in PowerShell:

```powershell
& {
    $ErrorActionPreference = "Stop"
    git fetch origin
    if ($LASTEXITCODE -ne 0) { throw "Fetch failed" }
    git switch --detach origin/feat/force-drop-illegal-mode
    if ($LASTEXITCODE -ne 0) { throw "Branch switch failed" }
    .\scripts\Test-ForceDropIllegalMode.ps1 `
        -GamePath "D:\SteamLibrary\steamapps\common\Last Epoch"
}
```

The script switches/updates the named branch, requires its revision to match origin, builds and runs tests against the fresh DLL, backs up the installed DLL, then installs. A build/test/update failure stops installation.

## In-game checks

| Check | Expected result |
|---|---|
| Legal Mode, compatible eight-tier Standard equipment affix in the sealed row, set T8 | Input stays at 8; label/preview say Primordial; item has T8 and a Primordial seal. Check exact roll and save/reload. |
| Same sealed row at T7; one-tier modifier; incompatible item/family in Legal Mode | T7 is a regular seal. A one-tier modifier stays T1. Primordial is not offered for an unsupported legal route. |
| Primordial seal plus a legal corruption; corruption toggled without an affix | Both modifiers/flags survive where the native game permits them. A changed tier/seal must reject the drop and log details. |
| Equip Primordial gear, then try a second Primordial item | The game's equip limit remains in effect. This change does not remove that limit. |
| Illegal Mode: wrong-type affix, conflicting class modifiers, four prefixes or four suffixes | Choices appear in Affix 1–4. Create/equip/tooltip and save/reload preserve the chosen values if native packing supports them. |
| Illegal Mode: open any ordinary slot, sealed slot and corruption selector | Prefix choices appear on the left, Suffix choices on the right. Next/Previous on one side leave the other side's page unchanged. Selecting either side fills only the slot that opened the picker. None clears it from either column. |
| Split picker search, clear search, last page, FR → EN → KO and item/category selector | Both columns search together and reset to page 1. Each side stops at its own last page; existing native family colors/grouping remain. Category, rarity and native unique modifier selectors retain their existing layouts. |
| Illegal Mode: four ordinary T8 affixes plus T8 sealed and T8 corruption, using definitions with eight tiers | All six inputs retain T8. Creation/packing must preserve every tier and seal or explicitly reject an unsupported native combination before spawning; confirm save/reload. There is no ordinary T7 gameplay cap in Illegal Mode, but missing native definition tiers are never invented. |
| Illegal Mode: ordinary T8, one-tier modifier and an arbitrary corruption | Real T8 survives, one-tier modifier stays T1, corruption uses its own seal. Repeat after turning Illegal Mode off and restarting. |
| Unique with four ordinary affixes, sealed affix and corruption | Native packing preserves every ID/tier/roll/seal or blocks the drop with an explicit error; it must never silently replace another affix. |
| Two Red Rings plus Omnis, each assigned a different matching Invoked Set-piece modifier | Unique names/properties remain; the three different pieces activate the full Set bonus. Repeating the same ring Set piece must not produce a third distinct member. Test with Remove Set Requirements off. |
| Above Set items after mode off, save/reload, unequip/re-equip | Set identity/bonus and original unique name persist. Ordinary legal Set items retain native behavior. |
| Switching modes, categories, FR → EN → KO; clear/search/page selectors | Selections clear on mode changes; native grouping/colors/None-first and locale changes remain correct. No overlap or leaked captions. |
| Unsated Rage / Withstand the Elements exclusive modifiers | Existing dedicated native modifier paths continue working with ordinary affixes/seals/corruption. Unsupported foreign fixed modifiers fail without spawning. |

Send `Latest.log` with the mode, item, modifier names, tiers and seal/corruption selections for any failure. Set bonus activation, Primordial native construction and every new illegal packing combination require game execution; a successful build alone does not confirm them.

## Verification performed here

- Full mod source compiled against .NET 6 references, the project's Harmony 2.3.1.1 and the supplied native game/Unity/TMP assemblies.
- All 1,146 xUnit cases passed without skips, including the native Harmony target-resolution/source-scanner checks, locale checks and new mode/Primordial packing regressions.
- CSharpier formatting and `git diff --check` passed.

The container's `dotnet` CLI cannot initialize its process-information API, so compilation used the SDK's Roslyn compiler directly and tests used xUnit's in-process runner. The guarded Windows script still performs the normal Release build, test run and installation. In-game results above are pending.

## Primordial rule references

- [EHG Primordial Items](https://support.lastepoch.com/hc/en-us/articles/46361924471067-Primordial-Items): one Primordial item equipped at a time across the Unique and Exalted routes.
- [EHG Runes and Glyphs](https://support.lastepoch.com/hc/en-us/articles/46361877750043-Runes-and-Glyphs): Rune of Evolution upgrades T7 into a separate T8 sealed affix.
- [EHG Primal Hunt announcement](https://forum.lastepoch.com/t/primal-hunt-coming-to-last-epoch-august-21st/78569): Primordial sealing does not consume the regular sealed slot; Idol modifiers and sealed Legendary transfers are excluded.
