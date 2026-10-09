# Require offline play while the mod is installed

Target: `Syncingoutt/LastEpoch_Mods:master`
Source: `Jenyne/LastEpoch_Mods:test/offline-guard-diagnostics`

The mod must always run offline. This change automatically selects Play Offline through the game’s normal landing flow, hides the online controls and blocks online requests from the landing and character-selection screens. There is no configuration or UI opt-out: online play requires uninstalling the mod. The obsolete `Login.Enable_AutoLoginOffline` setting is removed, so old false or missing values cannot bypass the restriction.

Startup waits for the client’s completed Login-state notification and settled landing UI, then dispatches once per visit. Early manual clicks and duplicate pending requests are blocked. This addresses the reported CharacterSelect request while SystemLoading was still in progress. The game retains responsibility for loading local characters and changing scenes. The gameplay/session observer was removed after character-entry crashes; this change does not reintroduce it or change HUD layout.

Validation: 1,018 core tests passed, six native-SDK checks skipped; CSharpier passed. The observer-free predecessor passed the user’s no-crash retest with two character entries. User reports a further successful in-game run: entered one character, backed out and entered another with no crash. No log or commit identifier accompanied this report. Old false/missing configuration and combat/echo regression checks remain outstanding.

The startup readiness adapter uses the current game’s exact application-state log notification and detaches after login. This new race correction needs native compilation and in-game confirmation on Sync’s HUD build; earlier successful tests remain evidence for the earlier source revisions.
