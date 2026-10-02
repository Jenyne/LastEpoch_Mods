# LE 1.5 migration: uploaded 4.4.11 base

The original compat15-experimental commit a25ddaae2763fac7f031d4ce77f52ddf474f7c7b
is preserved in backup/compat15-experimental-before-4.4.11-20261002.
The uploaded source was imported in a separate commit, without rewriting history.
master is not part of this migration.

Source archive: LastEpoch_Mods-4.4.11.zip
SHA256: 1592760C083FF6F8EF1FC1DC3E97B148315C3DDCB4C1EC4673CACC4A83197247
The archive still reports mod version 4.4.7 and contains both old NewItems and
new Items sources. Its filename is not a verified upstream release version.

## Targeted changes

- Mob density explicitly patches the parameterless RollSpawners overload.
- Skill levels are updated before SkillsTreesUIManager.OpenSkillTree(Ability),
  replacing the removed SkillsPanelManager.openSkillTree entry point.
- Temporalis no longer patches GetItemSprite with an incompatible Sprite result.
  That method returns SoftRef<Sprite>; the generated icon has no asset GUID.
  A SetItemImage postfix clears the image soft reference and assigns the runtime
  icon on the correct normal/comparison side. Ordinary items retain game behavior.
- Ten localization attributes in the relocated Items sources explicitly name
  Il2Cpp.Localization to avoid the mod's Localization namespace collision.

## Validation on 2026-10-02

Inspected the installed MelonLoader Il2Cpp assemblies in the game directory used
by the supplied runtime log. All four API signature assertions pass:

```powershell
./tools/Validate-Le15Targets.ps1 -LastEpochPath 'D:\SteamLibrary\steamapps\common\Last Epoch'
```

A normal net6.0 Release build still fails with 174 errors in unrelated legacy
sources, including NewItems, DamageMeter, Craft_MaxTier, Maxroll import,
summon modules, TimeBeast, teleport, and TwoHandedShield. The initial ten
localization errors masked these further errors; the archive is not build-ready.

A restricted net6.0 compiler validation passes with zero errors and one NU1900
package-audit connectivity warning. It excludes the legacy failures and their
dependent localization sources using tools/Le15-ValidationOnly.targets:

```powershell
$targets = (Resolve-Path ./tools/Le15-ValidationOnly.targets).Path
dotnet build ./LastEpoch_Hud/LastEpoch_Hud.csproj -c Release '-p:LastEpochPath=D:\SteamLibrary\steamapps\common\Last Epoch' "-p:DirectoryBuildTargetsPath=$targets"
```

This checks the changed source against real game types, not just stubs. The
exclusions are opt-in for validation only and do not change the normal project.
The resulting DLL is not a complete release and is not supplied for installation.
No in-game runtime validation has been performed.

## Remaining checks

Resolve the archive's legacy-source/build composition before creating a playable
build. Then test density off/on, skill levels off/on when opening a specialised
tree, and normal/comparison Temporalis tooltips followed by ordinary item tooltips.
Check for late icon loads replacing the custom image. Temporalis component
initialization is commented out in the donor Mods_Manager; this patch does not
enable that unfinished feature. Its icon must be initialized to test rendering.
## Second runtime repair batch

Quad Stash now leaves ordinary stash occupancy to the game. Its quad-only cache
is keyed by container identity, rebuilt from existing contents when absent, and
cleared on container reset. Slot updates return success and reject out-of-range
coordinates. New-tab resizing only applies to actual stash containers. Tab UI
refresh runs after native tab selection. Configuration uses OnModalOpen and
HandleConfigureTabResult, saves after the game processes the result, and refuses
size changes on nonempty tabs. Existing quad status survives a rename even when
an unsafe size change is rejected. This remains the donor's name-based settings
format; duplicate tab names are not independently configurable.

Memory Amber patches now target the typed pickup-set overload and only affect
MemoryAmber for the local player. Multiplication saturates at uint.MaxValue.
Auto-pickup records the next ID before creation and collects only that newly
created pickup, without changing the collection during enumeration.

Minimap initialization includes GroundItemRarityVisualsV2. Legendary Potential
uses RollLegendaryPotential's current signature and a postfix so the game's
out-parameter and side effects execute. The configured range is inclusive and
clamped to 0..4. Woven points refresh after OnPanelOpen.

Headhunter's optional unfinished page is looked up without throwing missing-child
errors; its button is only wired when both button and content exist. Missing
content hides the button. This does not create the absent page or replace assets.

Validation: restricted net6.0 build passes (0 errors, 1 NU1900 audit connectivity
warning); 13 game API signature checks pass; all 16 compiled runtime patch targets
resolve uniquely with matching injected argument/result types. The 18 managed
occupancy regression checks pass. Tests use the real hook bodies with small
managed stand-ins and do not exercise native persistence or UI.

```powershell
./tools/Test-QuadStashSlots.ps1
./tools/Validate-RuntimePatchBindings.ps1 -LastEpochPath 'D:\SteamLibrary\steamapps\common\Last Epoch' -ModAssembly ./Build/Release/net6.0/LastEpoch_Hud.dll -HarmonyDirectory "$env:USERPROFILE/.nuget/packages/lib.harmony/2.3.1.1/lib/net6.0"
```

Still required: a complete build and an in-game test of normal/quad tab switching,
moving/removing/sorting items, tab configuration/renaming, rejection of resizing
nonempty tabs, and save/reload. Verify modal child layout on the actual game UI.
Test Memory Amber with multiplier and auto-pickup separately and together; other
pickup types should remain unchanged. Test minimap cleanup on pickup, LP bounds,
and Woven panel reopen. These fixes have not been installed into the game.
