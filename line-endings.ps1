$files = dir -recurse *.* | ? { !$_.PSIsContainer } |  where {$_ -notmatch '\\nuget\\' } | where {$_ -notmatch '\\output\\' } | where {$_ -notmatch '\\intermediate\\' } | where {$_ -notmatch '\\obj\\' } | where {$_ -notmatch '\\bin\\' } | where {$_ -notmatch '\\attic\\' } | where {$_ -notmatch '\\packages\\' } | where {$_ -notmatch '\\testresults\\' } | where {$_ -notmatch '\\tools\\' } | where {$_ -notmatch '.pdb' }

foreach( $file in $files) {

    $byteArray = Get-Content  $file.FullName -Encoding Byte  -TotalCount 1024
    if( $byteArray -contains 0 ) {
        echo "$file => BINARY"
        continue;
    }
    

    $content = get-content -raw $file.FullName
    if( -NOT $content ){
    continue;
    }

    #turn CRLF to another character so we can see the difference between stuff
    $content = $content.replace("`r`n","`b" ) 

    $CRs = $content.split("`r").Length -1
    $LFs = $content.split("`n").Length -1
    $CRLFs = $content.split("`b").Length -1

    if( $CRs -eq 0 -and $LFs -eq 0 -and $CRLFs -gt 0 ) {
       # echo "$file => CRLF"
        continue;
    }

    if( $CRs -eq 0 -and $LFs -eq 0 -and $CRLFs -eq 0 ) {
        echo "$file => NONE"
        continue;
    }
    if( $CRs -gt 0 -and $LFs -eq 0 -and $CRLFs -eq 0 ) {
        echo "$file => CR"
        continue;
    }

    if( $CRs -eq 0 -and $LFs -gt 0 -and $CRLFs -eq 0 ) {
        echo "$file => LF"
        continue;
    }
    
    echo "$file => OTHER"
    continue;
    

}