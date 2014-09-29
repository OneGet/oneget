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
    using Api;
    using Providers;

    #region generate-delegates response-apis

    public delegate bool OkToContinue();

    public delegate bool YieldSoftwareIdentity(string fastPath, string name, string version, string versionScheme, string summary, string source, string searchKey, string fullPath, string packageFileName);

    public delegate bool YieldSoftwareMetadata(string parentFastPath, string name, string value);

    public delegate bool YieldEntity(string parentFastPath, string name, string regid, string role, string thumbprint);

    public delegate bool YieldLink(string parentFastPath, string referenceUri, string relationship, string mediaType, string ownership, string use, string appliesToMedia, string artifact);

    public delegate bool YieldSwidtag(string fastPath, string xmlOrJsonDoc);

    public delegate bool YieldMetadata(string fieldId, string @namespace, string name, string value);

    public delegate bool YieldPackageSource(string name, string location, bool isTrusted,bool isRegistered, bool isValidated);

    public delegate bool YieldDynamicOption(string name, string expectedType, bool isRequired);

    public delegate bool YieldKeyValuePair(string key, string value);

    public delegate bool YieldValue(string value);

    #endregion

    internal delegate bool YieldArchiver(string name, IArchiver instnace, ulong version, IRequest request);

    internal delegate bool YieldDownloader(string name, IDownloader instnace, ulong version, IRequest request);

    internal delegate bool YieldMetaProvider(string name, IMetaProvider instnace, ulong version, IRequest request);

    internal delegate bool YieldPackageProvider(string name, IPackageProvider instnace, ulong version, IRequest request);

    public delegate string GetMessageString(string messageText);

    public delegate bool IsCancelled();

}