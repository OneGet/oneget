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
    using IRequestObject = System.Object;

    public interface IProviderServices {
        bool IsElevated {get;}

        #region declare service-apis

        /* Synced/Generated code =================================================== */

        void DownloadFile(Uri remoteLocation, string localFilename, IRequestObject requestObject);

        bool IsSupportedArchive(string localFilename, IRequestObject requestObject);

        IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, IRequestObject requestObject);

        void AddPinnedItemToTaskbar(string item, IRequestObject requestObject);

        void RemovePinnedItemFromTaskbar(string item, IRequestObject requestObject);

        void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, IRequestObject requestObject);

        void SetEnvironmentVariable(string variable, string value, string context, IRequestObject requestObject);

        void RemoveEnvironmentVariable(string variable, string context, IRequestObject requestObject);

        void CopyFile(string sourcePath, string destinationPath, IRequestObject requestObject);

        void Delete(string path, IRequestObject requestObject);

        void DeleteFolder(string folder, IRequestObject requestObject);

        void CreateFolder(string folder, IRequestObject requestObject);

        void DeleteFile(string filename, IRequestObject requestObject);

        string GetKnownFolder(string knownFolder, IRequestObject requestObject);

        string CanonicalizePath(string text, string currentDirectory);

        bool FileExists(string path);

        bool DirectoryExists(string path);

        bool Install(string fileName, string additionalArgs, IRequestObject requestObject);

        bool IsSignedAndTrusted(string filename, IRequestObject requestObject);

        bool ExecuteElevatedAction(string provider, string payload, IRequestObject requestObject);

        #endregion

#if NOT_ADDED_YET
        
        void AddFileAssociation();

        void RemoveFileAssociation();

        void AddExplorerMenuItem();

        void RemoveExplorerMenuItem();

        void AddFolderToPath();

        void RemoveFolderFromPath();

        void StartProcess();

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