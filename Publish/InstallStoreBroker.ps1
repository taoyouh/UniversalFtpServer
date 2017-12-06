Write-Host "Cleaning Store Broker folder" -ForegroundColor Cyan
$installFolder = [System.IO.Path]::Combine($Home, "Documents\WindowsPowerShell\Modules\StoreBroker");
if (Test-Path -Path $installFolder)
{
    Remove-Item -Force -Recurse -Path $installFolder
}
New-Item -Type Directory -Force -Path $installFolder

Write-Host "Installing Store Broker" -ForegroundColor Cyan
Push-Location -Path $installFolder
nuget install Microsoft.Windows.StoreBroker
if ($LASTEXITCODE -ne 0)
{
    Write-Error ("msbuild exited with code " + $LASTEXITCODE)
    Pop-Location
    EXIT $LASTEXITCODE
}
Move-Item -Path ".\Microsoft.Windows.StoreBroker.*" -Destination ".\StoreBroker"
Pop-Location