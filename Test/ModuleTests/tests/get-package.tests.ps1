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

Describe "Get-package" {
    # make sure that packagemanagement is loaded
    import-packagemanagement

    It "EXPECTED: Get-package accepts array of strings for -providername parameter" {
        $x = (get-package -providername Programs,Msi)
    }
}
