$providerName ="OneGetTest"

function Initialize-Provider     { Write-Verbose "Fake provider. Should throw error" }
function Get-PackageProviderName { return "OneGetTest"
