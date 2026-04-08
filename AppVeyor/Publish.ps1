if ($env:APPVEYOR_REPO_BRANCH -eq "master") {
    if (-not [String]::IsNullOrEmpty($env:PUBLISH_CLIENTSECRET)) {
        try {
            & (Join-Path $PSScriptRoot ../Scripts/Publish.ps1 -Resolve)
            Add-AppveyorMessage "Published to Store"
        }
        catch {
            Add-AppveyorMessage "Publishing to Store failed: $($_.Message)" -Category Error
            Write-Host "Publishing to Store failed."
            throw $_
        }
    }
    else {
        Add-AppveyorMessage "Environment variable PUBLISH_CLIENTSECRET is not set. " -Category Error 
        throw "Environment variable PUBLISH_CLIENTSECRET is not set."
    }
}