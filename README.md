# Last Epoch HUD

Updated build of [Ash's Last Epoch HUD](https://github.com/RCInet/LastEpoch_Mods) for the current game (Unity 6000.4.8) and MelonLoader 0.7.3 Open-Beta.

In a zone, press **F3** to open and close the mod menu. Escape closes it.

## Install

1. Install [MelonLoader 0.7.3 Open-Beta](https://github.com/LavaGang/MelonLoader/releases). Point the installer at the game folder.
2. Start the game once, wait until the main menu, then close it.
3. From the [latest release](https://github.com/Syncingoutt/LastEpoch_Mods/releases), download `LastEpoch_Hud.zip` and `UnityEngine.CoreModule.dll`.
4. Extract the entire `LastEpoch_Hud.zip` into the game `Mods` folder. The ZIP includes both the DLL and the `LastEpoch_Hud` folder. You should have:

   ```text
   <Last Epoch>\Mods\LastEpoch_Hud.dll
   <Last Epoch>\Mods\LastEpoch_Hud
   ```

   The DLL stays next to the `LastEpoch_Hud` folder, not inside it.

5. Replace the downloaded `UnityEngine.CoreModule.dll` in this folder:

   ```text
   <Last Epoch>\MelonLoader\Il2CppAssemblies\
   ```

## In case of issues

1. Delete mods, delete melonloader
2. Head here: https://melon-loader.com/ (or github) and download MelonLoader 0.7.3
3. Launch the game once
4. Head into https://github.com/Syncingoutt/LastEpoch_Mods/releases/tag/v4.4.12 and download the zip and the .dll file
5. Add the LastEpoch_Hud.dll and folder into the Mods folder
6. Replace the UnityEngine.CoreModule.dll inside MelonLoader\Il2CppAssemblies
7. Head into the game and check if it works keybind F3

In case this does not work, please contact me on Discord: sync0333 (attach a log file from MelonLoader/latest.log)

## Linux

1. Check if your LastEpoch_Hud folder looks like this:
<img width="927" height="208" alt="image" src="https://github.com/user-attachments/assets/dd848129-0927-47c9-b330-5fd5af2b7992" />

2. If that is the case, it got unzipped badly, simply create 2 new folders (Locales | Assets)
3. Rename the files (e.g LastEpoch_Hud\Locales\base.json -> base.json)
4. Move the new files into the respective folders:
* Locales - base.json | en.json | fr.json | ko.json | zh.json
* Assets - lastepochmods
5. Head into the game and see if it works (F3)


## Build

The project references assemblies from your Last Epoch install. If the game is not in the default Steam folder:

```powershell
dotnet build .\LastEpoch_Hud\LastEpoch_Hud.csproj -c Release -p:LastEpochPath="D:\SteamLibrary\steamapps\common\Last Epoch"
```

Every Release build also generates `Build/Release/net6.0/LastEpoch_Hud.zip`. Upload this generated ZIP as the release asset; it includes `LastEpoch_Hud.dll`, the HUD asset and all locale JSON files from the same checkout. The package is refreshed even when only a translation changes. Missing required locales or the HUD asset fail packaging instead of producing an incomplete release.

## Updating

Close the game before installing an update. Extract the **entire** new `LastEpoch_Hud.zip` into the game's `Mods` folder and allow it to replace the existing DLL, HUD asset and locale files. Replacing only the DLL leaves old translations installed.

The ZIP contains only the mod DLL, HUD asset and locales; it does not contain saved mod settings or character data. Custom translations of shipped locale files should be backed up before replacing them.

For release maintainers: attach the generated ZIP to the next GitHub release after the changes are merged. Building the ZIP does not publish a release or update anyone's installed files automatically.
