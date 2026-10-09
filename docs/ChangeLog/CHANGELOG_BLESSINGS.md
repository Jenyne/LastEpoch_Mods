# Blessing controls update

## Player changes

- Choose Blessings opens the game's blessing panel and allows direct selection of discovered blessings.
- Discover All Blessings unlocks the full blessing catalog. Newly initialized rolls are random; existing rolls are preserved.
- Max Out Blessings is a separate button. It sets all three roll bytes to 255 for discovered and equipped blessings.
- Unlock Blessing Slots remains a separate action and fills empty compatible slots without replacing occupied slots.
- Discovery follows the game's shared blessing storage for characters sharing a stash. Missing character-specific rolls are initialized randomly; maximum rolls and equipped choices are not copied from another character.
- Refresh the blessing panel after selection, slot unlocking and roll changes.

## Developer changes

- Initialize all three roll bytes consistently in CreateBlessingDataForSave; random values use the integer range 0–255.
- Reconcile shared discoveries with missing character-specific roll data while preserving existing and equipped rolls.
- Replace the respec swap call with validated typed-container removal/addition using TryRemoveItem, and restore the previous blessing if addition fails.
- Refresh the UI's cached blessing data using actual timeline IDs rather than container indices.
- Preserve discovery roll data while saving container state and character data.
- Keep the direct-selection behavior scoped to the blessing panel opened by the mod.

## Validation

The earlier choose/discover/unlock implementation and persistence were confirmed in game. The discovery/max split and latest UI/container repair have passed C# syntax and native API metadata checks. Final build, selection after maximizing, UI refresh and save/reload verification of the latest repair are still required before merging.
