# Builds the mod and packs Build\Release\LastEpoch_Hud.zip. Optionally publishes it as a GitHub release.
# Works in Windows PowerShell 5 and PowerShell 7. See docs\RELEASING.md.
#   .\LastEpoch_Hud\MakeRelease.ps1
#   .\LastEpoch_Hud\MakeRelease.ps1 -LastEpochPath "D:\SteamLibrary\steamapps\common\Last Epoch"
#   .\LastEpoch_Hud\MakeRelease.ps1 -Publish -Tag v4.4.21 -Title "v4.4.21 HUD for Unity 6000.4.8" -NotesFile notes.md -CoreModule <patched UnityEngine.CoreModule.dll>
# Asset names are fixed: the installer (TriSSec-Lab/LE-Hud-Installer) downloads
# releases/latest/download/LastEpoch_Hud.zip and UnityEngine.CoreModule.dll, so every release needs both.
param(
    [string]$LastEpochPath,
    [switch]$Publish,
    [string]$Tag,
    [string]$Title,
    [string]$NotesFile,
    [string]$CoreModule
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $PSScriptRoot 'LastEpoch_Hud.csproj'
$config = 'Keyboard'
$buildDir = Join-Path $root "Build\$config\net6.0"
$zip = Join-Path $root 'Build\Release\LastEpoch_Hud.zip'

# Everything that can stop a publish is checked before the build, not after it.
function Assert-PublishInputs {
    if (-not $Tag) { throw '-Publish needs -Tag (e.g. v4.4.21)' }
    if (-not $NotesFile -or -not (Test-Path $NotesFile)) { throw '-Publish needs -NotesFile with the changelog' }
    if (-not $CoreModule -or -not (Test-Path $CoreModule)) { throw '-Publish needs -CoreModule: the installer downloads UnityEngine.CoreModule.dll from every latest release' }
    if ((Split-Path $CoreModule -Leaf) -ne 'UnityEngine.CoreModule.dll') { throw '-CoreModule must be a file named UnityEngine.CoreModule.dll' }
    if (-not (Get-Command gh -ErrorAction SilentlyContinue)) { throw 'Install the GitHub CLI: https://cli.github.com/' }
    gh auth status *> $null
    if ($LASTEXITCODE) { throw 'Run "gh auth login" first' }
    # The startup log prints the commit; a release from uncommitted code can't be traced back.
    if (git -C $root status --porcelain --untracked-files=no) { throw 'Commit your changes before publishing' }
    # The tag must point at the code being zipped (an existing tag on another commit would not).
    $tagCommit = git -C $root rev-parse -q --verify "refs/tags/$Tag^{commit}"
    if ($tagCommit -and $tagCommit -ne (git -C $root rev-parse HEAD)) { throw "Tag $Tag exists on another commit than HEAD" }
}

# A clean output folder, so files removed from the project don't ship from an old build.
function Build-Mod {
    Remove-Item (Join-Path $root "Build\$config") -Recurse -Force -ErrorAction SilentlyContinue
    $arguments = @('build', $project, '-c', $config, '--nologo', '-v', 'q')
    # Windows PowerShell breaks a quoted argument that ends in "\", so the path is trimmed.
    if ($LastEpochPath) { $arguments += "-p:LastEpochPath=$($LastEpochPath.TrimEnd('\'))" }
    dotnet @arguments | Out-Host
    if ($LASTEXITCODE) { throw "Build failed ($config)" }
}

# Only what players need: the DLL and its LastEpoch_Hud folder (Assets, Locales). No .pdb or .deps.json.
# Entry names are written with "/" by hand: Compress-Archive and Windows PowerShell's ZipFile write "\",
# which Linux unzips as flat file names like "LastEpoch_Hud\Locales\en.json".
function New-ReleaseZip {
    New-Item -ItemType Directory -Force (Split-Path $zip) | Out-Null
    Remove-Item $zip -Force -ErrorAction SilentlyContinue
    $files = @(Get-Item (Join-Path $buildDir 'LastEpoch_Hud.dll')) + @(Get-ChildItem (Join-Path $buildDir 'LastEpoch_Hud') -File -Recurse)
    $archive = [IO.Compression.ZipFile]::Open($zip, 'Create')
    try {
        foreach ($file in $files) {
            $name = $file.FullName.Substring($buildDir.Length + 1).Replace('\', '/')
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $file.FullName, $name) | Out-Null
        }
    }
    finally { $archive.Dispose() }
    "Created: $zip"
}

# --target pins the tag to HEAD; GitHub rejects a commit that isn't pushed, so nothing half-made is published.
function Publish-Release {
    $repo = (git -C $root remote get-url origin) -replace '^https://github.com/', '' -replace '\.git$', ''
    $releaseTitle = $Tag
    if ($Title) { $releaseTitle = $Title }
    gh release create $Tag $zip $CoreModule --repo $repo --target (git -C $root rev-parse HEAD) --title $releaseTitle --notes-file $NotesFile
    if ($LASTEXITCODE) { throw "gh release create failed for $Tag" }
    "Published $Tag to $repo"
}

if ($Publish) { Assert-PublishInputs }
Build-Mod
New-ReleaseZip
if ($Publish) { Publish-Release }
