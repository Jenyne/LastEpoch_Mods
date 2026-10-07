param(
    [Parameter(Mandatory = $true, Position = 0)][string]$BuildLink,
    [int]$Variant = 0,
    [string]$OutputPath = ".\BuildImports\MaxrollPreview"
)
$ErrorActionPreference = "Stop"
Set-Location (Split-Path $PSScriptRoot -Parent)
$previewArgs = @("run", "--project", ".\LastEpoch_Maxroll", "--", $BuildLink, "--output", $OutputPath)
if ($Variant -gt 0) { $previewArgs += @("--variant", "$Variant") }
dotnet @previewArgs
if ($LASTEXITCODE -eq 2) {
    Write-Warning "Snapshot saved with an unresolved selection or item fields needing review. Read preview.json."
} elseif ($LASTEXITCODE -ne 0) {
    throw "Retrieval failed; see the message above."
}
