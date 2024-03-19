$providerName ="OneGetTest"

function Initialize-Provider     { write-debug "In $($Providername) - Initialize-Provider" }
function Get-PackageProviderName { return $Providername }
function Get-InstalledPackage { Get-Process -id "wrongid" }
