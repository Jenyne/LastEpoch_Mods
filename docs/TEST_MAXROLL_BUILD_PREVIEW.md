# Maxroll link retrieval and read-only preview

Branch: `feat/maxroll-build-preview`, based on upstream v4.4.21 (`2bc921dc`).

This adds a read-only **Maxroll Build Preview** window at **Items → Force Drop**, a shared game-independent reader and a .NET 8 inspection command. It captures planner data without creating items, changing skills/blessings or writing character saves. Item creation and game-catalog validation are later work.

## Build and install the HUD preview

Close Last Epoch first. From your repository in PowerShell, paste this entire block together. Fetch, switch and update failures stop the block; the install script builds and runs tests before replacing the DLL.

```powershell
& {
    $ErrorActionPreference = "Stop"
    $branch = "feat/maxroll-build-preview"
    git fetch origin
    if ($LASTEXITCODE -ne 0) { throw "Fetch failed" }
    git show-ref --verify --quiet "refs/heads/$branch"
    if ($LASTEXITCODE -eq 0) {
        git switch $branch
    } else {
        git switch --track "origin/$branch"
    }
    if ($LASTEXITCODE -ne 0) { throw "Branch switch failed" }
    git pull --ff-only origin $branch
    if ($LASTEXITCODE -ne 0) { throw "Update failed" }
    .\scripts\Test-MaxrollPreview.ps1 -GamePath "D:\SteamLibrary\steamapps\common\Last Epoch"
}
```

The script also installs the supplied English, French, Korean and Chinese locale files. If copying them by hand, move the desired `en.json`, `fr.json`, `ko.json` or `zh.json` from `LastEpoch_Hud\LastEpoch_Hud\Locales` to `Last Epoch\Mods\LastEpoch_Hud\Locales` and keep the filenames unchanged.

Open **Items → Force Drop → Maxroll Build Preview**. Paste a public Last Epoch planner URL and click **Load Build**. Previous/Next beside the gear variant name changes the selected profile or equipment embed. The four category buttons list equipment, idols, blessings and Weaver items. Empty grid positions remain visible. Click an item to inspect every original top-level field; Previous/Next under the details pages through modifiers and remaining fields.

Names come from the game's current locale where available, with IDs shown as fallbacks. Normal, sealed, primordial and corrupted modifiers are separate; corruption rows are purple. JSON tier and roll values are shown as supplied by Maxroll: rolls are normalized 0..1, not the final in-game stat values. **Copy Item JSON** copies the complete original definition, including fields too long for a row. **Build Issues** shows reader diagnostics; a clean reader result does not certify game legality.

Loading runs in the background. Cancel, Close, hiding the HUD or leaving the game cancels a pending request; a late response cannot replace another build. Loaded previews are kept while the mod remains running, not persisted across game restarts. The window creates no items and does not populate or overwrite the current Force Drop selection.

The HUD currently accepts planner links; offline clipboard JSON can be inspected through the console command below.

## Fetch and inspect

From your repository in PowerShell:

```powershell
git fetch origin
git switch feat/maxroll-build-preview
# If the local branch does not exist, use instead:
# git switch --track origin/feat/maxroll-build-preview
git pull --ff-only origin feat/maxroll-build-preview
dotnet run --project LastEpoch_Hud.Tests
.\scripts\Inspect-MaxrollBuild.ps1 "https://maxroll.gg/last-epoch/planner/zge0t60e#2"
```

Use a saved Last Epoch planner link. A guide URL must first be opened to get its planner link. The script runs the separate inspection project; no LastEpochPath is needed. Existing game-dependent tests may skip if game assemblies are unavailable.

Equivalent command, including an explicit selection:

```powershell
dotnet run --project LastEpoch_Maxroll -- "https://maxroll.gg/last-epoch/planner/zge0t60e" --variant 2 --output ".\BuildImports\Example"
```

The default is the selection encoded in the link, or the saved active variant. `#2` means the second profile when there are no embeds. A profile-name fragment is supported; when the document has embeds, numeric fragments select embed IDs. Non-equipment or unknown selections are reported, not replaced with the first profile. `--variant` is a one-based row number from the command's variant list and overrides link selection explicitly.

For a clipboard export or previously captured response:

```powershell
dotnet run --project LastEpoch_Maxroll -- --file ".\maxroll-export.json" --output ".\BuildImports\Clipboard"
```

## Output

- `response.json`: original server response or file contents, including server metadata.
- `build.json`: decoded full planner data, including unused shared definitions, notes, trees and unknown fields.
- `preview.json`: variant list and selected variant, with item references resolved to complete definitions, original slot/index positions and issues.

Equipment, idols, blessings and Weaver item placements remain separate. Empty positions and repeated references are preserved. Only the chosen variant's referenced items appear in its preview; historical/unreferenced dictionary items are not treated as equipped items. The full archive remains available in build.json.

Normal, sealed, primordial and corrupted modifiers keep their original fields. Normalized rolls stay in 0..1, tiers stay in display numbering, and ID zero remains valid. No rounding, inferred defaults, tier conversion, rarity calculation or invented LP/WW/forging-potential values are applied. Tree histories and position cursors are retained without reallocating or interpreting points. Explicit stat-value affixes and templates are captured but reported as needing game-aware conversion.

Exit code 0 means the selected planner data was parsed without capture issues, not that it is legal or constructible. Code 2 means the capture was saved with a selection or item-data issue. Code 1 means retrieval/parsing failed. Successful reruns replace the three files in the chosen output folder; use separate folders to keep earlier captures.

## Retrieval behavior

The reader accepts exact HTTPS `maxroll.gg` / `www.maxroll.gg` Last Epoch planner URLs. It builds its own public API URLs, ignores incoming query parameters and does not follow redirects or forward credentials. It first reads `planners.maxroll.gg/profiles/le/{id}`, then tries the site's `_data=last-epoch-planner-by-id` route after network/server/format failures. Authentication denials, rate limits and redirects stop immediately. Total retrieval budget is 30 seconds; response size is capped at 4 MiB and JSON depth at 64. Duplicate JSON keys and mismatched returned build IDs are rejected.

These are the site's current endpoints, not a documented stable developer API. Clipboard JSON is the offline fallback.

## Reader test at home

1. Paste a guide's planner URL and compare the reported selected variant with Maxroll.
2. Try `#1`, `#2` and a profile-name fragment; check that each gives the corresponding equipment.
3. Confirm idol positions and both rings stay distinct even when definitions are shared.
4. Check an item with sealed, primordial and corruption modifiers in preview.json; compare their IDs, tiers and exact normalized rolls.
5. Try a bad variant such as `#999`; the tool should save the archive and request explicit selection, rather than displaying another gear set.
6. Save a clipboard export and inspect it using `--file` without a network request.

Send back preview.json and any retrieval message if a guide doesn't resolve correctly. For the HUD preview, also check:

1. Load the sample above and confirm `Aspirational Gear`; switch to `Endgame Gear` and compare both ring slots and idol positions with the planner.
2. Select several unique and base items, page through their fields and copy an item JSON. Confirm that the copied rolls match the planner data.
3. Switch English → French → Korean → English while the window is open. Labels and available game item/affix names should refresh without replacing the entered link or selected gear.
4. Load a malformed link, try an unknown fragment such as `#999`, and check that errors request a valid link/explicit variant instead of silently substituting gear.
5. Cancel or close during loading, reopen the window and load another link. Check responsiveness and that late results do not appear.
6. Confirm that closing the preview returns to the unchanged Force Drop screen and no inventory or character data changed.

## Preparation checks

The reader and inspection command compiled through Roslyn. New Maxroll tests were executed by direct invocation with the project's xUnit 4.0.1 assertions; all 47 cases passed. This includes verifying that retrieval runs off the calling thread. This was used because the local dotnet CLI/MSBuild test runner fails while reading process information before tests start; the full project runner remains to be checked on Windows.

The original inferred async delegate reproduced CS0656 when compiled with a conflicting `NullableAttribute` constructor. The explicit delegate and named async worker compile successfully with that same conflict and .NET 6 references; cancellation, error handling and main-thread result publication remain covered by session tests. Windows still needs a full build and in-game preview check.

Live retrieval of `zge0t60e#2` selected Aspirational Gear, resolved 23 item placements with zero capture issues, and matched every selected original item field and empty grid position. Formatting and locale completeness/placeholder checks passed. The new UI compiled against the supplied Unity/TMP/Harmony assemblies with stand-ins for the surrounding mod context and unavailable native game catalog. Core compiled against .NET 6 reference assemblies. A full mod build remains blocked in this environment by an unreadable supplied Il2CppLE.dll; Windows build and HUD behavior require the in-game checks above. No game construction or legal-item validation is claimed by these checks.
