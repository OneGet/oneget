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

Describe "get-packageprovider" {
    # make sure that packagemanagement is loaded
    import-packagemanagement

    It "lists package providers installed" {
        $x = (get-packageprovider -name "nuget").name | should match "nuget"
    }

    It "EXPECTED:  Gets The 'Programs' Package Provider" {
        $x = (get-packageprovider -name "Programs").name | should match "Programs"
    }
}

Describe "happy" -tag common {
    # make sure that packagemanagement is loaded
    import-packagemanagement

    It "looks for packages in bootstrap" {
        (find-package -provider bootstrap).Length | write-host
    }

    It "does something else" {
        $false | should be $false
    }
}

Describe "mediocre" -tag common,pristine {
    # make sure that packagemanagement is loaded
    import-packagemanagement

    It "does something useful" {
        $true | should be $true
    }
}

Describe "sad" -tag pristine {
    # make sure that packagemanagement is loaded
    import-packagemanagement

    It "does something useful" {
        $true | should be $true
    }
}

Describe "mad" -tag pristine {
    # make sure that packagemanagement is loaded
    import-packagemanagement

    It "does something useful too" {
        $true | should be $true
    }
}



