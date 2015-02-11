
$origdir = (pwd)
cd $PSScriptRoot

.\xunit.console.exe .\Microsoft.OneGet.Test.dll  -noshadow -xml .\xunit-results.xml

$results = [xml](get-content -raw .\xunit-results.xml)

foreach( $assembly in $results.assemblies.assembly) {
    foreach( $collection in $assembly.collection ) {
        Describe "xUnit Collection - $($collection.Name)" {
           foreach( $test in $collection.test ) {
                
                if( $test.result -eq "Fail" ) {
                    $f = $test.failure
                    It "$($test.Name)" {
                        $msg = $f.message.innerText -replace "\n","`n"
                        $trace = $f.'stack-trace'.innerText -replace "\n","`n"
                    
                       {throw "`n$msg `n$trace" } | should not throw
                    } 
                } else {
                    It "$($test.Name)" {
                        "PASS" | should be "PASS" 
                    }
                }
            }
        }        
    }
}


cd $origdir