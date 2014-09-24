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

namespace Microsoft.OneGet {
    using System;
    using System.Collections.Generic;
    using Utility.Extensions;

    public interface IProviderServices {
        #region declare service-apis
        /* Synced/Generated code =================================================== */

        void DownloadFile(Uri remoteLocation, string localFilename, Object requestImpl);

        bool IsSupportedArchive(string localFilename, Object requestImpl);

        IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, Object requestImpl);

        void AddPinnedItemToTaskbar(string item, Object requestImpl);

        void RemovePinnedItemFromTaskbar(string item, Object requestImpl);

        void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, Object requestImpl);

        void SetEnvironmentVariable(string variable, string value, string context, Object requestImpl);

        void RemoveEnvironmentVariable(string variable, string context, Object requestImpl);

        void CopyFile(string sourcePath, string destinationPath, Object requestImpl);

        void Delete(string path, Object requestImpl);

        void DeleteFolder(string folder, Object requestImpl);

        void CreateFolder(string folder, Object requestImpl);

        void DeleteFile(string filename, Object requestImpl);

        string GetKnownFolder(string knownFolder, Object requestImpl);

        bool IsElevated {get;}

        string CanonicalizePath(string text, string currentDirectory);

        bool FileExists(string path);

        bool DirectoryExists(string path);

        bool Install(string fileName, string additionalArgs, Object requestImpl);

        bool IsSignedAndTrusted(string filename, Object requestImpl);

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