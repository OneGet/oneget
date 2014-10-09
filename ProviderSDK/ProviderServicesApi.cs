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
    using IRequestObject = System.MarshalByRefObject;

#if SEPERATE_PROVIDER_API
    public abstract class ProviderServicesApi {
        public abstract bool IsElevated { get; }

        #region copy service-apis

        /* Synced/Generated code =================================================== */
        public abstract void DownloadFile(Uri remoteLocation, string localFilename, IRequestObject requestObject);

        public abstract bool IsSupportedArchive(string localFilename, IRequestObject requestObject);

        public abstract IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, IRequestObject requestObject);

        public abstract void AddPinnedItemToTaskbar(string item, IRequestObject requestObject);

        public abstract void RemovePinnedItemFromTaskbar(string item, IRequestObject requestObject);

        public abstract void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, IRequestObject requestObject);

        public abstract void SetEnvironmentVariable(string variable, string value, string context, IRequestObject requestObject);

        public abstract void RemoveEnvironmentVariable(string variable, string context, IRequestObject requestObject);

        public abstract void CopyFile(string sourcePath, string destinationPath, IRequestObject requestObject);

        public abstract void Delete(string path, IRequestObject requestObject);

        public abstract void DeleteFolder(string folder, IRequestObject requestObject);

        public abstract void CreateFolder(string folder, IRequestObject requestObject);

        public abstract void DeleteFile(string filename, IRequestObject requestObject);

        public abstract string GetKnownFolder(string knownFolder, IRequestObject requestObject);

        public abstract string CanonicalizePath(string text, string currentDirectory);

        public abstract bool FileExists(string path);

        public abstract bool DirectoryExists(string path);

        public abstract bool Install(string fileName, string additionalArgs, IRequestObject requestObject);

        public abstract bool IsSignedAndTrusted(string filename, IRequestObject requestObject);

        public abstract bool ExecuteElevatedAction(string provider, string payload, IRequestObject requestObject);
        #endregion

    }
#endif
}