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

namespace Microsoft.OneGet.Api {
    using System;
    using System.Collections.Generic;
    using Packaging;

    public interface IProviderServices {
        #region declare service-apis

        bool IsElevated {get;}

        IEnumerable<SoftwareIdentity> FindPackageByCanonicalId(string canonicalId, IRequest requestObject);

        string GetCanonicalPackageId(string providerName, string packageName, string version, string source);

        string ParseProviderName(string canonicalPackageId);

        string ParsePackageName(string canonicalPackageId);

        string ParsePackageVersion(string canonicalPackageId);

        string ParsePackageSource(string canonicalPackageId);

        void DownloadFile(Uri remoteLocation, string localFilename, IRequest requestObject);

        bool IsSupportedArchive(string localFilename, IRequest requestObject);

        IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, IRequest requestObject);

        void AddPinnedItemToTaskbar(string item, IRequest requestObject);

        void RemovePinnedItemFromTaskbar(string item, IRequest requestObject);

        void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, IRequest requestObject);

        void SetEnvironmentVariable(string variable, string value, string context, IRequest requestObject);

        void RemoveEnvironmentVariable(string variable, string context, IRequest requestObject);

        void CopyFile(string sourcePath, string destinationPath, IRequest requestObject);

        void Delete(string path, IRequest requestObject);

        void DeleteFolder(string folder, IRequest requestObject);

        void CreateFolder(string folder, IRequest requestObject);

        void DeleteFile(string filename, IRequest requestObject);

        string GetKnownFolder(string knownFolder, IRequest requestObject);

        string CanonicalizePath(string text, string currentDirectory);

        bool FileExists(string path);

        bool DirectoryExists(string path);

        bool Install(string fileName, string additionalArgs, IRequest requestObject);

        bool IsSignedAndTrusted(string filename, IRequest requestObject);

        bool ExecuteElevatedAction(string provider, string payload, IRequest requestObject);

        int StartProcess(string filename, string arguments, bool requiresElevation, out string standardOutput, IRequest requestObject);

        #endregion

#if NOT_ADDED_YET
        
        void AddFileAssociation();

        void RemoveFileAssociation();

        void AddExplorerMenuItem();

        void RemoveExplorerMenuItem();

        void AddFolderToPath();

        void RemoveFolderFromPath();

        void InstallVSIX();

        void UninstallVSIX();

        void InstallPowershellScript();

        void UninstallPowershellScript();

        void SearchForExecutable();
        
        void CopyFolder();

        void BeginTransaction();

        void AbortTransaction();

        void EndTransaction();
        
        void GenerateUninstallScript();
#endif
    }
}