# Releasing

`LastEpoch_Hud\MakeRelease.ps1` builds the mod and packs `Build\Release\LastEpoch_Hud.zip` (the DLL plus the `LastEpoch_Hud` folder). The zip unzips correctly on Windows and Linux.

```powershell
.\LastEpoch_Hud\MakeRelease.ps1
```

Add `-LastEpochPath "<game folder>"` if the game is not in the default Steam folder.

To publish the GitHub release in the same step (needs the [GitHub CLI](https://cli.github.com/), logged in), commit and push first, then:

```powershell
.\LastEpoch_Hud\MakeRelease.ps1 -Publish -Tag v4.4.21 -Title "v4.4.21 HUD for Unity 6000.4.8" -NotesFile notes.md -CoreModule "<path to patched UnityEngine.CoreModule.dll>"
```

The installer ([LE-Hud-Installer](https://github.com/TriSSec-Lab/LE-Hud-Installer)) downloads `LastEpoch_Hud.zip` and `UnityEngine.CoreModule.dll` from the latest release, so every release needs both. The script refuses to publish without the DLL, with uncommitted changes, or when the tag already points at another commit.
