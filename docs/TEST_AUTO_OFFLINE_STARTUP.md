# Manual offline-only startup test

Branch: `test/offline-guard-diagnostics`. The legacy helper filename is retained for existing scripts.

## Current behavior

The user clicks the normal Play Offline button. No mod patch intercepts that offline click; no automatic click, startup readiness listener, fixed delay or scene transition is added. The game handles readiness and local character selection.

Offline operation remains mandatory while the mod is installed: the online landing button and character-selection switch are hidden and their online actions are blocked. There is no UI/configuration opt-out, and the obsolete `Login.Enable_AutoLoginOffline` setting is not read. Online play requires uninstalling the mod.

The prior `9b888a1b` test failed: Latest.log shows `Client state: not confirmed`, no completed-state callback, and a deferred manual click. That listener/gate is removed together with all auto-selection code and its now-unused core helpers/tests. The earlier gameplay/session observer remains removed.

## Build/install

Close the game and use:

```powershell
git fetch origin
if ($LASTEXITCODE -ne 0) { throw 'Fetch failed' }
git switch --detach origin/test/offline-guard-diagnostics
if ($LASTEXITCODE -ne 0) { throw 'Switch failed' }
.\scripts\Test-AutoOfflineStartup.ps1 -GamePath 'D:\SteamLibrary\steamapps\common\Last Epoch'
```

The helper builds first, backs up the installed DLL and preserves assets/configuration. `-BuildOnly` compiles without installing.

## Acceptance checks

1. Launch normally. Expect `[Offline] Click Play Offline to continue. Online actions are blocked.` The game must wait for your click; no auto-selection or deferred-startup messages should appear.
2. Click Play Offline when the game enables it. Confirm the local character list opens. Test normal mouse/controller input and a cold launch.
3. Enter one character, return and enter another. Test a zone change, combat and an echo. Keep Latest.log and Player.log from the same run if anything stalls.
4. Confirm the landing online button and character-selection online switch stay hidden/blocked. Old false/missing Login configuration must not enable online actions.
5. Return to the landing screen if available and repeat manual selection. Confirm that only the game's native button flow runs.

Native compilation and runtime confirmation are pending for this revision. Core checks do not establish native UI behavior. Sync should apply this change to his HUD build too; the standalone test branch does not include that redesign.
