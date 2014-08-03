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

namespace Microsoft.OneGet.Providers.Service {
    using System;
    using System.Collections.Generic;
    using Package;
    using Utility.Extensions;
    using RequestImpl = System.Object;

    internal class ServicesProvider : ProviderBase<IServicesProvider> {
        private string _name;

        internal ServicesProvider(IServicesProvider provider) : base(provider) {
        }

        public override string Name {
            get {
                return _name ?? (_name = Provider.GetServicesProviderName());
            }
        }

        public void DownloadFile(Uri remoteLocation, string localFilename, RequestImpl requestImpl) {
            Provider.DownloadFile(remoteLocation, localFilename, ExtendRequest(requestImpl));
        }

        public bool IsSupportedArchive(string localFilename, RequestImpl requestImpl) {
            return Provider.IsSupportedArchive(localFilename, ExtendRequest(requestImpl));
        }

        public IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, RequestImpl requestImpl) {
            return Provider.UnpackArchive(localFilename, destinationFolder, ExtendRequest(requestImpl)).ByRefEnumerable();
        }

        public void AddPinnedItemToTaskbar(string item, RequestImpl requestImpl) {
            Provider.AddPinnedItemToTaskbar(item, ExtendRequest(requestImpl));
        }

        public void RemovePinnedItemFromTaskbar(string item, RequestImpl requestImpl) {
            Provider.RemovePinnedItemFromTaskbar(item, ExtendRequest(requestImpl));
        }

        public void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, RequestImpl requestImpl) {
            Provider.CreateShortcutLink(linkPath, targetPath, description, workingDirectory, arguments, ExtendRequest(requestImpl));
        }

        public void SetEnvironmentVariable(string variable, string value, int context, RequestImpl requestImpl) {
            Provider.SetEnvironmentVariable(variable, value, context, ExtendRequest(requestImpl));
        }

        public void RemoveEnvironmentVariable(string variable, int context, RequestImpl requestImpl) {
            Provider.RemoveEnvironmentVariable(variable, context, ExtendRequest(requestImpl));
        }

        public void CopyFile(string sourcePath, string destinationPath, RequestImpl requestImpl) {
            Provider.CopyFile(sourcePath, destinationPath, ExtendRequest(requestImpl));
        }

        public void Delete(string path, RequestImpl requestImpl) {
            Provider.Delete(path, ExtendRequest(requestImpl));
        }

        public void DeleteFolder(string folder, RequestImpl requestImpl) {
            Provider.DeleteFolder(folder, ExtendRequest(requestImpl));
        }

        public void CreateFolder(string folder, RequestImpl requestImpl) {
            Provider.CreateFolder(folder, ExtendRequest(requestImpl));
        }

        public void DeleteFile(string filename, RequestImpl requestImpl) {
            Provider.DeleteFile(filename, ExtendRequest(requestImpl));
        }

        public string GetKnownFolder(string knownFolder, RequestImpl requestImpl) {
            return Provider.GetKnownFolder(knownFolder, ExtendRequest(requestImpl));
        }

        public bool IsElevated(RequestImpl requestImpl) {
            return Provider.IsElevated(ExtendRequest(requestImpl));
        }
    }
}