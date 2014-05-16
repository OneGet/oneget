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

namespace Microsoft.OneGet.Core.Providers.Meta {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DuckTyping;
    using Extensions;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    public class NewMetaProvider : DuckTypedClass {
        public NewMetaProvider(object instance)
            : base(instance) {
            #region generate-memberinit MetaProvider-interface
            CreateProvider = GetOptionalDelegate<Interface.CreateProvider>(instance);
            GetMetaProviderName = GetOptionalDelegate<Interface.GetMetaProviderName>(instance);
            GetProviderNames = GetOptionalDelegate<Interface.GetProviderNames>(instance);
            InitializeProvider = GetOptionalDelegate<Interface.InitializeProvider>(instance);
            #endregion

        }

        public NewMetaProvider(Type type)
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

                #region generate-istypecompatible MetaProvider-interface

                #endregion

                ;
        }

        public class Interface {
            #region declare MetaProvider-interface

            /// <summary>
            ///     Will instantiate an instance of a provider given it's name
            /// </summary>
            /// <required />
            /// <param name="name">the name of the provider to create</param>
            /// <returns>an instance of the provider.</returns>
            public delegate object CreateProvider(string name);

            /// <summary>
            ///     Gets the name of this MetaProvider
            /// </summary>
            /// <required />
            /// <returns>the name of the MetaProvider.</returns>
            public delegate string GetMetaProviderName();

            /// <summary>
            ///     Returns a collection of all the names of Providers this MetaProvider can create.
            /// </summary>
            /// <required />
            /// <returns>a collection of all the names of Providers this MetaProvider can create</returns>
            public delegate IEnumerable<string> GetProviderNames();

            /// <summary>
            ///     Allows the MetaProvider to do one-time initialization.
            ///     This is called after the MetaProvider is instantiated and DuckTyped.
            /// </summary>
            /// <optional />
            /// <param name="c">Callback Delegate Reference</param>
            public delegate void InitializeProvider(Callback c);

            #endregion
        }

        #region generate-members MetaProvider-interface
        internal readonly Interface.CreateProvider CreateProvider;
        internal readonly Interface.GetMetaProviderName GetMetaProviderName;
        internal readonly Interface.GetProviderNames GetProviderNames;
        internal readonly Interface.InitializeProvider InitializeProvider;
        #endregion

    }
}