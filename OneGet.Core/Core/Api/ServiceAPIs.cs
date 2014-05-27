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
    using System.Collections.Generic;
    using Callback = System.Object;


    public interface IServicesApi {
    #region declare service-apis

        string GetNuGetExePath(Callback c);

        string GetNuGetDllPath(Callback c);

        string DownloadFile(string remoteLocation, string localLocation, Callback c);

        void AddPinnedItemToTaskbar(string item, Callback c);

        void RemovePinnedItemFromTaskbar(string item, Callback c);

        bool CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, Callback c);

        IEnumerable<string> UnzipFileIncremental(string zipFile, string folder, Callback c);

        IEnumerable<string> UnzipFile(string zipFile, string folder, Callback c);

        void AddFileAssociation();

        void RemoveFileAssociation();

        void AddExplorerMenuItem();

        void RemoveExplorerMenuItem();

        bool SetEnvironmentVariable(string variable, string value, string context, Callback c);

        bool RemoveEnvironmentVariable(string variable, string context, Callback c);

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

        void GetUserBinFolder();

        void GetSystemBinFolder();

        bool CopyFile(string sourcePath, string destinationPath, Callback c);

        void CopyFolder();

        void Delete(string path, Callback c);

        void DeleteFolder(string folder, Callback c);

        void CreateFolder(string folder, Callback c);

        void DeleteFile(string filename, Callback c);

        void BeginTransaction();

        void AbortTransaction();

        void EndTransaction();

        void GenerateUninstallScript();

        string GetKnownFolder(string knownFolder, Callback c);

        bool IsElevated(Callback c);

        object GetPackageManagementService(Callback c);

        #endregion
    }
}