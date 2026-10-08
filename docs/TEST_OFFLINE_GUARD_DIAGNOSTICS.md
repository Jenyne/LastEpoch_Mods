# Offline guard diagnostics test

Branch: `test/offline-guard-diagnostics`

Based on `test/auto-offline-character-select` at `4525381`. This includes the startup
shortcut and character-selection online switch hiding. The additional diagnostics
only observe game calls and state. They do not block gameplay patches, change game
arguments or results, force saves, write mode flags, or install mutation permissions.
`OfflineCandidate` is a diagnostic classification, not protection against online use.

## Build and install

Close the game. From your repository in PowerShell:

```powershell
& {
    git fetch origin
    if ($LASTEXITCODE -ne 0) { throw "Fetch failed" }
    git switch --detach origin/test/offline-guard-diagnostics
    if ($LASTEXITCODE -ne 0) { throw "Switch failed" }
    .\scripts\Test-OfflineGuardDiagnostics.ps1 `
        -GamePath "D:\SteamLibrary\steamapps\common\Last Epoch"
}
```

The installer reuses `Test-AutoOfflineStartup.ps1`, including its existing
`Login.Enable_AutoLoginOffline` check. It builds Release, checks the build result,
backs up the installed DLL under `UserData\LastEpoch_Hud\test-backups\auto-offline-*`,
and installs the new DLL. Existing config, assets, and saves remain in place.
Add `-BuildOnly` to build without installation. No profiling setting is required.

## Offline test sequence

1. Launch normally and confirm automatic offline character selection. Confirm the
   previous startup branch's online switch hiding still works.
2. Enter an existing offline character. Wait a few seconds after it becomes playable.
3. Change zones, enter an echo, finish it, and return. Play for at least 30 seconds so
   the log records a heartbeat. Use your normal enabled features as usual.
4. Return to character selection and load a different offline character. Return again
   and reload the first character. This checks that character correlation starts fresh.
5. If convenient, create and enter a new offline character. Also try canceling a load
   where the game provides that option; do not force a cancellation by killing the game.
6. Return to the title/menu and exit normally. Keep the complete
   `MelonLoader\Latest.log` before another launch overwrites it.

Online play is not needed for this test. This branch has not installed the runtime
mutation guard; do not use an online character to test the observers.

## Reading the log

All additional lines begin with `[OfflineGuard]`. Startup logs identify the build.
Character names, character IDs, user identities, save paths, inventory contents,
and native pointers are not included by these observers.

| Field or message | Meaning |
| --- | --- |
| `epoch` | Observation generation. A new offline play request or session exit advances it. Multiple exit events may advance it more than once. |
| `Unknown` | No offline character play request has been captured. |
| `Loading` | A request was captured, but independent live evidence is missing or unavailable. `reason` identifies the first missing condition. |
| `OfflineCandidate` | Offline service/file store, later matching character initialization, actual InGame state, live offline network group, local actor/tracker, loaded character identity and offline marker agree; online mode and established online session both report false. |
| `Revoked` | An online request/state, conflicting loaded character marker, or session exit invalidated the observation. Changing flags back does not recover it; a fresh offline StartPlay is required. |
| `unknown` | A source has not been captured or its getter was unavailable. It is never treated as a positive signal. |
| `authenticated` | Supplemental startup information. Authentication alone is not treated as online gameplay. |
| `requestMatch` | Current character data matches the offline request by native object identity or a nonempty character ID. The IDs themselves are not logged. |
| Save counters | Number of save requests observed during this process, not successful async completions. They do not affect classification. |

Scene load/unload notifications are logged without clearing the character epoch.
Temporary actor/network unavailability during a zone load can move a candidate back
to Loading; it can recover within the same epoch if the live evidence returns. A
transition to Login/CharacterSelect or InGame exit clears character correlation.

The async `StartPlay` prefix logs a request, never completion. The synchronous
`CharacterDataTracker.InitializeCharacter` postfix supplies a later correlation
signal, and a twice-per-second sample checks the current player from `PlayerFinder`
rather than the mod's cached actor. The precise ordering and sufficiency of these
signals still require in-game verification. If a signal is absent, the useful result
is its missing-condition log; no mutation protection depends on this classifier yet.

Snapshots are logged when evidence/classification changes, with a 30-second heartbeat.
Save calls are counted rather than logged individually. Hooks enqueue bounded messages;
logging and live polling occur on the main update thread. Observer errors are counted
and their exception types reported without potentially private exception messages.
No scene-wide object searches, event subscriptions, async awaits, or game service calls
that initiate operations are added.

## Revert

To return to the startup-only test:

```powershell
& {
    git fetch origin
    if ($LASTEXITCODE -ne 0) { throw "Fetch failed" }
    git switch --detach origin/test/auto-offline-character-select
    if ($LASTEXITCODE -ne 0) { throw "Switch failed" }
    .\scripts\Test-AutoOfflineStartup.ps1 `
        -GamePath "D:\SteamLibrary\steamapps\common\Last Epoch"
}
```

Alternatively, with the game closed, restore the backed-up DLL to
`Mods\LastEpoch_Hud.dll`. Keep backup DLLs outside the Mods directory.
