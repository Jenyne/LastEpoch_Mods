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
- Corruption is marked after native affix-slot allocation and before the adapter's first packing step. Corrupted requests use zero forging potential, legendary potential and Weaver's Will. Those fields keep their existing positions and selected values for later uncorrupted drops, are disabled while corrupted, and preview as 0.
- Every completed item, including a noncorrupted drop, is decoded from its final packed ID and compared. Checks cover base/subtype/unique identity, rarity, potential, implicit and unique rolls, affix IDs/tiers/rolls/special types, and seal ownership. Base-item socket counts remain strict; unique/set/legendary items may decode sockets as zero while preserving their complete affix list. FP above 63 still uses the existing mod persistence extension; its native packed value is checked separately from the restored live value.
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

## First runtime feedback and diagnostic update

The October 7 test on `a4df9452` loaded successfully, but rejected two drops while testing Unsated Rage. One reported a changed existing affix during variant addition; the other reported a socket-count mismatch after final decoding. The screenshot shows Unsated Rage with four ordinary T7 affixes and its Sanguine Rage modifier, with no seal or corruption selected. These failures are unresolved; the original messages do not contain enough data to identify the changed fields or interpret the socket count.

The diagnostic update retains every rejection check. Variant errors now print complete expected and actual ordinary-affix signatures (`id:tier:roll:seal:specialType:placement`). Final packing errors print both item snapshots and the packed ID in Base64. Snapshot affix signatures use numeric special-type/placement values and the named seal. This makes a native storage change distinguishable from an incorrect validator assumption without permitting an unverified item to drop.

Rebuild with the same script above, repeat the same Unsated Rage selections, and supply the complete new `Latest.log`. If time permits, also try the ring with only its exclusive modifier, then with one ordinary affix. The update is diagnostic, not a confirmed fix for either failure.

## Socket validation correction

The follow-up `77512efd` runtime log isolates the ring-only rejection: Unsated Rage (base 21, subtype 10, unique 477) with modifier 1138 decoded with `sockets=0` and exactly one affix. The affix ID, T1, roll 255, FakeUniqueMod type, prefix placement, unique identity, rarity, potential and all roll bytes matched the live item exactly. The saved modifier was intact; the assertion that sockets must equal affix count was wrong for this unique.

The correction accepts zero decoded sockets for unique/set/legendary storage (rarities 7–9). It still requires the exact complete affix multiset, including every selected variant and transferred affix, and checks all other item fields and seal flags. Nonzero mismatched counts remain rejected, and ordinary item socket checks are unchanged. The variant adapter also accepts this zero-socket representation. Construction, saved bytes and the HUD layout are unchanged.

Retest the ring with only its modifier, then with four ordinary affixes, and the nonvariant unique sword. Confirm effects and save/reload after successful drops. This corrects the demonstrated false rejection; the earlier `Unique variant changed an existing affix` report remains unresolved until reproduced with detailed signatures. The sword's older generic rejection did not provide decoded values, so its outcome still needs game testing.

## Corruption packing order correction

The next `57fef5ef` test reported successful noncorrupted drops, followed by a corruption failure on Unsated Rage. Affix 1074 remained in the affix list but lost `FromCorruption` and the presence flag during the adapter's refresh. The preceding pre-pack corruption check passed. Inspection found that the adapter refreshed the item before the creator marked it corrupted.

The adapter now creates the native corruption slot while the item is uncorrupted, then marks it corrupted before its first refresh. It verifies that the selected seal remains present both after marking and after packing. The creator still completes the native corruption action and verifies the final decoded ID before spawning anything. The next game test must confirm this corrects the observed loss; it is not yet runtime-confirmed.

Resolved corrupted requests normalize FP, LP and Weaver's Will to zero, including calls outside the HUD. This follows [the official corruption rules](https://support.lastepoch.com/hc/en-us/articles/52977667498267-Corrupted-Items). The preview and disabled controls reflect that behavior. Independent regular seals and fixed unique modifiers remain required to survive unchanged; corruption must not take over their slots.

Retest the ring with its exclusive modifier, four ordinary affixes and the same corruption choice. Then add a regular sealed affix if available for the selected item, and repeat on a base item. After successful creation, verify tooltip/stats and save/reload. Preserve the complete rejection if any stage fails; corruption errors now also print the item corruption bit and changed existing-affix signatures.

## Verification completed here

- 55 game-independent regression cases passed using xUnit assertions, including the exact ring socket regression, preservation of independent seals, rejection of a corruption modifier losing its seal, and potential normalization without losing selected modifiers.
- New core compiled for .NET 6.
- Changed Force Drop HUD and adapters compile against the supplied Unity/TMP/Harmony assemblies with game context stubs.
- Formatting and diff checks pass; the layout-building code and Harmony patch count are unchanged.

The supplied `Il2CppLE.dll` has unreadable metadata in this environment, so these checks are not a full mod/game SDK build. The guarded Windows script runs the actual build and full test suite. Native packing, equip behavior and persistence remain pending the game tests above.

## Next stages

Use this request/verification boundary for a shared route-aware legality evaluator. Bring the queued illegal controls and broader corruption pools onto current master without changing the layout. Validate ordinary versus corruption Set applications, actual outcome tier weights and compound outcomes, experimental/Champion placement, idol slot counts and enchantments, class-specific legendary routes, primordial storage and representable illegal T8 combinations. Keep structurally invalid items rejected in both modes.
