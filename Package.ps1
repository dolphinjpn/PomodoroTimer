<#
.SYNOPSIS
    Builds the Pomodoro Timer WinUI 3 app as a signed Release x64 MSIX package.

.DESCRIPTION
    Uses WinApp CLI project packaging (dotnet publish + MSIX) and signs the package with a
    local self-signed development certificate whose subject matches the manifest Publisher.
    If the certificate does not exist yet, it is generated under .certs\ (gitignored) together
    with a random password stored next to it. The certificate is NOT trusted automatically;
    see README.md for the one-time elevated trust step.

.EXAMPLE
    .\Package.ps1
    .\Package.ps1 -CertPath C:\path\to\other.pfx -CertPassword 'secret'
#>
[CmdletBinding()]
param(
    # Signing certificate (.pfx). Generated on demand when it does not exist.
    [string]$CertPath = (Join-Path $PSScriptRoot '.certs\devcert.pfx'),

    # Password for the .pfx. Defaults to the local-only password file next to the certificate.
    [string]$CertPassword
)

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'PomodoroTimer\PomodoroTimer.csproj'
$manifest = Join-Path $PSScriptRoot 'PomodoroTimer\Package.appxmanifest'
$outputDir = Join-Path $PSScriptRoot 'AppPackages'
$passwordFile = [IO.Path]::ChangeExtension($CertPath, '.password')

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

# Create the development certificate (subject taken from the manifest Publisher) if missing.
if (-not (Test-Path $CertPath)) {
    if (-not $CertPassword) {
        $CertPassword = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(24))
    }
    New-Item -ItemType Directory -Force (Split-Path $CertPath) | Out-Null
    & winapp cert generate --manifest $manifest --output $CertPath --password $CertPassword --export-cer
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Set-Content -Path $passwordFile -Value $CertPassword -NoNewline
    Write-Host "Generated development certificate: $CertPath"
}
elseif (-not $CertPassword) {
    if (-not (Test-Path $passwordFile)) {
        throw "No -CertPassword given and password file not found: $passwordFile"
    }
    $CertPassword = (Get-Content $passwordFile -Raw).Trim()
}

New-Item -ItemType Directory -Force $outputDir | Out-Null
$version = ([xml](Get-Content $manifest)).Package.Identity.Version
$msixPath = Join-Path $outputDir "PomodoroTimer_${version}_x64.msix"

# winapp package publishes the project in Release, creates the MSIX, and signs it.
& winapp package $project -c Release --arch x64 --cert $CertPath --cert-password $CertPassword --output $msixPath
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ''
Write-Host "MSIX package: $msixPath"
