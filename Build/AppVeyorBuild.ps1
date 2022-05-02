$projectPath = [System.IO.Path]::Combine($env:APPVEYOR_BUILD_FOLDER, $env:BUILD_PROJECTFILE)
$destination = [System.IO.Path]::Combine($env:APPVEYOR_BUILD_FOLDER, "AppxPackages")

Write-Host "Restoring packages" -ForegroundColor Cyan
msbuild $projectPath /t:restore /verbosity:minimal /logger:"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll" /p:PublishReadyToRun=true
if ($LASTEXITCODE -ne 0)
{
    Write-Error ("msbuild exited with code " + $LASTEXITCODE)
    exit $LASTEXITCODE
}

Write-Host "Building project" -ForegroundColor Cyan
if ($env:APPVEYOR_REPO_BRANCH -eq "master")
{
    msbuild $projectPath /verbosity:minimal /logger:"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll" /p:AppxBundlePlatforms="x86|x64|ARM64" /p:AppxPackageDir=$destination /p:AppxBundle=Always /p:UapAppxPackageBuildMode=StoreUpload /p:configuration="release"
}
else
{
    msbuild $projectPath /verbosity:minimal /logger:"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll" /p:AppxBundlePlatforms="x86" /p:AppxPackageDir=$destination /p:AppxBundle=Always /p:UapAppxPackageBuildMode=StoreUpload /p:configuration="release" /p:platform="x86"
}
if ($LASTEXITCODE -ne 0)
{
    Write-Error ("msbuild exited with code " + $LASTEXITCODE)
    exit $LASTEXITCODE
}