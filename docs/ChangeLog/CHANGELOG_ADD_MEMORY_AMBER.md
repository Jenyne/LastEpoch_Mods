# Currency buttons: Memory Amber and Soul Embers

- Adds an **Add 10,000 Memory Amber** button beside Ancient Bones in Character → Cheats.
- Adds a fixed 10,000 using the Weaver faction's native currency update and save/sync methods. The grant bypasses multipliers and reputation gain.
- Requires the character to be a Woven faction member; does not join or change membership automatically.
- Caps the grant at the native currency limit to avoid overflow.
- Fixes Soul Embers granting twice the entered amount: removes the duplicate construction-time listener and clears previous runtime listeners before rebinding. Keeps the native currency method and displayed-balance refresh.
- Includes English, French, Korean, and Chinese labels in the supplied locale files.

Validation: C# syntax, locale JSON, and native wrapper API checks completed. Full compilation and in-game verification remain pending.

In-game checks: click once and verify +10,000, including with the Memory Amber multiplier enabled; verify Ancient Bones still works independently; enter 1,000 Soul Embers and verify exactly +1,000, then reopen the HUD and repeat; verify save/reload persistence and translated button layout.

## Currency and blessing layout follow-up

- Group all Add buttons into a compact full-width bar Currencies section in Character → Cheats.
- Replace the Soul Ember input and Data row with Add 1,000 Soul Embers. Each click uses one native +1,000 grant.
- Move Choose Blessings, Discover All Blessings, Max Out Blessings and Unlock Blessing Slots to the bottom of Character → Data.
- Match native button appearance and gold section dividers; translate the new labels in English, French, Korean and Chinese.
- Initial integration gameplay was confirmed; this layout follow-up remains pending in-game testing.

- Layout follow-up: stack currency and blessing actions as full-width native button bars, with 20-pixel rows and 2-pixel gaps to reduce scroll space.
