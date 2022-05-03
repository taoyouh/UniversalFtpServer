# 从AppxPackages中获取appxupload格式安装包
# 从Publish\StoreBroker\Pdb中获取商店描述信息
# 自动替换已有的包和商店描述信息，并自动提交

$rootPath = Get-Location
$tenantId = $env:PUBLISH_TENANTID
$clientId = $env:PUBLISH_CLINETID
$clientSecret = $env:PUBLISH_CLIENTSECRET | ConvertTo-SecureString -AsPlainText -Force
$appId = $env:PUBLISH_APPID

$outName = "Submission"

$configPath = [System.IO.Path]::Combine($env:rootPath, "Publish\StoreBroker\SBConfig.json")
$pdpRootPath = [System.IO.Path]::Combine($env:rootPath, "Publish\StoreBroker\Pdp\")
$imageRootPath = [System.IO.Path]::Combine($env:rootPath, "Publish\StoreBroker\Images\")
$appxPath = [System.IO.Path]::Combine($env:rootPath, "AppxPackages\")

$outPath = [System.IO.Path]::Combine($env:rootPath, "Publish\SubmissionPackage\")

Write-Host "Initializing submission package path" -ForegroundColor Cyan
if (Test-Path -Path $outPath)
{
    Remove-Item -Force -Recurse -Path $outPath
}
New-Item -Type Directory -Force -Path $outPath

Write-Host "Logging in to Dev Center" -ForegroundColor Cyan
$cred = New-Object System.Management.Automation.PSCredential $clientId, $clientSecret
Set-StoreBrokerAuthentication -TenantId $tenantId -Credential $cred

Write-Host ("Looking for appxupload at " + $appxPath) -ForegroundColor Cyan
$appxuploads = (Get-ChildItem -Path $appxPath | Where-Object Name -like "*.msixupload")

Write-Host "Creating submission package:" -ForegroundColor Cyan
New-SubmissionPackage -ConfigPath $configPath -PDPRootPath $pdpRootPath -ImagesRootPath $imageRootPath -OutPath $outPath -OutName $outName -AppxPath $appxuploads.FullName

Write-Host "Submitting package to Dev Center" -ForegroundColor Cyan
Update-ApplicationSubmission -AppId $appId -SubmissionDataPath ($outPath + $outName + ".json") -PackagePath ($outPath + $outName + ".zip") -Force -ReplacePackages -UpdateListings -TargetPublishMode Manual -AutoCommit

Write-Host "Clearing Authentication" -ForegroundColor Cyan
Clear-StoreBrokerAuthentication