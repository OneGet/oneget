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

namespace Microsoft.PackageManagement.Providers.Internal.Bootstrap {
    using System;
    using System.Globalization;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using PackageManagement.Internal;
    using PackageManagement.Internal.Implementation;
    using PackageManagement.Internal.Packaging;
    using PackageManagement.Internal.Utility.Platform;
    using PackageManagement.Internal.Utility.Collections;
    using PackageManagement.Internal.Utility.Extensions;
    

    public abstract class BootstrapRequest : Request {
        internal readonly Uri[] _urls = {
#if LOCAL_DEBUG
            new Uri("https://localhost:81/providers.swidtag"),
#endif
#if CORECLR
            new Uri("https://go.microsoft.com/fwlink/?LinkID=627340&clcid=0x409"),
            // starting in 2015/05 builds, we bootstrap from here:
#else
            new Uri("https://go.microsoft.com/fwlink/?LinkID=627338&clcid=0x409"),
#endif
        };

        private IEnumerable<Feed> _feeds;

        private IEnumerable<Feed> Feeds {
            get {
                if (_feeds == null) {
                    if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) {
                        Warning(Constants.Messages.NetworkNotAvailable);
                    }
                    // right now, we only have one feed (can have many urls tho')
                    // so we just return a single feed in the collection
                    // but later, we can expand it to support multiple feeds.
                    var feed = new Feed(this, _urls);
                    if (feed.IsValid) {
                        _feeds = feed.SingleItemAsEnumerable().ReEnumerable();
                    } else {
                        Warning(Constants.Messages.ProviderSwidtagUnavailable);
                        return Enumerable.Empty<Feed>();
                    }
                }
                return _feeds;
            }
        }

        internal string DestinationPath(Request request) {

                var pms = PackageManagementService as PackageManagementService;

                var scope = GetValue("Scope");
                if (!string.IsNullOrWhiteSpace(scope)) {
                    if (scope.EqualsIgnoreCase("CurrentUser")) {
                        return pms.UserAssemblyLocation;
                    }
                    if (AdminPrivilege.IsElevated) {
                        return pms.SystemAssemblyLocation;
                    } else {
                        //a user specifies 'AllUsers' that requires Admin provilege. However his console gets launched by non-elevated.
                        Error(ErrorCategory.InvalidOperation, ErrorCategory.InvalidOperation.ToString(),
                            PackageManagement.Resources.Messages.InstallRequiresCurrentUserScopeParameterForNonAdminUser, pms.SystemAssemblyLocation, pms.UserAssemblyLocation);                 
                        return null;
                    }
                }

                var v = GetValue("DestinationPath");
                if (String.IsNullOrWhiteSpace(v)) {
                    // use a well-known path.
                    v = AdminPrivilege.IsElevated ? pms.SystemAssemblyLocation : pms.UserAssemblyLocation;
                    if (String.IsNullOrWhiteSpace(v)) {
                        return null;
                    }
                }
                return Path.GetFullPath(v);
        }

        internal IEnumerable<Package> Providers {
            get {
                return Feeds.SelectMany(feed => feed.Query());
            }
        }

        private string GetValue(string name) {
            // get the value from the request
            return (GetOptionValues(name) ?? Enumerable.Empty<string>()).LastOrDefault();
        }

        internal Package GetProvider(Uri uri) {
            return new Package(this, uri.SingleItemAsEnumerable());
        }

        internal Package GetProvider(string name) {
            return Feeds.SelectMany(feed => feed.Query(name)).FirstOrDefault();
        }

        internal Package GetProvider(string name, string version) {
            return Feeds.SelectMany(feed => feed.Query(name, version)).FirstOrDefault();
        }

        internal IEnumerable<Package> GetProviderAll(string name, string minimumversion, string maximumversion)
        {
            return Feeds.SelectMany(feed => feed.Query(name, minimumversion, maximumversion));
        }

        internal IEnumerable<Package> GetProvider(string name, string minimumversion, string maximumversion) {
            return new[] { GetProviderAll(name, minimumversion, maximumversion)
                .OrderByDescending(each => SoftwareIdentityVersionComparer.Instance).FirstOrDefault()};
        }

        internal string DownloadAndValidateFile(string name, Swidtag swidtag) {
            var file = DownLoadFileFromLinks(name, swidtag.Links.Where(each => each.Relationship == Iso19770_2.Relationship.InstallationMedia));
            if (string.IsNullOrWhiteSpace(file)) {
                return null;
            }

            var payload = swidtag.Payload;
            if (payload == null) {
                //We let the providers that are already posted in the public continue to be installed.
                return file;
            } else {
                //validate the file hash
                var valid = ValidateFileHash(file, payload);
                if (!valid) {
                    //if the hash does not match, delete the file in the temp folder
                    file.TryHardToDelete();
                    return null;
                }
                return file;
            }
        }

        /// <summary>
        /// Helper function to retry downloading a file.
        /// downloadFileFunction is the main function that is used to download the file when given a uri
        /// numberOfTry is how many times we can try to download it
        /// </summary>
        /// <param name="downloadFileFunction"></param>
        /// <param name="location"></param>
        /// <param name="numberOfTry"></param>
        /// <returns></returns>
        internal string RetryDownload(Func<Uri, string> downloadFileFunction, Uri location, int numberOfTry=3)
        {
            string file = null;

            // if scheme is not https, write warning and ignores this link
            if (!string.Equals(location.Scheme, "https"))
            {
                Warning(string.Format(CultureInfo.CurrentCulture, Resources.Messages.OnlyHttpsSchemeSupported, location.AbsoluteUri));
                return file;
            }

            // try 3 times to see whether we can download this
            int remainingTry = 3;

            // try to download the file for remainingTry times
            while (remainingTry > 0)
            {
                try
                {
                    file = downloadFileFunction(location);
                }
                finally
                {
                    if (file == null || !file.FileExists())
                    {
                        // file cannot be download
                        file = null;
                        remainingTry -= 1;
                        Verbose(string.Format(CultureInfo.CurrentCulture, Resources.Messages.RetryDownload, location.AbsoluteUri, remainingTry));
                    }
                    else
                    {
                        // file downloaded, no need to retry.
                        remainingTry = 0;
                    }
                }
            }

            return file;
        }

        internal string DownLoadFileFromLinks(string name, IEnumerable<Link> links) {
            string file = null;

            foreach (var link in links) {
                file = RetryDownload(
                    // the download function takes in a uri link and download it
                    (uri) =>
                        {
                            var tmpFile = link.Artifact.GenerateTemporaryFilename();
                            return ProviderServices.DownloadFile(uri, tmpFile, -1, true, this);
                        },
                    link.HRef);

                // got a valid file!
                if (file != null && file.FileExists())
                {
                    return file;
                }
            }

            return file;
        }

        private bool ValidateFileHash(string fileFullPath, Payload payload) {

            Debug("BoostrapRequest::ValidateFileHash");
            /* format: 
             * <Payload>
             *   <File name="nuget-anycpu-2.8.5.205.exe"  sha512:hash="a314fc2dc663ae7a6b6bc6787594057396e6b3f569cd50fd5ddb4d1bbafd2b6a" />
             * </Payload>
             */

            if (payload == null || fileFullPath == null || !fileFullPath.FileExists()) {
                return false;
            }

            try
            {
                if ((payload.Files == null) || !payload.Files.Any()) {
                    Error(ErrorCategory.InvalidData, "Payload", Constants.Messages.MissingFileTag);
                    return false;
                }
                var fileTag = payload.Files.FirstOrDefault();

                if ((fileTag.Attributes == null) || (fileTag.Attributes.Keys == null)) {
                    Error(ErrorCategory.InvalidData, "Payload", Constants.Messages.MissingHashAttribute);
                    return false;
                }

                var hashtag = fileTag.Attributes.Keys.FirstOrDefault(each => each.LocalName.Equals("hash"));
                if (hashtag == null) {
                    Error(ErrorCategory.InvalidData, "Payload", Constants.Messages.MissingHashAttribute);
                    return false;
                }

                //Note we cannot use switch here because these xname like Iso19770_2.Hash.Hash512, is not compiler time constant
                string packageHash = null;
                HashAlgorithm hashAlgorithm = null;

                if (hashtag.Equals(Iso19770_2.Hash.Hash512)) {
                    hashAlgorithm = SHA512.Create();
                    packageHash = fileTag.GetAttribute(Iso19770_2.Hash.Hash512);
                } else if (hashtag.Equals(Iso19770_2.Hash.Hash256)) {
                    hashAlgorithm = SHA256.Create();
                    packageHash = fileTag.GetAttribute(Iso19770_2.Hash.Hash256);
                } else if (hashtag.Equals(Iso19770_2.Hash.Md5)) {
                    hashAlgorithm = MD5.Create();
                    packageHash = fileTag.GetAttribute(Iso19770_2.Hash.Md5);
                } else {
                    //hash alroghtme not supported, we support 512, 256, md5 only 
                    Error(ErrorCategory.InvalidData, "Payload", Constants.Messages.UnsupportedHashAlgorithm, hashtag,
                        new[] {Iso19770_2.HashAlgorithm.Sha512, Iso19770_2.HashAlgorithm.Sha256, Iso19770_2.HashAlgorithm.Md5}.JoinWithComma());
                    return false;
                }

                if (string.IsNullOrWhiteSpace(packageHash) || hashAlgorithm == null) {
                    //missing hash content?
                    Error(ErrorCategory.InvalidData, "Payload", Constants.Messages.MissingHashContent);
                    return false;
                }

                // Verify the hash
                using (FileStream stream = System.IO.File.OpenRead(fileFullPath)) {
                    // compute the hash from the file
                    byte[] computedHash = hashAlgorithm.ComputeHash(stream);

                    try {
                        // convert the original hash we got from the payload tag
                        byte[] expectedHash = Convert.FromBase64String(packageHash);
                        //check if hash is equal
                        if (!computedHash.SequenceEqual(expectedHash)) {
                            // the file downloaded is not the same as expected. The file is modified.
                            Error(ErrorCategory.SecurityError, "Payload", Constants.Messages.HashNotEqual, packageHash, Convert.ToBase64String(computedHash));
                            return false;
                        }

                        return true;

                    } catch (FormatException ex) {
                        Warning(ex.Message);
                        Error(ErrorCategory.SecurityError, "Payload", Constants.Messages.InvalidHashFormat, packageHash);
                    }
                }
            } catch (Exception ex) {
                Warning(ex.Message);
            }
            return false;
        }

        internal bool YieldFromSwidtag(Package provider, string requiredVersion, string minimumVersion, string maximumVersion, string searchKey) {
            if (provider == null) {
                // if the provider isn't there, just return.
                return !IsCanceled;
            }

            if (AnyNullOrEmpty(provider.Name, provider.Version, provider.VersionScheme)) {
                Debug("Skipping yield on swid due to missing field \r\n", provider.ToString());
                return !IsCanceled;
            }

            if (!String.IsNullOrWhiteSpace(requiredVersion)) {
                if (provider.Version != requiredVersion) {
                    return !IsCanceled;
                }
            } else {
                if (!String.IsNullOrWhiteSpace(minimumVersion) && SoftwareIdentityVersionComparer.CompareVersions(provider.VersionScheme, provider.Version, minimumVersion) < 0) {
                    return !IsCanceled;
                }

                if (!String.IsNullOrWhiteSpace(maximumVersion) && SoftwareIdentityVersionComparer.CompareVersions(provider.VersionScheme, provider.Version, maximumVersion) > 0) {
                    return !IsCanceled;
                }
            }
            return YieldFromSwidtag(provider, searchKey);
        }

        internal bool YieldFromSwidtag(Package pkg, string searchKey) {
            if (pkg == null) {
                return !IsCanceled;
            }

            var provider = pkg._swidtag;
            var targetFilename = provider.Links.Select(each => each.Attributes[Iso19770_2.Discovery.TargetFilename]).WhereNotNull().FirstOrDefault();
            var summary = new MetadataIndexer(provider)[Iso19770_2.Attributes.Summary.LocalName].FirstOrDefault();

            var fastPackageReference = pkg.Location.AbsoluteUri;
            
            if (YieldSoftwareIdentity(fastPackageReference, provider.Name, provider.Version, provider.VersionScheme, summary, fastPackageReference, searchKey, null, targetFilename) != null) {
                // yield all the meta/attributes
                if (provider.Meta.Any(
                    m => {
                        var element = AddMeta(fastPackageReference);
                        var attributes = m.Attributes;
                        return attributes.Keys.Any(key => {
                            var nspace = key.Namespace.ToString();
                            if (String.IsNullOrWhiteSpace(nspace)) {
                                return AddMetadata(element, key.LocalName, attributes[key]) == null;
                            }

                            return AddMetadata(element, new Uri(nspace), key.LocalName, attributes[key]) == null;
                        });
                    })) {
                    return !IsCanceled;
                }

                if (provider.Links.Any(link => AddLink(link.HRef, link.Relationship, link.MediaType, link.Ownership, link.Use, link.Media, link.Artifact) == null)) {
                    return !IsCanceled;
                }

                if (provider.Entities.Any(entity => AddEntity(entity.Name, entity.RegId, entity.Role, entity.Thumbprint) == null)) {
                    return !IsCanceled;
                }

                //installing a package from bootstrap site needs to prompt a user. Only auto-boostrap is not prompted.
                var pm = PackageManagementService as PackageManagementService;
                string isTrustedSource = pm.InternalPackageManagementInstallOnly ? "false" : "true";
                if (AddMetadata(fastPackageReference, "FromTrustedSource", isTrustedSource) == null) {
                    return !IsCanceled;
                }
            }
            return !IsCanceled;
        }

        private static bool AnyNullOrEmpty(params string[] args) {
            return args.Any(String.IsNullOrWhiteSpace);
        }
    }
}