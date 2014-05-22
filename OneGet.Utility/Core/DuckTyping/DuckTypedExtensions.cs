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
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using AppDomains;
    using Extensions;

    public static class DuckTypedExtensions {
        private static readonly IDictionary<Tuple<Type, Type>, bool> _compatibilityMatrix = new Dictionary<Tuple<Type, Type>, bool>();

#if OLD_DUCKTYPER
        internal static IEnumerable<FieldInfo> GetRequiredMembers(this Type duckType) {
            if (duckType != null && typeof (DuckTypedClass).IsAssignableFrom(duckType)) {
                return duckType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(each => each.GetCustomAttributes(typeof (DuckTypedClass.RequiredAttribute), true).Any());
            }
            return Enumerable.Empty<FieldInfo>();
        }

        internal static IEnumerable<FieldInfo> GetOptionalMembers(this Type duckType) {
            if (duckType != null && typeof (DuckTypedClass).IsAssignableFrom(duckType)) {
                return duckType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(each => each.GetCustomAttributes(typeof (DuckTypedClass.OptionalAttribute), true).Any());
            }
            return Enumerable.Empty<FieldInfo>();
        }
#endif 

        private static readonly IDictionary<Type,MethodInfo[]>  _methodCache = new Dictionary<Type, MethodInfo[]>();
        private static readonly IDictionary<Type, FieldInfo[]> _delegateFieldsCache = new Dictionary<Type, FieldInfo[]>();
        private static readonly IDictionary<Type, PropertyInfo[]> _delegatePropertiesCache = new Dictionary<Type, PropertyInfo[]>();
        private static readonly Dictionary<Type, MethodInfo[]> _requiredMethodsCache = new Dictionary<Type, MethodInfo[]>();

        internal static MethodInfo[] GetPublicMethods(this Type candidateType) {
            return _methodCache.GetOrAdd(candidateType, () => {
                if (candidateType != null) {
                    return candidateType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                }
                return new MethodInfo[0];
            });
        }

        internal static IEnumerable<FieldInfo> GetPublicFields(this Type candidateType) {
            if (candidateType != null) {
                return candidateType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            }
            return Enumerable.Empty<FieldInfo>();
        }

        internal static FieldInfo[] GetPublicDelegateFields(this Type type) {
            return _delegateFieldsCache.GetOrAdd(type, () => type.GetPublicFields().Where(each => each.FieldType.BaseType == typeof (MulticastDelegate)).ToArray());
        }

        internal static PropertyInfo[] GetPublicDelegateProperties(this Type type) {
            return _delegatePropertiesCache.GetOrAdd(type, () => type.GetPublicProperties().Where(each => each.PropertyType.BaseType == typeof (MulticastDelegate)).ToArray());
        }

        internal static IEnumerable<PropertyInfo> GetPublicProperties(this Type candidateType) {
            if (candidateType != null) {
                return candidateType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            }
            return Enumerable.Empty<PropertyInfo>();
        }

        internal static MethodInfo[] GetRequiredMethods(this Type type) {
            return _requiredMethodsCache.GetOrAdd(type, () => type.GetMethods().Where(each => each.CustomAttributes.Any(attr => attr.AttributeType.Name.Equals("RequiredAttribute", StringComparison.OrdinalIgnoreCase))).ToArray());
        }


#if OLD_DUCKTYPER
        public static Type[] WhereCompatibleWith<T>(this IEnumerable<Type> types) {
            return types.Where(each => typeof (T).IsTypeCompatible(each)).ToArray();
        }

        public static bool IsTypeCompatible(this Type duckType, Type candidateType) {
            if (!typeof(DuckTypedClass).IsAssignableFrom(duckType)) {
                throw new Exception("Type {0} is not assignable from DuckTypeClass".format(duckType.Name));
            }

            lock (_compatibilityMatrix) {
                return _compatibilityMatrix.GetOrAdd(new Tuple<Type, Type>(duckType, candidateType), () => {
                    if (duckType != null && candidateType != null) {

                        // types can only be compatible if they have a parameterless constructor
                        if (candidateType.GetConstructor(Type.EmptyTypes) == null) {
                            return false;
                        }

                        var publicMethods = candidateType.GetPublicMethods().ToArray();

                        foreach (var member in duckType.GetRequiredMembers()) {
                            var expectedDelegateType = member.FieldType;

                            // check the 'type' of each member to see if the candidate type has a
                            // member with that same name.
                            // (the 'type' of the member will be a delegate)
                            if (!publicMethods.Any(each => IsNameACloseEnoughMatch(each.Name, expectedDelegateType.Name) && expectedDelegateType.IsDelegateAssignableFromMethod(each))) {
                                //Console.WriteLine( "Type '{0}' is not a type match for '{1}' because of missing member '{2}'",  candidateType.Name, duckType.Name, member );
#if DETAILED_DEBUG
                                Event<Verbose>.Raise("Not DUCKY", "Type '{0}' is not a type match for '{1}' because of missing member '{2}'", new object[] {candidateType.Name, duckType.Name, member});
#endif
                                return false;
                            }
                            // so far, so good...!
                        }
                        return true;
                    }
                    return false;
                });
            }
        }

        internal static bool IsNameACloseEnoughMatch(string nameGiven, string expectedName) {
            if (expectedName.Equals(nameGiven, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
            if (nameGiven.IndexOf("_") > 0) {
                // if the name starts with an underscore (index would be 0), we don't treat it as a possible match.
                if (expectedName.Equals(nameGiven.Replace("_", ""), StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }

            if (expectedName.TrimEnd('s').Equals(nameGiven.TrimEnd('s'), StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
            return false;
        }

        internal static bool IsObjectCompatible(this Type duckType, object candidateObject) {
            if (candidateObject != null) {
                var candidateType = candidateObject.GetType();

                if (duckType != null && candidateType != null) {
                    var publicMethods = candidateType.GetPublicMethods().ToArray();
                    var publicFields = candidateType.GetPublicFields().Where(each => each.FieldType.BaseType == typeof (MulticastDelegate)).ToArray();

                    foreach (var member in duckType.GetRequiredMembers()) {
                        var expectedDelegateType = member.FieldType;

                        // check the 'type' of each member to see if the candidate type has a
                        // member with that same name.
                        // (the 'type' of the member will be a delegate)
                        if (!publicMethods.Any(each => IsNameACloseEnoughMatch( each.Name, expectedDelegateType.Name ) && expectedDelegateType.IsDelegateAssignableFromMethod(each))) {
                            // or if a compatible delegate is present.
                            if (!publicFields.Any(each => IsNameACloseEnoughMatch(each.Name, expectedDelegateType.Name) && expectedDelegateType.IsDelegateAssignableFromDelegate(each.FieldType))) {
                                //Console.WriteLine("Type '{0}' is not a type match for '{1}' because of missing member '{2}'", candidateType.Name, duckType.Name, member);
#if DETAILED_DEBUG
                                
                                Event<Verbose>.Raise("Not DUCKY", "Type '{0}' is not a type match for '{1}' because of missing member '{2}'", new object[] {candidateType.Name, duckType.Name, member});
#endif

                                return false;
                            }
                        }
                        // so far, so good...!
                    }
                    return true;
                }
                return false;
            }

            return false;
        }

        internal static bool IsSupported(this Delegate d) {
            if (d == null) {
                return false;
            }

            DuckTypedClass.InstanceSupportsMethod(d.Target, d.GetType().Name);
            return true;
        }
#endif
    }
}