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
    using System.Linq;
    using Api;
    using Utility.Extensions;
    using Utility.Plugin;
    using RequestImpl = System.Object;

    /// <summary>
    ///     The The AggregateServicesProvider aggregates the functionality of all
    ///     the loaded service providers.
    /// </summary>
    public class AggregateServicesProvider : MarshalByRefObject {
        private readonly ServicesProvider _nullProvider;
        private readonly Dictionary<string, ServicesProvider> _preferredProviderForFunction = new Dictionary<string, ServicesProvider>(StringComparer.OrdinalIgnoreCase);
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

        public bool IsSupportedArchive(string localFilename, RequestImpl requestImpl) {
            return false;
        }

        public void DownloadFile(Uri remoteLocation, string localFilename, RequestImpl requestImpl) {
            // check the Uri type, see if we have anyone who can handle that 
            // if so, call that provider's download file
            if (remoteLocation == null) {
                throw new ArgumentNullException("remoteLocation");
            }
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            requestImpl.As<IRequest>().Error("PROTOCOL_NOT_SUPPORTED", "NotImplemented", remoteLocation.Scheme, "PROTOCOL_NOT_SUPPORTED");
        }

        public IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, RequestImpl requestImpl) {
            // check who supports the archive type
            // and call that provider.
            if (requestImpl == null) {
                throw new ArgumentNullException("requestImpl");
            }

            requestImpl.As<IRequest>().Error("ARCHIVE_NOT_SUPPORTED", "NotImplemented", localFilename, "PROTOCOL_NOT_SUPPORTED");
            return Enumerable.Empty<string>();
        }

        public void AddPinnedItemToTaskbar(string item, RequestImpl requestImpl) {
            Preferred("AddPinnedItemToTaskbar").AddPinnedItemToTaskbar(item, requestImpl);
        }

        public void RemovePinnedItemFromTaskbar(string item, RequestImpl requestImpl) {
            Preferred("RemovePinnedItemFromTaskbar").RemovePinnedItemFromTaskbar(item, requestImpl);
        }

        public void teShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, RequestImpl requestImpl) {
            Preferred("CreateShortcutLink").CreateShortcutLink(linkPath, targetPath, description, workingDirectory, arguments, requestImpl);
        }

        public void SetEnvironmentVariable(string variable, string value, int context, RequestImpl requestImpl) {
            Preferred("SetEnvironmentVariable").SetEnvironmentVariable(variable, value, context, requestImpl);
        }

        public void RemoveEnvironmentVariable(string variable, int context, RequestImpl requestImpl) {
            Preferred("RemoveEnvironmentVariable").RemoveEnvironmentVariable(variable, context, requestImpl);
        }

        public void CopyFile(string sourcePath, string destinationPath, RequestImpl requestImpl) {
            Preferred("CopyFile").CopyFile(sourcePath, destinationPath, requestImpl);
        }

        public void Delete(string path, RequestImpl requestImpl) {
            Preferred("Delete").Delete(path, requestImpl);
        }

        public void DeleteFolder(string folder, RequestImpl requestImpl) {
            Preferred("DeleteFolder").DeleteFolder(folder, requestImpl);
        }

        public void CreateFolder(string folder, RequestImpl requestImpl) {
            Preferred("CreateFolder").CreateFolder(folder, requestImpl);
        }

        public void DeleteFile(string filename, RequestImpl requestImpl) {
            Preferred("DeleteFile").DeleteFile(filename, requestImpl);
        }

        public string GetKnownFolder(string knownFolder, RequestImpl requestImpl) {
            return Preferred("GetKnownFolder").GetKnownFolder(knownFolder, requestImpl);
        }

        public bool IsElevated(RequestImpl requestImpl) {
            return Preferred("IsElevated").IsElevated(requestImpl);
        }
    }
}