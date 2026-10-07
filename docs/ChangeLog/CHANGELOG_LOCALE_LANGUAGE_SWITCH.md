# HUD language switching fix

Branch: `fix/locale-language-switch`
Base: Syncingoutt/master at `a7885bb7` (includes merged completed fixes and custom-item refactor).

- Keep canonical English keys for legacy prefab labels, matching the newer runtime controls.
- Reapply labels from those keys when switching languages instead of looking up already translated text.
- Refresh all HUD descendants, including inactive tabs and TMP labels, without the previous eight-level traversal limit.
- Preserve dynamic numeric values and native item names by registering only known static locale keys or explicitly registered runtime labels.
- Add the missing Safe Teleport description translation in English, French, Korean and Chinese.

## Test

1. Build/install this branch and manually copy its updated en/fr/ko/zh JSON files into `Mods/LastEpoch_Hud/Locales`.
2. Restart once so no captions remain translated by the previous DLL.
3. Switch English → French → English → Korean → English several times without restarting.
4. Check menu buttons, Camera, Monoliths, Minimap, Dungeons, Misc/Safe Teleport, Character, crafting and Force Drop, including tabs that were hidden during the change.
5. Confirm slider numbers, input values, keybindings and native item names remain correct.
6. Rerun `dotnet run --project LastEpoch_Hud.Tests` with LAST_EPOCH_PATH set.

Local validation: whitespace, JSON and translation-key coverage checked. The .NET runtime is unavailable in the current execution environment; compilation, formatting tooling and gameplay switching still need Windows verification.
