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
# ------------------ PackageManagement Test  ----------------------------------------------
ipmo "$PSScriptRoot\utility.psm1"


# ------------------------------------------------------------------------------
# Actual Tests:

Describe "PSGet Message Resolver" {
    # make sure that packagemanagement is loaded
    import-packagemanagement


    It "Changes message" {
        # bootstrap nuget
        get-packageprovider nuget -force
        
        $msg = powershell 'find-module -repository asdasdasd -ea silentlycontinue ; $ERROR[0].Exception.Message'
        $msg | Should match 'PSRepository' 
        $msg | Should match'module' 
        $msg | Should not match'package' 
    }
}
