## This just zips up the relavent binaries
ipmo coapp

function ZipFiles( $zipfilename, $sourcedir )
{
   Add-Type -Assembly System.IO.Compression.FileSystem
   $compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
   [System.IO.Compression.ZipFile]::CreateFromDirectory($sourcedir, $zipfilename, $compressionLevel, $false)
}

$n = [int] (([System.DateTime]::Now - [System.DateTime]::Parse("03/19/2014") ).Ticks / 80000000)
$n = "OneGet-#$n.zip"


erase .\OneGet*.zip
mkdir .\tmpfiles

copy  ..\..\..\..\..\LICENSE .\tmpfiles
copy Microsoft.OneGet.dll .\tmpfiles
copy Microsoft.OneGet.MetaProvider.PowerShell.dll .\tmpfiles
copy Microsoft.OneGet.Utility.dll .\tmpfiles
copy Microsoft.OneGet.Utility.PowerShell.dll .\tmpfiles
copy Microsoft.PowerShell.OneGet.dll .\tmpfiles
copy OneGet.psd1 .\tmpfiles
copy etc\PackageProviderFunctions.psm1 .\tmpfiles
copy OneGet.format.ps1xml .\tmpfiles

ZipFiles $n .\tmpFiles
rmdir -Recurse -Force  .\tmpfiles

copy-itemex -force $n oneget:providers\
copy-itemex -force $n oneget:providers\oneget.zip 