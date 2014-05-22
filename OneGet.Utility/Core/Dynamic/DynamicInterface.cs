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

namespace Microsoft.OneGet.Core.Dynamic {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using AppDomains;
    using Extensions;
    using Tasks;

    internal class RequiredAttribute : Attribute {
    }

    public class DynamicInterface {
        private readonly static Type[] _emptyTypes = {};

        private static int _counter = 1;
        private readonly Dictionary<Types, bool> _compatibilityMatrix = new Dictionary<Types, bool>();

        public TInterface Create<TInterface>(params Type[] actualTypes) {
            if (!typeof (TInterface).IsInterface) {
                throw new Exception("Type '{0}' is not an interface".format(typeof (TInterface).FullName));
            }

            if (!IsTypeCompatible<TInterface>(actualTypes)) {
                var missing = GetMissingMethods<TInterface>(actualTypes).ToArray();
                var badctors = FilterOnMissingDefaultConstructors(actualTypes).ToArray();

                var msg = badctors.Length == 0 ? "" 
                    : "\r\nTypes ({0}) do not support a Default Constructor\r\n".format(badctors.Select(each =>each.FullName).Quote().JoinWithComma()) ;

                msg += missing.Length == 0 ? "" :
                    "\r\nTypes ({0}) are missing the following methods from interface ('{1}'):\r\n  {2}".format(
                        actualTypes.Select(each => each.FullName).Quote().JoinWithComma(),
                        typeof (TInterface).FullNiceName(),
                        missing.Select(each => each.ToSignatureString()).Quote().JoinWith("\r\n  "));

                throw new Exception(msg);
            }

            // create actual instance 
            return CreateProxy<TInterface>(actualTypes.Select(Activator.CreateInstance).ToArray());
        }

        public TInterface Create<TInterface>(params string[] typeNames) {
            if (!typeof (TInterface).IsInterface) {
                throw new Exception("Type '{0}' is not an interface".format(typeof (TInterface).FullName));
            }

            return Create<TInterface>(typeNames.Select(Type.GetType));
        }

        public TInterface Create<TInterface>(params object[] instances) {
            if (instances.Length == 0) {
                throw new ArgumentException("No instances given","instances");
            }

            if (instances.Any(each => each == null)) {
                throw new ArgumentException("One or more instances are null", "instances");
            }

            if (!typeof (TInterface).IsInterface) {
                throw new Exception("Type '{0}' is not an interface".format(typeof (TInterface).FullName));
            }

            if (!IsInstanceCompatible<TInterface>(instances)) {
                var missing = GetMethodsMissingFromInstances<TInterface>(instances);
                var msg = "\r\nObjects are missing the following methods from interface ('{0}'):\r\n  {1}".format(
                        typeof (TInterface).FullNiceName(),
                        missing.Select(each => each.ToSignatureString()).Quote().JoinWith("\r\n  "));

                throw new Exception(msg);
            }

            return CreateProxy<TInterface>(instances);
        }

        public bool IsTypeCompatible<TInterface>(params Type[] actualTypes) {
            return _compatibilityMatrix.GetOrAdd(new Types(typeof (TInterface), actualTypes), () => {
                if (actualTypes.Any(actualType => actualType.GetDefaultConstructor() == null)) {
                    return false;
                }
                // verify that required methods are present.
                return !GetMissingMethods<TInterface>(actualTypes).Any();
            });
        }

        private IEnumerable<Type> FilterOnMissingDefaultConstructors(params Type[] actualTypes) {
            return actualTypes.Where(actualType => actualType.GetDefaultConstructor() == null);
        }

        private IEnumerable<MethodInfo> GetMissingMethods<TInterface>(params Type[] actualTypes) {
            var publicMethods = actualTypes.GetPublicMethods();
            return typeof (TInterface).GetRequiredMethods().Where(method => publicMethods.FindMethod(method) == null);
        }

        public bool IsInstanceCompatible<TInterface>(params object[] instances) {
            if (instances.Length == 0) {
                throw new ArgumentException("No instances given", "instances");
            }

            if (instances.Any(each => each == null)) {
                throw new ArgumentException("One or more instances are null", "instances");
            }

            // this will be faster if this type has been checked before.
            if (IsTypeCompatible<TInterface>(instances.Select(each => each.GetType()).ToArray())) {
                return true;
            }

            // see if any specified object has something for every required method.
            return !instances.Aggregate((IEnumerable<MethodInfo>)typeof(TInterface).GetRequiredMethods(), GetMethodsMissingFromInstance).Any();

        }

        private IEnumerable<MethodInfo> GetMethodsMissingFromInstances<TInterface>(params object[] instances) {
            return instances.Aggregate((IEnumerable<MethodInfo>)typeof(TInterface).GetRequiredMethods(), GetMethodsMissingFromInstance);
        }

        private IEnumerable<MethodInfo> GetMethodsMissingFromInstance(IEnumerable<MethodInfo> methods, object actualInstance) {
            var instanceSupportsMethod = DynamicInterfaceExtensions.GenerateInstanceSupportsMethod(actualInstance);
            var instanceType = actualInstance.GetType();

            var instanceMethods = instanceType.GetPublicMethods();
            var instanceFields = instanceType.GetPublicDelegateFields();
            var instanceProperties = instanceType.GetPublicDelegateProperties();

            // later, we can check for a delegate-creation function that can deliver a delegate to us by name and parameter types.
            // currently, we don't need that, so we're not going to implement it right away.

            return methods.Where( method =>
                    !instanceSupportsMethod(method.Name) || (
                        instanceMethods.FindMethod(method) == null && 
                        instanceFields.FindDelegate(actualInstance, method) == null && 
                        instanceProperties.FindDelegate(actualInstance, method) == null
                        )); 
        }


        private TInterface CreateProxy<TInterface>(params object[] instances) {
            var proxyClass = new ProxyClass(typeof (TInterface), instances);
            return (TInterface)proxyClass.Instance;
        }

#if NOT_HERE
        private TInterface xCreateProxy<TInterface>(params object[] instances) {
            var interfaceType = typeof (TInterface);
            var methodsToImplement = interfaceType.GetMethods();

            var dynamicType = DefineDynamicType<TInterface>(instances.Select(each => each.GetType().Name).JoinWith("_"));

            var afterInstantiation = new List<Action<object>>();
            var implementedMethods = new HashSet<string>();
            var backingFields = new List<FieldBuilder>();

            dynamicType.OverrideInitializeLifetimeService();

            foreach (var actualInstance in instances) {
                methodsToImplement = ImplementMethods(actualInstance, dynamicType, methodsToImplement, implementedMethods, afterInstantiation, backingFields);
            }

            foreach (var method in methodsToImplement) {
                // did not find a matching method or signature, or the instance told us that it doesn't actually support it 
                // that's ok, if we get here, it must not be a required method.
                // we'll implement a placeholder method for it.
                dynamicType.GenerateDefaultMethod(method);    
            }

            dynamicType.DefineConstructorWithBackingField(backingFields);

            var proxyType = dynamicType.CreateType();

            var proxyInstance = proxyType.CreateInstance(instances);

            foreach (var action in afterInstantiation) {
                action(proxyInstance);
            }

            var imf = proxyType.GetField("__implementedMethods", BindingFlags.NonPublic | BindingFlags.Instance);
            if (imf != null) {
                imf.SetValue(proxyInstance, implementedMethods);
            }

            return (TInterface)proxyInstance;
        }

        private MethodInfo[] ImplementMethods(object actualInstance, TypeBuilder dynamicType, MethodInfo[] methodsToImplement, HashSet<string> implementedMethods, List<Action<object>> afterInstantiation, List<FieldBuilder> backingFields) {
            var instanceSupportsMethod = DynamicInterfaceExtensions.GenerateInstanceSupportsMethod(actualInstance);

            var instanceType = actualInstance.GetType();
            var instanceMethods = instanceType.GetPublicMethods();
            var instanceFields = instanceType.GetPublicDelegateFields();
            var instanceProperties = instanceType.GetPublicDelegateProperties();
            // var backingField = dynamicType.DefineConstructorWithBackingField(instanceType);
            var backingField = dynamicType.DefineField("_instance_{0}".format(++_counter), instanceType, FieldAttributes.Private);
            backingFields.Add(backingField);

            return methodsToImplement.Where(method => !ImplementMethod(method, dynamicType, implementedMethods, afterInstantiation, actualInstance, instanceSupportsMethod, instanceMethods, instanceFields, instanceProperties, backingField)).ToArray();
        }

        private bool ImplementMethod(MethodInfo method, TypeBuilder dynamicType, HashSet<string> implementedMethods, List<Action<object>> afterInstantiation,object actualInstance, Func<string,bool> instanceSupportsMethod, MethodInfo[] instanceMethods, FieldInfo[] instanceFields, PropertyInfo[] instanceProperties, FieldBuilder backingField  ) {

            if (method.Name == "IsImplemented") {
                dynamicType.GenerateIsImplemented(method);

                implementedMethods.Add(method.Name);
                return true;
            }

            if (instanceSupportsMethod(method.Name)) {
                var instanceMethod = instanceMethods.FindMethod(method);
                if (instanceMethod != null) {
                    dynamicType.GenerateMethodForDirectCall(method, backingField, instanceMethod);

                    implementedMethods.Add(method.Name);
                    return true;
                }

                var instanceDelegate = instanceFields.FindDelegate(actualInstance, method) ?? instanceProperties.FindDelegate(actualInstance, method);
                if (instanceDelegate != null) {
                    var fieldName = dynamicType.GenerateMethodForDelegateCall(method);

                    // make sure this object loads the value of delegate into the field after instantiation time.
                    afterInstantiation.Add((instance) => {
                        var f = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                        f.SetValue(instance, instanceDelegate);
                    });
                    implementedMethods.Add(method.Name);
                    return true;
                }
            }

            return false;
        }

        private static TypeBuilder DefineDynamicType<TInterface>(string actualInstanceName) {
            var interfaceType = typeof (TInterface);
            var proxyName = "proxy_{0}{1}_{2}".format(interfaceType.Name, actualInstanceName, _counter++);

            var dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("{0}.Assembly".format(proxyName)), AssemblyBuilderAccess.Run);

            // Define a dynamic module in this assembly.
            var dynamicModule = dynamicAssembly.DefineDynamicModule("{0}.Module".format(proxyName));

            // Define a runtime class with specified name and attributes.
            var dynamicType = dynamicModule.DefineType(proxyName, TypeAttributes.Public, typeof (MarshalByRefObject));
            dynamicType.AddInterfaceImplementation(interfaceType);
            return dynamicType;
        }
#endif

        public IEnumerable<Type> FilterTypesCompatibleTo<TInterface>(IEnumerable<Type> types) {
            if (types == null) {
                return Enumerable.Empty<Type>();
            }

            return types.Where(each => IsTypeCompatible<TInterface>(each));
        }

        public IEnumerable<Type> FilterTypesCompatibleTo<TInterface>(Assembly assembly) {
            if (assembly == null) {
                return Enumerable.Empty<Type>();
            }
            return assembly.GetTypes().Where(each => each.IsPublic && each.BaseType != typeof (MulticastDelegate) && IsTypeCompatible<TInterface>(each));
        }
    }

    internal static class DynamicTypeExtensions {

        private static int _counter = 0;



        internal static FieldBuilder DefineConstructorWithBackingField(this TypeBuilder dynamicType, Type instanceType) {
            // backing store for the object instance
            var backingField = dynamicType.DefineField("_instance_{0}".format(++_counter), instanceType, FieldAttributes.Private);
            

            // add constructor that takes the specific type of object that we're going to bind
            var constructor = dynamicType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] {
                instanceType
            });

            var il = constructor.GetILGenerator();
            
            // store actualInstance in backingField
            il.LoadArgument(0);
            il.LoadArgument(1);
            il.StoreField(backingField);

            // return
            il.Return();
            return backingField;
        }

        internal static void OverrideInitializeLifetimeService(this TypeBuilder dynamicType) {
            // add override of InitLifetimeService so this object doesn't fall prey to timeouts
            var ils = dynamicType.DefineMethod("InitializeLifetimeService", MethodAttributes.Public, CallingConventions.HasThis, typeof (object), new Type[] {
            });
            var il = ils.GetILGenerator();

            il.LoadNull();
            il.Return();
        }

        internal static void GenerateIsImplemented(this TypeBuilder dynamicType, MethodInfo method) {
            // special case -- the IsImplemented method can give the interface owner information as to
            // which methods are actually implemented.
            var implementedMethodsField = dynamicType.DefineField("__implementedMethods", typeof (HashSet<string>), FieldAttributes.Private);

            var il = dynamicType.CreateMethod(method);

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
            var methodBuilder = dynamicType.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.HasThis, method.ReturnType, method.GetParameterTypes());
            return methodBuilder.GetILGenerator();
        }

        internal static string GenerateMethodForDelegateCall(this TypeBuilder dynamicType, MethodInfo method) {
            var il = dynamicType.CreateMethod(method);
            var fieldName = "__{0}".format(method.Name);

            // the target object has a property or field that matches the signature we're looking for.
            // let's use that.

            var delegateType = WrappedDelegate.GetFuncOrActionType(method.GetParameterTypes(), method.ReturnType);
            var field = dynamicType.DefineField(fieldName, delegateType, FieldAttributes.Private);

            il.LoadThis();
            il.LoadField(field);
            for (var i = 0; i < method.GetParameterTypes().Length; i++) {
                il.LoadArgument(i + 1);
            }
            il.CallVirutal(delegateType.GetMethod("Invoke"));
            il.Return();
            return fieldName;
        }

        internal static void GenerateDefaultMethod(this TypeBuilder dynamicType, MethodInfo method) {
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

        internal static object CreateInstance(this Type proxyType, object actualInstance) {
            var proxyConstructor = proxyType.GetConstructor(new[] {
                actualInstance.GetType()
            });

            var proxyInstance = proxyConstructor.Invoke(new[] {
                actualInstance
            });
            return proxyInstance;
        }

        internal static object CreateInstance(this Type proxyType, object[] actualInstances) {
            
            var proxyConstructor = proxyType.GetConstructor(actualInstances.Select(each => each.GetType()).ToArray());
            
            var proxyInstance = proxyConstructor.Invoke(actualInstances);
            return proxyInstance;
        }
    }
}