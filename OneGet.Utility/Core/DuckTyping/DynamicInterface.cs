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

namespace Microsoft.OneGet.Core.DuckTyping {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using AppDomains;
    using Extensions;


    internal class RequiredAttribute : Attribute {
    }

    public class DynamicInterface {
        private static int _counter = 1;
        private readonly Dictionary<TwoTypes, bool> _compatibilityMatrix = new Dictionary<TwoTypes, bool>();
        private readonly Dictionary<Type, MethodInfo[]> _requiredMethods = new Dictionary<Type, MethodInfo[]>();

        public T Create<T>(Type actualType) {
            if (!typeof (T).IsInterface) {
                throw new Exception("Type '{0}' is not an interface".format(typeof (T).FullName));
            }

            if (!IsTypeCompatible<T>(actualType)) {
                throw new Exception("Type '{0}' is not compatible with interface '{1}'".format(actualType.FullName, typeof (T).FullName));
            }

            // create actual instance 
            var actualInstance = Activator.CreateInstance(actualType);

            return CreateProxy<T>(actualInstance);
        }

        public T Create<T>(string typeName) {
            if (!typeof (T).IsInterface) {
                throw new Exception("Type '{0}' is not an interface".format(typeof (T).FullName));
            }

            return Create<T>(Type.GetType(typeName));
        }

        public T Create<T>(object instance) {
            if (instance == null) {
                throw new ArgumentNullException("instance");
            }

            if (!typeof (T).IsInterface) {
                throw new Exception("Type '{0}' is not an interface".format(typeof (T).FullName));
            }

            if (!IsInstanceCompatible<T>(instance)) {
                throw new Exception("Object of type '{0}' is not compatible with interface '{1}'".format(instance.GetType().FullName, typeof (T).FullName));
            }

            return CreateProxy<T>(instance);
        }

        public bool IsTypeCompatible<T>(Type type) {
            return _compatibilityMatrix.GetOrAdd(new TwoTypes(typeof (T), type), () => {
                // type-compatible implies a public parameterless constructor
                if (type.GetConstructor(new Type[] {
                }) == null) {
                    return false;
                }

                // verify that required methods are present.
                var candidateMethods = type.GetPublicMethods().ToArray();

                return GetRequiredMethods<T>().All(method => candidateMethods.FindMethod(method) != null);
            });
        }

        public bool IsInstanceCompatible<T>(object actualInstance) {
            if (actualInstance == null) {
                return false;
            }

            // this will be faster if this type has been checked before.
            if (IsTypeCompatible<T>(actualInstance.GetType())) {
                return true;
            }

            var isMethodImplemented = IsMethodImplentedFunction(actualInstance);

            var candidateType = actualInstance.GetType();

            var candidateMethods = candidateType.GetPublicMethods().ToArray();
            var candidateFields = candidateType.GetPublicFields().Where(each => each.FieldType.BaseType == typeof (MulticastDelegate)).ToArray();
            var candidateProperties = candidateType.GetPublicProperties().Where(each => each.PropertyType.BaseType == typeof (MulticastDelegate)).ToArray();

            return GetRequiredMethods<T>().All(
                method =>
                    isMethodImplemented(method.Name) && (
                        candidateMethods.FindMethod(method) != null ||
                        candidateFields.FindDelegate(actualInstance, method) != null ||
                        candidateProperties.FindDelegate(actualInstance, method) != null));

            // later, we can check for a delegate-creation function that can deliver a delegate to us by name and parameter types.
            // currently, we don't need that, so we're not going to implement it right away.
        }

        private Func<string, bool> IsMethodImplentedFunction(object actualInstance) {
            // if the object implements an IsMethodImplemented Method, we'll be using that 
            // to see if the method is actually supposed to be used.
            // this enables an implementor to physically implement the function in the class
            // yet treat it as if it didn't. (see the PowerShellPackageProvider)
            var imiMethodInfo = actualInstance.GetType().GetMethod("IsMethodImplemented", new[] {
                typeof (string)
            });
            return imiMethodInfo == null ? (s) => true : actualInstance.CreateProxiedDelegate<Func<string, bool>>(imiMethodInfo);
        }

        private T CreateProxy<T>(object actualInstance) {
            var interfaceType = typeof (T);
            var candidateType = actualInstance.GetType();

            var candidateMethods = candidateType.GetPublicMethods().ToArray();
            var candidateFields = candidateType.GetPublicFields().Where(each => each.FieldType.BaseType == typeof (MulticastDelegate)).ToArray();
            var candidateProperties = candidateType.GetPublicProperties().Where(each => each.PropertyType.BaseType == typeof (MulticastDelegate)).ToArray();

            var proxyName = "proxy_{0}{1}_{2}".format(interfaceType.Name, actualInstance.GetType().Name, _counter++);

            var dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("{0}.Assembly".format(proxyName)), AssemblyBuilderAccess.Run);

            // Define a dynamic module in this assembly.
            var dynamicModule = dynamicAssembly.DefineDynamicModule("{0}.Module".format(proxyName));

            // Define a runtime class with specified name and attributes.
            var dynamicType = dynamicModule.DefineType(proxyName, TypeAttributes.Public, typeof (MarshalByRefObject));
            dynamicType.AddInterfaceImplementation(interfaceType);

            // backing store for the object instance
            var backingField = dynamicType.DefineField("_instance", candidateType, FieldAttributes.Private);

            // add constructor that takes the specific type of object that we're going to bind
            var constructor = dynamicType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] {
                candidateType
            });

            var il = constructor.GetILGenerator();

            var delegatesToLoad = new List<Action<object>>();

            // store actualInstance in backingField
            il.LoadArgument(0);
            il.LoadArgument(1);
            il.StoreField(backingField);

            // return
            il.Return();

            // add override of InitLifetimeService so this object doesn't fall prey to timeouts
            var ils = dynamicType.DefineMethod("InitializeLifetimeService", MethodAttributes.Public, CallingConventions.HasThis, typeof (object), new Type[] {
            });
            il = ils.GetILGenerator();

            il.LoadNull();
            il.Return();

            var isMethodImplemented = IsMethodImplentedFunction(actualInstance);
            var implementedMethods = new HashSet<string>();
            FieldBuilder implementedMethodsField;

            foreach (var method in interfaceType.GetMethods()) {
                var parameterTypes = method.GetParameterTypes();
                var methodBuilder = dynamicType.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.HasThis, method.ReturnType, parameterTypes);
                il = methodBuilder.GetILGenerator();

                if (method.Name == "IsImplemented") {
                    // special case -- the IsImplemented method can give the interface owner information as to
                    // which methods are actually implemented.
                    implementedMethodsField = dynamicType.DefineField("__implementedMethods", typeof (HashSet<string>), FieldAttributes.Private);

                    il.LoadThis();
                    il.LoadField(implementedMethodsField);
                    il.LoadArgument(1);
                    il.CallVirutal(typeof(HashSet<string>).GetMethod("Contains"));
                    il.Return();
                    
                    implementedMethods.Add(method.Name);
                    continue;
                }
                

                if (isMethodImplemented(method.Name)) {
                    var instanceMethod = candidateMethods.FindMethod(method);
                    if (instanceMethod != null) {
                        // the target object has a method that matches.
                        // let's use that.
                        il.LoadThis();
                        il.LoadField(backingField);

                        for (var i = 0; i < parameterTypes.Length; i++) {
                            il.LoadArgument(i + 1);
                        }

                        il.CallVirutal(instanceMethod);
                        il.Return();
                        implementedMethods.Add(method.Name);
                        continue;
                    }

                    var instanceDelegate = candidateFields.FindDelegate(actualInstance, method) ?? candidateProperties.FindDelegate(actualInstance, method);
                    if (instanceDelegate != null) {
                        var fieldName = "__{0}".format(method.Name);

                        // the target object has a property or field that matches the signature we're looking for.
                        // let's use that.

                        var delegateType = WrappedDelegate.GetFuncOrActionType(parameterTypes, method.ReturnType);
                        var field = dynamicType.DefineField(fieldName, delegateType, FieldAttributes.Private);

                        il.LoadThis();
                        il.LoadField(field);
                        for (var i = 0; i < parameterTypes.Length; i++) {
                            il.LoadArgument(i + 1);
                        }
                        il.CallVirutal(delegateType.GetMethod("Invoke"));
                        il.Return();

                        // make sure this object loads the value of delegate into the field after instantiation time.
                        delegatesToLoad.Add((instance) => {
                            var f = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                            f.SetValue(instance, instanceDelegate);
                        });
                        implementedMethods.Add(method.Name);
                        continue;
                    }
                }

                // did not find a matching method or signature, or the instance told us that it doesn't actually support it 
                // that's ok, if we get here, it must not be a required method.
                // we'll implement a placeholder method for it.

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

            var proxyType = dynamicType.CreateType();
            var proxyConstructor = proxyType.GetConstructor(new[] {
                candidateType
            });
            var proxyInstance = proxyConstructor.Invoke(new[] {
                actualInstance
            });
            foreach (var action in delegatesToLoad) {
                action(proxyInstance);
            }

            var imf = proxyType.GetField("__implementedMethods", BindingFlags.NonPublic | BindingFlags.Instance);
            if (imf != null) {
                imf.SetValue(proxyInstance, implementedMethods);
            }

            return (T)proxyInstance;
        }

        private MethodInfo[] GetRequiredMethods<T>() {
            return _requiredMethods.GetOrAdd(typeof (T), () => typeof (T).GetMethods().Where(each => each.CustomAttributes.Any(attr => attr.AttributeType.Name.Equals("RequiredAttribute", StringComparison.OrdinalIgnoreCase))).ToArray());
        }

        public IEnumerable<Type> FilterTypesCompatibleTo<T>(IEnumerable<Type> types) {
            if (types == null) {
                return Enumerable.Empty<Type>();
            }

            return types.Where(IsTypeCompatible<T>);
        }

        public IEnumerable<Type> FilterTypesCompatibleTo<T>(Assembly assembly) {
            if (assembly == null) {
                return Enumerable.Empty<Type>();
            }
            return assembly.GetTypes().Where(each => each.IsPublic && each.BaseType != typeof(MulticastDelegate) && IsTypeCompatible<T>(each));
        }
    }
}