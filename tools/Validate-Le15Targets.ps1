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
        'Il2CppLE.AssetBundles.SoftRef`1<UnityEngine.Sprite> Il2Cpp.UITooltipItem::GetItemSprite(Il2Cpp.ItemData,Il2Cpp.ItemUIContext)'
    )
    $methods = @($assembly.MainModule.Types | ForEach-Object { $_.Methods } | ForEach-Object { $_.FullName })
    foreach ($signature in $expected) {
        if (@($methods | Where-Object { $_ -ceq $signature }).Count -ne 1) {
            throw "Missing or ambiguous LE 1.5 method: $signature"
        }
        Write-Output "PASS: $signature"
    }
} finally { $assembly.Dispose() }
