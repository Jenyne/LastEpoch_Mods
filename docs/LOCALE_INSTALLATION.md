# Locale installation / Installation des traductions / 언어 파일 설치 / 语言文件安装

## English

1. Close Last Epoch.
2. Download `en.json` and the language file you want:
   - `fr.json` — French
   - `ko.json` — Korean
   - `zh.json` — Simplified Chinese
   For English, only `en.json` is needed.
3. Open your Last Epoch installation folder and place the downloaded files here:

   ```text
   Last Epoch\Mods\LastEpoch_Hud\Locales\
   ```

   Create the `LastEpoch_Hud` and `Locales` folders if needed. Keep the filenames unchanged. For example, Korean uses:

   ```text
   Last Epoch\Mods\LastEpoch_Hud\Locales\en.json
   Last Epoch\Mods\LastEpoch_Hud\Locales\ko.json
   ```

4. Start the game and select your language under **Settings → Gameplay → Interface → Language**. The mod HUD follows that setting.

Keep `en.json` alongside your chosen language for English fallback. When updating translations, close the game and replace the old JSON files with the new ones. Back up any custom translations first. `base.json` is a template for translation authors and is not needed for installation.

## Français

1. Fermez Last Epoch.
2. Téléchargez `en.json` et `fr.json`.
3. Ouvrez le dossier d'installation de Last Epoch et placez les deux fichiers ici :

   ```text
   Last Epoch\Mods\LastEpoch_Hud\Locales\
   ```

   Créez les dossiers `LastEpoch_Hud` et `Locales` si nécessaire. Ne renommez pas les fichiers. Vous devez obtenir :

   ```text
   Last Epoch\Mods\LastEpoch_Hud\Locales\en.json
   Last Epoch\Mods\LastEpoch_Hud\Locales\fr.json
   ```

4. Lancez le jeu et choisissez le français dans **Paramètres → Jouabilité → Interface → Langue**. Le menu du mod suit la langue du jeu.

Conservez `en.json` avec `fr.json` : il fournit le texte anglais lorsqu'une traduction manque. Pour mettre à jour les traductions, fermez le jeu et remplacez les anciens fichiers JSON par les nouveaux. Sauvegardez d'abord vos traductions personnalisées. `base.json` est un modèle destiné aux traducteurs ; il n'est pas nécessaire pour l'installation.

## 한국어

1. Last Epoch를 종료하세요.
2. `en.json`과 `ko.json`을 다운로드하세요.
3. Last Epoch 설치 폴더를 열고 두 파일을 다음 위치에 넣으세요.

   ```text
   Last Epoch\Mods\LastEpoch_Hud\Locales\
   ```

   `LastEpoch_Hud` 또는 `Locales` 폴더가 없으면 직접 만드세요. 파일 이름은 변경하지 마세요. 설치 후 파일 위치는 다음과 같아야 합니다.

   ```text
   Last Epoch\Mods\LastEpoch_Hud\Locales\en.json
   Last Epoch\Mods\LastEpoch_Hud\Locales\ko.json
   ```

4. 게임을 실행하고 **설정 → 게임플레이 → 인터페이스 → 언어**에서 한국어를 선택하세요. 모드 메뉴는 게임에서 선택한 언어를 따릅니다.

번역이 없는 문구를 영어로 표시할 수 있도록 `en.json`도 함께 넣어 주세요. 번역 파일을 업데이트할 때는 게임을 종료한 뒤 기존 JSON 파일을 새 파일로 덮어쓰세요. 직접 수정한 번역은 먼저 백업하세요. `base.json`은 번역 제작용 템플릿이며 설치에 필요하지 않습니다.

## 简体中文

1. 关闭 Last Epoch。
2. 下载 `en.json` 和 `zh.json`。
3. 打开 Last Epoch 的安装目录，将这两个文件放入以下文件夹：

   ```text
   Last Epoch\Mods\LastEpoch_Hud\Locales\
   ```

   如果没有 `LastEpoch_Hud` 或 `Locales` 文件夹，请手动创建。不要更改文件名。安装后的文件位置应为：

   ```text
   Last Epoch\Mods\LastEpoch_Hud\Locales\en.json
   Last Epoch\Mods\LastEpoch_Hud\Locales\zh.json
   ```

4. 启动游戏，在 **设置 → 游戏玩法 → 界面 → 语言**中选择简体中文。模组菜单会使用游戏中选择的语言。

请保留 `en.json`，以便缺少翻译时显示英文。更新翻译文件时，请先关闭游戏，再用新的 JSON 文件替换旧文件。如果您自行修改过翻译，请先备份。`base.json` 是供翻译作者使用的模板，安装时不需要。
