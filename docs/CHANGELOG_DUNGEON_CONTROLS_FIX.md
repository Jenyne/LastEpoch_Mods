# Dungeon controls and objective reveal — pending in-game verification

## Player changes

- Rebuild the dungeon controls at runtime so they also appear with older HUD bundles.
- Use the HUD's rendering layer, checkbox sprites, gold divider and compact rows.
- Keep Reveal Dungeon Objectives and Enter Without Key together in Scenes. Use the dungeon section when available, otherwise append to Minimap.
- Reveal uses the game's objective-pulse activation instead of changing a threshold modifier.
- When keyless entry is enabled, the native continue button is enabled without inserting a key. The entry screen stays open so players can insert a dungeon portal charm before choosing their tier. Removing a key also keeps free entry available.
- Save the keyless toggle immediately; retain the existing saved reveal setting.

## Developer changes

- Leave the existing settings registry and reveal save key intact. Runtime controls save directly through native click/submit hooks. The initial legacy `DungeonReveal: Bound 0/1` warning may still appear because automatic binding precedes runtime creation; this warning alone no longer determines whether the new controls work.
- Handle native Toggle pointer and submit events instead of relying on a managed listener for the runtime checkboxes.
- Wait for native Start to finish and for a dungeon manager. Request activation once per pulse component, marking it before entering native code, and clear its guard on destruction.
- Remove the score-change hook: reveal activation no longer runs inside the native score-event callback.
- Log pulse identity and request/completion separately so further crashes can be correlated with a specific activation.
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
8. Check the log for `Dungeon controls: created`, `Dungeon objective reveal: requesting pulse` / `completed request for pulse` and the enabled native entry button. Supply the logs if the native entry request still rejects a missing key.

## Verification limits

Compared the hooks and properties against the supplied game's generated IL2CPP API metadata and reviewed the failed test logs. This environment has no dotnet SDK or running game; a full build and all gameplay behaviours remain unconfirmed.

This branch starts from feat/dungeon-objective-reveal, keeping the queued test work isolated. Switching between queued branches can change unrelated features until their fixes are merged together.

## Dungeon crash follow-up

The user's build 04146990 was stable in town but crashed within about a minute inside Temporal Sanctum. Latest(3).log showed four reveal activations in two timestamp pairs, without a fatal exception stack or pulse identities. This does not establish the cause; the score callback and repeated requests are a candidate being isolated. The follow-up removes that callback and guards activation. Retest for several minutes on each floor and through floor transitions. If it still crashes, restart with Reveal Dungeon Objectives disabled while leaving keyless entry enabled, then repeat to separate the two features. Gameplay stability remains unconfirmed.

The user confirmed the guarded build no longer crashed in their test, but reveal stopped working. The follow-up removes the Default/Unlocked state gate, which was an unverified readiness assumption, while retaining the one-request guard and removal of the score callback. Native zone type/state are now logged for evidence. Reveal and continued stability need retesting together.

## Confirmed gameplay and charm follow-up

The user confirmed objective reveal and free entry across two complete runs and different floors without crashes on build 117d8540. Settings persistence remains unconfirmed. The charm follow-up changes only entry UI handling: it enables the native continue button instead of immediately calling ProceedToTierSelection. Retest entry with no key both with and without a portal charm, verify the charm's actual dungeon modifier, and verify removing a key leaves the button available. Charm handling remains pending game testing; the confirmed reveal implementation is unchanged.
