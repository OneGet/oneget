# 
#  Copyright (c) Microsoft Corporation. All rights reserved. 
#  Licensed under the Apache License, Version 2.0 (the "License");
#  you may not use this file except in compliance with the License.
#  You may obtain a copy of the License at
#  http://www.apache.org/licenses/LICENSE-2.0
#  
#  Unless required by applicable law or agreed to in writing, software
#  distributed under the License is distributed on an "AS IS" BASIS,
#  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#  See the License for the specific language governing permissions and
#  limitations under the License.
#  

$origdir = (pwd)

cd $PSScriptRoot

# where stuff is
$root = resolve-path "$PSScriptRoot\.."

try {
    # quick check to see if port 80 is being listed to at all.
    if(-not (((netstat -o -n -a ) -match "0.0.0.0:80").length -gt 0 ) ){
        cd $origdir
        return $false
    }
    
    # see if it's the sandbox server listing.
    $r = wget http://localhost/about-sandbox
    
} catch {
    cd $origdir
    return $false
}

cd $origdir
return $true
