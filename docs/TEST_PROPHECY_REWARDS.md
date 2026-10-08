# Prophecy reward multiplier retry

Branch: `fix/prophecy-reward-trigger`, based on current master `92fb33de`.
This is an isolated test build; other queued feature branches are not included.

## Install

Close the game, then run this entire block from your checkout:

```powershell
& {
    $ErrorActionPreference = 'Stop'
    Set-Location 'D:\GitHub\LastEpoch_Mods'
    git fetch origin '+refs/heads/fix/prophecy-reward-trigger:refs/remotes/origin/fix/prophecy-reward-trigger'
    if ($LASTEXITCODE -ne 0) { throw 'Fetch failed' }
    git switch --detach origin/fix/prophecy-reward-trigger
    if ($LASTEXITCODE -ne 0) { throw 'Switch failed' }
    .\scripts\Test-ProphecyRewards.ps1 -GamePath 'D:\SteamLibrary\steamapps\common\Last Epoch'
}
```

The script rebuilds, validates that exact DLL against the installed game assemblies,
then copies the DLL and locale files. Failed builds/tests leave the installation
alone. Existing installed files are backed up, and copy failures restore them.

## In-game checks

Use **Character → Cheats → Prophecy Reward Multiplier**. Default off; whole numbers x1–x10.

1. With it off, trigger a prophecy and note its normal reward quantity and charges consumed.
2. Start with x2 on a small equipment reward. Confirm no crash and twice the native quantity.
3. Disable it and trigger another reward. Confirm quantities return to normal immediately.
4. Test x5 and x10 after x2 succeeds; test equipment, idols, materials and a boss-specific reward.
5. Test a lens and the native doubled-reward bonus. Quantity should be multiplied on top of normal rewards; charges, target progress and lens behavior should stay native.
6. Repeat a reward and change scenes. The temporary count must not accumulate across triggers.
7. Restart and confirm the toggle/value persist while reward quantities remain correct.

Keep `MelonLoader\Latest.log` and `Player.log` if it crashes, together with which
reward/lens, multiplier and native quantity you used. With Debug enabled in
SaveModUI.json, the log records the temporary trigger count override.

## Changes and limits

- Restores the off-by-default quantity multiplier without patching or invoking
  `SpawnRewardForPlayer` from mod code.
- The previous implementations shared that hook. Its nullable lens parameter is
  a plausible interop crash cause, matching an upstream nullable-parameter report:
  https://github.com/BepInEx/BepInEx/issues/566 . The old retained loader log has
  no fatal stack, so this is a diagnosis to test, not a proven root cause.
- Temporarily overrides the selected reward asset's base quantity around the
  local player's normal `ProphecySlot.TryTriggerReward` call. Restores it on
  success, rejected trigger or managed exception; nested use of the same asset
  cannot multiply it again. No repeated fulfillment/charge consumption calls.
- Verifies the native target signature (enum, bool, Actor, out int). The nullable
  lens call remains native. Arithmetic is clamped to x1–x10; overflow is skipped.
- Reuses the existing Favor Multiplier row style and retains the previous save keys.
- Native compilation and automated tests cannot establish runtime crash safety
  or counts for every reward route. These remain pending the in-game checks above.

## Local validation

- Full mod compiled with the .NET 6 reference pack against the supplied MelonLoader/game assemblies.
- 1,022 tests passed, zero failed or skipped, including patch targets and locale checks.
- Installer success, build failure, test failure, game-running rejection, and copy rollback checked with mocked native commands and the real locale files.
- Gameplay quantity/crash/charge/persistence checks remain pending.
