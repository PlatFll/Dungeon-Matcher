$ErrorActionPreference='Stop'
$root=[System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$editor='C:\Users\USER\Downloads\libresprite-development-windows-x86_64\libresprite.exe'
foreach($source in Get-ChildItem -LiteralPath ($root+'\Native\Stage03') -Filter '*.ase') {
 $args=@('-b',('"'+$source.FullName+'"'),'--save-as',('"'+$root+'\QA\Stage03\'+$source.BaseName+'-roundtrip.png"'))
 $job=Start-Process -FilePath $editor -ArgumentList $args -WindowStyle Hidden -PassThru
 $job.WaitForExit(); $job.Refresh()
 if($job.ExitCode -ne 0){throw ($source.BaseName+' failed '+$job.ExitCode)}
 Write-Output ($source.BaseName+' reopened and exported')
}
