<#
.SYNOPSIS
    Builds the Pomodoro Timer WinUI 3 app and launches it as a packaged (debug-registered) app.

.EXAMPLE
    .\BuildAndRun.ps1
    .\BuildAndRun.ps1 -Configuration Release
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    # Return immediately after launching instead of waiting for the app to exit.
    [switch]$Detach
)

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'PomodoroTimer\PomodoroTimer.csproj'

# WinApp CLI installs an execution alias under WindowsApps, which is not always on PATH.
if (-not (Get-Command winapp -ErrorAction SilentlyContinue)) {
    $windowsApps = Join-Path $env:LOCALAPPDATA 'Microsoft\WindowsApps'
    if (Test-Path (Join-Path $windowsApps 'winapp.exe')) {
        $env:Path += ";$windowsApps"
    }
    else {
        throw 'WinApp CLI (winapp) 0.7+ is required: winget install --id Microsoft.WinAppCli --exact'
    }
}

# winapp run builds the project (dotnet build), registers the debug package identity, and launches it.
$runArgs = @('run', $project, '-c', $Configuration)
if ($Detach) {
    $runArgs += '--detach'
}

& winapp @runArgs
exit $LASTEXITCODE
