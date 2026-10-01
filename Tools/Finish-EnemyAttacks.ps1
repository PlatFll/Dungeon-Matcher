param([string]$LibreSprite='C:\Users\USER\Downloads\libresprite-development-windows-x86_64\libresprite.exe',[string[]]$Names)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$jobs=Get-Content (Join-Path $root 'ArtSource/EnemyAttacks/selected.json') -Raw|ConvertFrom-Json
if($Names){$jobs=@($jobs|Where-Object {$_.name -in $Names})}
function Run-Libre([string[]]$arguments){
 $job=Start-Process -FilePath $LibreSprite -ArgumentList $arguments -WindowStyle Hidden -PassThru
 $job.WaitForExit();$job.Refresh();if($job.ExitCode -ne 0){throw "LibreSprite exit $($job.ExitCode)"}
}
foreach($entry in $jobs){
 $folder=Join-Path $root ('.utmp/AttackAssembly/'+$entry.name);New-Item -ItemType Directory -Path $folder -Force|Out-Null
 $resolved=[IO.Path]::GetFullPath($folder)
 if(!$resolved.StartsWith((Join-Path $root '.utmp/AttackAssembly/'),[StringComparison]::OrdinalIgnoreCase)){throw 'Assembly path escaped workspace'}
 Get-ChildItem -LiteralPath $resolved -Filter '*.png' -File | ForEach-Object { Remove-Item -LiteralPath $_.FullName }
 for($i=0;$i -lt $entry.frames.Count;$i++){
  Copy-Item -LiteralPath (Join-Path $root ($entry.candidate+'/'+('{0:00}' -f $entry.frames[$i])+'.png')) -Destination (Join-Path $folder (('{0:00}' -f $i)+'.png')) -Force
 }
 $assembly='.utmp/AttackAssembly/'+$entry.name+'.aseprite'
 $entry|Add-Member -NotePropertyName assembly -NotePropertyValue $assembly -Force
 Run-Libre @('-b',('"{0}"' -f (Join-Path $folder '00.png')),'--save-as',('"{0}"' -f (Join-Path $root $assembly)))
}
$script=Join-Path $root '.utmp/FinishAttacks.js'
$template=Get-Content (Join-Path $root 'ArtSource/EnemyAttacks/FinishAttacks.js') -Raw
[IO.File]::WriteAllText($script,$template.Replace('__ROOT__',$root.Replace('\','/')+'/').Replace('__JOBS__',(ConvertTo-Json -InputObject @($jobs) -Depth 8 -Compress)))
Run-Libre @('--script',('"{0}"' -f $script))
& (Join-Path $PSScriptRoot 'Normalize-FinalizedSourceLayers.ps1') -SourceRoot (Join-Path $root 'ArtSource/EnemyAttacks')
foreach($entry in $jobs){
 $stem=Join-Path $root ('ArtSource/EnemyAttacks/'+$entry.name+'_AutoAttack');$source=$stem+'.aseprite'
 # Set frame exposure metadata only; LibreSprite owns all image conversion.
 $bytes=[IO.File]::ReadAllBytes($source);$offset=128
 if([BitConverter]::ToUInt16($bytes,6) -ne $entry.durations.Count){throw "Frame count differs: $($entry.name)"}
 for($i=0;$i -lt $entry.durations.Count;$i++){
  [BitConverter]::GetBytes([UInt16]$entry.durations[$i]).CopyTo($bytes,$offset+8)
  $offset += [BitConverter]::ToUInt32($bytes,$offset)
 }
 [IO.File]::WriteAllBytes($source,$bytes)
 Run-Libre @('-b',('"{0}"' -f $source),'--sheet-type','horizontal','--sheet',('"{0}"' -f ($stem+'.png')),'--format','json-array','--data',('"{0}"' -f ($stem+'.json')),'--save-as',('"{0}"' -f ($stem+'.gif')))
}
Write-Output "Finished $($jobs.Count) native attack sources."
