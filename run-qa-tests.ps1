[CmdletBinding()]
param(
    [ValidateSet("component", "component-templates", "playwright", "all")]
    [string]$Suite = "component",
    [switch]$NoRestore,
    [switch]$InstallPlaywright,
    [string]$BaseUrl
)

$ErrorActionPreference = "Stop"

if ($BaseUrl) {
    $env:CALENDAR_TASKING_BASE_URL = $BaseUrl.TrimEnd("/")
    Write-Host "CALENDAR_TASKING_BASE_URL=$($env:CALENDAR_TASKING_BASE_URL)"
}

$qaRoot = Join-Path $PSScriptRoot "qa"
$qaSolution = Join-Path $qaRoot "CalendarTasking.QA.sln"
$componentProject = Join-Path $qaRoot "component-tests\CalendarTasking.ComponentTests\CalendarTasking.ComponentTests.csproj"
$playwrightProject = Join-Path $qaRoot "playwright-tests\CalendarTasking.PlaywrightTests\CalendarTasking.PlaywrightTests.csproj"
$playwrightInstallScript = Join-Path $qaRoot "playwright-tests\CalendarTasking.PlaywrightTests\bin\Debug\net9.0\playwright.ps1"

function Invoke-Dotnet([string[]]$Arguments) {
    Write-Host ">> dotnet $($Arguments -join ' ')"
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command failed."
    }
}

function Test-Project([string]$ProjectPath, [string]$Filter = "") {
    $args = @("test", $ProjectPath)
    if ($NoRestore) {
        $args += "--no-restore"
    }

    if ($Filter) {
        $args += "--filter"
        $args += $Filter
    }

    $args += "-v"
    $args += "normal"
    $args += "--logger"
    $args += "console;verbosity=normal"

    Invoke-Dotnet $args
}

if (-not $NoRestore) {
    switch ($Suite) {
        "component" {
            Invoke-Dotnet @("restore", $componentProject)
        }
        "component-templates" {
            Invoke-Dotnet @("restore", $componentProject)
        }
        "playwright" {
            Invoke-Dotnet @("restore", $playwrightProject)
        }
        "all" {
            Invoke-Dotnet @("restore", $qaSolution)
        }
    }
}

if ($InstallPlaywright) {
    Invoke-Dotnet @("build", $playwrightProject)
    if (-not (Test-Path $playwrightInstallScript)) {
        throw "Playwright install script not found at '$playwrightInstallScript'. Build failed or output path changed."
    }

    Write-Host ">> pwsh $playwrightInstallScript install"
    & pwsh $playwrightInstallScript install
    if ($LASTEXITCODE -ne 0) {
        throw "Playwright browser install failed."
    }
}

switch ($Suite) {
    "component" {
        Test-Project $componentProject
    }
    "component-templates" {
        Test-Project $componentProject "FullyQualifiedName~Templates"
    }
    "playwright" {
        Test-Project $playwrightProject
    }
    "all" {
        Test-Project $qaSolution
    }
}

Write-Host ""
Write-Host "QA test run finished successfully."
