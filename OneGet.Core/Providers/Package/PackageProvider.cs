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

namespace Microsoft.OneGet.Providers.Package {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Api;
    using Collections;
    using Extensions;
    using Packaging;
    using Plugin;

    public class Response<T> : MarshalByRefObject,IResponseApi {

        private readonly CancellableBlockingCollection<T> _result = new CancellableBlockingCollection<T>();
        private readonly string _packageStatus;
        private readonly IsCancelled _isCancelled;
        private readonly object _context;
        private PackageSource _currentPackageSource = null;
        private SoftwareIdentity _currentSoftwareIdentity = null;
        private PackageProvider _provider;

        internal Response(object c, PackageProvider provider) {
            _provider = provider;
            _context = c;
            _isCancelled = _context.As<IsCancelled>();
        }

        public override object InitializeLifetimeService() {
            return null;
        }

        public Response(object c, PackageProvider provider, string packageStatus, Action<object> call) : this(c,provider) {
            _packageStatus = packageStatus;
            Task.Factory.StartNew(() => {
                try {
                    call(_context.Extend<IRequest>(provider.Context, this));
                }
                catch (Exception e) {
                    e.Dump();
                }
                finally {
                    Complete();
                }
            });
        }

        public Response(object c, PackageProvider provider, Action<object> call)
            : this(c, provider) {
            Task.Factory.StartNew(() => {
                try {
                    call(_context.Extend<IRequest>(provider.Context, this));
                }
                catch (Exception e) {
                    e.Dump();
                }
                finally {
                    Complete();
                }
            });
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
                ProviderName = name,
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
                _currentSoftwareIdentity.Set(name, value);    
            }

            return Continue;
        }

        public bool YieldEntity(string parentFastPath, string name, string regid, string role, string thumbprint) {
            if (_currentSoftwareIdentity == null || parentFastPath != _currentSoftwareIdentity.FastPackageReference) {
                Console.WriteLine("TEMPORARY: SHOULD NOT GET HERE [YieldSoftwareMetadata] ================================================");
            }
            if (_currentSoftwareIdentity != null) {
                _currentSoftwareIdentity.AddEntity(name,regid, role, thumbprint);
            }

            return Continue;
        }

        public bool YieldLink(string parentFastPath, string referenceUri, string relationship, string mediaType, string ownership, string use, string appliesToMedia, string artifact) {
            if (_currentSoftwareIdentity == null || parentFastPath != _currentSoftwareIdentity.FastPackageReference) {
                Console.WriteLine("TEMPORARY: SHOULD NOT GET HERE [YieldSoftwareMetadata] ================================================");
            }

            if (_currentSoftwareIdentity != null) {
                _currentSoftwareIdentity.AddLink(referenceUri, relationship,mediaType,ownership,use,appliesToMedia,artifact);
            }

            return Continue;
        }

        internal bool Continue {
            get {
                return !(_isCancelled() || _result.IsCancelled);
            }
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

        public bool YieldDynamicOption(int category, string name, int expectedType, bool isRequired) {
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

        private void CommitPackageSource() {
            if (_currentPackageSource != null && typeof(T) == typeof(PackageSource)) {
                _result.Add((T)(object)_currentPackageSource);
            }
            _currentPackageSource = null;
        }
        private void CommitSoftwareIdentity() {
            if (_currentSoftwareIdentity!= null && typeof(T) == typeof(SoftwareIdentity)) {
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

    #region generate-delegates response-apis

    public delegate bool OkToContinue();

    public delegate bool YieldSoftwareIdentity(string fastPath, string name, string version, string versionScheme, string summary, string source, string searchKey, string fullPath, string packageFileName);

    public delegate bool YieldSoftwareMetadata(string parentFastPath, string name, string value);

    public delegate bool YieldEntity(string parentFastPath, string name, string regid, string role, string thumbprint);

    public delegate bool YieldLink(string parentFastPath, string referenceUri, string relationship, string mediaType, string ownership, string use, string appliesToMedia, string artifact);

    public delegate bool YieldSwidtag(string fastPath, string xmlOrJsonDoc);

    public delegate bool YieldMetadata(string fieldId, string @namespace, string name, string value);

    public delegate bool YieldPackageSource(string name, string location, bool isTrusted,bool isRegistered, bool isValidated);

    public delegate bool YieldDynamicOption(int category, string name, int expectedType, bool isRequired);

    public delegate bool YieldKeyValuePair(string key, string value);

    public delegate bool YieldValue(string value);

    #endregion

    public delegate bool IsCancelled();

    public class PackageProvider : ProviderBase<IPackageProvider> {
        private string _name;

        internal PackageProvider(IPackageProvider provider) : base(provider) {
        }

        public override string Name {
            get {
                return _name ?? (_name = Provider.GetPackageProviderName());
            }
        }

        // Friendly APIs

        public ICancellableEnumerable<PackageSource> AddPackageSource(string name, string location, bool trusted, Object c) {
            // Provider.AddPackageSource(name, location, trusted, DynamicInterface.Instance.Create<IRequest>(c, Context));
            return new Response<PackageSource>(c, this, response => Provider.AddPackageSource(name, location, trusted, response)).CompleteResult;
        }

        public ICancellableEnumerable<PackageSource> RemovePackageSource(string name, Object c) {
            return new Response<PackageSource>(c, this, response => Provider.RemovePackageSource(name, response)).CompleteResult;
            // Provider.RemovePackageSource(name, DynamicInterface.Instance.Create<IRequest>(c, Context));
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackageByUri(Uri uri, int id, Object c) {
            return new Response<SoftwareIdentity>(c, this, "Available", response => Provider.FindPackageByUri(uri, id, response)).Result;

            // return CallAndCollect(c,new Response<SoftwareIdentity>(c,"Available"), response => Provider.FindPackageByUri(uri, id, response));

            // return new Response<SoftwareIdentity>(c, "Available").CallAndCollect(Context, response => Provider.FindPackageByUri(uri, id, response));
            
        }

        public ICancellableEnumerable<SoftwareIdentity> GetPackageDependencies(SoftwareIdentity package, Object c) {
            return new Response<SoftwareIdentity>(c, this, "Dependency", response => Provider.GetPackageDependencies(package.FastPackageReference, response) ).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackageByFile(string filename, int id, Object c) {
            return new Response<SoftwareIdentity>(c, this, "Available", response => Provider.FindPackageByFile(filename, id, response)).Result;
        }

        public int StartFind(Object c) {
            return Provider.StartFind(c.As<IRequest>());
        }

        public ICancellableEnumerable<SoftwareIdentity> CompleteFind(int i, Object c) {
            return new Response<SoftwareIdentity>(c, this, "Available", response => Provider.CompleteFind(i, response)).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackages(string[] names, string requiredVersion, string minimumVersion, string maximumVersion, Object c) {
            c = c.Extend<IRequest>(Context);
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), names.SelectMany(each => FindPackage(each, requiredVersion, minimumVersion, maximumVersion, id, c)).ToArray().Concat(CompleteFind(id, c)).ToArray());
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackagesByUris(Uri[] uris, Object c) {
            c = c.Extend<IRequest>(Context);
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), uris.SelectMany(each => FindPackageByUri(each, id, c)).ToArray().Concat(CompleteFind(id, c)));
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackagesByFiles(string[] filenames, Object c) {
            c = c.Extend<IRequest>(Context);
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), filenames.SelectMany(each => FindPackageByFile(each, id, c)).ToArray().Concat(CompleteFind(id, c)));
        }

        public ICancellableEnumerable<SoftwareIdentity> FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Object c) {
            return new Response<SoftwareIdentity>(c, this, "Available", response => Provider.FindPackage(name, requiredVersion, minimumVersion, maximumVersion, id, response)).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> GetInstalledPackages(string name, Object c) {
            return new Response<SoftwareIdentity>(c, this, "Installed", response => Provider.GetInstalledPackages(name, response)).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> InstallPackage(SoftwareIdentity softwareIdentity, Object c) {
            var isCancelled = c.As<IsCancelled>();

            var request = c.Extend<IRequest>(Context);

            if (softwareIdentity == null) {
                throw new ArgumentNullException("softwareIdentity");
            }

            if (c == null) {
                throw new ArgumentNullException("c");
            }

            // check if this source is trusted first.
            var src = ResolvePackageSources(c.Extend<IRequest>(new {
                GetSources = new Func<IEnumerable<string>>(() => {
                    return new string[] {
                        softwareIdentity.Source
                    };
                })
            }, Context)).FirstOrDefault();

            var trusted = (src != null && src.IsTrusted);

            if (!trusted) {
                try {
                    if (!request.ShouldContinueWithUntrustedPackageSource(softwareIdentity.Name, softwareIdentity.Source)) {
                        request.Error("User declined to trust package source ");
                        throw new Exception("cancelled");
                    }
                } catch {
                    request.Error("User declined to trust package source ");
                    throw new Exception("cancelled");
                }
            }

            return new Response<SoftwareIdentity>(c, this, "Installed", response => Provider.InstallPackage(softwareIdentity.FastPackageReference, response)).Result;
        }

        public ICancellableEnumerable<SoftwareIdentity> UninstallPackage(SoftwareIdentity softwareIdentity, Object c) {
            return new Response<SoftwareIdentity>(c, this, "Not Installed", response => Provider.UninstallPackage(softwareIdentity.FastPackageReference, response)).Result;
        }

        public ICancellableEnumerable<PackageSource> ResolvePackageSources(Object c) {
            return new Response<PackageSource>(c, this, response => Provider.ResolvePackageSources(response)).Result;
        }

        public void DownloadPackage(SoftwareIdentity softwareIdentity, string destinationFilename, Object c) {
            Provider.DownloadPackage(softwareIdentity.FastPackageReference, destinationFilename,c.Extend<IRequest>(Context) );
        }
    }

    #region declare PackageProvider-types

    public enum OptionCategory {
        Package = 0,
        Provider = 1,
        Source = 2,
        Install = 3
    }

    public enum OptionType {
        String = 0,
        StringArray = 1,
        Int = 2,
        Switch = 3,
        Folder = 4,
        File = 5,
        Path = 6,
        Uri = 7,
        SecureString = 8
    }

    public enum EnvironmentContext {
        All = 0,
        User = 1,
        System = 2
    }

    #endregion
}