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
3. Change zones, enter an echo, finish it, and return. Play for at least 30 seconds to check stability. Use your normal enabled features as usual.
4. Return to character selection and load a different offline character. Return again
   and reload the first character. This checks that character correlation starts fresh.
5. If convenient, create and enter a new offline character. Also try canceling a load
   where the game provides that option; do not force a cancellation by killing the game.
6. Return to the title/menu and exit normally. Keep the complete
   `MelonLoader\Latest.log` before another launch overwrites it.

Online play is not needed for this test. This branch has not installed the runtime
mutation guard; do not use an online character to test the observers.

## Reading the log

Offline startup is confirmed by `[Offline] Offline character selection reached.`
The observer emits `[OfflineGuard] Offline session signals confirmed (observation only).`
once per loaded character when the independent live signals agree. It does not imply
that mutation protection has been installed.

Normal service initialization, scene changes, loading snapshots, save requests and
periodic heartbeats are silent. A first observer exception and revocation of a
previously confirmed observation remain visible. Temporary evidence loss while
changing zones does not repeat the confirmation. Returning to character selection
and loading a character creates a fresh observation generation.

Character names, IDs, native pointers, inventories and save paths are not logged by
the observer. The observation state machine, native hooks and twice-per-second live
sampling remain active; this change only reduces their console output.

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
