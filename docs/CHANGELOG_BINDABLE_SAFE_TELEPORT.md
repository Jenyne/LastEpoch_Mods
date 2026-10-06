# Bindable Safe Teleport

## Player changes
- Restores the disabled Safe Teleport feature with an enable switch and a configurable keyboard key or gamepad button in Character → Cheats.
- Disabled and unbound by default; no hardcoded Ctrl+Q shortcut.
- Returns to the End of Time through the existing scene transition service. Its waypoint must already be unlocked on the character.
- Clear removes the binding. Enable state and binding are saved in Mods/LastEpoch_Hud/SaveModUI.json.
- One request per press, with a ten-second retry delay. Holding the button does not repeatedly teleport.
- Ignores input while the mod menu or pause menu is open, while capturing a binding, while a selected text field is focused, and while the application is unfocused.
- New labels are supplied in English, French, Korean, and Chinese locale files.

## Developer changes
- Uses the existing ModUI settings, keybind capture, keyboard/gamepad matching, and localization registry.
- Builds controls at runtime so existing HUD asset bundles can display the new options.
- Tracks input edges even while blocked and suppresses the press that changes or enables a binding.
- Removes the temporary saved God Mode mutation: restoring it immediately did not cover the asynchronous scene transition. Safe Teleport is a return shortcut, without an invulnerability guarantee.
- Does not unlock End of Time as a side effect.

## Validation
- C# syntax and locale JSON checks completed.
- Full compilation and gameplay cannot be run in this environment. Pending in-game checks: control layout, rebinding and Clear, keyboard/gamepad travel, typing/menu suppression, and save/restart persistence.
