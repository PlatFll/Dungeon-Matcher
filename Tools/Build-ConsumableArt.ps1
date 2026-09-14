[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$LibreSpritePath)
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$sourceRoot=Join-Path $projectRoot 'ArtSource/Consumables'
$reference=Join-Path $sourceRoot 'ReferenceSlot.png'
foreach($artName in @('Potion','Bomb','Slot')) {
    $script=Join-Path $sourceRoot ($artName+'.js')
    $editable=Join-Path $sourceRoot ($artName+'.ase')
    $png=Join-Path $projectRoot ('Assets/_Game/Resources/UI/Consumables/'+$artName+'.png')
    $arguments=@(('"{0}"' -f $reference),'--script',('"{0}"' -f $script))
    $artProcess=Start-Process -FilePath $LibreSpritePath -ArgumentList $arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru
    $artProcess.WaitForExit();$artProcess.Refresh()
    if($artProcess.ExitCode -ne 0){throw "LibreSprite drawing failed: $artName"}
    $nativeSize=24;if($artName -eq 'Slot'){$nativeSize=32}
    $arguments=@('-b',('"{0}"' -f $editable),'--crop',("0,0,$nativeSize,$nativeSize"),'--save-as',('"{0}"' -f $editable),'--save-as',('"{0}"' -f $png))
    $artProcess=Start-Process -FilePath $LibreSpritePath -ArgumentList $arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -PassThru
    $artProcess.WaitForExit();$artProcess.Refresh()
    if($artProcess.ExitCode -ne 0){throw "LibreSprite export failed: $artName"}
    $bytes=[IO.File]::ReadAllBytes($png)
    if($bytes[19] -ne $nativeSize -or $bytes[23] -ne $nativeSize){throw "Unexpected PNG canvas for $artName"}
    Write-Output "$artName exported at ${nativeSize}x${nativeSize}."
}
