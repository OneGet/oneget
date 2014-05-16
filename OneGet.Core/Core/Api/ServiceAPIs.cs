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
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    #region declare service-apis

    public delegate string GetNuGetExePath(Callback c);

    public delegate string GetNuGetDllPath(Callback c);

    public delegate string DownloadFile(string remoteLocation, string localLocation, Callback c);

    public delegate void AddPinnedItemToTaskbar(string item, Callback c);

    public delegate void RemovePinnedItemFromTaskbar(string item, Callback c);

    public delegate bool CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, Callback c);

    public delegate IEnumerable<string> UnzipFileIncremental(string zipFile, string folder, Callback c);

    public delegate IEnumerable<string> UnzipFile(string zipFile, string folder, Callback c);

    public delegate void AddFileAssociation();

    public delegate void RemoveFileAssociation();

    public delegate void AddExplorerMenuItem();

    public delegate void RemoveExplorerMenuItem();

    public delegate bool SetEnvironmentVariable(string variable, string value, string context, Callback c);

    public delegate bool RemoveEnvironmentVariable(string variable, string context, Callback c);

    public delegate void AddFolderToPath();

    public delegate void RemoveFolderFromPath();

    public delegate void InstallMSI();

    public delegate void RemoveMSI();

    public delegate void StartProcess();

    public delegate void InstallVSIX();

    public delegate void UninstallVSIX();

    public delegate void InstallPowershellScript();

    public delegate void UninstallPowershellScript();

    public delegate void SearchForExecutable();

    public delegate void GetUserBinFolder();

    public delegate void GetSystemBinFolder();

    public delegate bool CopyFile(string sourcePath, string destinationPath, Callback c);

    public delegate void CopyFolder();

    public delegate void Delete(string path, Callback c);

    public delegate void DeleteFolder(string folder, Callback c);

    public delegate void CreateFolder(string folder, Callback c);

    public delegate void DeleteFile(string filename, Callback c);

    public delegate void BeginTransaction();

    public delegate void AbortTransaction();

    public delegate void EndTransaction();

    public delegate void GenerateUninstallScript();

    public delegate string GetKnownFolder(string knownFolder, Callback c);

    public delegate bool IsElevated(Callback c);

    public delegate object GetPackageManagementService(Callback c);

    #endregion
}