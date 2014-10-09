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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Collections;
    using Extensions;

    public class DynamicInterface {
        public static readonly DynamicInterface Instance = new DynamicInterface();

        private static readonly Dictionary<Types, bool> _isCompatibleCache = new Dictionary<Types, bool>();
        private static readonly Dictionary<string, ProxyClass> _proxyClassDefinitions = new Dictionary<string, ProxyClass>();

        public TInterface Create<TInterface>(params Type[] types) {
            return (TInterface)Create(typeof (TInterface), types);
        }

        public object Create(Type tInterface, params Type[] types) {
            if (tInterface == null) {
                throw new ArgumentNullException("tInterface");
            }

            types = types ?? new Type[0];

            if (!tInterface.GetVirtualMethods().Any()) {
                throw new Exception("Interface Type '{0}' doesn not have any virtual or abstract methods".format(tInterface.FullNiceName()));
            }

            if (!IsTypeCompatible(tInterface, types)) {
                var missing = GetMissingMethods(tInterface, types).ToArray();
                var badctors = FilterOnMissingDefaultConstructors(types).ToArray();

                var msg = badctors.Length == 0 ? ""
                    : "\r\nTypes ({0}) do not support a Default Constructor\r\n".format(badctors.Select(each => each.FullName).Quote().JoinWithComma());

                msg += missing.Length == 0 ? "" :
                    "\r\nTypes ({0}) are missing the following methods from interface ('{1}'):\r\n  {2}".format(
                        types.Select(each => each.FullName).Quote().JoinWithComma(),
                        tInterface.FullNiceName(),
                        missing.Select(each => each.ToSignatureString()).Quote().JoinWith("\r\n  "));

                throw new Exception(msg);
            }

            // create actual instance 
            return CreateProxy(tInterface, types.Select(Activator.CreateInstance).ToArray());
        }

        public object Create(Type tInterface, params string[] typeNames) {
            if (tInterface == null) {
                throw new ArgumentNullException("tInterface");
            }
            typeNames = typeNames ?? new string[0];

            return Create(tInterface, (Type[])typeNames.Select(Type.GetType));
        }

        public TInterface Create<TInterface>(params string[] typeNames) {
            return (TInterface)Create(typeof (TInterface), typeNames);
        }

        private IEnumerable<object> Flatten(IEnumerable<object> items) {
            if (items == null) {
                yield break;
            }

            if (items is IAsyncAction) {
                yield return items;
                yield break;
            }
            

            foreach (var item in items) {
                if (item is object[] || item is IEnumerable<object>) {
                    foreach (var inner in Flatten(item as IEnumerable<object>)) {
                        if (inner != null) {
                            yield return inner;
                        }
                    }
                    continue;
                }
                yield return item;
            }
        }

        private IEnumerable<object> Flatten(params object[] items) {
            return Flatten(items as IEnumerable<object>);
        }

        public TInterface Create<TInterface>(params object[] instances) {
            return (TInterface)Create(typeof (TInterface), instances);
        }

        public object Create(Type tInterface, params object[] instances) {
            if (tInterface == null) {
                throw new ArgumentNullException("tInterface");
            }

            if (instances.Length == 0) {
                throw new ArgumentException("No instances given", "instances");
            }

            if (!tInterface.GetVirtualMethods().Any()) {
                throw new Exception("Interface Type '{0}' doesn not have any virtual or abstract methods".format(tInterface.FullNiceName()));
            }
            instances = Flatten(instances).ToArray();

            if (instances.Any(each => each == null)) {
                throw new ArgumentException("One or more instances are null", "instances");
            }

            // shortcut if the interface is already implemented in the object.
            if (instances.Length == 1 && tInterface.IsInstanceOfType(instances[0])) {
                return instances[0];
            }

            if (!IsInstanceCompatible(tInterface, instances)) {
                var missing = GetMethodsMissingFromInstances(tInterface, instances);
                var msg = "\r\nObjects are missing the following methods from interface ('{0}'):\r\n  {1}".format(
                    tInterface.FullNiceName(),
                    missing.Select(each => each.ToSignatureString()).Quote().JoinWith("\r\n  "));

                throw new Exception(msg);
            }

            return CreateProxy(tInterface, instances);
        }

        public bool IsTypeCompatible(Type tInterface, params Type[] types) {
            return _isCompatibleCache.GetOrAdd(new Types(tInterface, types), () => {
#if DEEPDEBUG
                Debug.WriteLine(String.Format("IsTypeCompatible {0} for {1}",typeof(TInterface).Name , types[0].Name));

                foreach (var s in types.Where(each => each.GetDefaultConstructor() == null).Select(each => string.Format("{0} has no default constructor", each.Name))) {
                    Debug.WriteLine(s);
                }
#endif
                try {
                    if (types.Any(actualType => actualType.GetDefaultConstructor() == null)) {
                        return false;
                    }
                } catch {
                    // if the actualType's assembly isn't available to this appdomain, we can't create an object from the type anyway.
                    return false;
                }
#if DEEPDEBUG
                foreach (var s in types) {
                    Debug.WriteLine(string.Format("»»»{0}",s.Name));

                    var mm = GetMissingMethods<TInterface>(types);
                    foreach (var method in mm) {
                        Debug.WriteLine(string.Format("»»»    MISSING {0}", method.Name));
                    }
                }
#endif
                // verify that required methods are present.
                return !GetMissingMethods(tInterface, types).Any();
            });
        }

        public bool IsTypeCompatible<TInterface>(params Type[] types) {
            return IsTypeCompatible(typeof (TInterface), types);
        }

        private IEnumerable<Type> FilterOnMissingDefaultConstructors(params Type[] types) {
            return types.Where(actualType => actualType.GetDefaultConstructor() == null);
        }

        private IEnumerable<MethodInfo> GetMissingMethods(Type tInterface, params Type[] types) {
            var publicMethods = types.GetPublicMethods();
            return tInterface.GetRequiredMethods().Where(method => publicMethods.FindMethod(method) == null);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Provided for completion, future work may use this instead of the less-type-safe version")]
        private IEnumerable<MethodInfo> GetMissingMethods<TInterface>(params Type[] types) {
            return GetMissingMethods(typeof (TInterface), types);
        }

        public bool IsInstanceCompatible<TInterface>(params object[] instances) {
            return IsInstanceCompatible(typeof (TInterface), instances);
        }

        public bool IsInstanceCompatible(Type tInterface, params object[] instances) {
            if (tInterface == null) {
                throw new ArgumentNullException("tInterface");
            }

            if (instances == null) {
                throw new ArgumentNullException("instances");
            }

            if (instances.Length == 0) {
                throw new ArgumentException("No instances given", "instances");
            }

            if (instances.Any(each => each == null)) {
                throw new ArgumentException("One or more instances are null", "instances");
            }

            // this will be faster if this type has been checked before.
            if (IsTypeCompatible(tInterface, instances.Select(each => each.GetType()).ToArray())) {
                return true;
            }

#if DEEPDEBUG
            var missing = GetMethodsMissingFromInstances<TInterface>(instances).ToArray();

            if (missing.Length > 0 ) {
                var msg = "\r\nObjects are missing the following methods from interface ('{0}'):\r\n  {1}".format(
                    typeof (TInterface).FullNiceName(),
                    missing.Select(each => each.ToSignatureString()).Quote().JoinWith("\r\n  "));
                Debug.WriteLine(msg);
            }
#endif

            // see if any specified object has something for every required method.
            return !instances.Aggregate((IEnumerable<MethodInfo>)tInterface.GetRequiredMethods(), GetMethodsMissingFromInstance).Any();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Provided for completion, future work may use this instead of the less-type-safe version")]
        private IEnumerable<MethodInfo> GetMethodsMissingFromInstances<TInterface>(params object[] instances) {
            return GetMethodsMissingFromInstances(typeof (TInterface), instances);
        }

        private IEnumerable<MethodInfo> GetMethodsMissingFromInstances(Type tInterface, params object[] instances) {
            return instances.Aggregate((IEnumerable<MethodInfo>)tInterface.GetRequiredMethods(), GetMethodsMissingFromInstance);
        }

        private IEnumerable<MethodInfo> GetMethodsMissingFromInstance(IEnumerable<MethodInfo> methods, object instance) {
            var instanceSupportsMethod = DynamicInterfaceExtensions.GenerateInstanceSupportsMethod(instance);
            var instanceType = instance.GetType();

            var instanceMethods = instanceType.GetPublicMethods();
            var instanceFields = instanceType.GetPublicDelegateFields();
            var instanceProperties = instanceType.GetPublicDelegateProperties();

            // later, we can check for a delegate-creation function that can deliver a delegate to us by name and parameter types.
            // currently, we don't need that, so we're not going to implement it right away.

            return methods.Where(method =>
                !instanceSupportsMethod(method.Name) || (
                    instanceMethods.FindMethod(method) == null &&
                    instanceFields.FindDelegate(instance, method) == null &&
                    instanceProperties.FindDelegate(instance, method) == null
                    ));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Provided for completion, future work may use this instead of the less-type-safe version")]
        private TInterface CreateProxy<TInterface>(params object[] instances) {
            return (TInterface)CreateProxy(typeof (TInterface), instances);
        }

        private object CreateProxy(Type tInterface, params object[] instances) {
            var matrix = instances.SelectMany(instance => {
                var instanceType = instance.GetType();
                // get all the public interfaces for the instances and place them at the top
                // and let it try to bind against those.
                return instanceType.GetInterfaces().Where(each => each.IsPublic).Select(each => new {
                    instance,
                    SupportsMethod = DynamicInterfaceExtensions.GenerateInstanceSupportsMethod(instance),
                    Type = each,
                    Methods = each.GetPublicMethods(),
                    Fields = each.GetPublicDelegateFields(),
                    Properties = each.GetPublicDelegateProperties()
                }).ConcatSingleItem(new {
                    instance,
                    SupportsMethod = DynamicInterfaceExtensions.GenerateInstanceSupportsMethod(instance),
                    Type = instanceType,
                    Methods = instanceType.GetPublicMethods(),
                    Fields = instanceType.GetPublicDelegateFields(),
                    Properties = instanceType.GetPublicDelegateProperties()
                });
            }).ToArray();

            var instanceMethods = new OrderedDictionary<Type, List<MethodInfo, MethodInfo>>();
            var delegateMethods = new List<Delegate, MethodInfo>();
            var stubMethods = new List<MethodInfo>();
            var usedInstances = new List<Type, object>();

            foreach (var method in tInterface.GetVirtualMethods()) {
                // figure out where it's going to get implemented
                var found = false;
                foreach (var instance in matrix) {
                    if (method.Name == "IsMethodImplemented") {
                        // skip for now, we'll implement this at the end
                        found = true;
                        break;
                    }

                    if (instance.SupportsMethod(method.Name)) {
                        var instanceMethod = instance.Methods.FindMethod(method);
                        if (instanceMethod != null) {
                            instanceMethods.GetOrAdd(instance.Type, () => new List<MethodInfo, MethodInfo>()).Add(method, instanceMethod);
                            if (!usedInstances.Contains(instance.Type, instance.instance)) {
                                usedInstances.Add(instance.Type, instance.instance);
                            }
                            found = true;
                            break;
                        }

                        var instanceDelegate = instance.Fields.FindDelegate(instance.instance, method) ?? instance.Properties.FindDelegate(instance.instance, method);
                        if (instanceDelegate != null) {
                            delegateMethods.Add(instanceDelegate, method);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found && (tInterface.IsInterface || method.IsAbstract)) {
#if DEEPDEBUG
                    Debug.WriteLine(" Generating stub method for {0} -> {1}".format(typeof (TInterface).NiceName(), method.ToSignatureString()));
#endif
                    stubMethods.Add(method);
                }
            }

            // now we can calculate the key based on the content of the *Methods collections
            var key = tInterface.FullName + ":::" + instanceMethods.Keys.Select(each => each.FullName + "." + instanceMethods[each].Select(mi => mi.Value.ToSignatureString()).JoinWithComma()).JoinWith(";\r\n") +
                      "::" + delegateMethods.Select(each => each.GetType().FullName).JoinWith(";\r\n") +
                      "::" + stubMethods.Select(mi => mi.ToSignatureString()).JoinWithComma();

            var proxyClass = _proxyClassDefinitions.GetOrAdd(key, () => new ProxyClass(tInterface, instanceMethods, delegateMethods, stubMethods));

            return proxyClass.CreateInstance(usedInstances, delegateMethods);
        }

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
            return assembly.CreatableTypes().Where(each => IsTypeCompatible<TInterface>(each));
        }
    }
}