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

namespace Microsoft.OneGet.ServicesProvider.Common {
    internal static class Constants {
        #region copy constants-implementation
/* Synced/Generated code =================================================== */

        internal const string MSGPrefix = "MSG:";
        internal const string TerminatingError = "MSG:TerminatingError";
        internal const string SourceLocationNotValid = "MSG:SourceLocationNotValid_Location";
        internal const string UriSchemeNotSupported = "MSG:UriSchemeNotSupported_Scheme";
        internal const string UnableToResolveSource = "MSG:UnableToResolveSource_NameOrLocation";
        internal const string PackageFailedInstall = "MSG:UnableToInstallPackage_package_reason";
        internal const string DependencyResolutionError = "MSG:UnableToResolveDependency_dependencyPackage";
        internal const string DependentPackageFailedInstall = "MSG:DependentPackageFailedInstall_dependencyPackage";
        internal const string PackageProviderExists = "MSG:PackageProviderExists";
        internal const string MissingRequiredParameter = "MSG:MissingRequiredParameter";

        internal const string IsUpdateParameter = "IsUpdatePackageSource";

        internal const string NameParameter = "Name";
        internal const string LocationParameter = "Location";

        #endregion

        internal const string UnsupportedArchive = "MSG:UnsupportedArchive";
        internal const string CreatefolderFailed = "MSG:CreatefolderFailed";
        internal const string UnableToOverwriteExistingFile = "MSG:UnableToOverwriteExistingFile";
        internal const string UnableToCopyFileTo = "MSG:UnableToCopyFileTo";
        internal const string UnableToCreateShortcutTargetDoesNotExist = "MSG:UnableToCreateShortcutTargetDoesNotExist";
        internal const string RemoveEnvironmentVariableRequiresElevation = "MSG:RemoveEnvironmentVariableRequiresElevation";
        internal const string UnknownFolderId = "MSG:UnknownFolderId";
    }

    #region copy errorcategory-implementation
/* generated code ====================================================== */

    public enum ErrorCategory {
        NotSpecified,
        OpenError,
        CloseError,
        DeviceError,
        DeadlockDetected,
        InvalidArgument,
        InvalidData,
        InvalidOperation,
        InvalidResult,
        InvalidType,
        MetadataError,
        NotImplemented,
        NotInstalled,
        ObjectNotFound,
        OperationStopped,
        OperationTimeout,
        SyntaxError,
        ParserError,
        PermissionDenied,
        ResourceBusy,
        ResourceExists,
        ResourceUnavailable,
        ReadError,
        WriteError,
        FromStdErr,
        SecurityError,
        ProtocolError,
        ConnectionError,
        AuthenticationError,
        LimitsExceeded,
        QuotaExceeded,
        NotEnabled,
    }

    #endregion

}