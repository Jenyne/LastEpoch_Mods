#requires -Version 5.1
[CmdletBinding()]
param(
    [string]$RepoPath = 'D:\GitHub\LastEpoch_Mods',
    [string]$GamePath = 'D:\SteamLibrary\steamapps\common\Last Epoch',
    [int]$Select = 0,
    [switch]$ListOnly
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2.0
$queueJson = @'
[
  {
    "id": 1,
    "title": "Force Drop / legal + illegal / global item search",
    "branch": "feat/force-drop",
    "status": "Single combined branch; global search/scrolling/cyan and LP regressions await testing",
    "location": "Items > Force Drop",
    "checks": [
      "Search all items before choosing category/rarity: try seed, partial names, aliases and seed helmet. Choosing a result fills category/rarity/exact item; clearing search returns to that category list. Check base/Unique/Set items, duplicate names, empty results, paging and locale changes, with Illegal mode off/on. Native search UI and actual drop identity are not yet confirmed.",
      "Keep Illegal mode off. Prefix-only/suffix-only slots use full-width scrolling; enchantment/sealed/corruption pools use independent Prefix/Suffix lists. Check wheel/drag/handles, last entries, layout transitions, pinned None and search reset; legal restrictions and green Set/purple corruption colors stay intact.",
      "With Corrupted: No and every ordinary affix None, LP and Fixed/Random must be editable; dedicated unique modifiers alone preserve LP. Adding ordinary affixes clears/disables LP; clearing them unlocks it. Corrupted: Yes independently locks LP at zero even with every affix None. Turn corruption off and confirm unlock; this report is not yet confirmed.",
      "Test ordinary equipment with Set + Champion + two suffixes, then sealed + corruption; verify item identity, set effects and save/reload.",
      "Test Unsated Rage, Withstand the Elements and idols. One-tier affixes should clamp rather than fail. Report missing choices with item type and affix name.",
      "Check mode switching clears selections. Illegal picker has independently scrolling Prefix/Suffix columns and scrollbar handles; either side edits the opened slot. None stays pinned. Search resets both scroll positions. Check cyan idol affixes in choices and selected rows.",
      "Test T8 in all four ordinary rows, the Primordial sealed row and corruption using real eight-tier definitions. Retest legal T8 Primordial with Illegal mode off; one-tier definitions stay T1.",
      "Test a unique with Set membership: unique name remains, correct set piece counting and actual set bonus. Check Unsated Rage/Withstand special modifiers.",
      "Test four affixes + sealed + corruption, then equip/stats and save/reload. Record rejected combinations exactly."
    ]
  },
  {
    "id": 3,
    "title": "Mastery chains / combat ground-item hover",
    "branch": "fix/mastery-lock-ground-tooltips",
    "status": "Checkbox no longer crashes (user-confirmed); allocation blocked; hover on hold",
    "location": "Skills > Unlock Other Mastery Trees",
    "checks": [
      "User confirms 764ea599 no longer crashes when clicking Unlock Other Mastery Trees; the option is still not functional for allocation. Keep Remove Node Requirements off (it works/persists). Retest off/on, panel closed/open, page changes and restart before extending crash confirmation beyond the reported checkbox click.",
      "The global cap stays unchanged. This is a crash-isolation candidate, not a completed allocation bypass. Try beyond-chain nodes with adequate prerequisites and points, and report whether a real point is spent. Check selected mastery, innate bonus, point costs and rank caps.",
      "Keep [MasteryTrace] toggle, Visual unlock active, click/spend lines and the once-per-run [MasteryApi] method signatures. If it crashes again, keep the matching MelonLoader log and native crash stack.",
      "Hover investigation is on hold at the user's request. Existing hover implementation is unchanged; no F9 test is requested in this pass."
    ]
  },
  {
    "id": 4,
    "title": "Maxroll graphical passives / skills preview",
    "branch": "feat/maxroll-tree-preview",
    "status": "Graphical trees and item retrieval/preview confirmed; equipment view deferred",
    "location": "Items > Force Drop > Maxroll Build Preview",
    "checks": [
      "Load https://maxroll.gg/last-epoch/planner/2ai4s0qh#1; inspect named passive/mastery and skill trees, connections and allocated ranks.",
      "Test pan, zoom, Fit, node hover/click details, variant switching and close/reopen. This is read-only; no character point allocation is expected.",
      "Retest gear, idols, blessings and Copy Item JSON. Empty Weaver Items is expected for the sample build. Check EN/FR/KO/EN controls."
    ]
  },
  {
    "id": 5,
    "title": "Normal forge T6/T7 / craft options",
    "branch": "feat/advanced-forge-t7",
    "status": "Done: user confirmed 2026-10-09; retained for optional regression",
    "location": "Items > Crafting",
    "checks": [
      "Enable Craft Affixes to T7 plus Infinite Forging Potential on expendable normal equipment. Keep [ForgeTrace] from selecting a T5 affix. Check T5 -> T6 -> T7 and stopping at T7.",
      "Test Max Crafted Roll. Slot a real Hope or Despair glyph to test its guarantee; inspect actual FP, seal outcome and material consumption. With Guarantee Despair on and Despair slotted, an item with any existing sealed affix must reject the craft with no second seal or item/FP/material changes. Check regular/Primordial/corruption seals, T7 on/off, immediate retry after the first seal and item swaps; an unsealed item must still allow the first seal.",
      "Toggle off, swap items, reopen the forge and restart. Deselect All should leave Infinite FP and the four advanced controls alone.",
      "User confirmed selection 5 is good and called it done on 2026-10-09. These checks are optional regressions; retain logs if a new issue appears."
    ]
  },
  {
    "id": 6,
    "title": "Separate natural drop rate controls",
    "branch": "feat/independent-drop-rates",
    "status": "UI recovery added; native build and runtime confirmation pending",
    "location": "Items > Drop > Natural Drop Rates",
    "checks": [
      "Check separate Unique, Set, Exalted Affix and T7 Affix rows; 100% is normal, 1000% is 10x, not a guaranteed final chance.",
      "Test each alone over enough ordinary drops: all off/100%, then 1000%, then 50%/0%. Exclude forced/guaranteed rewards from comparisons.",
      "Enable each row, drag and type 0, 50, 100 and 1000; click outside the input. Check slider/input agreement, legacy rows, close/reopen and restart. Save the bind confirmation and [DropRates] baseline lines."
    ]
  },
  {
    "id": 7,
    "title": "Offline startup — observer removed for isolation",
    "branch": "test/offline-guard-diagnostics",
    "status": "951f4e01 user-confirmed no crash; character entries and normal shutdown logged",
    "location": "Launch -> offline character selection; MelonLoader/Latest.log",
    "checks": [
      "Offline is mandatory while the mod is installed: confirm automatic offline character selection and hidden/blocked online controls. Old Login.Enable_AutoLoginOffline=false or a missing Login section must not bypass this.",
      "Cold launch and early/repeated manual clicks: completed client Login notification must precede offline dispatch. Check no SystemLoading transition exception; keep Latest.log and Player.log from the same run. Missing startup notification must defer rather than bypass readiness.",
      "Load an offline character, change zones, run an echo and remain playable for 30+ seconds. Switch characters and reload the first.",
      "Confirm build 951f4e01, load the same character that worked on selection 9, fight and switch characters. Keep Latest.log and Player.log. The session observer is removed; no [OfflineGuard] lines are expected."
    ]
  },
  {
    "id": 8,
    "title": "Idol rerolling regression (already confirmed)",
    "branch": "feat/idol-reroll-misc",
    "status": "Confirmed working; optional restart/locale regression",
    "location": "Scenes > Misc > Idol Rerolling",
    "checks": [
      "Test No Memory Amber Cost and Unlimited Idol Altar Uses independently and together.",
      "Turn them off while the altar is open. Check normal amber costs/use limits return and unrelated amber purchases still cost amber.",
      "Confirm restart persistence and EN -> FR -> KO -> EN captions."
    ]
  },
  {
    "id": 9,
    "title": "Current main baseline",
    "branch": "master",
    "status": "Baseline/control build",
    "location": "Normal mod UI",
    "checks": [
      "Use this to compare behavior with current main after testing a feature branch.",
      "Each selection installs one branch DLL; this runner does not combine pending features."
    ]
  },
  {
    "id": 11,
    "title": "Combined QoL / key teleports / session counters / Travel Anywhere",
    "branch": "feat/travel-anywhere",
    "status": "Dungeon preset hang reproduced; campaign-approach fix 811f6bed awaiting retest; 10 merged into 11",
    "location": "Character > Cheats > Currencies; Scenes > Misc; world map",
    "checks": [
      "Combined build: selection 10 is retired. Check Spawn 1,000,000 Gold with auto-pickup off/on and multipliers; test Alt-left-drag, saved/clamped counter position, Reset counter position, natural XP/Favour/Amber, no double counting or manual grant credit, pause/reset and zone continuity.",
      "Only the Travel Anywhere toggle/status should remain; no full scene picker. Open the map and refresh key teleports. Dungeon presets must resolve to Ruined Coast, Shrouded Ridge (Surface fallback), and Felled Wood, never Dun1Q10/Dun2Q10/Dun3Q10. Test unlocked/locked/missing waypoints and old saved dungeon favourites (redirect or block). Check End of Time, Bazaar and Observatory. Preserve normal key/tier entry and current-character waypoint locks. Keep [KeyTeleports] lines. Test extra saved waypoint favourites and restart.",
      "Map-menu travel is user-confirmed on 2466cc0a. Retest non-waypoint left-click menus and Travel with the option on, plus spawn, movement, camera, enemies, loot, exits and NPCs. Non-waypoint right-click is a known limitation accepted for now; check supported right-click nodes and popups without expanding that scope.",
      "The old full-picker attempt EoT -> WE502 failed to start loading and left the busy guard set. The current build removes the picker and immediately releases rejected loads only with the original source/player intact and no target loaded. If a direct attempt fails, key and ordinary waypoint travel must resume after cleanup. Real pending async loads/cleanup must still block overlapping requests; keep [TravelAnywhere] lines.",
      "Double-click and disable during loading or with an area menu open. Block stale temporary actions; reopen for normal availability. Check map flags return on close/off, Unlock All Waypoints combinations, gates, era widgets, failed placement/source retention and non-waypoint favourites enabled versus disabled.",
      "Repeat trips and leave an echo/arena for a static area; check actors, portals, quests and memory. Retest Safe Teleport, Misc scrolling, loot hover/casting and locales. See docs/TEST_TRAVEL_ANYWHERE.md and docs/TEST_GOLD_FAVOURITES_SESSION_STATS.md."
    ]
  }
]
'@
# Windows PowerShell 5.1 emits the JSON array as a single pipeline object.
# Assign it directly: wrapping that pipeline in @() produces a nested array.
$queue = ConvertFrom-Json -InputObject $queueJson
$archiveRoot = Join-Path $GamePath 'UserData\LastEpoch_Hud\TestQueue'
$stateFile = Join-Path $archiveRoot 'installed-test.json'

function Show-Queue {
    Write-Host "`nLast Epoch test queue - choose ONE build at a time" -ForegroundColor Cyan
    foreach ($entry in $queue) {
        Write-Host ('{0}. {1}' -f $entry.id, $entry.title)
        Write-Host ('   {0} | {1}' -f $entry.branch, $entry.status) -ForegroundColor DarkGray
    }
    Write-Host 'L. Save the current game log | Q. Quit'
}

function Show-Checks($Entry) {
    Write-Host ("`nTest location: " + $Entry.location) -ForegroundColor Yellow
    foreach ($check in $Entry.checks) { Write-Host ('  - ' + $check) }
}

function Assert-GameClosed {
    if (Get-Process -Name 'Last Epoch' -ErrorAction SilentlyContinue) {
        throw 'Close Last Epoch before switching or installing another build.'
    }
}

function Get-GitText([string[]]$Arguments) {
    $previousPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        $output = @(& git @Arguments 2>&1)
        $code = $LASTEXITCODE
    } finally { $ErrorActionPreference = $previousPreference }
    if ($code -ne 0) { throw ('Git failed: ' + ($output -join "`n")) }
    return ($output -join "`n").Trim()
}

function Invoke-Checked([string]$Command, [string[]]$Arguments, [string]$LogPath) {
    $previousPreference = $ErrorActionPreference
    try {
        # Windows PowerShell treats native stderr as ErrorRecords; capture it
        # without terminating before the native exit code can be inspected.
        $ErrorActionPreference = 'Continue'
        & $Command @Arguments 2>&1 |
            ForEach-Object { $_.ToString() } |
            Tee-Object -FilePath $LogPath -Append | Out-Host
        $code = $LASTEXITCODE
    } finally { $ErrorActionPreference = $previousPreference }
    if ($code -ne 0) { throw "$Command failed (exit $code). See $LogPath" }
}

function Save-GameLog {
    $latest = Join-Path $GamePath 'MelonLoader\Latest.log'
    if (!(Test-Path -LiteralPath $latest -PathType Leaf)) {
        Write-Host 'No MelonLoader Latest.log found yet.'
        return
    }
    $destination = Join-Path $archiveRoot 'unassigned'
    if (Test-Path -LiteralPath $stateFile -PathType Leaf) {
        $state = Get-Content -LiteralPath $stateFile -Raw | ConvertFrom-Json
        # If no game was launched after installation, Latest.log still belongs
        # to an earlier DLL. Do not label that old log as the new branch's run.
        $logTime = (Get-Item -LiteralPath $latest).LastWriteTimeUtc
        $installTime = [DateTime]::Parse($state.installed).ToUniversalTime()
        if ($logTime -ge $installTime -and (Test-Path -LiteralPath $state.runFolder -PathType Container)) {
            $destination = $state.runFolder
        }
    }
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
    $copy = Join-Path $destination ('Latest-' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff') + '.log')
    Copy-Item -LiteralPath $latest -Destination $copy
    Write-Host "Saved game log: $copy"
}

function Install-QueuedBuild($Entry) {
    Assert-GameClosed
    if (!(Test-Path -LiteralPath (Join-Path $GamePath 'MelonLoader\Il2CppAssemblies\Il2CppLE.dll'))) {
        throw 'Game assemblies are missing. Check -GamePath and launch once with MelonLoader.'
    }
    if (!(Test-Path -LiteralPath (Join-Path $RepoPath 'LastEpoch_Hud.sln'))) {
        throw 'LastEpoch_Hud.sln was not found. Check -RepoPath.'
    }
    Get-Command git, dotnet -ErrorAction Stop | Out-Null
    Push-Location $RepoPath
    try {
        $remote = Get-GitText @('remote', 'get-url', 'origin')
        if ($remote -notmatch '(?i)[:/]Jenyne/LastEpoch_Mods(?:\.git)?/?$') {
            throw 'origin must point to Jenyne/LastEpoch_Mods for this testing queue.'
        }
        if (Get-GitText @('status', '--porcelain', '--untracked-files=no')) {
            throw 'Commit or stash tracked changes before switching branches.'
        }
        Save-GameLog
        $safeBranch = $Entry.branch -replace '[^A-Za-z0-9._-]', '_'
        $runFolder = Join-Path $archiveRoot ((Get-Date -Format 'yyyyMMdd-HHmmss-fff') + '-' + $safeBranch)
        New-Item -ItemType Directory -Path $runFolder -Force | Out-Null
        $buildLog = Join-Path $runFolder 'build.log'
        $testLog = Join-Path $runFolder 'tests.log'
        $branch = $Entry.branch
        Invoke-Checked 'git' @('fetch', 'origin', "+refs/heads/${branch}:refs/remotes/origin/$branch") $buildLog
        Invoke-Checked 'git' @('switch', '--detach', "origin/$branch") $buildLog
        $sha = Get-GitText @('rev-parse', 'HEAD')
        if ($sha -ne (Get-GitText @('rev-parse', "origin/$branch"))) {
            throw 'Selected checkout differs from the fetched remote commit.'
        }
        $metadata = [ordered]@{
            branch = $branch; commit = $sha; title = $Entry.title
            started = (Get-Date).ToString('o'); runFolder = $runFolder; status = 'building'
        }
        $metadataPath = Join-Path $runFolder 'result.json'
        $metadata | ConvertTo-Json | Set-Content -LiteralPath $metadataPath -Encoding UTF8
        Show-Checks $Entry
        Write-Host "`nBuilding $branch ($sha)" -ForegroundColor Cyan
        try {
            $modDll = Join-Path $RepoPath 'Build\Release\net6.0\LastEpoch_Hud.dll'
            if (Test-Path -LiteralPath $modDll) { Remove-Item -LiteralPath $modDll }
            Invoke-Checked 'dotnet' @('build', '.\LastEpoch_Hud\LastEpoch_Hud.csproj', '-c', 'Release', '-t:Rebuild', "-p:LastEpochPath=$GamePath") $buildLog
            if (!(Test-Path -LiteralPath $modDll -PathType Leaf)) { throw 'Fresh Release DLL was not produced.' }
            $modHash = (Get-FileHash -LiteralPath $modDll -Algorithm SHA256).Hash

            $oldGameEnv = $env:LAST_EPOCH_PATH
            $oldModEnv = $env:LAST_EPOCH_MOD_DLL
            $aliasPath = Join-Path $RepoPath 'Build\Keyboard\net6.0\LastEpoch_Hud.dll'
            $aliasBackup = Join-Path $runFolder 'Keyboard-before.dll'
            $needsAlias = (Get-Content '.\LastEpoch_Hud.Tests\Support\GameEnvironment.cs' -Raw) -match '"Build"\s*,\s*"Keyboard"'
            $aliasExisted = Test-Path -LiteralPath $aliasPath -PathType Leaf
            $aliasWritten = $false
            try {
                $env:LAST_EPOCH_PATH = $GamePath
                $env:LAST_EPOCH_MOD_DLL = $modDll
                if ($needsAlias) {
                    if ($aliasExisted) { Copy-Item -LiteralPath $aliasPath -Destination $aliasBackup }
                    New-Item -ItemType Directory -Path (Split-Path -Parent $aliasPath) -Force | Out-Null
                    # Older branch tests hardcode this path. Use the exact new
                    # Release DLL, then restore their previous validation file.
                    $aliasWritten = $true
                    Copy-Item -LiteralPath $modDll -Destination $aliasPath -Force
                    if ((Get-FileHash -LiteralPath $aliasPath -Algorithm SHA256).Hash -ne $modHash) {
                        throw 'Legacy test DLL alias differs from the fresh Release build.'
                    }
                }
                Invoke-Checked 'dotnet' @('run', '--project', '.\LastEpoch_Hud.Tests', '-c', 'Release') $testLog
            } finally {
                $env:LAST_EPOCH_PATH = $oldGameEnv
                $env:LAST_EPOCH_MOD_DLL = $oldModEnv
                if ($aliasWritten) {
                    if ($aliasExisted) { Copy-Item -LiteralPath $aliasBackup -Destination $aliasPath -Force }
                    elseif (Test-Path -LiteralPath $aliasPath) { Remove-Item -LiteralPath $aliasPath }
                }
            }
            Assert-GameClosed
            $files = @(@{ source = $modDll; destination = (Join-Path $GamePath 'Mods\LastEpoch_Hud.dll') })
            foreach ($language in @('en', 'fr', 'ko', 'zh')) {
                $source = Join-Path $RepoPath "LastEpoch_Hud\LastEpoch_Hud\Locales\$language.json"
                # Repository locale tests already parse these files with the
                # game's case-sensitive key semantics. PowerShell 5.1's JSON
                # parser rejects valid keys that differ only in capitalization.
                if (!(Test-Path -LiteralPath $source -PathType Leaf)) {
                    throw "Missing tested locale file: $source"
                }
                $files += @{ source = $source; destination = (Join-Path $GamePath "Mods\LastEpoch_Hud\Locales\$language.json") }
            }
            # Back up every destination before touching the installed files.
            $backupFolder = Join-Path $runFolder 'before-install'
            New-Item -ItemType Directory -Path $backupFolder -Force | Out-Null
            for ($i = 0; $i -lt $files.Count; $i++) {
                $file = $files[$i]
                $file.existed = Test-Path -LiteralPath $file.destination -PathType Leaf
                $file.backup = Join-Path $backupFolder ([string]$i + '-' + (Split-Path -Leaf $file.destination))
                if ($file.existed) { Copy-Item -LiteralPath $file.destination -Destination $file.backup }
            }
            try {
                foreach ($file in $files) {
                    New-Item -ItemType Directory -Path (Split-Path -Parent $file.destination) -Force | Out-Null
                    Copy-Item -LiteralPath $file.source -Destination $file.destination -Force
                }
                if ((Get-FileHash -LiteralPath $files[0].destination -Algorithm SHA256).Hash -ne $modHash) {
                    throw 'Installed DLL hash does not match the tested build.'
                }
            } catch {
                $installFailure = $_
                foreach ($file in $files) {
                    if ($file.existed) { Copy-Item -LiteralPath $file.backup -Destination $file.destination -Force }
                    elseif (Test-Path -LiteralPath $file.destination) { Remove-Item -LiteralPath $file.destination }
                }
                throw $installFailure
            }
            $metadata.status = 'installed'
            $metadata.installed = (Get-Date).ToString('o')
            $metadata.dllSHA256 = $modHash
            $metadata | ConvertTo-Json | Set-Content -LiteralPath $metadataPath -Encoding UTF8
            $metadata | ConvertTo-Json | Set-Content -LiteralPath $stateFile -Encoding UTF8
            Write-Host "`nInstalled: $branch ($sha)" -ForegroundColor Green
            Write-Host "DLL + locales installed. Logs and previous files: $runFolder"
            Show-Checks $Entry
            Write-Host 'Launch the game and test. Close it before choosing the next build.'
        } catch {
            $metadata.status = 'failed'
            $metadata.error = $_.Exception.Message
            $metadata | ConvertTo-Json | Set-Content -LiteralPath $metadataPath -Encoding UTF8
            throw
        }
    } finally { Pop-Location }
}

if ($ListOnly) {
    Show-Queue
    foreach ($entry in $queue) { Write-Host ("`n" + $entry.title); Show-Checks $entry }
    return
}
if ($Select -ne 0) {
    $entry = @($queue | Where-Object { $_.id -eq $Select })
    if ($entry.Count -ne 1) { throw "Unknown selection: $Select" }
    Install-QueuedBuild $entry[0]
    return
}
while ($true) {
    Show-Queue
    $choice = Read-Host 'Build number, L, or Q'
    if ($choice -match '^(?i)q$') { break }
    try {
        if ($choice -match '^(?i)l$') { Save-GameLog; continue }
        $entry = @($queue | Where-Object { [string]$_.id -eq $choice })
        if ($entry.Count -ne 1) { Write-Host 'Choose a listed build number.'; continue }
        Install-QueuedBuild $entry[0]
    } catch { Write-Host ("Stopped: " + $_.Exception.Message) -ForegroundColor Red }
}
