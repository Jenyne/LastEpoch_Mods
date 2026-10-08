[CmdletBinding()]
param(
    [string]$GamePath = 'D:\SteamLibrary\steamapps\common\Last Epoch',
    [switch]$BuildOnly
)

$ErrorActionPreference = 'Stop'
# Reuse the startup branch's checked build, closed-game check, and DLL backup/install.
& (Join-Path $PSScriptRoot 'Test-AutoOfflineStartup.ps1') @PSBoundParameters
if (-not $BuildOnly) {
    Write-Host 'Guard diagnostics are enabled independently of profiling settings.'
    Write-Host 'This build observes signals; gameplay mutation protection is not installed yet.'
    Write-Host 'Test offline character loading, zone changes, echoes, and switching characters.'
    Write-Host 'Look for [OfflineGuard] messages in MelonLoader\Latest.log and keep the full log.'
}
