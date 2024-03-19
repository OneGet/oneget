$providerName ="OneGetTest"

function Initialize-Provider     { write-debug "In $($Providername) - Initialize-Provider" }
function Get-PackageProviderName { return $Providername }

function Get-Feature {
	# customized output display
    Write-Output -InputObject (New-Feature -name "DisplayLongName")   
}

function Get-DynamicOptions {
    param
    (
        [Microsoft.PackageManagement.MetaProvider.PowerShell.OptionCategory] 
        $Category
    )

    switch($Category)
    {
		install
				{
                    Write-Output -InputObject (New-DynamicOption -Category $Category -Name "DisplayCulture" -ExpectedType Switch -IsRequired $false)                    
                }
        Package {
                    Write-Output -InputObject (New-DynamicOption -Category $Category -Name "DisplayCulture" -ExpectedType Switch -IsRequired $false)                    
                }

    }
}

function Get-InstalledPackage
{
    [CmdletBinding()]
    param
    (
        [string]
        $Name,

        [Version]
        $RequiredVersion,

        [Version]
        $MinimumVersion,

        [Version]
        $MaximumVersion
    )

# this package use 2015 namespace

    $package = @"
<?xml version="1.0" encoding="utf-16" standalone="yes"?>
<SoftwareIdentity
    name="jQuery"
    version="2.1.4"
    xml:lang="en-US"
    versionScheme="semver" xmlns="http://standards.iso.org/iso/19770/-2/2015/schema.xsd">
    <Meta
    description="jQuery is a new kind of JavaScript Library.&#xA;        jQuery is a fast and
concise JavaScript Library that simplifies HTML document traversing, event handling, animating,
and Ajax interactions for rapid web development. jQuery is designed to change the way that you
write JavaScript.&#xA;        NOTE: This package is maintained on behalf of the library owners by
the NuGet Community Packages project at http://nugetpackages.codeplex.com/"
    tags="jQuery"
    title="jQuery"
    developmentDependency="False" />
    <Link
    href="http://jquery.org/license"
    rel="license" />
    <Link
    href="http://jquery.com/"
    rel="project" />
    <Entity
    name="jQuery Foundation"
    regId="jQuery Foundation"
    role="author" />
    <Entity
    name="Inc."
    regId="Inc."
    role="author" />
    <Entity
    name="jQuery Foundation"
    regId="jQuery Foundation"
    role="owner" />
    <Entity
    name="Inc."
    regId="Inc."
    role="owner" />
</SoftwareIdentity>
"@

# this package use namespace 2015-current

    $package2 = @"
<?xml version="1.0" encoding="utf-16" standalone="yes"?>
<SoftwareIdentity
    name="jWhat"
    version="2.1.4"
    lang="en"
    tagId="jWhat-jWhere-jWho-jQuery"
    versionScheme="semver" xmlns="http://standards.iso.org/iso/19770/-2/2015-current/schema.xsd">
    <Meta
    description="jWhat is a new kind of JavaScript Library.&#xA;        jWhat is a fast and
concise JavaScript Library that simplifies HTML document traversing, event handling, animating,
and Ajax interactions for rapid web development. jWhat is designed to change the way that you
write JavaScript.&#xA;        NOTE: This package is maintained on behalf of the library owners by
the NuGet Community Packages project at http://nugetpackages.codeplex.com/"
    tags="jWhat"
    title="jWhat"
    developmentDependency="False" />
    <Link
    href="http://jquery.org/license"
    rel="license" />
    <Link
    href="http://jquery.com/"
    rel="project" />
    <Entity
    name="jWhat Foundation"
    regId="jWhat Foundation"
    role="author" />
    <Entity
    name="Inc."
    regId="Inc."
    role="author" />
    <Entity
    name="jWhat Foundation"
    regId="jWhat Foundation"
    role="owner" />
    <Entity
    name="Inc."
    regId="Inc."
    role="owner" />
</SoftwareIdentity>
"@

# this xml does not have closing </SoftwareIdentity>

    $errorPackage = @"
<?xml version="1.0" encoding="utf-16" standalone="yes"?>
<SoftwareIdentity
    name="jError"
    version="2.1.4"
    lang="en-US"
    tagId="jWhat-jWhere-jWho-jQuery"
    versionScheme="semver" xmlns="http://standards.iso.org/iso/19770/-2/2015/schema.xsd">
    <Meta
    description="jWhat is a new kind of JavaScript Library.&#xA;        jWhat is a fast and
concise JavaScript Library that simplifies HTML document traversing, event handling, animating,
and Ajax interactions for rapid web development. jWhat is designed to change the way that you
write JavaScript.&#xA;        NOTE: This package is maintained on behalf of the library owners by
the NuGet Community Packages project at http://nugetpackages.codeplex.com/"
    tags="jWhat"
    title="jWhat"
    developmentDependency="False" />
    <Link
    href="http://jquery.org/license"
    rel="license" />
    <Link
    href="http://jquery.com/"
    rel="project" />
    <Entity
    name="jWhat Foundation"
    regId="jWhat Foundation"
    role="author" />
    <Entity
    name="Inc."
    regId="Inc."
    role="author" />
    <Entity
    name="jWhat Foundation"
    regId="jWhat Foundation"
    role="owner" />
    <Entity
    name="Inc."
    regId="Inc."
    role="owner" />
"@

# this package has 2014 namespace, which is wrong (don't support)

    $errorPackage2 = @"
<?xml version="1.0" encoding="utf-16" standalone="yes"?>
<SoftwareIdentity
    name="jErrrr"
    version="2.1.4"
    tagId="jWhat-jWhere-jWho-jQuery"
    versionScheme="semver" xmlns="http://standards.iso.org/iso/19770/-2/2014-current/schema.xsd">
    <Meta
    description="jWhat is a new kind of JavaScript Library.&#xA;        jWhat is a fast and
concise JavaScript Library that simplifies HTML document traversing, event handling, animating,
and Ajax interactions for rapid web development. jWhat is designed to change the way that you
write JavaScript.&#xA;        NOTE: This package is maintained on behalf of the library owners by
the NuGet Community Packages project at http://nugetpackages.codeplex.com/"
    tags="jWhat"
    title="jWhat"
    developmentDependency="False" />
    <Link
    href="http://jquery.org/license"
    rel="license" />
    <Link
    href="http://jquery.com/"
    rel="project" />
    <Entity
    name="jWhat Foundation"
    regId="jWhat Foundation"
    role="author" />
    <Entity
    name="Inc."
    regId="Inc."
    role="author" />
    <Entity
    name="jWhat Foundation"
    regId="jWhat Foundation"
    role="owner" />
    <Entity
    name="Inc."
    regId="Inc."
    role="owner" />
</SoftwareIdentity>
"@

    $i = 0
    while ($i -lt 101) {
        Write-Progress -Activity Updating -PercentComplete $i -Id 10
        $i += 25

        for($j = 0; $j -lt 101; $j+=25) {
            Write-Progress -Activity "Updating Inner" -PercentComplete $j -ParentId 10
        }
    }

    Write-Output (New-SoftwareIdentityFromXml $package)
    Write-Output (New-SoftwareIdentityFromXml $package2)

    Write-Output (New-SoftwareIdentityFromXml $errorPackage)
    Write-Output (New-SoftwareIdentityFromXml $errorPackage2)

}

function Find-Package
{ 
    [CmdletBinding()]
    param
    (
        [string[]]
        $names,

        [string]
        $requiredVersion,

        [string]
        $minimumVersion,

        [string]
        $maximumVersion
    )

    Write-Progress -ParentId 0 -Activity "Getting some Progress"
    Write-Progress -Activity "Starting some progress" -PercentComplete 22 -CurrentOperation Starting -SecondsRemaining 10
    Write-Progress -Activity "Taking some time" -PercentComplete 50 -SecondsRemaining 5
    Write-Progress -Activity "Finally" -PercentComplete 99 -SecondsRemaining 1
    Write-Progress -Activity "Complete" -PercentComplete 100 -Completed

    $i = 0
    while ($i -lt 100) {
        Write-Progress -Activity Updating -PercentComplete $i -Status "Finding packages"
        $i += 25

        for($j = 0; $j -lt 101; $j+=25) {
            Write-Progress -Activity "Updating Inner" -PercentComplete $j -Id 5 -Status "Finding inner packages"
        }
    }


    $params = @{
                FastPackageReference = "fast";
                Name = "11160201-1500_amd64fre_ServerDatacenterCore_en-us.wim";
                Version = "1.0.0.0";
                Source = "from a funland";
                versionScheme  = "MultiPartNumeric";
                TagId = "MyVeryUniqueTagId";
				Culture ="en-US"
               }

    $sid = New-SoftwareIdentity @params
    Write-Output -InputObject $sid


	if (!$names)
	{
		$params2 = @{
					FastPackageReference = "fast";
					Name = "22160201-1500_amd64fre_ServerDatacenterCore_en-us.wim";
					Version = "1.0.0.1";
					Source = "from a neverland";
					versionScheme  = "MultiPartNumeric";
					TagId = "MyVeryUniqueTagId";
					Culture ="de-de"
				   }

		$sid2 = New-SoftwareIdentity @params2
		Write-Output -InputObject $sid2
	}

}

function Install-Package
{ 
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $fastPackageReference
    )

    $params = @{
                FastPackageReference = "fast";
                Name = "11160201-1500_amd64fre_ServerDatacenterCore_en-us.wim";
                Version = "1.0.0.0";
                Source = "from a funland";
                versionScheme  = "MultiPartNumeric";
                TagId = "MyVeryUniqueTagId";
				Culture ="en-US";
                Destination = "$env:TMP\Test"
               }

    $sid = New-SoftwareIdentity @params
    Write-Output -InputObject $sid
}

function Download-Package
{ 
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $FastPackageReference,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Location
    )
   
    Write-Debug ($LocalizedData.ProviderDebugMessage -f ('Download-Package'))
    Write-Debug -Message ($LocalizedData.FastPackageReference -f $fastPackageReference)
	

    <#
        You need to add code here in your real provider:
     1. parse the FastPackageReference for package name, version, source etc.
     2. Find the matched source from the registered ones
     3. Use the Source to download packages
    #>

    Install-Package -FastPackageReference $fastPackageReference
}