param([Parameter(Mandatory=$true)][string]$LastEpochPath)
$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $LastEpochPath 'MelonLoader/net6/Mono.Cecil.dll')
$assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly(
    (Join-Path $LastEpochPath 'MelonLoader/Il2CppAssemblies/Il2CppLE.dll'))
try {
    $expected = @(
        'System.Void Il2Cpp.SpawnerPlacementManager::RollSpawners()',
        'System.Void Il2Cpp.SkillsTreesUIManager::OpenSkillTree(Il2Cpp.Ability)',
        'System.Void Il2Cpp.UITooltipItem::SetItemImage(Il2Cpp.ItemDataUnpacked,Il2Cpp.UITooltipItem/ItemTooltipInfo,System.Boolean)',
        'Il2CppLE.AssetBundles.SoftRef`1<UnityEngine.Sprite> Il2Cpp.UITooltipItem::GetItemSprite(Il2Cpp.ItemData,Il2Cpp.ItemUIContext)',
        'System.Void Il2Cpp.GroundItemVisuals::initialise(Il2Cpp.ItemDataUnpacked,System.UInt32,Il2Cpp.GroundItemLabel,Il2Cpp.GroundItemRarityVisualsV2,System.Boolean)',
        'System.Void Il2CppLE.Factions.PickupableObjectsManager::CreatePickupableObjectForPlayer(Il2CppLE.Factions.PickupableObjectType,Il2CppLE.Factions.PickupableObjectSet,UnityEngine.Vector3,System.UInt32)',
        'System.Int32 Il2Cpp.ItemData::RollLegendaryPotential(Il2Cpp.UniqueList/Entry,System.Int32,System.Int32,System.Single,System.Single,System.Boolean&,System.Single)',
        'System.Void Il2CppLE.Factions.FactionRankUIWeaver::OnPanelOpen()',
        'System.Void Il2Cpp.ConfigureTabUI::OnModalOpen(Il2Cpp.StashTabbedUIControls,System.Int32,System.Int32,System.String,System.Int32,System.Int32,Il2CppSystem.Collections.Generic.List`1<System.String>,Il2CppSystem.Collections.Generic.List`1<System.Int32>,System.Boolean,Il2Cpp.StashPriority,System.Boolean)',
        'System.Void Il2Cpp.StashTabbedUIControls::HandleConfigureTabResult(Il2CppLE.UI.PanelSystem.StashConfigureTabModalResult)',
        'System.Boolean Il2Cpp.ItemContainer::CheckSlotsOccupied(UnityEngine.Vector2Int,UnityEngine.Vector2Int,Il2Cpp.Context)',
        'System.Boolean Il2Cpp.ItemContainer::SetSlotsOccupied(UnityEngine.Vector2Int,UnityEngine.Vector2Int,System.Boolean)',
        'System.Void Il2Cpp.ItemContainer::Clear()'
    )
    $methods = @($assembly.MainModule.Types | ForEach-Object { $_.Methods } | ForEach-Object { $_.FullName })
    foreach ($signature in $expected) {
        if (@($methods | Where-Object { $_ -ceq $signature }).Count -ne 1) {
            throw "Missing or ambiguous LE 1.5 method: $signature"
        }
        Write-Output "PASS: $signature"
    }
} finally { $assembly.Dispose() }
