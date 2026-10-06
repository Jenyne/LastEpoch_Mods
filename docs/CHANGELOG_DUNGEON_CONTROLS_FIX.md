# Dungeon controls and objective reveal — pending in-game verification

## Player changes

- Rebuild the dungeon controls at runtime so they also appear with older HUD bundles.
- Use the HUD's rendering layer, checkbox sprites, gold divider and compact rows.
- Keep Reveal Dungeon Objectives and Enter Without Key together in Scenes. Use the dungeon section when available, otherwise append to Minimap.
- Reveal uses the game's objective-pulse activation instead of changing a threshold modifier.
- When keyless entry is enabled, opening the dungeon entry panel requests the game's normal tier-selection flow without waiting for a key-insertion event. Already visible tier selection is left alone.
- Save the keyless toggle immediately; retain the existing saved reveal setting.

## Developer changes

- Separate persisted reveal settings from the runtime UI group, removing the automatic binding attempt before control creation.
- Handle native Toggle pointer and submit events instead of relying on a managed listener for the runtime checkboxes.
- Wait for pulse initialization, restrict activation to components with a dungeon manager, and clear the reference on destruction.
- Leave the native pulse's activated state intact. Disabling the option prevents subsequent automatic reveals; it does not hide a floor already revealed.
- Preserve native dungeon tier unlocking and entry processing. No fabricated key, tier unlock, objective completion or reward is added.

## Required tests

1. Build this branch, copy Build/Release/net6.0/LastEpoch_Hud.dll into the game's Mods folder, then launch offline.
2. In Scenes, find both dungeon checkboxes. Check rendering, click targets, alignment and absence of overlap. Scroll the center panel if needed.
3. Toggle each option and restart the game. Verify both saved states.
4. With reveal off, enter a dungeon and verify normal reveal timing. Enable it while on the floor: the native objective pulse should appear without killing additional enemies. It must not open doors or complete the dungeon.
5. With reveal on, enter the next floor and another dungeon. Verify the new floor reveals automatically. Disable it before another floor and verify normal behaviour returns. Check monolith reveal behaviour is unchanged.
6. With keyless off and no inserted key, verify normal entry restriction. Enable keyless before opening the entry panel; verify tier selection, then enter Temporal Sanctum, Soulfire Bastion and Lightless Arbor without inserting a key. Confirm actual floor transition, not just enabled UI. Repeat normal entry with the option off and an actual key.
7. Record whether a key is consumed when keyless entry is on and a real key is inserted. The native consumption path has not been established from the supplied wrapper metadata; do not treat preservation of inserted keys as confirmed.
8. Check the log for `Dungeon controls: created`, `Dungeon objective reveal: activated native pulse` and `Keyless dungeon entry: requested native tier selection`. Supply the logs if the native entry request still rejects a missing key.

## Verification limits

Compared the hooks and properties against the supplied game's generated IL2CPP API metadata and reviewed the failed test logs. This environment has no dotnet SDK or running game; a full build and all gameplay behaviours remain unconfirmed.

This branch starts from feat/dungeon-objective-reveal, keeping the queued test work isolated. Switching between queued branches can change unrelated features until their fixes are merged together.
