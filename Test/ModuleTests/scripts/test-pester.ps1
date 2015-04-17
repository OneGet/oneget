$origdir = (pwd)

try {
    cd $PSScriptRoot
    
    if( test-path $PSScriptRoot\..\Pester\Vendor\packages )  {
        return $true
    }
    
} finally {
    cd $origdir 
}

return $false    