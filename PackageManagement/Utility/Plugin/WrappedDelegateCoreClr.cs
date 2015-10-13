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

namespace Microsoft.PackageManagement.Internal.Utility.Plugin {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;

    public static class WrappedDelegate {
        internal static T CreateProxiedDelegate<T>(this Delegate delegateInstance) {
            return (T)(object)delegateInstance.CreateProxiedDelegate(typeof (T));
        }

        internal static Delegate CreateProxiedDelegate(this Delegate dlg, Type expectedDelegateType) {
            // the delegate to the proxy object
            MethodInfo method = expectedDelegateType.GetMethod("Invoke");
            return method.CreateDelegate(expectedDelegateType, expectedDelegateType.CreateWrappedProxy(dlg));
        } 

        internal static T CreateProxiedDelegate<T>(this object instance, MethodInfo method) {
            return (T)(object)CreateProxiedDelegate(instance, method, typeof (T));
        }

        internal static Delegate CreateProxiedDelegate(this object instance, MethodInfo method, Type expectedDelegateType) {
            #region DEAD CODE

            // we need our public delegate to be calling an object that is MarshalByRef
            // instead, we're creating a delegate thats getting bound to the DTI, which isn't
            // extra hoops not needed:
            // var actualDelegate = Delegate.CreateDelegate(expectedDelegateType, duckTypedInstance, method);
            //   var huh = (object)Delegate.CreateDelegate(
            //    proxyDelegateType,
            //    actualDelegate.Target,
            //    actualDelegate.Method,
            //    true);

            #endregion

            // the func/action type for the proxied delegate.
            var proxyDelegateType = GetFuncOrActionType(expectedDelegateType.GetDelegateParameterTypes(), expectedDelegateType.GetDelegateReturnType());

            // create the actual delegate with the function/action instead
            var actualDelegate = method.CreateDelegate(proxyDelegateType, instance);

            return actualDelegate;
        }

        internal static object CreateWrappedProxy(this Type expectedDelegateType, Delegate dlg) {
            // the func/action type for the proxied delegate.
            var proxyDelegateType = GetFuncOrActionType(expectedDelegateType.GetDelegateParameterTypes(), expectedDelegateType.GetDelegateReturnType());

            MethodInfo method = dlg.GetMethodInfo();
            // create the actual delegate with the function/action instead
            // we already have a viable delegate to use.
            // var actualDelegate = Delegate.CreateDelegate(proxyDelegateType, instance, method);
            var actualDelegate = (object)method.CreateDelegate(proxyDelegateType, dlg.Target);

            // the wrapped delegate class
            var wrappedType = dlg.GetWrappedDelegateType();

            // Create an instance of the WrappedDelegate object (this ties keeps the actual delegate in the right appdomain)
            // and exposes a delegate that is tied to a MarshalByRef object.
            return wrappedType.GetConstructor(new[] {
                proxyDelegateType
            }).Invoke(new[] {
                actualDelegate
            });
        }

        public static Type GetWrappedDelegateType(this MethodInfo method) {
            if (method.ReturnType == typeof (void)) {
                return GetWrappedActionType(method.GetParameterTypes());
            }
            return GetWrappedFunctionType(method.GetParameterTypes(), method.ReturnType);
        }

        public static Type GetWrappedDelegateType(this Delegate dlg) {
            var delegateType = dlg.GetType();

            var returnType = delegateType.GetDelegateReturnType();
            if (returnType == typeof (void)) {
                return GetWrappedActionType(delegateType.GetDelegateParameterTypes());
            }
            return GetWrappedFunctionType(delegateType.GetDelegateParameterTypes(), returnType);
        }

        public static Type GetFuncOrActionType(IEnumerable<Type> argTypes, Type returnType) {
            return returnType == typeof (void) ? Expression.GetActionType(argTypes.ToArray()) : Expression.GetFuncType(argTypes.ConcatSingleItem(returnType).ToArray());
        }

        public static Type GetWrappedActionType(IEnumerable<Type> argTypes) {
            var types = argTypes.ToArray();

            switch (types.Length) {
                case 0:
                    return typeof (WrappedAction);
                case 1:
                    return typeof (WrappedAction<>).MakeGenericType(types);
                case 2:
                    return typeof (WrappedAction<,>).MakeGenericType(types);
                case 3:
                    return typeof (WrappedAction<,,>).MakeGenericType(types);
                case 4:
                    return typeof (WrappedAction<,,,>).MakeGenericType(types);
                case 5:
                    return typeof (WrappedAction<,,,,>).MakeGenericType(types);
                case 6:
                    return typeof (WrappedAction<,,,,,>).MakeGenericType(types);
                case 7:
                    return typeof (WrappedAction<,,,,,,>).MakeGenericType(types);
                case 8:
                    return typeof (WrappedAction<,,,,,,,>).MakeGenericType(types);
                case 9:
                    return typeof (WrappedAction<,,,,,,,,>).MakeGenericType(types);
                case 10:
                    return typeof (WrappedAction<,,,,,,,,,>).MakeGenericType(types);
                case 11:
                    return typeof (WrappedAction<,,,,,,,,,,>).MakeGenericType(types);
                case 12:
                    return typeof (WrappedAction<,,,,,,,,,,,>).MakeGenericType(types);
                case 13:
                    return typeof (WrappedAction<,,,,,,,,,,,,>).MakeGenericType(types);
                case 14:
                    return typeof (WrappedAction<,,,,,,,,,,,,,>).MakeGenericType(types);
                case 15:
                    return typeof (WrappedAction<,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 16:
                    return typeof (WrappedAction<,,,,,,,,,,,,,,,>).MakeGenericType(types);
                default:
                    return (Type)null;
            }
        }

        public static Type GetWrappedFunctionType(IEnumerable<Type> argTypes, Type returnType) {
            var types = argTypes.ConcatSingleItem(returnType).ToArray();
            switch (types.Length) {
                case 1:
                    return typeof (WrappedFunc<>).MakeGenericType(types);
                case 2:
                    return typeof (WrappedFunc<,>).MakeGenericType(types);
                case 3:
                    return typeof (WrappedFunc<,,>).MakeGenericType(types);
                case 4:
                    return typeof (WrappedFunc<,,,>).MakeGenericType(types);
                case 5:
                    return typeof (WrappedFunc<,,,,>).MakeGenericType(types);
                case 6:
                    return typeof (WrappedFunc<,,,,,>).MakeGenericType(types);
                case 7:
                    return typeof (WrappedFunc<,,,,,,>).MakeGenericType(types);
                case 8:
                    return typeof (WrappedFunc<,,,,,,,>).MakeGenericType(types);
                case 9:
                    return typeof (WrappedFunc<,,,,,,,,>).MakeGenericType(types);
                case 10:
                    return typeof (WrappedFunc<,,,,,,,,,>).MakeGenericType(types);
                case 11:
                    return typeof (WrappedFunc<,,,,,,,,,,>).MakeGenericType(types);
                case 12:
                    return typeof (WrappedFunc<,,,,,,,,,,,>).MakeGenericType(types);
                case 13:
                    return typeof (WrappedFunc<,,,,,,,,,,,,>).MakeGenericType(types);
                case 14:
                    return typeof (WrappedFunc<,,,,,,,,,,,,,>).MakeGenericType(types);
                case 15:
                    return typeof (WrappedFunc<,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 16:
                    return typeof (WrappedFunc<,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 17:
                    return typeof (WrappedFunc<,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                default:
                    return (Type)null;
            }
        }
    }
}