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

namespace Microsoft.OneGet.ServicesProvider.Test.Utility {
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     This generated class can be copied to any project that implements a OneGet provider
    ///     This gives type-safe access to the callbacks and APIs without having to take a direct
    ///     dependency on the OneGet core Assemblies.
    /// </summary>
    internal static class CallbackExtensions {
        /// <summary>
        ///     This transforms a generic delegate into a type-specific delegate so that you can
        ///     call the target delegate with the appropriate signature.
        /// </summary>
        /// <typeparam name="TDelegate"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static TDelegate CastDelegate<TDelegate>(this Delegate src) where TDelegate : class {
            return (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), src.Target, src.Method, true); // throw on fail
        }

        /// <summary>
        ///     This calls the supplied delegate with the name of the callback that we're actaully looking for
        ///     and then casts the resulting delegate back to the type that we're expecting.
        /// </summary>
        /// <typeparam name="TDelegate"></typeparam>
        /// <param name="c"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TDelegate Resolve<TDelegate>(this Func<string, IEnumerable<object>, object> c, params object[] args) where TDelegate : class {
            var delegateType = typeof(TDelegate);
            if (delegateType.BaseType != typeof(MulticastDelegate)) {
                throw new Exception("Generic Type Incorrect");
            }
            // calling with null args set returns the delegate instead of calling the delegate.
            // return CastDelegate<TDelegate>(CastDelegate<Func<string, IEnumerable<object>, Delegate>>(call)(delegateType.Name, null));
            // var m = call(delegateType.Name, null);
            var m = (Delegate)c(delegateType.Name, null);
            return m == null ? null : CastDelegate<TDelegate>(m);
        }
    }
}