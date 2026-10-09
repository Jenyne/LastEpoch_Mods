[CmdletBinding()]
param(
    [string]$GamePath = 'D:\SteamLibrary\steamapps\common\Last Epoch',
    [switch]$BuildOnly
)

$ErrorActionPreference = 'Stop'
# Reuse the startup branch's checked build, closed-game check, and DLL backup/install.
& (Join-Path $PSScriptRoot 'Test-AutoOfflineStartup.ps1') @PSBoundParameters
if (-not $BuildOnly) {
    Write-Host 'Manual offline-only candidate: click Play Offline; no auto-selection, startup readiness listener or session observer.'
    Write-Host 'Test character entry, zone changes, combat, and switching characters.'
    Write-Host 'Keep [Offline] messages and complete Latest.log / Player.log. No [OfflineGuard] messages are expected.'
}
