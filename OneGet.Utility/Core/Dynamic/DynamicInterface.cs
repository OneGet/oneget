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
    using Collections;
    using Extensions;

    internal class RequiredAttribute : Attribute {
    }

    public class DynamicInterface {
        public static readonly DynamicInterface Instance = new DynamicInterface();
        private static readonly Dictionary<Types, bool> _isCompatibleCache = new Dictionary<Types, bool>();
        private static readonly Dictionary<string, ProxyClass> _proxyClassDefinitions = new Dictionary<string, ProxyClass>();

        public TInterface Create<TInterface>(params Type[] types) {
            if (!typeof (TInterface).GetVirtualMethods().Any()) {
                throw new Exception("Interface Type '{0}' doesn not have any virtual or abstract methods".format(typeof (TInterface).FullNiceName()));
            }

            if (!IsTypeCompatible<TInterface>(types)) {
                var missing = GetMissingMethods<TInterface>(types).ToArray();
                var badctors = FilterOnMissingDefaultConstructors(types).ToArray();

                var msg = badctors.Length == 0 ? ""
                    : "\r\nTypes ({0}) do not support a Default Constructor\r\n".format(badctors.Select(each => each.FullName).Quote().JoinWithComma());

                msg += missing.Length == 0 ? "" :
                    "\r\nTypes ({0}) are missing the following methods from interface ('{1}'):\r\n  {2}".format(
                        types.Select(each => each.FullName).Quote().JoinWithComma(),
                        typeof (TInterface).FullNiceName(),
                        missing.Select(each => each.ToSignatureString()).Quote().JoinWith("\r\n  "));

                throw new Exception(msg);
            }

            // create actual instance 
            return CreateProxy<TInterface>(types.Select(Activator.CreateInstance).ToArray());
        }

        public TInterface Create<TInterface>(params string[] typeNames) {
            return Create<TInterface>((Type[])typeNames.Select(Type.GetType));
        }


        private IEnumerable<object> Flatten(IEnumerable<object> items) {
            if (items == null) {
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
            if (instances.Length == 0) {
                throw new ArgumentException("No instances given", "instances");
            }

            if (!typeof(TInterface).GetVirtualMethods().Any()) {
                throw new Exception("Interface Type '{0}' doesn not have any virtual or abstract methods".format(typeof(TInterface).FullNiceName()));
            }
            instances = Flatten(instances).ToArray();

            if (instances.Any(each => each == null)) {
                throw new ArgumentException("One or more instances are null", "instances");
            }


            // shortcut if the interface is already implemented in the object.
            if (instances.Length == 1 && instances[0] is TInterface) {
                return (TInterface)instances[0] ;
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

        public bool IsTypeCompatible<TInterface>(params Type[] types) {
            return _isCompatibleCache.GetOrAdd(new Types(typeof (TInterface), types), () => {
                if (types.Any(actualType => actualType.GetDefaultConstructor() == null)) {
                    return false;
                }
                // verify that required methods are present.
                return !GetMissingMethods<TInterface>(types).Any();
            });
        }

        private IEnumerable<Type> FilterOnMissingDefaultConstructors(params Type[] types) {
            return types.Where(actualType => actualType.GetDefaultConstructor() == null);
        }

        private IEnumerable<MethodInfo> GetMissingMethods<TInterface>(params Type[] types) {
            var publicMethods = types.GetPublicMethods();
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
            return !instances.Aggregate((IEnumerable<MethodInfo>)typeof (TInterface).GetRequiredMethods(), GetMethodsMissingFromInstance).Any();
        }

        private IEnumerable<MethodInfo> GetMethodsMissingFromInstances<TInterface>(params object[] instances) {
            return instances.Aggregate((IEnumerable<MethodInfo>)typeof (TInterface).GetRequiredMethods(), GetMethodsMissingFromInstance);
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

        private TInterface CreateProxy<TInterface>(params object[] instances) {
            var matrix = instances.Select(instance => new {
                instance,
                SupportsMethod = DynamicInterfaceExtensions.GenerateInstanceSupportsMethod(instance),
                Type = instance.GetType(),
                Methods = instance.GetType().GetPublicMethods(),
                Fields = instance.GetType().GetPublicDelegateFields(),
                Properties = instance.GetType().GetPublicDelegateProperties()
            }).ToArray();

            var instanceMethods = new OrderedDictionary<Type, List<MethodInfo, MethodInfo>>();
            var delegateMethods = new List<Delegate, MethodInfo>();
            var stubMethods = new List<MethodInfo>();
            var usedInstances = new List<object>();

            foreach (var method in typeof (TInterface).GetVirtualMethods()) {
                // figure out where it's going to get implemented
                foreach (var instance in matrix) {
                    if (method.Name == "IsMethodImplemented") {
                        // skip for now, we'll implement this at the end
                        continue;
                    }

                    if (instance.SupportsMethod(method.Name)) {
                        var instanceMethod = instance.Methods.FindMethod(method);
                        if (instanceMethod != null) {
                            instanceMethods.GetOrAdd(instance.Type, () => new List<MethodInfo, MethodInfo>()).Add(method, instanceMethod);
                            if (!usedInstances.Contains(instance.instance)) {
                                usedInstances.Add(instance.instance);
                            }
                            continue;
                        }

                        var instanceDelegate = instance.Fields.FindDelegate(instance.instance, method) ?? instance.Properties.FindDelegate(instance.instance, method);
                        if (instanceDelegate != null) {
                            delegateMethods.Add(instanceDelegate, method);
                            continue;
                        }
                    }
                    if (typeof (TInterface).IsInterface || method.IsAbstract) {
                        stubMethods.Add(method);
                    }
                }
            }

            // now we can calculate the key based on the content of the *Methods collections
            var key = instanceMethods.Keys.Select(each => each.FullName + instanceMethods[each].Select(mi => mi.Value.ToSignatureString()).JoinWithComma()).JoinWith(";") +
                      "::" + delegateMethods.Select(each => each.GetType().FullName).JoinWith(";") +
                      "::" + stubMethods.Select(mi => mi.ToSignatureString()).JoinWithComma();

            var proxyClass = _proxyClassDefinitions.GetOrAdd(key, () => new ProxyClass(typeof (TInterface), instanceMethods, delegateMethods, stubMethods));

            return (TInterface)proxyClass.CreateInstance(usedInstances, delegateMethods);
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
            return assembly.GetTypes().Where(each => each.IsPublic && each.BaseType != typeof (MulticastDelegate) && IsTypeCompatible<TInterface>(each));
        }
    }
}