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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Api;
    using Packaging;
    using Providers;
    using Utility.Async;
    using Utility.Collections;
    using Utility.Extensions;
    using Utility.Plugin;
    using Utility.Versions;
    using IRequestObject = System.Object;

    public abstract class ProviderBase  {
        public abstract string ProviderName {get;}
    }

    public abstract class ProviderBase<T> : ProviderBase where T : IProvider {
        private static Regex _canonicalPackageRegex = new Regex("(.*?):(.*?)/(.*)");

        private List<DynamicOption> _dynamicOptions;
        private Dictionary<string, List<string>> _features;
        private bool _initialized;
        private byte[][] _magicSignatures;
        private string[] _supportedFileExtensions;
        private string[] _supportedSchemes;
        private FourPartVersion _version;

        public ProviderBase(T provider) {
            Provider = provider;
        }

        protected T Provider {get; private set;}

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "This is required for the PowerShell Providers.")]
        public IDictionary<string, List<string>> Features {
            get {
                // todo: this dictionary should be read only (.net 4.0 doesn't have that!)
                return _features;
            }
        }

        public FourPartVersion Version {
            get {
                return _version;
            }
            set {
                if (_version == 0) {
                    _version = value;
                }
            }
        }

        public IEnumerable<string> SupportedFileExtensions {
            get {
                return (_supportedFileExtensions ?? (_supportedFileExtensions = Features.ContainsKey(Constants.Features.SupportedExtensions) ? Features[Constants.Features.SupportedExtensions].ToArray() : Constants.Empty));
            }
        }

        public IEnumerable<string> SupportedUriSchemes {
            get {
                return (_supportedSchemes ?? (_supportedSchemes = Features.ContainsKey(Constants.Features.SupportedSchemes) ? Features[Constants.Features.SupportedSchemes].ToArray() : Constants.Empty));
            }
        }

        internal byte[][] MagicSignatures {
            get {
                return _magicSignatures ?? (_magicSignatures = Features.ContainsKey(Constants.Features.MagicSignatures) ? Features[Constants.Features.MagicSignatures].Select(each => each.FromHex()).ToArray() : new byte[][] {});
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "This is required for the PowerShell Providers.")]
        public List<DynamicOption> DynamicOptions {
            get {
                if (_dynamicOptions == null) {
                    var o = new object();
                    var result = new List<DynamicOption>();
                    result.AddRange(GetDynamicOptions(OptionCategory.Install, o));
                    result.AddRange(GetDynamicOptions(OptionCategory.Package, o));
                    result.AddRange(GetDynamicOptions(OptionCategory.Provider, o));
                    result.AddRange(GetDynamicOptions(OptionCategory.Source, o));
                    if (Features.ContainsKey("IsChainingProvider")) {
                        // chaining package providers should not cache results
                        return result;
                    }

                    _dynamicOptions = result;
                }
                return _dynamicOptions;
            }
        }

        public virtual bool IsSupportedFileName(string filename) {
            try {
                var extension = Path.GetExtension(filename);
                if (!string.IsNullOrWhiteSpace(extension)) {
                    return SupportedFileExtensions.ContainsIgnoreCase(extension);
                }
            } catch {
            }
            return false;
        }

        public virtual bool IsSupportedFile(string filename) {
            if (filename == null) {
                throw new ArgumentNullException("filename");
            }
            if (filename.FileExists()) {
                var buffer = new byte[1024];
                var sz = 0;
                try {
                    using (var file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        sz = file.Read(buffer, 0, 1024);
                    }
                    return MagicSignatures.Any(magic => BufferMatchesMagicBytes(magic, buffer, sz));
                } catch {
                    // not openable. whatever.
                }
            }
            return false;
        }

        public virtual bool IsFileSupported(byte[] header) {
            return MagicSignatures.Any(magic => BufferMatchesMagicBytes(magic, header, header.Length));
        }

        public virtual bool IsSupportedScheme(Uri uri) {
            try {
                return SupportedUriSchemes.ContainsIgnoreCase(uri.Scheme);
            } catch {
            }
            return false;
        }

        private bool BufferMatchesMagicBytes(byte[] magic, byte[] buffer, int maxSize) {
            if (magic.Length <= maxSize) {
                return !magic.Where((t, i) => t != buffer[i]).Any();
            }
            return false;
        }

        
        public bool IsMethodImplemented(string methodName) {
            return Provider.IsMethodImplemented(methodName);
        }

        internal void Initialize(IRequest request) {
            if (!_initialized) {
                _features = GetFeatures(request).Value;
                _initialized = true;
            }
        }

        public IAsyncValue<Dictionary<string, List<string>>> GetFeatures(IRequestObject requestObject) {
            if (requestObject == null) {
                throw new ArgumentNullException("requestObject");
            }

            return new DictionaryRequestObject(this, requestObject.As<IHostApi>(), request => Provider.GetFeatures(request));
        }

        public IAsyncEnumerable<DynamicOption> GetDynamicOptions(OptionCategory category, IRequestObject requestObject) {
            requestObject = requestObject ?? new object();

            return new DynamicOptionRequestObject(this, requestObject.As<IHostApi>(), request => Provider.GetDynamicOptions(category.ToString(), request), category);
        }
    }
}