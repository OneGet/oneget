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

namespace Microsoft.OneGet.Core.Api {
    using System;
    using System.Collections.Generic;
    using Callback = System.Object;

    public interface IServicesApi {
        #region declare service-apis

        void DownloadFile(Uri remoteLocation, string localFilename, Callback c);

        bool IsSupportedArchive(string localFilename, Callback c);

        IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, Callback c);

        void AddPinnedItemToTaskbar(string item, Callback c);

        void RemovePinnedItemFromTaskbar(string item, Callback c);

        void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, Callback c);

        void SetEnvironmentVariable(string variable, string value, int context, Callback c);

        void RemoveEnvironmentVariable(string variable, int context, Callback c);

        void CopyFile(string sourcePath, string destinationPath, Callback c);

        void Delete(string path, Callback c);

        void DeleteFolder(string folder, Callback c);

        void CreateFolder(string folder, Callback c);

        void DeleteFile(string filename, Callback c);

        string GetKnownFolder(string knownFolder, Callback c);

        bool IsElevated(Callback c);
        #endregion

#if NOT_ADDED_YET

        
        void UnzipFileIncremental(string zipFile, string folder, Callback c);

        void UnzipFile(string zipFile, string folder, Callback c);


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

             string GetNuGetExePath(Callback c);

        string GetNuGetDllPath(Callback c);

#endif
    }
}