$IsLegacyTestRun = (Get-Variable -Name IsLegacyTestRun -ErrorAction Ignore) -and $global:IsLegacyTestRun

Describe "Legacy tests" -Tags "Legacy" {
    It "Blocks import on legacy PowerShell Core" -Skip:((-not $IsLegacyTestRun)) {
        try { Import-Module PackageManagement -RequiredVersion 1.1.5.0 } catch {}
        Get-Module PackageManagement | should benullorempty
    }
}