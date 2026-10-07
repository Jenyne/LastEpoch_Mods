# Idol rerolling test

Branch: `feat/idol-reroll-misc`
Base: Syncingoutt/master `2bc921dc` (v4.4.21, including language-switch and custom-item fixes).
Status: user confirmed idol rerolling works perfectly on 2026-10-07, after testing build `98570ae2`.

## Changes

- Add independent No Memory Amber Cost and Unlimited Idol Altar Uses checkboxes under Scenes → Misc → Idol Rerolling.
- Use native checkbox sprites/fonts, gold separators and left-aligned captions.
- Save both preferences in SaveModUI.json; new installations default to off.
- Free cost covers class-specific and Weaver idol rerolls. Native unlock/item checks still run. Other Weaver spending keeps its normal cost.
- Unlimited uses applies to the idol altar only. Temporary native limit changes and reroll debit scope are unwound by Harmony finalizers, including exceptions.
- Refresh open crafting controls after a preference changes.
- Add English/French/Korean/Chinese labels.

## Build/install

Close Last Epoch. From the repository root, fetch/switch/pull this branch, then run:

```powershell
.\scripts\Test-IdolReroll.ps1
```

The script builds Keyboard, runs repository tests with LAST_EPOCH_PATH, then copies the DLL and locales. A failed build or test stops installation.

## In-game checklist

1. Confirm both controls appear under Scenes → Misc without overlapping Safe Teleport or Minimap. Log should show IdolReroll Bound 2/2.
2. Both off: confirm ordinary amber cost and use-limit behavior.
3. Free only: reroll class-specific and Weaver idols; confirm displayed cost zero and balance unchanged, while uses are consumed normally. Test with a low/zero amber balance if possible.
4. Unlimited only: reroll more times than the altar normally permits; confirm amber is still charged and the use count is preserved.
5. Both on: repeat past the limit without paying amber. Leave/re-enter the altar and test again.
6. Turn options off while the crafting UI is open. Confirm native cost/use limits return and there are no UI errors.
7. Check another Memory Amber purchase or non-reroll idol action: the free-cost option must not waive that purchase.
8. Restart: check both preferences persist and normal rerolls still work.
9. Switch EN → FR → EN → KO → EN; captions must follow the selected language.
10. Check other zone-restricted crafting still uses its normal limits.

Metadata validation found each patch method in the supplied game assembly catalog; OnUse overloads use explicit parameter types. CSharpier checks pass for all 219 C# files. Locale JSON/key coverage and whitespace checks pass. The local test runner is blocked by a process-information initialization error in this environment. The user subsequently confirmed that idol rerolling works perfectly in game on 2026-10-07. No separate automated test output or individually reported restart/locale results accompanied that confirmation.
