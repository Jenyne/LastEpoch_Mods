# Last Epoch HUD

Updated build of [Ash's Last Epoch HUD](https://github.com/RCInet/LastEpoch_Mods) for the current game (Unity 6000.4.8) and MelonLoader 0.7.3 Open-Beta.

In a zone, press **F3** to open and close the mod menu. Escape closes it.

## Install

1. Install [MelonLoader 0.7.3 Open-Beta](https://github.com/LavaGang/MelonLoader/releases). Point the installer at the game folder.
2. Start the game once, wait until the main menu, then close it.
3. From the [latest release](https://github.com/Syncingoutt/LastEpoch_Mods/releases), download `LastEpoch_Hud.dll` and the HUD asset `lastepochmods`. Download `UnityEngine.CoreModule.dll` too if the release requires it.
4. Put `LastEpoch_Hud.dll` directly in the game `Mods` folder. Create `Mods/LastEpoch_Hud/Assets` if needed and put `lastepochmods` there. You should have:

   ```text
   <Last Epoch>\Mods\LastEpoch_Hud.dll
   <Last Epoch>\Mods\LastEpoch_Hud\Assets\lastepochmods
   ```

   The DLL stays next to the `LastEpoch_Hud` folder, not inside it.

5. If required by the release, replace the downloaded `UnityEngine.CoreModule.dll` in this folder:

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

## Optional translations

Locale JSON files are supplied as individual release downloads. To use translated HUD labels, download the desired language file and `en.json` for English fallback, then place them manually in:

```text
<Last Epoch>/Mods/LastEpoch_Hud/Locales/
```

Create the folder if needed. Supported files are `en.json` (English), `fr.json` (French), `ko.json` (Korean), and `zh.json` (Simplified Chinese). Keep the filenames unchanged. `base.json` is a template for translation authors.

The HUD follows the language selected in the game. Without a matching translation, it uses English fallback. Close the game before replacing locale files; restart afterward. When updating the mod, download and replace the locale files you use to get the latest labels. Back up any custom translations first.

## Release files and updates

Release builds copy the HUD asset and locale JSON files into `Build/Release/net6.0/LastEpoch_Hud/`, alongside `Build/Release/net6.0/LastEpoch_Hud.dll`. No ZIP is generated.

For the next release, attach `LastEpoch_Hud.dll`, `lastepochmods`, and each locale JSON as separate assets from the same build. Users updating an existing install replace the DLL in `Mods`; those who use translations replace their JSON files manually in `Mods/LastEpoch_Hud/Locales`. Replace the HUD asset if it changed.

Preparing these files does not publish a GitHub release automatically.
