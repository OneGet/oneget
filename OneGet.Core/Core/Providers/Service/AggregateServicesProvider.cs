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

namespace Microsoft.OneGet.Core.Providers.Service {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Dynamic;
    using Extensions;
    using Callback = System.Object;


    /// <summary>
    ///     The The AggregateServicesProvider aggregates the functionality of all
    ///     the loaded service providers.
    /// </summary>
    public class AggregateServicesProvider : MarshalByRefObject {
        private readonly ServicesProvider _nullProvider;
        private readonly Dictionary<string, ServicesProvider> _preferredProviderForFunction = new Dictionary<string, ServicesProvider>();
        private readonly IDictionary<string, ServicesProvider> _providers;

        internal AggregateServicesProvider(IDictionary<string, ServicesProvider> providers) {
            _providers = providers;
            _nullProvider = new ServicesProvider(new {
                GetServicesProviderName = new Func<string>(() => "null"),
                InitializeProvider = new Action<object, object>((o, p) => {})
            }.As<IServicesProvider>());
        }

        public override object InitializeLifetimeService() {
            return null;
        }


        private ServicesProvider Preferred(string function) {
            return _preferredProviderForFunction.GetOrAdd(function, () => _providers.Values.FirstOrDefault(each => each.IsMethodImplemented(function)) ?? _nullProvider);
        }

        public bool IsSupportedArchive(string localFilename, Callback c) {
            return false;
        }

        public void DownloadFile(Uri remoteLocation, string localFilename, Callback c) {
            // check the Uri type, see if we have anyone who can handle that 
            // if so, call that provider's download file

            c.As<IRequest>().Error("Protocol Scheme '{0}' not supported".format(remoteLocation.Scheme));
        }

        public IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, Callback c) {
            // check who supports the archive type
            // and call that provider.

            c.As<IRequest>().Error("Unsupported archive type for file '{0}' not supported".format(localFilename));
            return Enumerable.Empty<string>();
        }

        public void AddPinnedItemToTaskbar(string item, Callback c) {
            Preferred("AddPinnedItemToTaskbar").AddPinnedItemToTaskbar(item, c);
        }

        public void RemovePinnedItemFromTaskbar(string item, Callback c) {
            Preferred("RemovePinnedItemFromTaskbar").RemovePinnedItemFromTaskbar(item, c);
        }

        public void teShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, Callback c) {
            Preferred("CreateShortcutLink").CreateShortcutLink(linkPath, targetPath, description, workingDirectory, arguments, c);
        }

        public void SetEnvironmentVariable(string variable, string value, int context, Callback c) {
            Preferred("SetEnvironmentVariable").SetEnvironmentVariable(variable, value, context, c);
        }

        public void RemoveEnvironmentVariable(string variable, int context, Callback c) {
            Preferred("RemoveEnvironmentVariable").RemoveEnvironmentVariable(variable, context, c);
        }

        public void CopyFile(string sourcePath, string destinationPath, Callback c) {
            Preferred("CopyFile").CopyFile(sourcePath, destinationPath, c);
        }

        public void Delete(string path, Callback c) {
            Preferred("Delete").Delete(path, c);
        }

        public void DeleteFolder(string folder, Callback c) {
            Preferred("DeleteFolder").DeleteFolder(folder, c);
        }

        public void CreateFolder(string folder, Callback c) {
            Preferred("CreateFolder").CreateFolder(folder, c);
        }

        public void DeleteFile(string filename, Callback c) {
            Preferred("DeleteFile").DeleteFile(filename, c);
        }

        public string GetKnownFolder(string knownFolder, Callback c) {
            return Preferred("GetKnownFolder").GetKnownFolder(knownFolder, c);
        }

        public bool IsElevated(Callback c) {
            return Preferred("IsElevated").IsElevated(c);
        }
    }
}