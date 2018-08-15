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

namespace Microsoft.PackageManagement.Internal.Implementation
{
    using Api;
    using PackageManagement.Implementation;
    using PackageManagement.Internal.Packaging;
    using PackageManagement.Packaging;
    using System;
    using System.Globalization;
    using System.Xml.Linq;
    using Messages = Resources.Messages;

    internal class SoftwareIdentityRequestObject : EnumerableRequestObject<SoftwareIdentity>
    {
        private SoftwareIdentity _currentItem;
        private readonly string _status;

        public SoftwareIdentityRequestObject(ProviderBase provider, IHostApi request, Action<RequestObject> action, string status)
            : base(provider, request, action)
        {
            _status = status;
            InvokeImpl();
        }

        private void CommitCurrentItem()
        {
            if (_currentItem != null)
            {
                Results.Add(_currentItem);
            }
            _currentItem = null;
        }

        public override string YieldSoftwareIdentity(string fastPath, string name, string version, string versionScheme, string summary, string source, string searchKey, string fullPath, string packageFileName)
        {
            Activity();
            CommitCurrentItem();

            _currentItem = new SoftwareIdentity
            {
                FastPackageReference = fastPath,
                Name = name,
                Version = version,
                VersionScheme = versionScheme,
                Summary = summary,
                Provider = (PackageProvider)Provider,
                Source = source,
                Status = _status,
                SearchKey = searchKey,
                FullPath = fullPath,
                PackageFilename = packageFileName
            };

            return fastPath;
        }

        public override string YieldSoftwareIdentityXml(string xmlSwidTag, bool commitImmediately = false)
        {
            Activity();
            CommitCurrentItem();

            if (string.IsNullOrWhiteSpace(xmlSwidTag))
            {
                return null;
            }

            try
            {
                XDocument xdoc = XDocument.Parse(xmlSwidTag);

                if (xdoc != null && xdoc.Root != null && Swidtag.IsSwidtag(xdoc.Root))
                {
                    _currentItem = new SoftwareIdentity(xdoc);
                    if (Provider != null)
                    {
                        _currentItem.Provider = (PackageProvider)Provider;
                    }
                }
                else
                {
                    Verbose(string.Format(CultureInfo.CurrentCulture, Resources.Messages.SwidTagXmlInvalidNameSpace, xmlSwidTag, Iso19770_2.Namespace.Iso19770_2));
                }
            }
            catch (Exception e)
            {
                Verbose(e.Message);
                Verbose(e.StackTrace);
            }

            if (_currentItem == null)
            {
                Warning(string.Format(CultureInfo.CurrentCulture, Messages.SwidTagXmlNotValid));
                return null;
            }

            string result = _currentItem.FastPackageReference;

            // provider author wants us to commit at once
            if (commitImmediately)
            {
                CommitCurrentItem();
            }

            return result;
        }

        /// <summary>
        ///     Adds a metadata key/value pair to the Swidtag.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override string AddMetadata(string name, string value)
        {
            Activity();

            SoftwareIdentity swid = _currentItem;
            return swid?.AddMetadataValue(swid.FastPackageReference, name, value);
        }

        /// <summary>
        /// Adds a tagID to the swidtag
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        public override string AddTagId(string tagId)
        {
            Activity();

            SoftwareIdentity swid = _currentItem;

            if (swid == null)
            {
                return null;
            }

            // we can't modify it once add so check that there is no swidtag
            if (string.IsNullOrWhiteSpace(swid.TagId))
            {
                swid.TagId = tagId;
            }

            return swid.TagId;
        }

        public override string AddCulture(string xmlLang)
        {
            Activity();

            SoftwareIdentity swid = _currentItem;

            if (swid == null)
            {
                return null;
            }

            // we can't modify it once add, so check that there is no swidtag
            if (string.IsNullOrWhiteSpace(swid.Culture))
            {
                swid.Culture = xmlLang;
            }

            return swid.Culture;
        }

        /// <summary>
        ///     Adds an attribute to a 'Meta' object. If called for a Swidtag or Entity, it implicitly adds a child Meta object.
        ///     Any other elementPath is an error and will be ignored.
        /// </summary>
        /// <param name="elementPath">
        ///     the string that represents one of :
        ///     a Swidtag (the fastPackageReference) (passing null as the elementPath will default to the swidtag)
        ///     an Entity (the result gained from an YieldEntity(...) call )
        ///     a Meta object (the result from previously calling AddMetadataValue(...) )
        /// </param>
        /// <param name="name">the name of the attribute to add</param>
        /// <param name="value">the value of the attribute to add</param>
        /// <returns></returns>
        public override string AddMetadata(string elementPath, string name, string value)
        {
            Activity();

            SoftwareIdentity swid = _currentItem;
            return swid?.AddMetadataValue(elementPath, name, value);
        }

        public override string AddMetadata(string elementPath, Uri @namespace, string name, string value)
        {
            Activity();

            SoftwareIdentity swid = _currentItem;
            if (swid == null || string.IsNullOrWhiteSpace(elementPath))
            {
                return null;
            }

            return swid.AddMetadataValue(elementPath, @namespace, name, value);
        }

        public override string AddMeta(string elementPath)
        {
            Activity();

            SoftwareIdentity swid = _currentItem;
            return swid?.AddMeta(elementPath);
        }

        public override string AddPayload()
        {
            SoftwareIdentity swid = _currentItem;
            return swid?.AddPayload().ElementUniqueId;
        }

        public override string AddEvidence(DateTime date, string deviceId)
        {
            SoftwareIdentity swid = _currentItem;
            return swid?.AddEvidence(date, deviceId).ElementUniqueId;
        }

        public override string AddDirectory(string elementPath, string directoryName, string location, string root, bool isKey)
        {
            Activity();

            SoftwareIdentity swid = _currentItem;
            return swid?.AddDirectory(elementPath, directoryName, location, root, isKey);
        }

        public override string AddFile(string elementPath, string fileName, string location, string root, bool isKey, long size, string version)
        {
            Activity();

            SoftwareIdentity swid = _currentItem;
            return swid?.AddFile(elementPath, fileName, location, root, isKey, size, version);
        }

        public override string AddProcess(string elementPath, string processName, int pid)
        {
            Activity();

            SoftwareIdentity swid = _currentItem;
            return swid?.AddProcess(elementPath, processName, pid);
        }

        public override string AddResource(string elementPath, string type)
        {
            Activity();

            SoftwareIdentity swid = _currentItem;
            return swid?.AddResource(elementPath, type);
        }

        public override string AddEntity(string name, string regid, string role, string thumbprint)
        {
            Activity();

            SoftwareIdentity swid = _currentItem;
            return swid?.AddEntity(name, regid, role, thumbprint);
        }

        public override string AddLink(Uri referenceUri, string relationship, string mediaType, string ownership, string use, string appliesToMedia, string artifact)
        {
            Activity();

            SoftwareIdentity swid = _currentItem;
            return swid?.AddLink(referenceUri, relationship, mediaType, ownership, use, appliesToMedia, artifact);
        }

        public override string AddDependency(string providerName, string packageName, string version, string source, string appliesTo)
        {
            Activity();
            SoftwareIdentity swid = _currentItem;
            return swid?.AddDependency(providerName, packageName, version, source, appliesTo);
        }

        protected override void Complete()
        {
            CommitCurrentItem();
            base.Complete();
        }
    }
}