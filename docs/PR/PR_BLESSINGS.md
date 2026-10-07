# Fix blessing selection and slots; separate discovery from max rolls

Choose Blessings now opens the native panel and supports direct selection. Discover All Blessings gives new discoveries random rolls while preserving existing rolls; the separate Max Out Blessings button sets all three roll bytes to 255 for discovered and equipped blessings. Unlock Blessing Slots remains separate and preserves occupied slots.

Address the review finding in CreateBlessingDataForSave: initialize all three bytes consistently, then randomize all three for discovery. Maximum-roll creation explicitly keeps all three at 255.

Repair missing character-specific roll data for shared discoveries, rebuild the panel's cached rolls after edits, and select the actual timeline ID. Replace the respec swap with validated container removal/addition through the supported TryRemoveItem API, retaining rollback on rejected additions. Preserve saved discovery rolls through container save synchronization.

Discovery uses the game's shared stash storage; maximum rolls and equipped choices remain character-specific. Add the Max Out Blessings label to shipped locales.

Changelog: docs/CHANGELOG_BLESSINGS.md.
Validation: earlier choose/discover/unlock persistence confirmed in game; latest changes passed C# syntax and native API metadata checks. Final build, UI refresh, changing blessings after maximizing and save/reload of the latest repair remain to be verified. Coordinate merge with localization PR #13.
