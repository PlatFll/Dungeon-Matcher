param([switch]$ExportOnly)
$ErrorActionPreference='Stop'
$kitRoot=$PSScriptRoot
$projectRoot=[IO.Path]::GetFullPath((Join-Path $kitRoot '../../..'))
$librePath='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
function Get-NativeSha256([string]$path){
 $hasher=[Security.Cryptography.SHA256]::Create()
 try {return ([BitConverter]::ToString($hasher.ComputeHash([IO.File]::ReadAllBytes($path)))).Replace('-','').ToLowerInvariant()}
 finally {$hasher.Dispose()}
}
$kitScript=Join-Path $kitRoot 'logs/build.js'
if(-not $ExportOnly){
 $buildRoot=Join-Path $kitRoot ('logs/build-'+[Guid]::NewGuid().ToString('N'))
 New-Item -ItemType Directory -Path $buildRoot | Out-Null
 $source=(Get-Content -LiteralPath (Join-Path $kitRoot 'Build.js') -Raw).Replace('__TOP_RECONSTRUCTION__',(Get-Content -LiteralPath (Join-Path $kitRoot 'TopReconstruction.js') -Raw)).Replace('__LAYERED_MASTER__',(Get-Content -LiteralPath (Join-Path $kitRoot 'LayeredMaster.js') -Raw)).Replace('__KIT__',$kitRoot.Replace('\','/')).Replace('__PROJECT__',$projectRoot.Replace('\','/')).Replace('__OUTPUT__',$buildRoot.Replace('\','/'))
 $source=(Get-Content -LiteralPath (Join-Path $kitRoot 'Palette.js') -Raw)+"`n"+$source
 [IO.File]::WriteAllText($kitScript,"try {`n"+$source+"`n} catch(e) {console.log('KIT_FAILED '+e);} app.command.Exit();")
 $job=Start-Process -FilePath $librePath -ArgumentList @('--script',('"{0}"' -f $kitScript)) -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $kitRoot 'logs/build.log') -RedirectStandardError (Join-Path $kitRoot 'logs/build-errors.log')
 $handle=$job.Handle
 if(-not $job.WaitForExit(60000)){$job.Kill();throw 'Owned LibreSprite build exceeded one minute; inspect logs.'}
 $job.WaitForExit();$job.Refresh()
 if($job.ExitCode -ne 0 -or -not (Select-String -LiteralPath (Join-Path $kitRoot 'logs/build.log') -Pattern '^KIT_COMPLETE' -Quiet)){throw 'LibreSprite build failed; inspect logs/build.log'}
 $manifest=(Select-String -LiteralPath (Join-Path $kitRoot 'logs/build.log') -Pattern '^KIT_MANIFEST ').Line.Substring(13)
 [IO.File]::WriteAllText((Join-Path $kitRoot 'layout.json'),$manifest)
 Get-ChildItem -LiteralPath $buildRoot -Filter '*.aseprite' | Copy-Item -Destination (Join-Path $kitRoot 'native') -Force
}
& (Join-Path $projectRoot 'Tools/Normalize-FinalizedSourceLayers.ps1') -SourceRoot (Join-Path $kitRoot 'native')
foreach($asset in Get-ChildItem -LiteralPath (Join-Path $kitRoot 'native') -Filter '*.aseprite'){
 $job=Start-Process -FilePath $librePath -ArgumentList @('--batch',('"{0}"' -f $asset.FullName),'--save-as',('"{0}"' -f ([IO.Path]::ChangeExtension($asset.FullName,'.png')))) -WindowStyle Hidden -Wait -PassThru -RedirectStandardOutput (Join-Path $kitRoot 'logs/export.log') -RedirectStandardError (Join-Path $kitRoot 'logs/export-errors.log')
 if($job.ExitCode -ne 0){throw "Export failed: $($asset.Name)"}
}
$receipt=@{
 status='Completed';completedUtc=[DateTime]::UtcNow.ToString('o')
 sourceSha256=Get-NativeSha256 (Join-Path $kitRoot 'Build.js')
 sourceFiles=@{
  'Build.js'=Get-NativeSha256 (Join-Path $kitRoot 'Build.js')
  'TopReconstruction.js'=Get-NativeSha256 (Join-Path $kitRoot 'TopReconstruction.js')
  'Palette.js'=Get-NativeSha256 (Join-Path $kitRoot 'Palette.js')
  'LayeredMaster.js'=Get-NativeSha256 (Join-Path $kitRoot 'LayeredMaster.js')
 }
 editor='LibreSprite 1.1-dev';editorSha256=Get-NativeSha256 $librePath
 aseCount=@(Get-ChildItem -LiteralPath (Join-Path $kitRoot 'native') -Filter '*.aseprite').Count
 generationCalls=0;unityIntegration=$false
}
$receipt | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $kitRoot 'build-receipt.json') -Encoding UTF8
Write-Output 'Dungeon-only native kit saved, reopened and exported by LibreSprite.'
