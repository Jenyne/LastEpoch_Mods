# Maxroll graphical build preview

Branch: `feat/maxroll-tree-preview`. This replaces the initial tree ID lists with a larger graphical preview. The allocation decoder remains read-only.

Close Last Epoch, then run from the repository in PowerShell:

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

The script builds Release, runs tests against that exact DLL, and installs the DLL and locales only after checks pass.

## In-game checks

Open **Items > Force Drop > Maxroll Build Preview** and load:

`https://maxroll.gg/last-epoch/planner/2ai4s0qh#1`

1. The preview opens as a larger window with the six section buttons across the top. The equipment, idols, blessings and Weaver Items views still work. This build has no Weaver Items in either profile.
2. **Passives** has named buttons for **Acolyte**, **Necromancer**, **Lich**, and **Warlock**. Their current allocated totals are 20, 0, 83 and 10, respectively: 113 overall. Each view shows only its own nodes, in the planner's positions.
3. **Skills** has a **Skill Slots** overview and named tree buttons in specialization order: **Flay** (26 points), **Reaper Form** (22), **Chaos Bolts** (24), **Marrow Shards** (24), and **Harvest** (26).
4. Select each skill/mastery. The graph shows prerequisite connections and current ranks out of the node's maximum, with allocated nodes highlighted in gold. No planner IDs are used as normal node or skill labels.
5. Drag empty graph space to pan, use **− / +** to zoom, and **Fit** to see the whole layout. At smaller zoom levels, cards show initials or ranks to avoid unreadable names. Hovering/clicking still gives the full name and description in the details pane on the right.
6. Hover a different node, move out of the graph, then click a node. Hover previews should restore the selected node when the pointer leaves. Scrolling the details pane should reveal longer text. Base node stat values are labeled separately; this preview does not calculate your character's resulting stats.
7. Switch variants and sections repeatedly. The displayed trees and skills must belong to the selected variant; node selection and zoom reset between views. An unallocated mastery should be readable with all ranks at zero.
8. Check **Skill Slots**. Both skill bars use readable names and retain the source's ordering/empty slots. Trees are ordered by the specialization bar, not by arbitrary tree IDs.
9. Switch English → French → Korean → English. Control labels should update. Node/skill names and descriptions come from Maxroll's English catalog, as in its planner.
10. Close/reopen the window and load another build. The item views, JSON copying, cancellation and normal HUD close behavior should still work. Check that node hover redraws do not produce errors or frame spikes.

The first load also retrieves Maxroll's public tree catalog; later loads reuse it for that game session. If it cannot be retrieved, imported gear remains available and the tree view explains that names/layout could not be loaded. Reloading retries. Unknown tree/node IDs are never mapped to a similarly named node; incomplete matching is reported.

**Build Issues** opens the diagnostic view, where original IDs may still appear for troubleshooting and **Copy Tree JSON** remains available. Normal graphical browsing does not require them.

## Scope and data source

Names, positions, node caps and connections come from the public JSON resource used by Maxroll's Last Epoch planner:

`https://assets-ng.maxroll.gg/leplanner/game/data.json`

Only display metadata is read; no planner JavaScript is executed in the game. Node cards use text and the mod's gold styling rather than copied planner artwork. The view preserves Maxroll's spatial layout and separates overlapping passive mastery layouts.

This does not create/equip items, equip skills, respec or allocate points. Native game node mapping is still required for any future automatic allocation. Full histories and unknown fields remain preserved, including future entries after the saved cursor. A history with an invalid active step displays no partial allocation.

Sample totals describe the build inspected during development. Guide authors and Maxroll can update their data later; include the URL and selected variant with any report.
