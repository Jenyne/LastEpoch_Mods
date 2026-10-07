# Force Drop creation and packing checks

Branch: `fix/force-drop-integrity`.
Base: Syncingoutt/master `28d7c731` (includes the latest custom-item changes).

This pass keeps the current Force Drop layout and changes its creation path. It is separate from Maxroll and the older queued illegal/corruption-pool branches. Those branches are not merged here. This is the first creation-engine stage, not a certification of every legal acquisition path or illegal combination.

## Changes to test

- The displayed selections become an immutable item request. Construction no longer depends on copying values into hidden sliders and calling the old creator.
- Random values are resolved once per item and reused for verification. Each copy in a batch still receives its own rolls.
- Ordinary, corruption and variant definitions use a shared live catalog. Ordinary affixes use native constructor metadata rather than manual field initialization.
- Tier ranges follow the selected definition. In the supplied catalog, ordinary idol affixes have one tier; their fields now stop at T1. The existing route cap remains T7.
- Regular sealing uses the game's seal operation. A regular seal and a corruption seal are tracked independently. The corruption operation's regular-seal output no longer incorrectly vetoes their coexistence.
- Corruption is marked after affix addition. Corrupted items retain zero forging potential; the old creator restored the selected FP afterward. The existing FP field stays in place and is disabled while corrupted; the preview shows 0.
- Every completed item, including a noncorrupted drop, is decoded from its final packed ID and compared. Checks cover base/subtype/unique identity, rarity, potential, implicit and unique rolls, affix IDs/tiers/rolls/special types, seal ownership, and socket counts. FP above 63 still uses the existing mod persistence extension; its native packed value is checked separately from the restored live value.
- Batch status reports completed items if a later copy fails. Rejected items never reach the ground-drop call.
- The testing script checks the Release DLL it just built, instead of an older Keyboard build.

## Install

Close Last Epoch. From `D:\GitHub\LastEpoch_Mods`, run the whole block:

```powershell
& {
    $ErrorActionPreference = "Stop"
    git fetch origin
    if ($LASTEXITCODE -ne 0) { throw "Fetch failed" }
    git show-ref --verify --quiet refs/heads/fix/force-drop-integrity
    if ($LASTEXITCODE -eq 0) {
        git switch fix/force-drop-integrity
    } else {
        git switch --track origin/fix/force-drop-integrity
    }
    if ($LASTEXITCODE -ne 0) { throw "Branch switch failed" }
    git pull --ff-only origin fix/force-drop-integrity
    if ($LASTEXITCODE -ne 0) { throw "Update failed" }
    .\scripts\Test-ForceDropIntegrity.ps1 -GamePath "D:\SteamLibrary\steamapps\common\Last Epoch"
}
```

The script stops before replacing the installed DLL if the build or tests fail, and backs up the installed DLL before copying. It does not change mod settings, locale files, or character saves.

## Focused game tests

| Test | Expected result |
| --- | --- |
| Base equipment with four fixed ordinary affixes | Correct modifiers and tiers; item has a working tooltip; equip, save and reload retain them. |
| Base equipment with just a regular sealed affix, then with four ordinary affixes plus that seal | Regular seal retains its selected modifier and roll. |
| Base helmet/amulet with corruption only | Selected compatible corruption is correctly sealed; FP is 0. |
| Same item with regular sealed + selected corruption affix | Both seals survive independently, alongside ordinary affixes. This combination is the main runtime test. |
| Random preset, corrupted item, quantity 5 | No verification failure caused by a second RNG sample; each item preserves its resolved values. |
| Ordinary idol, choose prefix/suffix, then Maximum preset | Both selected definitions stay at T1, display correct effects, and persist after reload. This test does not validate the remaining idol slot-count rules. |
| Unique with 4LP, no transferred affixes | Correct unique identity, potential and selected unique rolls survive packing/reload. |
| Unsated Rage and Withstand the Elements | Ring retains one selected native modifier; gloves retain two distinct selected native modifiers; variants stay separate from ordinary affixes. |
| Plain base weapon/armor with FP 100, then FP 20 | Existing FP extension still preserves 100 through save/reload; ordinary packed value 20 also survives. |
| Runes, glyphs, shards and keys | Existing non-equipment choices still create correctly named usable items. |
| Category/item changes and FR → EN → KO switching | Same layout and translated selectors; no wrong-item identity retained from previous selection. |

If creation is rejected, keep the complete error and `Latest.log`. Do not mark an item working from its label alone: equip/stat behavior and save/reload still require game testing.

## Verification completed here

- 38 game-independent regression cases passed using xUnit assertions.
- New core compiled for .NET 6.
- Changed Force Drop HUD and adapters compile against the supplied Unity/TMP/Harmony assemblies with game context stubs.
- Formatting and diff checks pass; the layout-building code and Harmony patch count are unchanged.

The supplied `Il2CppLE.dll` has unreadable metadata in this environment, so these checks are not a full mod/game SDK build. The guarded Windows script runs the actual build and full test suite. Native packing, equip behavior and persistence remain pending the game tests above.

## Next stages

Use this request/verification boundary for a shared route-aware legality evaluator. Bring the queued illegal controls and broader corruption pools onto current master without changing the layout. Validate ordinary versus corruption Set applications, actual outcome tier weights and compound outcomes, experimental/Champion placement, idol slot counts and enchantments, class-specific legendary routes, primordial storage and representable illegal T8 combinations. Keep structurally invalid items rejected in both modes.
