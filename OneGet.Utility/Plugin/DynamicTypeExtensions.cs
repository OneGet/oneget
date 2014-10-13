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

namespace Microsoft.OneGet.Utility.Plugin {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using Extensions;

    internal static class DynamicTypeExtensions {
        private static readonly Type[] _emptyTypes = {
        };

        internal static void OverrideInitializeLifetimeService(this TypeBuilder dynamicType) {
            // add override of InitLifetimeService so this object doesn't fall prey to timeouts
            var il = dynamicType.DefineMethod("InitializeLifetimeService", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual, CallingConventions.HasThis, typeof (object), _emptyTypes).GetILGenerator();

            il.LoadNull();
            il.Return();
        }

        internal static void GenerateIsMethodImplemented(this TypeBuilder dynamicType) {
            // special case -- the IsMethodImplemented method can give the interface owner information as to
            // which methods are actually implemented.
            var implementedMethodsField = dynamicType.DefineField("__implementedMethods", typeof (HashSet<string>), FieldAttributes.Private);

            var il = dynamicType.CreateMethod("IsMethodImplemented", typeof (bool), typeof (string));

            il.LoadThis();
            il.LoadField(implementedMethodsField);
            il.LoadArgument(1);
            il.CallVirutal(typeof (HashSet<string>).GetMethod("Contains"));
            il.Return();
        }

        internal static void GenerateMethodForDirectCall(this TypeBuilder dynamicType, MethodInfo method, FieldBuilder backingField, MethodInfo instanceMethod) {
            var il = dynamicType.CreateMethod(method);
            // the target object has a method that matches.
            // let's use that.
            il.LoadThis();
            il.LoadField(backingField);

            for (var i = 0; i < method.GetParameterTypes().Length; i++) {
                il.LoadArgument(i + 1);
            }

            il.CallVirutal(instanceMethod);
            il.Return();
        }

        internal static ILGenerator CreateMethod(this TypeBuilder dynamicType, MethodInfo method) {
            return dynamicType.CreateMethod(method.Name, method.ReturnType, method.GetParameterTypes());
        }

        internal static ILGenerator CreateMethod(this TypeBuilder dynamicType, string methodName, Type returnType, params Type[] parameterTypes) {
            var methodBuilder = dynamicType.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, CallingConventions.HasThis, returnType, parameterTypes);

            return methodBuilder.GetILGenerator();
        }

        internal static void GenerateMethodForDelegateCall(this TypeBuilder dynamicType, MethodInfo method, FieldBuilder field) {
            var il = dynamicType.CreateMethod(method);

            // the target object has a property or field that matches the signature we're looking for.
            // let's use that.

            var delegateType = WrappedDelegate.GetFuncOrActionType(method.GetParameterTypes(), method.ReturnType);

            il.LoadThis();
            il.LoadField(field);
            for (var i = 0; i < method.GetParameterTypes().Length; i++) {
                il.LoadArgument(i + 1);
            }
            il.CallVirutal(delegateType.GetMethod("Invoke"));
            il.Return();
        }

        internal static void GenerateStubMethod(this TypeBuilder dynamicType, MethodInfo method) {
            var il = dynamicType.CreateMethod(method);
            do {
                if (method.ReturnType != typeof (void)) {
                    if (method.ReturnType.IsPrimitive) {
                        if (method.ReturnType == typeof (double)) {
                            il.LoadDouble(0.0);
                            break;
                        }

                        if (method.ReturnType == typeof (float)) {
                            il.LoadFloat(0.0F);
                            break;
                        }

                        il.LoadInt32(0);

                        if (method.ReturnType == typeof (long) || method.ReturnType == typeof (ulong)) {
                            il.ConvertToInt64();
                        }

                        break;
                    }

                    if (method.ReturnType.IsEnum) {
                        // should really find out the actual default?
                        il.LoadInt32(0);
                        break;
                    }

                    if (method.ReturnType.IsValueType) {
                        var result = il.DeclareLocal(method.ReturnType);
                        il.LoadLocalAddress(result);
                        il.InitObject(method.ReturnType);
                        il.LoadLocation(0);
                        break;
                    }

                    il.LoadNull();
                }
            } while (false);
            il.Return();
        }
    }
}