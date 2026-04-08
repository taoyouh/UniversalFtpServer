Write-Host "Cleaning Store Broker folder" -ForegroundColor Cyan
$installFolder = (Join-Path -Path ([System.Environment]::GetFolderPath('MyDocuments')) -ChildPath 'WindowsPowerShell\Modules\StoreBroker')
if (Test-Path -Path $installFolder) {
    Remove-Item -Force -Recurse -Path $installFolder
}
New-Item -Type Directory -Force -Path $installFolder

Write-Host "Installing Store Broker" -ForegroundColor Cyan
Push-Location -Path $installFolder
nuget install Microsoft.Windows.StoreBroker
if ($LASTEXITCODE -ne 0) {
    Write-Error ("nuget exited with code " + $LASTEXITCODE)
    Pop-Location
    throw ("nuget exited with code " + $LASTEXITCODE)
}
Move-Item -Path ".\Microsoft.Windows.StoreBroker.*" -Destination ".\StoreBroker"
Pop-Location