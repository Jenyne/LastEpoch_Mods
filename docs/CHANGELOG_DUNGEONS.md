# Dungeon objective reveal and keyless entry

## Player changes

- Reveal dungeon objectives automatically on new floors, or enable reveal while already on a floor.
- Enter dungeons without inserting a key when Enter Without Key is enabled.
- Keep the entry screen available for an optional dungeon portal charm before continuing to tier selection.
- Removing an inserted key keeps the continue button available when free entry is enabled.
- Use compact dungeon controls with the existing HUD checkbox sprites, gold divider and rendering layer.

Revealed objectives remain visible for the current floor after disabling the option. The option does not complete objectives, open dungeon exits or unlock higher dungeon tiers.

## Developer changes

- Apply the isolated dungeon changes to upstream master at 35c6a14e, preserving the HUD partial-file refactor, CI configuration and release packaging.
- Request native objective-pulse activation once per component, marking the request before calling native code and clearing the guard on destruction.
- Activate after native Start finishes; do not invoke activation from the score-change callback.
- Preserve the existing DungeonReveal.Enabled save key and legacy keyless setting.
- Enable the native entry button instead of bypassing charm selection.

## Validation

The earlier isolated implementation was confirmed in game across two complete runs and different floors, with reveal, free entry and no crashes. Optional charm entry was subsequently confirmed working. Settings persistence has not been separately confirmed.

The integrated branch needs a brief in-game regression check: reveal on multiple floors, keyless entry with and without a charm, the charm's modifier, and a few minutes of dungeon play without crashes.

Use the existing MakeRelease.ps1 and docs/RELEASING.md for publishing. This change does not alter the installer asset names or packaging.

CSharpier 1.3.0 formatting and check passed for all 156 C# files. The normal build and test suite could not run here because MSBuild failed during process-information initialization. A direct compiler attempt was also blocked by the local Il2CppLE.dll being unreadable as managed metadata. These are verification limitations; a normal Windows build, `dotnet run --project LastEpoch_Hud.Tests`, and the integration gameplay check remain required.

## Scenes layout follow-up

- Replace the oversized Teleport/Minimap arrangement with compact Dungeons, Misc and Minimap panels.
- Keep dungeon reveal and keyless entry in Dungeons; move Safe Teleport to Misc.
- Minimap contains only Max Zoom Out and Remove Fog of War.
- Reuse native gold panel headers and borders, with translated section labels.
- Preserve saved settings and keybinds. This layout follow-up needs in-game verification.

- Correct panel positioning to target the outer scroll panels, keep inner content at the top, and provide the standard Misc/Viewport/Content hierarchy for Safe Teleport binding. Hide unused scrollbars in the compact panels.
