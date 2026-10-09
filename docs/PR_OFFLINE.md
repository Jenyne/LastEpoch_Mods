# Require offline play while the mod is installed

Target: `Syncingoutt/LastEpoch_Mods:master`
Source: `Jenyne/LastEpoch_Mods:test/offline-guard-diagnostics`

Keep the mod offline-only by hiding online controls and blocking online requests from the landing and character-selection screens. There is no configuration or UI opt-out; online play requires uninstalling the mod. The obsolete `Login.Enable_AutoLoginOffline` option is removed.

Players click the normal Play Offline button. Automatic selection and the startup readiness gate have been removed after the gate failed to receive its completion notification and blocked manual selection. The offline click handler is unpatched, leaving loading and transitions to the game. The gameplay/session observer remains removed, and this change does not edit HUD layout.

Validation: CSharpier passed; 999 core checks passed and six native-SDK checks were skipped; native build and in-game confirmation remain pending for this manual-selection revision. Retest mouse/controller Play Offline, two character entries and combat/echoes on the standalone build and Sync’s HUD build.
