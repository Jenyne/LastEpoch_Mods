# Require offline play while the mod is installed

Target: `Syncingoutt/LastEpoch_Mods:master`
Source: `Jenyne/LastEpoch_Mods:test/offline-guard-diagnostics`

Keep the mod offline-only by hiding online controls and blocking online requests from the landing and character-selection screens. There is no configuration or UI opt-out; online play requires uninstalling the mod. The obsolete `Login.Enable_AutoLoginOffline` option is removed.

Players click the normal Play Offline button. Automatic selection and the startup readiness gate have been removed after the gate failed to receive its completion notification and blocked manual selection. The offline click handler is unpatched, leaving loading and transitions to the game. The gameplay/session observer remains removed, and this change does not edit HUD layout.

Validation: the user confirmed manual offline selection works on `fd79f606`. CSharpier passed; 999 core checks passed and six native-SDK checks were skipped. Controller input, combat/echo endurance and integration with Sync’s HUD build still need separate confirmation.
