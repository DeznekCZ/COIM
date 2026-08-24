# Build the CustomAssets mod with full MSBuild.
#
# `dotnet build` cannot build this project: CustomAssets.csproj declares an inline
# ProcessManifest task via CodeTaskFactory, which the .NET Core MSBuild engine does not
# support (it fails with MSB4801/MSB4036, which reads like a project error but is only
# the wrong engine). This script locates Visual Studio's MSBuild and uses that.
#
# Usage:
#   ./build.ps1                 # build the core mod, errors only
#   ./build.ps1 -Verbose        # normal MSBuild output instead of errors-only
#   ./build.ps1 -Project <path> # build a different project (e.g. a pack)
[CmdletBinding()]
param(
    [string] $Project = "$PSScriptRoot\src\CustomRecipes\CustomAssets.csproj",
    [string] $Configuration = "Debug"
)

# Every game-DLL HintPath in the csproj is relative to COI_ROOT, and the project
# defaults it to %APPDATA%\Captain of Industry — which is the MODS folder, not the
# install. Set it here unless the environment already has it.
if (-not $env:COI_ROOT) {
    $env:COI_ROOT = 'C:\Program Files (x86)\Steam\steamapps\common\Captain of Industry'
}
if (-not (Test-Path $env:COI_ROOT)) {
    Write-Error "COI_ROOT does not exist: $env:COI_ROOT"
    exit 1
}

$msbuild = $null
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (Test-Path $vswhere) {
    $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild `
        -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
}
if (-not $msbuild) {
    $msbuild = Get-ChildItem 'C:\Program Files\Microsoft Visual Studio\*\*\MSBuild\Current\Bin\MSBuild.exe' `
        -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $msbuild) {
    Write-Error "MSBuild not found. Install Visual Studio with the '.NET desktop development' workload."
    exit 1
}

# Errors only by default: the project carries ~20 pre-existing warnings (CS0108 in
# PythonAPI/Expressions, CS8073, CS0649) that drown out anything new.
$logArgs = if ($VerbosePreference -eq 'Continue') { @('-v:n') } else { @('-v:m', '-clp:ErrorsOnly') }

& $msbuild $Project -nologo -p:Configuration=$Configuration @logArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "BUILD FAILED ($LASTEXITCODE)" -ForegroundColor Red
    exit $LASTEXITCODE
}
Write-Host "BUILD OK" -ForegroundColor Green
