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

namespace Microsoft.OneGet.Implementation {
    using System;
    using Api;
    using Packaging;
    using Utility.Extensions;

    public class SoftwareIdentityRequestObject : EnumerableRequestObject<SoftwareIdentity> {
        private readonly string _status;

        private SoftwareIdentity _currentItem;

        public SoftwareIdentityRequestObject(ProviderBase provider, IHostApi request, Action<RequestObject> action, string status)
            : base(provider, request, action) {
            _status = status;
            InvokeImpl();
        }

        private void CommitCurrentItem() {
            if (_currentItem != null) {
                Results.Add(_currentItem);
            }
            _currentItem = null;
        }

        public override bool YieldSoftwareIdentity(string fastPath, string name, string version, string versionScheme, string summary, string source, string searchKey, string fullPath, string packageFileName) {
            Activity();
            CommitCurrentItem();

            _currentItem = new SoftwareIdentity {
                FastPackageReference = fastPath,
                Name = name,
                Version = version,
                VersionScheme = versionScheme,
                Summary = summary,
                ProviderName = Provider.ProviderName,
                Source = source,
                Status = _status,
                SearchKey = searchKey,
                FullPath = fullPath,
                PackageFilename = packageFileName
            };

            return !IsCanceled;
        }

        public override bool YieldSoftwareMetadata(string parentFastPath, string name, string value) {
            Activity();

            if (_currentItem == null) {
                Console.WriteLine("TODO: SHOULD NOT GET HERE [YieldSoftwareMetadata] ================================================");
                return !IsCanceled;
            }

            // special cases:
            if (name != null) {
                if (name.EqualsIgnoreCase("FromTrustedSource")) {
                    _currentItem.FromTrustedSource = (value ?? string.Empty).IsTrue();
                    return !IsCanceled;
                }
                _currentItem.Set(name, value);
            }

            return !IsCanceled;
        }

        public override bool YieldEntity(string parentFastPath, string name, string regid, string role, string thumbprint) {
            Activity();

            if (_currentItem == null) {
                Console.WriteLine("TODO: SHOULD NOT GET HERE [YieldEntity] ================================================");
                return !IsCanceled;
            }

            _currentItem.AddEntity(name, regid, role, thumbprint);
            return !IsCanceled;
        }

        public override bool YieldLink(string parentFastPath, string referenceUri, string relationship, string mediaType, string ownership, string use, string appliesToMedia, string artifact) {
            Activity();

            if (_currentItem == null) {
                Console.WriteLine("TODO: SHOULD NOT GET HERE [YieldLink] ================================================");
                return !IsCanceled;
            }

            if (_currentItem != null) {
                _currentItem.AddLink(referenceUri, relationship, mediaType, ownership, use, appliesToMedia, artifact);
            }

            return !IsCanceled;
        }

        protected override void Complete() {
            CommitCurrentItem();
            base.Complete();
        }
    }
}