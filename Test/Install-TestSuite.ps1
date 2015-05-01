# Installs the test suit for running.

$origDir = (pwd)
cd $PSScriptRoot

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
cd $PSScriptRoot
foreach ($n in 1..10) {
    if( -not (& .\scripts\test-sandbox.ps1) ) {
        cd $PSScriptRoot
        Write-Host -fore Yellow "Waiting for sandbox..."
        start-sleep 1
    } else {
        break;
    }
}    

cd $PSScriptRoot
# check to see that sandbox was able to run
if( -not ( .\scripts\test-sandbox.ps1 )  ) {
    throw "UNABLE TO START SANDBOX"
}

#ok, stop it for now, seems ok.
cd $PSScriptRoot
.\scripts\stop-sandbox.ps1 

cd $origDir