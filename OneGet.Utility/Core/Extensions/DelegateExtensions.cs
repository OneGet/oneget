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

namespace Microsoft.OneGet.Core.Extensions {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    internal static class DelegateExtensions {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "shhh.")]
        internal static Type GetDelegateReturnType(this Delegate delegateInstance) {
            return GetDelegateReturnType(delegateInstance.GetType());
        }

        internal static Type GetDelegateReturnType(this Type delegateType) {
            if (delegateType.BaseType != typeof (MulticastDelegate)) {
                throw new ApplicationException("Not a delegate.");
            }

            var invoke = delegateType.GetMethod("Invoke");
            if (invoke == null) {
                throw new ApplicationException("Not a delegate.");
            }
            return invoke.ReturnType;
        }

        internal static IEnumerable<Type> GetDelegateParameterTypes(this Type delegateType) {
            if (delegateType.BaseType != typeof (MulticastDelegate)) {
                throw new ApplicationException("Not a delegate.");
            }

            var invoke = delegateType.GetMethod("Invoke");
            if (invoke == null) {
                throw new ApplicationException("Not a delegate.");
            }

            return invoke.GetParameters().Select(each => each.ParameterType);
        }

        internal static IEnumerable<string> GetDelegateParameterNames(this Type delegateType) {
            if (delegateType.BaseType != typeof(MulticastDelegate)) {
                throw new ApplicationException("Not a delegate.");
            }

            var invoke = delegateType.GetMethod("Invoke");
            if (invoke == null) {
                throw new ApplicationException("Not a delegate.");
            }

            return invoke.GetParameters().Select(each => each.Name);
        }

        internal static IEnumerable<Type> GetParameterTypes(this MethodInfo methodInfo) {
            return methodInfo.GetParameters().Select(each => each.ParameterType);
        }

        internal static bool IsDelegateAssignableFromMethod(this Type delegateType, MethodInfo methodInfo) {
            if (delegateType == null || methodInfo == null) {
                return false;
            }

            // are the return types the same?
            if (methodInfo.ReturnType != delegateType.GetDelegateReturnType()) {
                return false;
            }

#if CAN_WE_CHEAT
            // this is an opportunity to decide if a set of parameters is equal.
            // unfortunately, even if we *say* they are, they don't coerce
            // so unless we put something somewhere else that could
            // coerce one type to another I can't see how we can use this.
            //
            // I wanted to have the Invoke delegate type declared independently in providers
            // and just nod my head that yes, the types are equal but of course
            // the typechecking is more stringent than that, and we'd have to perform
            // some miracle-level idenfication and marshalling to pass that thru
            // when it was just easier and alias Invoke to Callback
            //
            // when plugins are loaded out-of-proc, this is all up for grabs again
            // since we're going to have to detect and marshal those manually anyway.

            var methTypes = methodInfo.GetParameterTypes();
            var deleTypes = delegateType.GetDelegateParameterTypes();

            // are all the parameters the same types?
            using (var e1 = methTypes.GetEnumerator())
            using (var e2 = deleTypes.GetEnumerator()) {
                while (e1.MoveNext()) {
                    if (!(e2.MoveNext() && (e1.Current.Equals(e2.Current) || (e1.Current.Name == "Invoke" && e2.Current.Name == "Invoke"))))
                        return false;
                }
                if (e2.MoveNext())
                    return false;
            }
#else
            if (!methodInfo.GetParameterTypes().SequenceEqual(delegateType.GetDelegateParameterTypes())) {
                return false;
            }
#endif
            return true;
        }

        internal static bool IsDelegateAssignableFromDelegate(this Type delegateType, Type delegateType2) {
            if (delegateType == null || delegateType2 == null) {
                return false;
            }

            // ensure both are actually delegates
            if (delegateType.BaseType != typeof (MulticastDelegate) || delegateType2.BaseType != typeof (MulticastDelegate)) {
                return false;
            }

            // are the return types the same?
            if (delegateType2.GetDelegateReturnType() != delegateType.GetDelegateReturnType()) {
                return false;
            }

            // are all the parameters the same types?
            if (!delegateType2.GetDelegateParameterTypes().SequenceEqual(delegateType.GetDelegateParameterTypes())) {
                return false;
            }
            return true;
        }


        private static readonly Dictionary<Type, Delegate> _emptyDelegates = new Dictionary<Type, Delegate>();

        internal static Delegate CreateEmptyDelegate(this Type delegateType) {
            if (delegateType == null) {
                throw new ArgumentNullException("delegateType");
            }
            if (delegateType.BaseType != typeof (MulticastDelegate)) {
                throw new ArgumentException("must be a delegate", "delegateType");
            }

            return _emptyDelegates.GetOrAdd(delegateType, () => {
                var delegateReturnType = delegateType.GetDelegateReturnType();

                var dynamicMethod = new DynamicMethod(string.Empty, delegateReturnType, delegateType.GetDelegateParameterTypes().ToArray());
                var il = dynamicMethod.GetILGenerator();

                if (delegateReturnType.FullName != "System.Void") {
                    if (delegateReturnType.IsValueType) {
                        il.Emit(OpCodes.Ldc_I4, 0);
                    } else {
                        il.Emit(OpCodes.Ldnull);
                    }
                }
                il.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(delegateType);
            });
        }
    }
}
