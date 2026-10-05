# Locale update

## Player changes

- Force Drop controls, picker headings, affix slot labels, previews, validation and drop results follow the game language.
- Localize Choose Blessings, Discover All Blessings, Max Out Blessings and Unlock Blessing Slots.
- Update French, Korean and Simplified Chinese translations, including missing Korean labels for newer controls.
- Supply locale JSON files individually for optional manual installation and updates. English remains the fallback.

## Developer changes

- Register canonical labels for generated Text and TMP controls so changing language does not translate previously translated text.
- Translate numbered labels and formatted status messages while preserving native item and affix names.
- Store dictionaries in the actual project packaging directory.
- Include the latest blessing implementation and three-button layout from PR #12. Merge PR #12 first.
- No automatic ZIP packaging is added.

## Validation

The corrected locale files and in-game display were confirmed. Locale coverage, format placeholders and C# syntax were checked. The newly added Max Out label and three-button layout still need an in-game locale check with the latest blessing repair.
