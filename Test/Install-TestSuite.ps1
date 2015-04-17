# Installs the test suit for running.

$origDir = (pwd)
cd $PSScriptRoot

$outputPath = "$PSScriptRoot\output\debug\bin"

# make sure the output path exists before we go anywhere
if( -not (test-path $outputPath) ) {
    $null = mkdir $outputPath -ea continue
}

# put all the binaries into output path
copy -force .\*PackageManagement* $outputPath
copy -force .\*.psm1 $outputPath

# copy the xunit test runner
copy -force  ".\packages\xunit.runner.console.2*\tools\*" $outputPath

# build the xUnit tests
& "$env:SystemRoot\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" /p:Configuration=Debug .\unit.sln

# remove the strongname from the binaries -- xUnit is giving me nothing but pain from that
tools\snremove.exe -r "$outputPath\*PackageManagement*.dll"

# Running the xunit tests 
<# 
    # make sure the output path is in the module path
    $env:PSModulePath = "$env:PSModulePath;$(resolve-path $outputPath)"
    
    cd $outputPath

    .\xunit.console  .\Microsoft.PackageManagement.Test.dll -noshadow -xml output.xml
#>


# Make sure the sandbox can run
if (get-command remove-iissite -ea silentlycontinue) { 

    if (get-iissite -Name "Default Web Site" ) { 
        remove-iissite -Name "Default Web Site" -Confirm:$false
        
        if (get-iissite -Name "Default Web Site" ) { 
            throw "UNABLE TO REMOVE DEFAULT WEBSITE"
        }
    }
}

#install sandbox certificates
cd $PSScriptRoot
.\scripts\install-certificates.ps1 

# run sandbox script
cd $PSScriptRoot
.\scripts\start-sandbox.ps1 

# wait for sandbox to start
start-sleep 5

cd $PSScriptRoot
# check to see that sandbox was able to run
if( -not ( .\scripts\test-sandbox.ps1 )  ) {
    throw "UNABLE TO START SANDBOX"
}

#ok, stop it for now, seems ok.
cd $PSScriptRoot
.\scripts\stop-sandbox.ps1 

cd $origDir