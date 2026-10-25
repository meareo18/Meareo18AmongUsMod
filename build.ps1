# Script de compilación reproducible para Meareo18 Among Us Mod
param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "=========================================================="
Write-Host "   COMPILADOR MEAREO18 AMONG US MOD (C# / NET8.0 / ARM64) "
Write-Host "=========================================================="

$dotnetCmd = "dotnet"
if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    $userDotnet = Join-Path $env:USERPROFILE ".dotnet\dotnet.exe"
    if (Test-Path $userDotnet) {
        $dotnetCmd = $userDotnet
    } else {
        Write-Error "[ERROR] .NET SDK no encontrado."
        exit 1
    }
}

& $dotnetCmd build src\Meareo18Mod\Meareo18Mod.csproj -c $Configuration
