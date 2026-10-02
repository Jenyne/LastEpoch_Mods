param([Parameter(Mandatory=$true)][string]$LastEpochPath,
      [Parameter(Mandatory=$true)][string]$ModAssembly,
      [Parameter(Mandatory=$true)][string]$HarmonyDirectory)
$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $LastEpochPath 'MelonLoader/net6/Mono.Cecil.dll')
$game = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Join-Path $LastEpochPath 'MelonLoader/Il2CppAssemblies/Il2CppLE.dll'))
$resolver = [Mono.Cecil.DefaultAssemblyResolver]::new()
$resolver.AddSearchDirectory((Resolve-Path $HarmonyDirectory).Path)
$reader = [Mono.Cecil.ReaderParameters]::new()
$reader.AssemblyResolver = $resolver
$mod = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Resolve-Path $ModAssembly).Path, $reader)
function AllTypes($types) { foreach($type in $types) { $type; AllTypes $type.NestedTypes } }
try {
    $count = 0
    $types = AllTypes $mod.MainModule.Types
    foreach ($type in $types | Where-Object FullName -match '/|\+') {
        if ($type.FullName -notmatch 'Bank_Quad/|Character_MemoryAmber_Multiplier/|Items_AutoPickup_MemoryAmber/|Items_Drop_LegendaryPotencial/|Faction_Woven_TreePoints/|Minimap_Icons/') { continue }
        foreach ($attr in $type.CustomAttributes | Where-Object { $_.AttributeType.FullName -eq 'HarmonyLib.HarmonyPatch' }) {
            $args = $attr.ConstructorArguments
            if ($args.Count -lt 2) { throw "Unsupported target metadata on $($type.FullName)" }
            $declaring = $args[0].Value.FullName
            $name = $args[1].Value
            $targetType = $game.MainModule.Types | Where-Object FullName -CEQ $declaring
            $candidates = @($targetType.Methods | Where-Object Name -CEQ $name)
            if ($args.Count -ge 3) {
                $wanted = @($args[2].Value | ForEach-Object { $_.Value.FullName })
                if ($args.Count -eq 4) {
                    for($i=0; $i -lt $wanted.Count; $i++) {
                        # Harmony ArgumentType.Ref=1, Out=2.
                        if ($args[3].Value[$i].Value -in 1,2) { $wanted[$i] += '&' }
                    }
                }
                $candidates = @($candidates | Where-Object { (($_.Parameters | ForEach-Object { $_.ParameterType.FullName }) -join ',') -ceq ($wanted -join ',') })
            }
            if ($candidates.Count -ne 1) { throw "Missing/ambiguous patch target: $declaring::$name" }
            $target = $candidates[0]
            foreach($patch in $type.Methods | Where-Object { $_.CustomAttributes.AttributeType.FullName -match 'HarmonyLib.Harmony(Prefix|Postfix)' }) {
                foreach($p in $patch.Parameters) {
                    $actual = $p.ParameterType.FullName.TrimEnd('&')
                    if ($p.Name -eq '__result' -and $actual -cne $target.ReturnType.FullName) { throw "Wrong result type: $($patch.FullName)" }
                    if ($p.Name -match '^__(\d+)$') {
                        $index = [int]$Matches[1]
                        if ($index -ge $target.Parameters.Count -or $actual -cne $target.Parameters[$index].ParameterType.FullName.TrimEnd('&')) { throw "Wrong argument $index on $($patch.FullName)" }
                    }
                    if ($p.Name -eq '__instance' -and ($target.IsStatic -or $actual -cne $declaring)) { throw "Wrong instance on $($patch.FullName)" }
                }
            }
            $count++
            Write-Output "PASS: $($type.FullName) -> $($target.FullName)"
        }
    }
    if ($count -lt 10) { throw "Expected runtime patch classes were not included in the assembly ($count found)." }
    Write-Output "Validated $count patch targets and their injected argument/result types. Native Harmony installation still requires an in-game test."
} finally { $game.Dispose(); $mod.Dispose(); $resolver.Dispose() }
