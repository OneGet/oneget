
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

$f = "OneGet[$COMMITID].zip"
$n = "$OD\$f" 

zip "$n" `
"$OD\Microsoft.OneGet.dll" `
"$OD\Microsoft.OneGet.MetaProvider.PowerShell.dll" `
"$OD\Microsoft.OneGet.ServicesProvider.Common.dll" `
"$OD\Microsoft.OneGet.Utility.dll" `
"$OD\Microsoft.OneGet.Utility.PowerShell.dll" `
"$OD\Microsoft.PowerShell.OneGet.dll" `
"$OD\OneGet.PackageProvider.NuGet.dll" `
"$OD\Microsoft.OneGet.pdb" `
"$OD\Microsoft.OneGet.MetaProvider.PowerShell.pdb" `
"$OD\Microsoft.OneGet.ServicesProvider.Common.pdb" `
"$OD\Microsoft.OneGet.Utility.pdb" `
"$OD\Microsoft.OneGet.Utility.PowerShell.pdb" `
"$OD\Microsoft.PowerShell.OneGet.pdb" `
"$OD\OneGet.PackageProvider.NuGet.pdb" `
"$OD\OneGet.format.ps1xml" `
"$OD\OneGet.psd1" `
"$OD\TestChainingPackageProvider.psm1" `
"$OD\TestPackageProvider.psm1" `
"$OD\nuget.exe" `
"$OD\etc\*" 

echo ""
echo ""
echo "Binary build:"
echo "    $n"

if ($env:COMPUTERNAME -eq "gs-pc" ) {
    ipmo coapp
    copy-itemex $n coapp:\files
    echo "build can be downloaded from "
    echo "    http://downloads.coapp.org/files/$f"
}
