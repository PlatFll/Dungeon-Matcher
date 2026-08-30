[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-ValidationFailure {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message,

        [AllowNull()]
        [string]$LogPath,

        [string[]]$Details = @()
    )

    [Console]::Error.WriteLine("Unity validation failed: $Message")

    if ($Details.Count -gt 0) {
        [Console]::Error.WriteLine('Relevant Unity log lines:')
        foreach ($detail in $Details) {
            [Console]::Error.WriteLine("  $detail")
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($LogPath)) {
        if (Test-Path -LiteralPath $LogPath -PathType Leaf) {
            [Console]::Error.WriteLine("Complete Unity log: $LogPath")
        }
        else {
            [Console]::Error.WriteLine("Unity did not create a log. Intended log path: $LogPath")
        }
    }

    exit 1
}

$logPath = $null

try {
    $projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
    $projectVersionPath = Join-Path $projectRoot 'ProjectSettings\ProjectVersion.txt'

    if (-not (Test-Path -LiteralPath $projectVersionPath -PathType Leaf)) {
        throw "Project version file was not found: $projectVersionPath"
    }

    $projectVersionText = Get-Content -LiteralPath $projectVersionPath -Raw
    $versionMatch = [regex]::Match(
        $projectVersionText,
        '(?m)^m_EditorVersion:\s*(?<Version>\S+)\s*$'
    )

    if (-not $versionMatch.Success) {
        throw "Could not read m_EditorVersion from: $projectVersionPath"
    }

    $projectVersion = $versionMatch.Groups['Version'].Value

    if (-not [string]::IsNullOrWhiteSpace($env:UNITY_EXE)) {
        $unityCandidate = [Environment]::ExpandEnvironmentVariables(
            $env:UNITY_EXE.Trim().Trim('"')
        )
    }
    else {
        $programFiles = [Environment]::GetEnvironmentVariable('ProgramFiles')
        if ([string]::IsNullOrWhiteSpace($programFiles)) {
            $programFiles = 'C:\Program Files'
        }

        $unityCandidate = Join-Path $programFiles (
            "Unity\Hub\Editor\$projectVersion\Editor\Unity.exe"
        )
    }

    if (-not (Test-Path -LiteralPath $unityCandidate -PathType Leaf)) {
        throw (
            "Required Unity $projectVersion executable was not found at '$unityCandidate'. " +
            'Install that exact editor version with Unity Hub or set UNITY_EXE to its Unity.exe path.'
        )
    }

    $unityExe = (Resolve-Path -LiteralPath $unityCandidate).Path
    $installedVersion = (Get-Item -LiteralPath $unityExe).VersionInfo.ProductVersion
    $requiredVersionPattern = '^{0}(?:_|$)' -f [regex]::Escape($projectVersion)

    if (
        [string]::IsNullOrWhiteSpace($installedVersion) -or
        $installedVersion -notmatch $requiredVersionPattern
    ) {
        throw (
            "Unity executable '$unityExe' reports version '$installedVersion'; " +
            "the project requires exact version '$projectVersion'."
        )
    }

    $logFileName = 'DungeonMatcher-UnityValidation-{0}.log' -f [guid]::NewGuid()
    $logPath = Join-Path ([System.IO.Path]::GetTempPath()) $logFileName

    $unityArguments = @(
        '-batchmode'
        '-nographics'
        '-quit'
        '-projectPath'
        ('"{0}"' -f $projectRoot)
        '-logFile'
        ('"{0}"' -f $logPath)
    )

    Write-Host "Validating with Unity $projectVersion..."
    $unityProcess = Start-Process `
        -FilePath $unityExe `
        -ArgumentList $unityArguments `
        -WindowStyle Hidden `
        -Wait `
        -PassThru
    $unityExitCode = $unityProcess.ExitCode

    if (-not (Test-Path -LiteralPath $logPath -PathType Leaf)) {
        Write-ValidationFailure `
            -Message "Unity exited with code $unityExitCode without producing a log." `
            -LogPath $logPath
    }

    $failurePatterns = @(
        'error CS\d{4}'
        'Scripts have compiler errors'
        'Compilation failed'
        'Aborting batchmode due to failure'
        'already open in another Unity instance'
        'another Unity instance is running'
        'project.*already open'
    )

    $failureMatches = @(
        Select-String -LiteralPath $logPath -Pattern $failurePatterns
    )

    if ($unityExitCode -ne 0 -or $failureMatches.Count -gt 0) {
        $detailMatches = $failureMatches

        if ($detailMatches.Count -eq 0) {
            $detailMatches = @(
                Select-String -LiteralPath $logPath `
                    -Pattern '(?i)\b(error|exception|failed|failure|abort)\b' |
                    Select-Object -Last 40
            )
        }

        $details = @(
            $detailMatches |
                ForEach-Object { '{0}: {1}' -f $_.LineNumber, $_.Line.Trim() } |
                Select-Object -Unique
        )

        Write-ValidationFailure `
            -Message "Unity exited with code $unityExitCode or reported a compilation failure." `
            -LogPath $logPath `
            -Details $details
    }

    Write-Host "Unity validation succeeded with Unity $projectVersion. Log: $logPath"
    exit 0
}
catch {
    Write-ValidationFailure -Message $_.Exception.Message -LogPath $logPath
}
