<#
.SYNOPSIS
    Publishes TeamUp.Api (.NET 10) for free IIS hosting on MonsterASP.NET.
.DESCRIPTION
    Date: 2026-11-22
    Purpose: Builds a Release package in artifacts/monsterasp-publish, strips dev config & SQLite files,
    validates required runtime assets (TeamUp.Api.dll, web.config, seeded people portraits),
    and compresses the package to artifacts/teamup-api-monsterasp.zip.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "backend\src\TeamUp.Api\TeamUp.Api.csproj"
$publishDir = Join-Path $repoRoot "artifacts\monsterasp-publish"
$zipPath = Join-Path $repoRoot "artifacts\teamup-api-monsterasp.zip"

if (Test-Path $publishDir) {
    Remove-Item -Path $publishDir -Recurse -Force
}
if (Test-Path $zipPath) {
    Remove-Item -Path $zipPath -Force
}

New-Item -ItemType Directory -Path $publishDir -Force | Out-Null

Write-Host "Publishing TeamUp.Api (Release) to $publishDir..." -ForegroundColor Cyan
& dotnet publish $projectPath -c Release -o $publishDir
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

# Remove development-only config, templates, and local SQLite files
$devArtifacts = @(
    (Join-Path $publishDir "appsettings.Development.json"),
    (Join-Path $publishDir "appsettings.Production.example.json")
)
foreach ($file in $devArtifacts) {
    if (Test-Path $file) {
        Remove-Item -Path $file -Force
    }
}
Get-ChildItem -Path $publishDir -Filter "*.dev.db*" -ErrorAction SilentlyContinue | Remove-Item -Force

# Validate required runtime deliverables
$requiredFiles = @(
    (Join-Path $publishDir "TeamUp.Api.dll"),
    (Join-Path $publishDir "web.config"),
    (Join-Path $publishDir "wwwroot\images\people\salma-khaled.jpg")
)
foreach ($req in $requiredFiles) {
    if (-not (Test-Path $req)) {
        throw "Required publish deliverable missing: $req"
    }
}

Write-Host "Compressing package to $zipPath..." -ForegroundColor Cyan
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -CompressionLevel Optimal

Write-Host ""
Write-Host "MonsterASP.NET release package ready:" -ForegroundColor Green
Write-Host "  ZIP: $zipPath"
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Copy backend/src/TeamUp.Api/appsettings.Production.example.json -> appsettings.Production.json and fill in real secrets."
Write-Host "  2. Extract teamup-api-monsterasp.zip + upload appsettings.Production.json into your MonsterASP.NET wwwroot folder (or upload & unzip in Control Panel)."
Write-Host "  3. Set ASP.NET Core Hosting Version to .NET 10 in MonsterASP.NET Control Panel -> Websites -> Scripting and restart the site."
