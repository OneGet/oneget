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

namespace Microsoft.PackageManagement.Utility.PowerShell {
    internal static class Constants {
        internal const string MSGPrefix = "MSG:";
        internal static object[] NoParameters = new object[0];

        internal static class Messages {
            internal const string MoreThanOneFileMatched = "MSG:MoreThanOneFileMatched";
            internal const string FileNotFound = "MSG:FileNotFound";
            internal const string FolderNotFound = "MSG:FolderNotFound";
            internal const string MoreThanOneFolderMatched = "MSG:MoreThanOneFolderMatched";
        }

        internal static class Methods {
            internal const string StopProcessingAsyncMethod = "StopProcessingAsync";
            internal const string ProcessRecordAsyncMethod = "ProcessRecordAsync";
            internal const string GenerateDynamicParametersMethod = "GenerateDynamicParameters";
            internal const string BeginProcessingAsyncMethod = "BeginProcessingAsync";
            internal const string EndProcessingAsyncMethod = "EndProcessingAsync";
        }

        internal static class Parameters {
            internal const string ConfirmParameter = "Confirm";
            internal const string WhatIfParameter = "WhatIf";
        }
    }
}
