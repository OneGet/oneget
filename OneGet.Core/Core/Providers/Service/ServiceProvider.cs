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
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using DuckTyping;
    using Extensions;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    public class ServiceProviderInstance : DuckTypedClass {
        internal ServiceProviderInstance(object instance)
            : base(instance) {
            #region generate-memberinit ServiceProvider-interface
            GetServiceProviderName = GetRequiredDelegate<Interface.GetServiceProviderName>(instance);
            InitializeProvider = GetOptionalDelegate<Interface.InitializeProvider>(instance);
            SupportedDownloadSchemes = GetOptionalDelegate<Interface.SupportedDownloadSchemes>(instance);
            DownloadFile = GetOptionalDelegate<Interface.DownloadFile>(instance);
            SupportedArchiveExtensions = GetOptionalDelegate<Interface.SupportedArchiveExtensions>(instance);
            IsSupportedArchive = GetOptionalDelegate<Interface.IsSupportedArchive>(instance);
            UnpackArchive = GetOptionalDelegate<Interface.UnpackArchive>(instance);
            #endregion

        }

        #region generate-members ServiceProvider-interface
        internal readonly Interface.GetServiceProviderName GetServiceProviderName;
        internal readonly Interface.InitializeProvider InitializeProvider;
        internal readonly Interface.SupportedDownloadSchemes SupportedDownloadSchemes;
        internal readonly Interface.DownloadFile DownloadFile;
        internal readonly Interface.SupportedArchiveExtensions SupportedArchiveExtensions;
        internal readonly Interface.IsSupportedArchive IsSupportedArchive;
        internal readonly Interface.UnpackArchive UnpackArchive;
        #endregion

        internal ServiceProviderInstance(Type type)
            : this(Activator.CreateInstance(type)) {
        }

        public static bool IsInstanceCompatible(object instance) {
            return instance != null && IsTypeCompatible(instance.GetType());
        }

        public static bool IsTypeCompatible(Type type) {
            if (type == null) {
                return false;
            }

            var publicMethods = type.GetPublicMethods().ToArray();

            return true
            #region generate-istypecompatible ServiceProvider-interface
                && publicMethods.Any(each => DuckTypedExtensions.IsNameACloseEnoughMatch(each.Name, "GetServiceProviderName") && typeof (Interface.GetServiceProviderName).IsDelegateAssignableFromMethod(each))
            #endregion

            ;
        }

        public class Interface {
            #region declare ServiceProvider-interface
            /// <summary>
            /// Returns the name of the Provider. Doesn't need callback .
            /// </summary>
            /// <required/>
            /// <returns></returns>
            internal delegate string GetServiceProviderName();

            internal delegate void InitializeProvider(Callback c);

            internal delegate IEnumerable<string> SupportedDownloadSchemes(Callback c);
            internal delegate void DownloadFile(Uri remoteLocation, string localFilename, Callback c);

            internal delegate IEnumerable<string> SupportedArchiveExtensions(Callback c);
            internal delegate bool IsSupportedArchive(string localFilename, Callback c);

            internal delegate void UnpackArchive(string localFilename, string destinationFolder ,Callback c);

            #endregion
        }
    }

    public enum ServiceProviderApi {
        #region generate-enum ServiceProvider-interface
        GetServiceProviderName,
        InitializeProvider,
        SupportedDownloadSchemes,
        DownloadFile,
        SupportedArchiveExtensions,
        IsSupportedArchive,
        UnpackArchive,
        #endregion

    }

    public class ServiceProvider {
        private readonly ServiceProviderInstance _provider;

        internal ServiceProvider(ServiceProviderInstance provider) {
            _provider = provider;
        }

        public bool IsSupported(ServiceProviderApi api) {
            switch (api) {
                #region generate-issupported ServiceProvider-interface
                 case ServiceProviderApi.GetServiceProviderName:
                    return _provider.GetServiceProviderName.IsSupported();
                 case ServiceProviderApi.InitializeProvider:
                    return _provider.InitializeProvider.IsSupported();
                 case ServiceProviderApi.SupportedDownloadSchemes:
                    return _provider.SupportedDownloadSchemes.IsSupported();
                 case ServiceProviderApi.DownloadFile:
                    return _provider.DownloadFile.IsSupported();
                 case ServiceProviderApi.SupportedArchiveExtensions:
                    return _provider.SupportedArchiveExtensions.IsSupported();
                 case ServiceProviderApi.IsSupportedArchive:
                    return _provider.IsSupportedArchive.IsSupported();
                 case ServiceProviderApi.UnpackArchive:
                    return _provider.UnpackArchive.IsSupported();
                #endregion

            }
            return false;
        }

    }
}