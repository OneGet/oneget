###
# ==++==
#
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#
###
@{
    GUID = "0069E2B7-9D7D-4441-B549-827381DF0739"
    Author = "Microsoft Corporation"
    CompanyName = "Microsoft Corporation"
    Copyright = "(C) Microsoft Corporation. All rights reserved."
    HelpInfoUri = "http://go.microsoft.com/fwlink/?linkid=392040"
    ModuleVersion = "1.1.0.0"
    PowerShellVersion = "3.0"
    ClrVersion = "4.0"

    # force loading of the community build of OneGet and the PowerShellGet that goes along with it.
    NestedModules = @('..\oneget\oneget.psd1','..\powershellget\powershellget.psd1')
}
