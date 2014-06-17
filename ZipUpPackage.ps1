
$COMMITID = (git rev-list --full-history --all --abbrev-commit | head -1)

$STATUS = (git status) 

if ( $STATUS -like "*Your branch is ahead*"  -or $STATUS -like "Changes not staged for commit" -or $STATUS -like "Untracked files" ) { 
    Echo "*********************************************************************"
    Echo "You must push your changes first"
    Echo "*********************************************************************"
    echo ""
    git status
    return
}

# package script
$OD = ( resolve-path .\output\v451\AnyCPU\Debug\bin\ ).Path


erase "$OD\*.zip"
# $n = [int] (([System.DateTime]::Now - [System.DateTime]::Parse("03/19/2014") ).Ticks / 80000000)
# $n = "$OD\OneGet[#$n].zip" 

$f = "OneGet-build-$COMMITID.zip"
$n = "$OD\$f" 

copy ".\release-notes.md" $OD
copy ".\readme.md"  $OD

pushd $OD
zip "$n" `
"Microsoft.OneGet.dll" `
"Microsoft.OneGet.MetaProvider.PowerShell.dll" `
"Microsoft.OneGet.ServicesProvider.Common.dll" `
"Microsoft.OneGet.Utility.dll" `
"Microsoft.OneGet.Utility.PowerShell.dll" `
"Microsoft.PowerShell.OneGet.dll" `
"OneGet.PackageProvider.NuGet.dll" `
"Microsoft.OneGet.pdb" `
"Microsoft.OneGet.MetaProvider.PowerShell.pdb" `
"Microsoft.OneGet.ServicesProvider.Common.pdb" `
"Microsoft.OneGet.Utility.pdb" `
"Microsoft.OneGet.Utility.PowerShell.pdb" `
"Microsoft.PowerShell.OneGet.pdb" `
"OneGet.PackageProvider.NuGet.pdb" `
"OneGet.format.ps1xml" `
"OneGet.psd1" `
"TestChainingPackageProvider.psm1" `
"TestPackageProvider.psm1" `
"nuget.exe" `
"etc\*" `
".\release-notes.md" `
".\readme.md" 

echo ""
echo ""
echo "Binary build:"
echo "    $n"

popd

# push up the build to the download server
if ($env:COMPUTERNAME -eq "gs-pc" ) {
    ipmo coapp
    pushd $OD
    copy-item $f oneget-latest.zip 
    
    copy-itemex -force $f coapp:\files
    copy-itemex -force oneget-latest.zip coapp:\files
    
    echo "This build can be downloaded from "
    echo "    http://downloads.coapp.org/files/$f"
    echo "or the latest is always at "
    echo "    http://downloads.coapp.org/files/oneget-latest.zip"
    popd
}
