# Manual offline-only startup candidate

Branch: `test/offline-guard-diagnostics`; test queue selection 7.

Automatic offline selection and the startup readiness listener/gate are removed. The previous candidate `9b888a1b` never observed the completed Login notification and blocked manual clicks. Play Offline now uses the unpatched game handler. Online landing and character-selection actions remain hidden/blocked unconditionally, with no configuration opt-out. Online play requires uninstalling the mod.

The gameplay/session observer remains removed. Earlier observer-free builds passed character-entry testing, but this manual-selection revision still needs native testing.

## Test

1. Close the game and rebuild/install selection 7. Confirm the new commit in the runner and Latest.log.
2. Wait for the landing screen and click Play Offline with mouse, then repeat a fresh launch with controller if available. Confirm selection is manual and the button responds.
3. Confirm online controls remain unavailable, including with an old false or missing Login configuration.
4. Load a character, enter combat and an echo, return to selection and load a second character. Exit normally.
5. Repeat on Sync's HUD build after applying this correction. Retain Latest.log and Player.log from the same run if loading fails or crashes.

Core validation: 999 passed, six native-SDK checks skipped; formatting passed. Native build/runtime confirmation remains pending.

The legacy `Test-OfflineGuardDiagnostics.ps1` and `Test-AutoOfflineStartup.ps1` filenames are retained for compatibility. They build/install the manual offline-only candidate. See `TEST_AUTO_OFFLINE_STARTUP.md` for commands.
