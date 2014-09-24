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
    using System.Threading.Tasks;
    using Api;
    using Packaging;
    using Utility.Collections;
    using Utility.Extensions;
    using Utility.Plugin;

    public class Response<T> : MarshalByRefObject, IResponseApi {
        private readonly object _context;
        private readonly IsCancelled _isCancelled;
        private readonly string _packageStatus;
        private readonly CancellableBlockingCollection<T> _result = new CancellableBlockingCollection<T>();
        private PackageSource _currentPackageSource = null;
        private SoftwareIdentity _currentSoftwareIdentity = null;
        private PackageProvider _provider;

        internal Response(Object requestImpl, PackageProvider provider) {
            _provider = provider;
            _context = requestImpl;
            _isCancelled = _context.As<IsCancelled>();
        }

        public Response(object requestImpl, PackageProvider provider, string packageStatus, Action<object> call) : this(requestImpl, provider) {
            _packageStatus = packageStatus;
            Task.Factory.StartNew(() => {
                try {
                    call(provider.ExtendRequest(_context, this));

                } catch (Exception e) {
                    e.Dump();
                } finally {
                    Complete();
                }
            });
        }

        public Response(object requestImpl, PackageProvider provider, Action<object> call)
            : this(requestImpl, provider) {
            Task.Factory.StartNew(() => {
                try {
                    call(provider.ExtendRequest(_context, this));
                } catch (Exception e) {
                    e.Dump();
                } finally {
                    Complete();
                }
            });
        }

        internal bool Continue {
            get {
                return !(_isCancelled() || _result.IsCancelled);
            }
        }

        public CancellableEnumerable<T> Result {
            get {
                return _result;
            }
        }

        public CancellableEnumerable<T> CompleteResult {
            get {
                _result.WaitForCompletion();
                return _result;
            }
        }

        public bool OkToContinue() {
            return !_isCancelled();
        }

        public bool YieldSoftwareIdentity(string fastPath, string name, string version, string versionScheme, string summary, string source, string searchKey, string fullPath, string packageFileName) {
            CommitSoftwareIdentity();

            _currentSoftwareIdentity = new SoftwareIdentity {
                FastPackageReference = fastPath,
                Name = name,
                Version = version,
                VersionScheme = versionScheme,
                Summary = summary,
                ProviderName = _provider.ProviderName,
                Source = source,
                Status = _packageStatus,
                SearchKey = searchKey,
                FullPath = fullPath,
                PackageFilename = packageFileName
            };

            return Continue;
        }

        public bool YieldSoftwareMetadata(string parentFastPath, string name, string value) {
            if (_currentSoftwareIdentity == null || parentFastPath != _currentSoftwareIdentity.FastPackageReference) {
                Console.WriteLine("TEMPORARY: SHOULD NOT GET HERE [YieldSoftwareMetadata] ================================================");
            }
            if (_currentSoftwareIdentity != null) {
                // special cases:
                if (name != null) {
                    if (name.EqualsIgnoreCase("FromTrustedSource")) {
                        _currentSoftwareIdentity.FromTrustedSource = (value ?? string.Empty ).IsTrue();
                        return Continue;
                    }
                    _currentSoftwareIdentity.Set(name, value);
                }
            }

            return Continue;
        }

        public bool YieldEntity(string parentFastPath, string name, string regid, string role, string thumbprint) {
            if (_currentSoftwareIdentity == null || parentFastPath != _currentSoftwareIdentity.FastPackageReference) {
                Console.WriteLine("TEMPORARY: SHOULD NOT GET HERE [YieldSoftwareMetadata] ================================================");
            }
            if (_currentSoftwareIdentity != null) {
                _currentSoftwareIdentity.AddEntity(name, regid, role, thumbprint);
            }

            return Continue;
        }

        public bool YieldLink(string parentFastPath, string referenceUri, string relationship, string mediaType, string ownership, string use, string appliesToMedia, string artifact) {
            if (_currentSoftwareIdentity == null || parentFastPath != _currentSoftwareIdentity.FastPackageReference) {
                Console.WriteLine("TEMPORARY: SHOULD NOT GET HERE [YieldSoftwareMetadata] ================================================");
            }

            if (_currentSoftwareIdentity != null) {
                _currentSoftwareIdentity.AddLink(referenceUri, relationship, mediaType, ownership, use, appliesToMedia, artifact);
            }

            return Continue;
        }

        public bool YieldPackageSource(string name, string location, bool isTrusted, bool isRegistered, bool isValidated) {
            CommitPackageSource();

            _currentPackageSource = new PackageSource {
                Name = name,
                Location = location,
                Provider = _provider,
                IsTrusted = isTrusted,
                IsRegistered = isRegistered,
                IsValidated = isValidated,
            };
            return Continue;
        }

        public bool YieldDynamicOption(string name, string expectedType, bool isRequired) {
            Console.WriteLine("TEMPORARY: SHOULD NOT GET HERE [YieldDynamicOption] ================================================");
            return false;
        }

        public bool YieldKeyValuePair(string key, string value) {
            if (_currentPackageSource != null) {
                _currentPackageSource.DetailsCollection.AddOrSet(key, value);
            }
            return Continue;
        }

        public bool YieldValue(string value) {
            Console.WriteLine("TEMPORARY: SHOULD NOT GET HERE [YieldValue] ================================================");
            return false;
        }

        public override object InitializeLifetimeService() {
            return null;
        }

        private void CommitPackageSource() {
            if (_currentPackageSource != null && typeof (T) == typeof (PackageSource)) {
                _result.Add((T)(object)_currentPackageSource);
            }
            _currentPackageSource = null;
        }

        private void CommitSoftwareIdentity() {
            if (_currentSoftwareIdentity != null && typeof (T) == typeof (SoftwareIdentity)) {
                _result.Add((T)(object)_currentSoftwareIdentity);
            }
            _currentSoftwareIdentity = null;
        }

        public void Complete() {
            // add the last package source if it's still waiting...
            CommitPackageSource();
            CommitSoftwareIdentity();

            _result.CompleteAdding();
        }

        /*
        internal CancellableEnumerable<T> CallAndCollect(object context, Action<object> call) {
            Task.Factory.StartNew(() => {
                try {
                    call(_context.Extend<IRequest>(context, this));
                }
                catch (Exception e) {
                    e.Dump();
                }
                finally {
                    Complete();
                }
            });

            return Result;
        }*/
    }
}