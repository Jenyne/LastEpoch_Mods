# Offline startup isolation candidate

Branch: `test/offline-guard-diagnostics`; test queue selection 7.

The user reported character-load crashes on `99eebd98`. The same character loaded successfully on baseline selection 9 (`92fb33de`) and exited normally. The logs do not capture a fatal stack, so the observer is a suspect rather than a proven cause.

This candidate removes the session observer entirely, including its Harmony hooks, per-frame sampling call, networking assembly reference and observer-only core/tests. Auto-offline startup and online-switch hiding/blocking are unchanged from `4525381`. The last diagnostic build remains in branch history for comparison. There should be no `[OfflineGuard]` messages from this candidate.

## Test

1. Close the game and rerun selection 7 in `Test-LastEpochBranches.ps1`. Confirm the newly printed commit, not `99eebd98` or `dda8b7f`.
2. Confirm automatic offline selection and hidden online switch.
3. Load the same character that worked on selection 9. Wait at least 30 seconds, enter a zone/echo, fight and return.
4. Return to selection and load a second character, then reload the first. Exit normally and retain Latest.log and Player.log.
5. If it crashes, retain the logs before another launch and report whether the stop was at character entry, zone transition or combat. Startup-only isolation will then be required within the remaining UI hooks.

This is an isolation candidate, not a confirmed crash fix. Hold upstream submission until successful runtime retesting. The startup behavior uses the game's normal offline click handler; it does not enforce comprehensive offline mutation protection.

The existing `Test-OfflineGuardDiagnostics.ps1` name is retained for compatibility; it delegates to the checked startup build/install helper and preserves config, assets and saves. See `TEST_AUTO_OFFLINE_STARTUP.md` for that flow and manual build instructions.
