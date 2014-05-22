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
    using System.Globalization;
    using System.Linq;
    using AppDomains;
    using Extensions;

#if OLD_DUCKTYPER
    public class DuckTypedClass : MarshalByRefObject {
        private readonly object _instance;
        private readonly CreateDelegate _createDelegateImpl;
        private readonly HashSet<string> _supportedMethods = new HashSet<string>();

        // we don't want these objects being gc's out because they remain unused...
        public override object InitializeLifetimeService() {
            return null;
        }

        public object Instance {
            get {
                return _instance;
            }
        }

        internal DuckTypedClass(object instance) {
            _instance = instance;
#if OLD_DYNAMIC_DUCK_TYPING
            var duckType = GetType();
            // this model lets us construct the proxy
            // class without knowing how the original object
            // was constructed.
            if (!duckType.IsObjectCompatible(instance)) {
                throw new Exception("object instance is not compatible with DuckTypeClass");
            }

            var instanceMethods = instance.GetType().GetPublicMethods().ToArray();
            var instanceFields = instance.GetType().GetPublicFields().Where(each => each.FieldType.BaseType == typeof (MulticastDelegate)).ToArray();

            foreach (var member in duckType.GetRequiredMembers()) {
                var expectedDelegateType = member.FieldType;
                var method = instanceMethods.FirstOrDefault(each => DuckTypedExtensions.IsNameACloseEnoughMatch(each.Name, expectedDelegateType.Name) && expectedDelegateType.IsDelegateAssignableFromMethod(each));
                if (method == null) {
                    var delegatefield = instanceFields.FirstOrDefault(each => DuckTypedExtensions.IsNameACloseEnoughMatch(each.Name, expectedDelegateType.Name) && expectedDelegateType.IsDelegateAssignableFromDelegate(each.FieldType));
                    if (delegatefield == null) {
                        throw new Exception("object does not expose required method");
                    }
                    var dlg = delegatefield.GetValue(instance) as Delegate;
                    if (dlg != null) {
                        _supportedMethods.Add(expectedDelegateType.Name.ToLower(CultureInfo.CurrentCulture));
                        member.SetValue(this, expectedDelegateType.CreateProxiedDelegate(dlg));
                        continue;
                    }
                    throw new Exception("object does not expose required method");
                }
                // record that the implementer did supply an implementation
                // AddSupportedMethod(expectedDelegateType.Name);
                AddSupportedMethod(member.Name);

                member.SetValue(this, expectedDelegateType.CreateProxiedDelegate(instance, method));
            }

            foreach (var member in duckType.GetOptionalMembers()) {
                var expectedDelegateType = member.FieldType;
                var method = instanceMethods.FirstOrDefault(each => DuckTypedExtensions.IsNameACloseEnoughMatch(each.Name, expectedDelegateType.Name) && expectedDelegateType.IsDelegateAssignableFromMethod(each));
                if (method == null) {
                    var delegatefield = instanceFields.FirstOrDefault(each => DuckTypedExtensions.IsNameACloseEnoughMatch(each.Name, expectedDelegateType.Name) && expectedDelegateType.IsDelegateAssignableFromDelegate(each.FieldType));
                    if (delegatefield == null) {
                        // not supported.. that's ok.
#if DETAILED_DEBUG
                        Console.WriteLine("Type '{0}' is only a partial ducky-type of '{1}' because of missing member '{2}'", duckTypedInstance.GetType().Name, duckType.Name, member);
                        Event<Verbose>.Raise("QUACK1", "Type '{0}' is only a partial ducky-type of '{1}' because of missing member '{2}'", new object[] {duckTypedInstance.GetType().Name, duckType.Name, member});
#endif
                        if (member.GetValue(this) == null) {
                            member.SetValue(this, expectedDelegateType.CreateEmptyDelegate());
                        }
                        continue;
                    }
                    var dlg = delegatefield.GetValue(instance) as Delegate;
                    if (dlg != null) {
                        // AddSupportedMethod(expectedDelegateType.Name);
                        AddSupportedMethod(member.Name);
                        member.SetValue(this, expectedDelegateType.CreateProxiedDelegate(dlg));
                        continue;
                    }

                    // not supported.. that's ok. if the value is null, we should probably give it an empty delegate
                    if (member.GetValue(this) == null) {
                        member.SetValue(this, expectedDelegateType.CreateEmptyDelegate());
#if DETAILED_DEBUG
                        Console.WriteLine("Type '{0}' is only a partial ducky-type of '{1}' because of missing member '{2}'", duckTypedInstance.GetType().Name, duckType.Name, member);
                        Event<Verbose>.Raise("QUACK2", "Type '{0}' is only a partial ducky-type of '{1}' because of missing member '{2}'", new object[] {duckTypedInstance.GetType().Name, duckType.Name, member});
#endif
                    }
                    continue;
                }
                // record that the implementer did supply an implementation
                // AddSupportedMethod(expectedDelegateType.Name);
                AddSupportedMethod(member.Name);

                member.SetValue(this, expectedDelegateType.CreateProxiedDelegate(instance, method));
            }
#endif
        }

        protected T GetOptionalDelegate<T>(object instance) {
            return default(T);
        }

        protected T GetRequiredDelegate<T>(object instance) {
            // instance.CreateProxiedDelegate<T>(null);
            return default(T);
        }

        protected static bool InstanceSupports<T>(object instance) {
            // check to see if the instance has a method "public bool IsMethodImplemented(string methodName)"
            // if it does, ask the object if the method is supported first
            // this allows the object to have the method physically implemented, yet not acutally expose it
            // (useful for metaprovider-provided providers)


            return false;
        }

        private void AddSupportedMethod(string name) {
            // Event<Verbose>.Raise("BindingMethod", "{0} on {1}", name, _instance.GetType().Name);
            _supportedMethods.Add(name.ToLower(CultureInfo.CurrentCulture));
        }

        internal static bool InstanceSupportsMethod(object target, string name) {
            var dtt = target as DuckTypedClass;
            return dtt != null && dtt._supportedMethods.Contains(name.ToLower(CultureInfo.CurrentCulture));
        }

        internal class DirectAttribute : Attribute {
        }

        internal class MethodAttribute : Attribute {
        }

        internal class OptionalAttribute : Attribute {
        }
        internal class AtLeastOneSetAttribute : Attribute {
        }

        internal class InSetAttribute : Attribute {
            internal readonly int SetNumber;
            internal InSetAttribute(int number) {
                SetNumber = number;
            }
        }

        internal class PropertyAttribute : Attribute {
        }

        internal class ProxiedAttribute : Attribute {
        }

        internal class RequiredAttribute : Attribute {
        }

     
    }
#endif
}