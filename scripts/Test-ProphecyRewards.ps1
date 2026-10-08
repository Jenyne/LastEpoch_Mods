param([string]$GamePath = 'D:\SteamLibrary\steamapps\common\Last Epoch')
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
Push-Location $repo
try {
    if (Get-Process -Name 'Last Epoch' -ErrorAction SilentlyContinue) { throw 'Close Last Epoch before building/installing.' }
    if (!(Test-Path -LiteralPath (Join-Path $GamePath 'MelonLoader\Il2CppAssemblies\Il2CppLE.dll'))) { throw 'Last Epoch game assemblies were not found.' }
    $dll = Join-Path $repo 'Build\Release\net6.0\LastEpoch_Hud.dll'
    if (Test-Path -LiteralPath $dll) { Remove-Item -LiteralPath $dll }
    dotnet build .\LastEpoch_Hud\LastEpoch_Hud.csproj -c Release -t:Rebuild "-p:LastEpochPath=$GamePath"
    if ($LASTEXITCODE -ne 0 -or !(Test-Path -LiteralPath $dll)) { throw 'Build failed; installed files were not replaced.' }
    $oldGame = $env:LAST_EPOCH_PATH
    $oldMod = $env:LAST_EPOCH_MOD_DLL
    try {
        $env:LAST_EPOCH_PATH = $GamePath
        $env:LAST_EPOCH_MOD_DLL = $dll
        dotnet run --project .\LastEpoch_Hud.Tests -c Release
        if ($LASTEXITCODE -ne 0) { throw 'Tests failed; installed files were not replaced.' }
    } finally {
        $env:LAST_EPOCH_PATH = $oldGame
        $env:LAST_EPOCH_MOD_DLL = $oldMod
    }
    if (Get-Process -Name 'Last Epoch' -ErrorAction SilentlyContinue) { throw 'Close Last Epoch before installing.' }
    $files = @(@{ source = $dll; target = (Join-Path $GamePath 'Mods\LastEpoch_Hud.dll') })
    foreach ($language in @('en', 'fr', 'ko', 'zh')) {
        $files += @{ source = (Join-Path $repo "LastEpoch_Hud\LastEpoch_Hud\Locales\$language.json"); target = (Join-Path $GamePath "Mods\LastEpoch_Hud\Locales\$language.json") }
    }
    $backup = Join-Path $GamePath ('UserData\LastEpoch_Hud\ProphecyTestBackup\' + (Get-Date -Format 'yyyyMMdd-HHmmss-ffff'))
    New-Item -ItemType Directory -Path $backup -Force | Out-Null
    for ($i = 0; $i -lt $files.Count; $i++) {
        $file = $files[$i]
        if (!(Test-Path -LiteralPath $file.source)) { throw "Missing tested file: $($file.source)" }
        $file.existed = Test-Path -LiteralPath $file.target
        $file.backup = Join-Path $backup "$i.bak"
        if ($file.existed) { Copy-Item -LiteralPath $file.target -Destination $file.backup }
    }
    $written = @()
    try {
        foreach ($file in $files) {
            New-Item -ItemType Directory -Path (Split-Path -Parent $file.target) -Force | Out-Null
            $written += $file
            Copy-Item -LiteralPath $file.source -Destination $file.target -Force
            if ((Get-FileHash -LiteralPath $file.source).Hash -ne (Get-FileHash -LiteralPath $file.target).Hash) { throw 'Installed file hash mismatch.' }
        }
    } catch {
        foreach ($file in $written) {
            if ($file.existed) { Copy-Item -LiteralPath $file.backup -Destination $file.target -Force }
            elseif (Test-Path -LiteralPath $file.target) { Remove-Item -LiteralPath $file.target }
        }
        throw
    }
    Write-Host 'Installed prophecy test build. Character > Cheats > Prophecy Reward Multiplier (off by default, x1-x10).'
    Write-Host "Previous DLL/locales backed up at: $backup"
} finally { Pop-Location }
