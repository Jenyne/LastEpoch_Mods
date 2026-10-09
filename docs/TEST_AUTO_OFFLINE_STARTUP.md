# Auto-offline startup test

Branch: `test/offline-guard-diagnostics`, based on `master` commit `92fb33de487acfe21b92211f620b76042aaf3400` (Syncingoutt v4.4.21 plus README clarification).

Offline operation is mandatory while this mod is installed. There is no UI or saved-configuration opt-out; online play requires uninstalling the mod. These startup patches are not protection against someone editing or replacing the DLL.

## Flow and confirmed 1.5 API

- `LandingZonePanel.OnOnEnable` postfix schedules work; it does not click during `OnEnable`.
- `Main.OnLateUpdate` waits for an active panel, the controller's `LandingZone` state, an interactable offline button, and no active loading indicator or Steam-required transition.
- Readiness must hold across at least two frames and 250 ms. It then calls the game's `OnPlayOfflineClicked()` once for that landing visit.
- The online button is hidden and `OnPlayOnlineClicked` is blocked unconditionally, including before config loading finishes.
- On offline character selection, buttons owned by `OnlineOfflineSwitch` are disabled and hidden, retaining the parent status label/icon. `OnlineOfflineSwitch.OnButtonPressed`, `CharacterSelect.SwitchOnlineOffline`, and online requests to `CharacterSelect.SetIsOnlineTabShowing` are blocked. A toggle back to offline from an already-online tab is allowed.
- A manual offline click consumes the pending automatic attempt. Leaving the panel clears its cached reference. Returning to the landing screen starts a new visit.
- The game remains responsible for mode changes, local character loading, and scene transitions. The bootstrap does not launch a character automatically, force `IsOnlinePlay`, change the login FSM, or access character save files.
- Logs report dispatch and arrival at `CharacterSelectScene` with `GameplayEnvironment.IsOnlinePlay == false`. A 30-second timeout reports lack of confirmation without dispatching another transition.

The supplied October 1 1.5 `Il2CppLE.dll` confirms these patch targets and UI members. It also exposes `AdvancePlayerToCharacterSelect(bool isPlayingOnline)` and `BypassLogin()`, but the generated interop wrappers do not establish their native prerequisites. This test deliberately uses the normal offline click handler rather than calling either lower-level method directly.

## Build and install

Use the v4.4.21 assets already installed. Close the game, fetch and switch to the test branch in a clean checkout, then run:

```powershell
git fetch origin
if ($LASTEXITCODE -ne 0) { throw 'Fetch failed' }
git switch --detach origin/test/offline-guard-diagnostics
if ($LASTEXITCODE -ne 0) { throw 'Switch failed' }
.\scripts\Test-AutoOfflineStartup.ps1 -GamePath 'D:\SteamLibrary\steamapps\common\Last Epoch'
```

The helper builds before replacing the DLL and backs up the previous DLL under `UserData\LastEpoch_Hud\test-backups`. It preserves the installed assets and configuration. The obsolete `Login.Enable_AutoLoginOffline` field is no longer read or saved. Old false or missing values cannot enable online play.

Use `-BuildOnly` to compile without installing. To revert, close the game and copy the helper's printed backup DLL over `Mods\LastEpoch_Hud.dll`.

## In-game acceptance checks

1. Launch normally with the installed mod. Expect the offline character list without manually choosing Play Offline; do not expect automatic character entry.
2. Verify `[Offline] Selecting Play Offline through the game's landing flow.` and `[Offline] Offline character selection reached.` in `MelonLoader\Latest.log`.
3. Choose a disposable offline character and enter/leave gameplay. Confirm the existing offline characters and mod features still work.
4. Return to the landing screen. Confirm one offline transition per visit and no loop or repeated dispatch logs.
5. Click Play Offline immediately before the automatic action. Confirm one transition rather than two.
6. Confirm the character screen's Switch to Online button disappears and `[Offline] Character selection online switch hidden.` is logged. Test with a controller: neither the landing online action nor the character screen mode-switch action should start online while the mod is installed. If an activation still reaches a patched handler, expect a blocked-action log and no online transition.
7. If character loading stalls, confirm the timeout warning and no repeated automatic clicks. Report the screen and log; do not treat compilation as proof of in-game success.
8. With the game closed, add the old `Login.Enable_AutoLoginOffline` field set to false in a test copy of Save.json and relaunch. Automatic offline selection and all online blocks must still apply. Repeat with the Login section missing. Do not use your real characters to test online access.

Unit tests exercise settling time, readiness interruptions, single dispatch, manual-click consumption, and a new landing visit. Patch-target checks validate that each hook resolves against the supplied game assemblies. Actual UI completion still requires an in-game test on the current installation.

Validation completed for this branch: Release build against the supplied 1.5 assemblies, all 1,011 tests with game assemblies enabled (zero failures or skips), and CSharpier formatting. The game itself and PowerShell helper were not executed in this environment.

The user's first in-game run (build `2ec2b0bb`, LE 1.5.12) confirmed automatic offline dispatch at 13:23:13.155 and offline character selection at 13:23:16.730. The new character selection button hiding and action blocks require another in-game run.

## Mandatory-offline revision

The configuration bypass and Login settings group have been removed. The observer-free predecessor `951f4e01` passed the user’s no-crash retest with two character entries. This revision still needs a native build and the acceptance checks above, especially old false/missing configuration. No gameplay observer has been reintroduced.

The user subsequently reported another successful character-switch run: entered one character, backed out and entered a second without a crash. No log or commit identifier was supplied for that run. Legacy false/missing configuration and combat/echo endurance remain unconfirmed.

## Client startup transition race — Sync retest

Sync's full Player log (2026-10-09 15:38 UTC session) records `Cannot transition to CharacterSelect while transition to SystemLoading is in progress` at 15:39:01.411. Login's scene had loaded at 15:38:59.429, but the character-selection scene finished loading at 15:39:04.596 and the client completed entry into Login at 15:39:04.682. Landing UI readiness alone was premature.

This revision additionally waits for the exact completed-state notification `ClientStateManager: Application state changed to Login.`. It receives that signal via Unity's main-thread log callback, registered during mod initialization, rather than polling native client/session objects or guessing an asynchronous-state member. The notification text is observed in both Sync's failing log and the user's successful log. The callback detaches on CharacterSelect/InGame and reattaches for a new landing visit. The existing two-frame/250 ms landing-settle rule still applies after client readiness.

Manual offline clicks are also rejected before client readiness, and duplicate requests are blocked while the first is pending. Automatic selection proceeds when startup is ready. Online requests remain unconditionally blocked.

This implementation depends on the current game's exact completion notification. A missing/changed notification keeps offline requests deferred and logs the unconfirmed client state; there is no timed bypass. A direct typed state API can replace this adapter once the current native API is verified. Current game assemblies are not available here, so a native build and in-game test are required.

Retest on Sync's HUD build and on the ordinary offline candidate:

1. Launch normally, including a slower/cold launch. Expect `[Offline] Client startup reached Login; waiting for landing readiness.` before automatic offline dispatch, then confirmed offline character selection. Check no SystemLoading transition exception in Player.log.
2. Click Play Offline early or repeatedly/use a controller. Early clicks should defer; one transition should follow readiness, without a second request.
3. Enter a character, return, enter another, then test combat and an echo. Online actions stay blocked with old false/missing config.
4. Return to the landing screen if available, and confirm a fresh ready transition without duplicate listeners. Retain both Latest.log and Player.log from the same run and the build identifier.

Regression coverage replays the observed slow startup ordering and checks that panel/scene-load messages cannot permit dispatch, that missing/unknown states remain blocked, and that a fresh listener requires a new completed-state signal.
