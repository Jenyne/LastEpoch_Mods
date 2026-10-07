# Localize new Force Drop HUD and blessing controls

The Force Drop HUD, new blessing controls and dynamic HUD text now follow the selected game language. Translate picker titles, numbered affix/roll labels, previews, validation and drop results; preserve native game item and affix names and English fallback.

Update English, French, Korean and Simplified Chinese dictionaries in the actual packaged directory, LastEpoch_Hud/LastEpoch_Hud/Locales. Register Choose Blessings, Discover All Blessings, Max Out Blessings and Unlock Blessing Slots for locale changes, including the three-button discovery/max/slot layout from PR #12.

Distribute the DLL and locale JSON files as individual downloads. Translations are optional and installed or updated manually in Mods/LastEpoch_Hud/Locales. No automatic ZIP packaging.

This branch incorporates the revised blessing implementation from PR #12 to avoid shipping stale blessing controls. Merge PR #12 first; the blessing implementation is not a separate change to review here.

Validation: locale display confirmed in game after correcting the packaged paths. JSON coverage, placeholder consistency and C# syntax checked. The new Max Out label and updated three-button layout require an in-game locale check alongside final verification of PR #12.
