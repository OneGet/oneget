

Param(
	[string]$build = ''
)

if( $build ) {
    # check if it's pointing to a module 
    powershell ipmo $build | out-null

    if( $lastexitcode -eq 0 ) {
      #it's a path to a powershell module
      $module = $build 
      $testdll = $null
    } else { 
        if( test-path $build)  {
        # points to a build directory
            $module = "$build\oneget.psd1"
            $testdll = "$build\Microsoft.OneGet.Test.dll"
        } else {
            # otherwise, it's a build configuration.
            if( test-path "$PSScriptRoot\output\v45\$build\bin\" ) {
                $testdll = "$PSScriptRoot\output\v45\$build\bin\Microsoft.OneGet.Test.dll"
                $module = "$PSScriptRoot\output\v45\$build\bin\oneget.psd1"
            } else {
                Write-host -fore red "can't find OneGet module given '$build'"
                return;
            }
        }
    }
} else {
    # autodetect based on what is newer
    if( (test-path $PSScriptRoot\output\v45\Debug\bin\microsoft.oneget.exe) -and (test-path $PSScriptRoot\output\v45\release\bin\microsoft.oneget.exe) ) {
        if(  (dir $PSScriptRoot\output\v45\Debug\bin\microsoft.oneget.exe).LastWriteTime  -gt  (dir $PSScriptRoot\output\v45\release\bin\microsoft.oneget.exe).LastWriteTime ) {
            $module = "$PSScriptRoot\output\v45\Debug\bin\oneget.psd1"
            $testdll = "$PSScriptRoot\output\v45\Debug\bin\Microsoft.OneGet.Test.dll"
        }  else  {
            $module = "$PSScriptRoot\output\v45\Release\bin\oneget.psd1"
            $testdll = "$PSScriptRoot\output\v45\Release\bin\Microsoft.OneGet.Test.dll"
        }
    } else {
        if (test-path $PSScriptRoot\output\v45\Debug\bin\microsoft.oneget.exe)  {
            $module = "$PSScriptRoot\output\v45\Debug\bin\oneget.psd1"
            $testdll = "$PSScriptRoot\output\v45\Debug\bin\Microsoft.OneGet.Test.dll"
        } else {
            if (test-path $PSScriptRoot\output\v45\Release\bin\microsoft.oneget.exe)  {
                $module = "$PSScriptRoot\output\v45\Release\bin\oneget.psd1"
                $testdll = "$PSScriptRoot\output\v45\Relase\bin\Microsoft.OneGet.Test.dll"
            }
        }
    }
}

    if( -not (test-path $module) ) {
        Write-host -fore red "can't find OneGet module at '$module'"
        return;
    } else {
        Write-host -fore green "OneGet Module to test: '$module'"
    }
    if( $testdll ) {
        if(-not (test-path $testdll) ) {
            Write-host -fore yellow "can't find test assembly at '$testdll' -- skipping unit tests"
            $testdll = $null
        } else {
            Write-host -fore green "OneGet TestDLL to test: '$testdll'"
        }   
    } else {
        Write-host -fore yellow "No test assembly selected -- skipping unit tests"    
    }

if( $testdll ) {
    cd $PSSCRIPTROOT
    # run unit tests
    .\packages\xunit.runners.2.0.0-beta5-build2785\tools\xunit.console.exe $testdll -noshadow -xml .\xunit-test-results.xml
    
} 

cd $PSSCRIPTROOT
# run the OneGet Pester Tests
.\test\BVT\cmdlet-testsuite\test-oneget.ps1 $module 
#-enableSandbox 

cd $PSSCRIPTROOT
#.\test\BVT\cmdlet-testsuite\scripts\stop-sandbox.ps1

cd $PSSCRIPTROOT
# powershell "ipmo $module; ipmo powershellget; cd c:\tests\psget\tests\psgettests\; .\invoketest.ps1 -priority all"