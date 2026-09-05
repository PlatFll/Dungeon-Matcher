[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
function Check($condition, $message) {
    if (-not $condition) { throw $message }
}
function Field($text, $name) {
    $match = [regex]::Match($text, "(?m)^  ${name}: ([^\r\n]*)")
    Check $match.Success "Missing field $name"
    return $match.Groups[1].Value.Trim()
}
$assets = @{}
$ids = @{}
$guids = @{}
Get-ChildItem (Join-Path $root 'Assets') -Recurse -Filter *.meta | ForEach-Object {
    $guid = [regex]::Match([IO.File]::ReadAllText($_.FullName), '(?m)^guid: (\w+)').Groups[1].Value
    Check (-not $guids.ContainsKey($guid)) "Duplicate GUID: $guid"
    $guids[$guid] = $_.FullName
}
$database = [IO.File]::ReadAllText((Join-Path $root 'Assets/_Game/Data/Enemies/EnemyDatabase_Main.asset'))
Get-ChildItem (Join-Path $root 'Assets/_Game/Data/Enemies') -Filter Enemy_*.asset | ForEach-Object {
    $text = [IO.File]::ReadAllText($_.FullName)
    $assets[$_.BaseName] = $text
    $id = Field $text 'enemyId'
    Check (-not $ids.ContainsKey($id)) "Duplicate enemy ID $id"
    $ids[$id] = $true
    $topFields = [regex]::Matches($text, '(?m)^  (\w+):') | ForEach-Object { $_.Groups[1].Value }
    Check (@($topFields | Group-Object | Where-Object Count -gt 1).Count -eq 0) "Duplicate YAML fields in $($_.Name)"
    foreach ($reference in [regex]::Matches($text, 'guid: (\w+)')) {
        Check ($guids.ContainsKey($reference.Groups[1].Value)) "Broken GUID in $($_.Name)"
    }
}
foreach ($name in @('Knight', 'SpearKnight', 'ShieldKnight')) {
    $text = $assets["Enemy_$name"]
    Check ((Field $text 'minimumWave') -eq '17') "$name must enter in Chapter 3"
    Check ((Field $text 'crownSoldier') -eq '1') "$name must participate in Crown commands"
}
$shield = $assets['Enemy_ShieldKnight']
Check ((Field $shield 'category') -eq '1') 'Shield Knight must be Special'
Check ((Field $shield 'baseSpecialTurnRequirement') -eq '7') 'Shield cadence changed'
Check ((Field $shield 'lockSpecialTurnRequirement') -eq '1') 'Shield cadence must be locked'
Check ((Field $shield 'baseFollowUpDamage') -eq '0') 'Shield Knight must have one hit'
$spear = $assets['Enemy_SpearKnight']
foreach ($pair in @(@('category','0'), @('baseMaxHealth','120'), @('baseDamage','5'),
    @('baseFollowUpDamage','7'), @('baseAttackInterval','10'))) {
    Check ((Field $spear $pair[0]) -eq $pair[1]) "Spear Knight changed: $($pair[0])"
}
$captain = $assets['Enemy_KnightCaptain']
foreach ($pair in @(@('category','2'), @('specialAbilityKind','7'),
    @('baseSpecialTurnRequirement','4'), @('lockSpecialTurnRequirement','1'), @('maximumSpecialEscorts','1'))) {
    Check ((Field $captain $pair[0]) -eq $pair[1]) "Captain configuration: $($pair[0])"
}
$captainGuid = [regex]::Match([IO.File]::ReadAllText((Join-Path $root 'Assets/_Game/Data/Enemies/Enemy_KnightCaptain.asset.meta')), 'guid: (\w+)').Groups[1].Value
Check ($database.Contains($captainGuid)) 'Captain missing from database'
foreach ($name in @('Knight', 'SpearKnight', 'ShieldKnight')) {
    $guid = [regex]::Match([IO.File]::ReadAllText((Join-Path $root "Assets/_Game/Data/Enemies/Enemy_$name.asset.meta")), 'guid: (\w+)').Groups[1].Value
    Check ($captain.Contains($guid)) "Missing $name escort reference"
}
$profile = [IO.File]::ReadAllText((Join-Path $root 'Assets/_Game/Data/Balance/WaveSpawnProfile_Standard.asset'))
$exact = [regex]::Matches($profile, '(?m)^  - wave: (\d+)') | ForEach-Object { [int]$_.Groups[1].Value }
Check (($exact -join ',') -eq '5,8,16') 'Unexpected fixed-wave override'
Check ([regex]::Matches($profile, '(?m)^    fixedEnemies:').Count -eq 1) 'Only Sergeant retains a fixed identity'
foreach ($band in [regex]::Matches($profile, '(?ms)^  - ruleName:.*?(?=^  - ruleName:|\z)')) {
    $minimum = [int][regex]::Match($band.Value, 'minimumEnemyCount: (\d+)').Groups[1].Value
    $maximum = [int][regex]::Match($band.Value, 'maximumEnemyCount: (\d+)').Groups[1].Value
    Check ($minimum -ge 1 -and $minimum -le $maximum -and $maximum -le 3) 'Invalid slot capacity'
}
'Chapter pool static checks passed: identities, YAML fields, GUID references, cadence, escorts and capacity. No Unity execution.'
