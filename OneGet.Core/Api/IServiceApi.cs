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
    using RequestImpl = System.Object;

    public interface IServicesApi {
        #region declare service-apis
        /* Synced/Generated code =================================================== */

        void DownloadFile(Uri remoteLocation, string localFilename, RequestImpl requestImpl);

        bool IsSupportedArchive(string localFilename, RequestImpl requestImpl);

        IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, RequestImpl requestImpl);

        void AddPinnedItemToTaskbar(string item, RequestImpl requestImpl);

        void RemovePinnedItemFromTaskbar(string item, RequestImpl requestImpl);

        void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, RequestImpl requestImpl);

        void SetEnvironmentVariable(string variable, string value, int context, RequestImpl requestImpl);

        void RemoveEnvironmentVariable(string variable, int context, RequestImpl requestImpl);

        void CopyFile(string sourcePath, string destinationPath, RequestImpl requestImpl);

        void Delete(string path, RequestImpl requestImpl);

        void DeleteFolder(string folder, RequestImpl requestImpl);

        void CreateFolder(string folder, RequestImpl requestImpl);

        void DeleteFile(string filename, RequestImpl requestImpl);

        string GetKnownFolder(string knownFolder, RequestImpl requestImpl);

        bool IsElevated(RequestImpl requestImpl);

        #endregion

#if NOT_ADDED_YET
        
        void UnzipFileIncremental(string zipFile, string folder, RequestImpl requestImpl);

        void UnzipFile(string zipFile, string folder, RequestImpl requestImpl);

        void AddFileAssociation();

        void RemoveFileAssociation();

        void AddExplorerMenuItem();

        void RemoveExplorerMenuItem();

        void AddFolderToPath();

        void RemoveFolderFromPath();

        void InstallMSI();

        void RemoveMSI();

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

        string GetNuGetExePath(RequestImpl requestImpl);

        string GetNuGetDllPath(RequestImpl requestImpl);
#endif
    }
}