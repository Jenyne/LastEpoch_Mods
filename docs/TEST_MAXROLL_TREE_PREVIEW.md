# Maxroll passive and skill preview

Branch: `feat/maxroll-tree-preview`, based on the confirmed item preview in `feat/maxroll-build-preview`.

Close Last Epoch, then run this block from the repository in PowerShell. The script builds Release, runs tests against that exact DLL, and installs the DLL and supplied locale files only after the checks pass.

```powershell
& {
    $ErrorActionPreference = "Stop"
    git fetch origin
    if ($LASTEXITCODE -ne 0) { throw "Fetch failed" }
    git switch feat/maxroll-tree-preview
    if ($LASTEXITCODE -ne 0) { throw "Branch switch failed" }
    git pull --ff-only origin feat/maxroll-tree-preview
    if ($LASTEXITCODE -ne 0) { throw "Update failed" }
    .\scripts\Test-MaxrollPreview.ps1 -GamePath "D:\SteamLibrary\steamapps\common\Last Epoch"
}
```

On the first switch, Git normally creates a local tracking branch from `origin/feat/maxroll-tree-preview`. If a local branch already exists, the same command switches to it.

## In-game checks

Open **Items > Force Drop > Maxroll Build Preview** and load:

`https://maxroll.gg/last-epoch/planner/2ai4s0qh#1`

1. The existing equipment, idols, blessings and Weaver Items views still work. This build has no Weaver Items in either profile.
2. **Passives** shows 113 points for Endgame and Aspirational. Check node ranks, switch to **Allocation History**, and page through the ordered steps.
3. **Skills** has a **Skill Slots** overview followed by five planner trees. The current sample contains `ch4bo` (24 points), `rf1azz` (22), `ha84` (26), `bp2nk` (24), and `fl44` (26).
4. Open each skill tree, check its ranks/history, and use **Copy Tree JSON**. The clipboard should contain that tree's original `history` and `position`.
5. Switch gear variants and sections repeatedly. Selection, detail pagination and history mode should reset; the displayed trees and slots must belong to the selected variant.
6. Switch English → French → Korean → English. The new labels should update along with the existing preview. Raw planner IDs and diagnostic messages are preserved verbatim.
7. Close and reopen the preview, and load a different build. Check that equipment selection, copying JSON and cancellation still work.

Sample totals describe the build as inspected during development; a guide author can change its planner later. If your build differs, record the URL, selected variant and copied tree JSON.

## Scope

This is a read-only preview. It does not create items, equip skills, respec or allocate points. It shows Maxroll's planner tree/node IDs and raw ability IDs; native game node mapping and names are the next step.

The decoder uses the saved cursor: numeric history entries add one rank, while object entries overwrite only the ranks they name. Later history entries remain in the copied source. This history can reflect planner edits and bulk assignments, so it is not yet treated as a validated leveling order. An invalid active history reports a problem and displays no partial allocation.

The standalone inspector also includes decoded trees in `preview.json`:

```powershell
dotnet run --project LastEpoch_Maxroll -- "https://maxroll.gg/last-epoch/planner/2ai4s0qh#1" --output .\BuildImports\tree-preview
```
