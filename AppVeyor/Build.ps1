$extraParams = '/logger:"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll"'
if ($env:APPVEYOR_REPO_BRANCH -eq "master") {
    $platform = "x86|x64|ARM64"
}
else {
    $platform = "x64"
}

& (Join-Path $PSScriptRoot ../Scripts/Build.ps1 -Resolve)