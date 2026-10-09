# Require offline play while the mod is installed

Target: `Syncingoutt/LastEpoch_Mods:master`
Source: `Jenyne/LastEpoch_Mods:test/offline-guard-diagnostics`

The mod must always run offline. This change automatically selects Play Offline through the game’s normal landing flow, hides the online controls and blocks online requests from the landing and character-selection screens. There is no configuration or UI opt-out: online play requires uninstalling the mod. The obsolete `Login.Enable_AutoLoginOffline` setting is removed, so old false or missing values cannot bypass the restriction.

Startup waits for the landing UI to be ready and dispatches once per visit. The game retains responsibility for loading local characters and changing scenes. The gameplay/session observer was removed after character-entry crashes; this change does not reintroduce it or change HUD layout.

Validation: 1,005 core tests passed, six native-SDK checks skipped; CSharpier passed. The observer-free predecessor passed the user’s no-crash retest with two character entries. User reports a further successful in-game run: entered one character, backed out and entered another with no crash. No log or commit identifier accompanied this report. Old false/missing configuration and combat/echo regression checks remain outstanding.
