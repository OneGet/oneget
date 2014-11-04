## This just zips up the relavent binaries
ipmo coapp

function ZipFiles( $zipfilename, $sourcedir )
{
   Add-Type -Assembly System.IO.Compression.FileSystem
   $compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
   [System.IO.Compression.ZipFile]::CreateFromDirectory($sourcedir, $zipfilename, $compressionLevel, $false)
}

$n = [int] (([System.DateTime]::Now - [System.DateTime]::Parse("03/19/2014") ).Ticks / 80000000)
$n = "oneget-$n.zip"

erase .\OneGet*.zip
mkdir .\tmpfiles

copy  ..\..\..\..\..\LICENSE .\tmpfiles
copy Microsoft.OneGet.exe .\tmpfiles
copy Microsoft.OneGet.MetaProvider.PowerShell.dll .\tmpfiles
copy .\Merged\NuGet-AnyCPU.exe .\tmpfiles
copy Microsoft.OneGet.Utility.dll .\tmpfiles
copy Microsoft.OneGet.Utility.PowerShell.dll .\tmpfiles
copy Microsoft.PowerShell.OneGet.dll .\tmpfiles
copy OneGet.psd1 .\tmpfiles
copy etc\PackageProviderFunctions.psm1 .\tmpfiles
copy OneGet.format.ps1xml .\tmpfiles
copy c:\root\bin\sysinternals\streams.exe .\tmpfiles
copy RunToUnBlock.cmd .\tmpfiles
copy ReadMe.txt .\tmpfiles


#git log --pretty=oneline b0d586c636ed3a806b87c760805f5de3bb0eb97f..HEAD
$LOG = (git log "--pretty=format:~~%h [%an/%ar] %s" b0d586c636ed3a806b87c760805f5de3bb0eb97f..HEAD)

$content = Get-Content ".\tmpfiles\ReadMe.txt" 
$content = $content.replace("==LOG==",$LOG)
$content = $content.replace("~~","`r`n")
$content | out-file ".\tmpfiles\ReadMe.txt"

echo $content

ZipFiles $n .\tmpFiles
rmdir -Recurse -Force  .\tmpfiles

copy-itemex -force $n oneget:providers\
copy-itemex -force $n oneget:providers\oneget.zip 

send-tweet -Message "Posted new #OneGet *Experimental* build https://oneget.org/$n"
echo build at https://oneget.org/$n