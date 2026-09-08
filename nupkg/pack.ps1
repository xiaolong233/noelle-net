<#
.SYNOPSIS
    Packs all Noelle.Net NuGet packages (.nupkg + .snupkg).

.DESCRIPTION
    Runs the test suite first (use -SkipTests to bypass), then packs every project
    under framework\src with dotnet pack (Release by default).
    Output goes to .\nupkgs next to this script unless -OutputDir is given.
    Stale .nupkg / .snupkg files in the output directory are removed first.

.PARAMETER Configuration
    Build configuration to pack (Release | Debug). Default: Release.

.PARAMETER OutputDir
    Output directory for the packages. Default: <script dir>\nupkgs.

.PARAMETER SkipTests
    Skip the test run before packing (tests run by default).

.PARAMETER Push
    After packing, push all packages to nuget.org (main packages first, then symbol
    packages). Requires the API key in the NUGET_API_KEY environment variable.
    Duplicate versions are skipped.

.EXAMPLE
    cd nupkg
    ./pack.ps1

.EXAMPLE
    ./pack.ps1 -OutputDir D:\out
    ./pack.ps1 -SkipTests
    ./pack.ps1 -Push    # NUGET_API_KEY must be set
#>
[CmdletBinding()]
param(
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',

    [string]$OutputDir,

    [switch]$SkipTests,

    [switch]$Push
)

$ErrorActionPreference = 'Stop'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'dotnet CLI not found. Install the .NET SDK first.'
}

# Locate repo layout from the script location, so the script works from any CWD.
$scriptDir = $PSScriptRoot
$rootPath  = Split-Path $scriptDir -Parent                     # repo root (nupkg\..)
$outputDir = if ($OutputDir) { $OutputDir } else { Join-Path $scriptDir 'nupkgs' }

$srcPath = Join-Path $rootPath 'framework\src'
if (-not (Test-Path $srcPath)) {
    throw "Source folder not found: $srcPath"
}

# Run tests before packing, unless skipped.
if (-not $SkipTests) {
    $slnx = Join-Path $rootPath 'framework\Noelle.Net.slnx'
    Write-Host 'Running tests...' -ForegroundColor Cyan
    dotnet test $slnx --configuration $Configuration --nologo
    if ($LASTEXITCODE -ne 0) {
        throw "Tests failed (exit code $LASTEXITCODE). Fix them or bypass with -SkipTests."
    }
    Write-Host 'Tests passed.' -ForegroundColor Green
}

# Clean up stale packages (.nupkg and .snupkg) from the output directory.
if (Test-Path $outputDir) {
    Remove-Item -Path (Join-Path $outputDir '*.nupkg'), (Join-Path $outputDir '*.snupkg') -Force -ErrorAction SilentlyContinue
}
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

$projects = @(Get-ChildItem -Path $srcPath -Filter '*.csproj' -Recurse | Sort-Object Name)
if ($projects.Count -eq 0) {
    throw "No project files found under: $srcPath"
}

$failed = @()

foreach ($project in $projects) {
    Write-Host "Packing: $($project.Name)" -ForegroundColor Cyan

    dotnet pack $project.FullName --configuration $Configuration -o $outputDir
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "PACK FAILED: $($project.FullName) (exit code $LASTEXITCODE)"
        $failed += $project.Name
    }
}

if ($failed.Count -gt 0) {
    throw "Packaging failed for: $($failed -join ', ')"
}

$mainPkgs = @(Get-ChildItem -Path $outputDir -Filter '*.nupkg' | Sort-Object Name)
$symPkgs  = @(Get-ChildItem -Path $outputDir -Filter '*.snupkg' | Sort-Object Name)

Write-Host ''
Write-Host "Packed $($mainPkgs.Count) package(s) and $($symPkgs.Count) symbol package(s) to ${outputDir}:" -ForegroundColor Green
foreach ($pkg in $mainPkgs) {
    Write-Host ('  {0,-48} {1,8:N0} KB' -f $pkg.Name, ($pkg.Length / 1KB))
}

if ($Push) {
    $apiKey = $env:NUGET_API_KEY
    if ([string]::IsNullOrWhiteSpace($apiKey)) {
        throw 'Push requested but NUGET_API_KEY environment variable is empty. Set it and run again.'
    }

    $source = 'https://api.nuget.org/v3/index.json'

    # Main packages first (consumers resolve against them), then symbol packages.
    foreach ($pkg in $mainPkgs) {
        Write-Host "Pushing: $($pkg.Name)" -ForegroundColor Cyan
        dotnet nuget push $pkg.FullName --source $source --api-key $apiKey --skip-duplicate
        if ($LASTEXITCODE -ne 0) { throw "Push failed: $($pkg.Name)" }
    }
    foreach ($pkg in $symPkgs) {
        Write-Host "Pushing: $($pkg.Name)" -ForegroundColor Cyan
        dotnet nuget push $pkg.FullName --source $source --api-key $apiKey --skip-duplicate
        if ($LASTEXITCODE -ne 0) { throw "Push failed: $($pkg.Name)" }
    }
    Write-Host 'Push completed.' -ForegroundColor Green
}
