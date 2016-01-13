$providerName ="OneGetTest"

function Initialize-Provider     { write-debug "In $($Providername) - Initialize-Provider" }
function Get-PackageProviderName { return $Providername }

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
    versionScheme="semver" xmlns="http://standards.iso.org/iso/19770/-2/2015/schema.xsd">
    <Meta
    description="jQuery is a new kind of JavaScript Library.&#xA;        jQuery is a fast and
concise JavaScript Library that simplifies HTML document traversing, event handling, animating,
and Ajax interactions for rapid web development. jQuery is designed to change the way that you
write JavaScript.&#xA;        NOTE: This package is maintained on behalf of the library owners by
the NuGet Community Packages project at http://nugetpackages.codeplex.com/"
    language="en-US"
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
    tagId="jWhat-jWhere-jWho-jQuery"
    versionScheme="semver" xmlns="http://standards.iso.org/iso/19770/-2/2015-current/schema.xsd">
    <Meta
    description="jWhat is a new kind of JavaScript Library.&#xA;        jWhat is a fast and
concise JavaScript Library that simplifies HTML document traversing, event handling, animating,
and Ajax interactions for rapid web development. jWhat is designed to change the way that you
write JavaScript.&#xA;        NOTE: This package is maintained on behalf of the library owners by
the NuGet Community Packages project at http://nugetpackages.codeplex.com/"
    language="en-US"
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
    tagId="jWhat-jWhere-jWho-jQuery"
    versionScheme="semver" xmlns="http://standards.iso.org/iso/19770/-2/2015/schema.xsd">
    <Meta
    description="jWhat is a new kind of JavaScript Library.&#xA;        jWhat is a fast and
concise JavaScript Library that simplifies HTML document traversing, event handling, animating,
and Ajax interactions for rapid web development. jWhat is designed to change the way that you
write JavaScript.&#xA;        NOTE: This package is maintained on behalf of the library owners by
the NuGet Community Packages project at http://nugetpackages.codeplex.com/"
    language="en-US"
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
    language="en-US"
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

    $id = Write-Progress -ParentId 0 -Activity "Getting some Progress"
    Write-Progress -Activity "Starting some progress" -PercentComplete 22 -Id $id
    Write-Progress -Activity "Taking some time" -PercentComplete 50 -Id $id
    Write-Progress -Activity "Finally" -PercentComplete 99 -Id $id
    Write-Progress -Completed -Id $id

    $params = @{
                FastPackageReference = "fast";
                Name = "name";
                Version = "version";
                Source = "Source";
                versionScheme  = "MultiPartNumeric";
                TagId = "MyVeryUniqueTagId";
               }

    $sid = New-SoftwareIdentity @params
    Write-Output -InputObject $sid
}
