# Infinite Forging Potential — pending in-game testing

## Player changes
- Items > Crafting: new Infinite toggle beside the existing Forging Potential control.
- Disabled by default; saved in SaveModUI.json.
- Keeps the item's current Forging Potential by requesting zero FP cost from native crafting.
- Independent of the existing fixed FP value control and its crafting-slot modification toggle.
- English, French, Korean and Simplified Chinese labels.

## Developer changes
- Scope the override to CraftingManager.Forge for the local player's actor.
- Temporarily enable native debugNoForgingPotentialCost and restore its previous value in a Harmony finalizer, including exception paths.
- Set native noPotentialCost arguments on ItemData.applyForgingPotentialCost and applyForgingPotentialCostFromShard only while inside that local crafting scope.
- Do not continually write item FP or replenish inventory values.
- Leave native crafting execution, shard/rune/glyph handling and other eligibility checks in place.

## Validation and pending tests
C# syntax checked and native actor, Forge, debug flag and cost-method signatures verified against supplied Il2CppLE.dll. No complete compilation or in-game testing in this environment.

- Build and check the Infinite toggle's placement beside the FP controls.
- Enable with a known nonzero FP amount; add/upgrade affixes and use FP-consuming runes/glyphs. Verify the exact FP amount remains unchanged and crafting materials are consumed normally.
- Disable; verify ordinary FP spending resumes.
- Test low/zero FP eligibility, failed attempts, repeated crafts, item swapping and panel reopening.
- Test Ice/Blood Forging Potential variants where available.
- Item-transforming crafts can have additional native paths; report any craft that still reduces FP.
- Confirm item and setting save/reload persistence.
