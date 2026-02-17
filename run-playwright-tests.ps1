[CmdletBinding()]
param(
    [string]$BaseUrl = "http://localhost:5170",
    [switch]$InstallBrowser,
    [switch]$NoBuild,
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"

$project = "qa\playwright-tests\CalendarTasking.PlaywrightTests\CalendarTasking.PlaywrightTests.csproj"
$env:CALENDAR_TASKING_BASE_URL = $BaseUrl.TrimEnd("/")

Write-Host "CALENDAR_TASKING_BASE_URL=$($env:CALENDAR_TASKING_BASE_URL)"

if (-not $NoRestore) {
    Write-Host ">> dotnet restore $project"
    dotnet restore $project
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed."
    }
}

if (-not $NoBuild) {
    Write-Host ">> dotnet build $project"
    dotnet build $project
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed."
    }
}

if ($InstallBrowser) {
    $playwrightInstallScript = "qa\playwright-tests\CalendarTasking.PlaywrightTests\bin\Debug\net8.0\playwright.ps1"
    if (-not (Test-Path $playwrightInstallScript)) {
        throw "Playwright install script not found at '$playwrightInstallScript'. Build first."
    }

    Write-Host ">> powershell -ExecutionPolicy Bypass -File $playwrightInstallScript install"
    powershell -ExecutionPolicy Bypass -File $playwrightInstallScript install
    if ($LASTEXITCODE -ne 0) {
        throw "Playwright browser install failed."
    }
}

Write-Host ">> dotnet test $project --logger ""console;verbosity=normal"""
dotnet test $project --logger "console;verbosity=normal"
if ($LASTEXITCODE -ne 0) {
    throw "Playwright tests failed."
}

Write-Host ""
Write-Host "Playwright test run finished successfully."
