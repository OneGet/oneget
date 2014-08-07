// 
//  Copyright (c) Microsoft Corporation. All rights reserved. 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  

namespace Microsoft.PowerShell.OneGet.CmdLets {
    using System.Management.Automation;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Providers.Package;
    using Microsoft.OneGet.Utility.Extensions;

    public abstract class CmdletWithSource : CmdletWithProvider {
        protected CmdletWithSource(OptionCategory[] categories)
            : base(categories) {
        }

        [Parameter(ParameterSetName = Constants.SourceByInputObjectSet, Mandatory = true, ValueFromPipeline = true)]
        public PackageSource[] InputObject {get; set;}

        [Parameter()]
        public PSCredential Credential {get; set;}

        public override string GetCredentialUsername() {
            if (Credential != null) {
                return Credential.UserName;
            }
            return null;
        }

        public override string GetCredentialPassword() {
            if (Credential != null) {
                return Credential.Password.ToProtectedString("salt");
            }
            return null;
        }
    }
}