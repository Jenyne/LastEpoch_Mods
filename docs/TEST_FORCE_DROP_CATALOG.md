# Force Drop catalog export

Branch: `audit/force-drop-catalog`, based on upstream v4.4.21 (`2bc921dc`).
This is an opt-in diagnostic build. It inspects game definitions, not your inventory or character save. It does not create, pack, equip or drop items, or invoke corruption outcomes.

## Build and install

Close the game. From your repository in PowerShell:

```powershell
git fetch origin
git switch audit/force-drop-catalog
# If the local branch does not exist, use instead:
# git switch --track origin/audit/force-drop-catalog
git pull --ff-only origin audit/force-drop-catalog
.\scripts\Test-ForceDropCatalog.ps1
```

The script defaults to `D:\SteamLibrary\steamapps\common\Last Epoch`. For another installation, pass `-GamePath "your game folder"`.
It builds Keyboard/net6.0, runs the existing tests against your game assemblies, then backs up the installed DLL and SaveModUI.json before installing and enabling the export. A failed build or test stops installation. Locale files do not need replacement for this diagnostic.

## Capture

1. Enter a character in town and wait for the catalogs to load. A large export may briefly pause the game.
2. Check the log for `Force Drop catalog audit saved:` and its file path.
3. Collect `Mods\LastEpoch_Hud\ForceDropCatalog_*.json` plus that session's MelonLoader log.
4. Close normally so settings finish saving. `ForceDropAudit.DumpOnNextLaunch` returns to false after a successful file write. No repeated export is scheduled.

If the log reports failure, collect it; the request remains enabled for the next launch. Disable it manually if needed. To capture again, close the game and set this group in `Mods\LastEpoch_Hud\SaveModUI.json`:

```json
"ForceDropAudit": {
  "DumpOnNextLaunch": true
}
```

Keep the rest of the settings file intact. To return to your previous build, close the game and restore the DLL backup created by the script, or build/install your normal branch. The backup extension is not `.dll`, so MelonLoader will not load it as a second mod.

## What the JSON establishes

- Affix IDs, catalog sources, placement and special categories, restrictions, tiers, groups and modifiers exposed by the wrappers.
- Base item/subtype definitions and unique metadata, including special affix pools and set IDs where exposed.
- Native corruption category/equipment configurations, outcomes, weights, tier weights and replacement flags.
- Relevant method signatures and an `issues` list for unavailable members, read failures or capture limits.

The reader is bounded and uses an explicit field whitelist. `availableProperties` helps identify uncaptured structures. Null/missing/truncated data means unknown; it is not proof that a restriction is absent. This export does not yet provide a per-item legality verdict, invoke CanRollOn for every combination, or demonstrate set bonuses and persistence. Those are subsequent audit/test steps.

See [FORCE_DROP_RULES_AUDIT.md](FORCE_DROP_RULES_AUDIT.md) for current findings and the planned rule boundary.
