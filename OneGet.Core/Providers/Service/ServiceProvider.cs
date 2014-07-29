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
    using Api;
    using Extensions;
    using Package;
    using Plugin;

    internal class ServicesProvider : ProviderBase<IServicesProvider> {
        private string _name;

        internal ServicesProvider(IServicesProvider provider) : base(provider) {
        }

        public override string Name {
            get {
                return _name ?? (_name = Provider.GetServicesProviderName());
            }
        }

        public void DownloadFile(Uri remoteLocation, string localFilename, Object c) {
            Provider.DownloadFile(remoteLocation, localFilename, ExtendCallback(c));
        }

        public bool IsSupportedArchive(string localFilename, Object c) {
            return Provider.IsSupportedArchive(localFilename, ExtendCallback(c));
        }

        public IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, Object c) {
            return Provider.UnpackArchive(localFilename, destinationFolder, ExtendCallback(c)).ByRefEnumerable();
        }

        public void AddPinnedItemToTaskbar(string item, Object c) {
            Provider.AddPinnedItemToTaskbar(item, ExtendCallback(c));
        }

        public void RemovePinnedItemFromTaskbar(string item, Object c) {
            Provider.RemovePinnedItemFromTaskbar(item, ExtendCallback(c));
        }

        public void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, Object c) {
            Provider.CreateShortcutLink(linkPath, targetPath, description, workingDirectory, arguments, ExtendCallback(c));
        }

        public void SetEnvironmentVariable(string variable, string value, int context, Object c) {
            Provider.SetEnvironmentVariable(variable, value, context, ExtendCallback(c));
        }

        public void RemoveEnvironmentVariable(string variable, int context, Object c) {
            Provider.RemoveEnvironmentVariable(variable, context, ExtendCallback(c));
        }

        public void CopyFile(string sourcePath, string destinationPath, Object c) {
            Provider.CopyFile(sourcePath, destinationPath, ExtendCallback(c));
        }

        public void Delete(string path, Object c) {
            Provider.Delete(path, ExtendCallback(c));
        }

        public void DeleteFolder(string folder, Object c) {
            Provider.DeleteFolder(folder, ExtendCallback(c));
        }

        public void CreateFolder(string folder, Object c) {
            Provider.CreateFolder(folder, ExtendCallback(c));
        }

        public void DeleteFile(string filename, Object c) {
            Provider.DeleteFile(filename, ExtendCallback(c));
        }

        public string GetKnownFolder(string knownFolder, Object c) {
            return Provider.GetKnownFolder(knownFolder, ExtendCallback(c));
        }

        public bool IsElevated(Object c) {
            return Provider.IsElevated(ExtendCallback(c));
        }
    }
}