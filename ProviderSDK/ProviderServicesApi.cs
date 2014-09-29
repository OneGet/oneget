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

namespace OneGet.ProviderSDK {
    using System;
    using System.Collections.Generic;
    using RequestImpl = System.MarshalByRefObject;

    public abstract class ProviderServicesApi {
        public abstract bool IsElevated { get; }

        #region copy service-apis

        /* Synced/Generated code =================================================== */
        public abstract void DownloadFile(Uri remoteLocation, string localFilename, RequestImpl requestImpl);

        public abstract bool IsSupportedArchive(string localFilename, RequestImpl requestImpl);

        public abstract IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, RequestImpl requestImpl);

        public abstract void AddPinnedItemToTaskbar(string item, RequestImpl requestImpl);

        public abstract void RemovePinnedItemFromTaskbar(string item, RequestImpl requestImpl);

        public abstract void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, RequestImpl requestImpl);

        public abstract void SetEnvironmentVariable(string variable, string value, string context, RequestImpl requestImpl);

        public abstract void RemoveEnvironmentVariable(string variable, string context, RequestImpl requestImpl);

        public abstract void CopyFile(string sourcePath, string destinationPath, RequestImpl requestImpl);

        public abstract void Delete(string path, RequestImpl requestImpl);

        public abstract void DeleteFolder(string folder, RequestImpl requestImpl);

        public abstract void CreateFolder(string folder, RequestImpl requestImpl);

        public abstract void DeleteFile(string filename, RequestImpl requestImpl);

        public abstract string GetKnownFolder(string knownFolder, RequestImpl requestImpl);

        public abstract string CanonicalizePath(string text, string currentDirectory);

        public abstract bool FileExists(string path);

        public abstract bool DirectoryExists(string path);

        public abstract bool Install(string fileName, string additionalArgs, RequestImpl requestImpl);

        public abstract bool IsSignedAndTrusted(string filename, RequestImpl requestImpl);

        public abstract bool ExecuteElevatedAction(string provider, string payload, RequestImpl requestImpl);
        #endregion

    }
}