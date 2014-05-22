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
    using AppDomains;
    using Extensions;

    internal static class DynamicInterfaceExtensions {
        private readonly static Type[] _emptyTypes = { };

        private static readonly IDictionary<Tuple<Type, Type>, bool> _compatibilityMatrix = new Dictionary<Tuple<Type, Type>, bool>();

        private static readonly IDictionary<Type, MethodInfo[]> _methodCache = new Dictionary<Type, MethodInfo[]>();
        private static readonly IDictionary<Type[], MethodInfo[]> _methodCacheForTypes = new Dictionary<Type[], MethodInfo[]>();
        private static readonly IDictionary<Type, FieldInfo[]> _delegateFieldsCache = new Dictionary<Type, FieldInfo[]>();
        private static readonly IDictionary<Type, PropertyInfo[]> _delegatePropertiesCache = new Dictionary<Type, PropertyInfo[]>();
        private static readonly Dictionary<Type, MethodInfo[]> _requiredMethodsCache = new Dictionary<Type, MethodInfo[]>();

        public static MethodInfo FindMethod(this MethodInfo[] methods, MethodInfo methodSignature) {
            return methods.FirstOrDefault(each => DoNamesMatchAcceptably(methodSignature.Name, each.Name) && DoSignaturesMatchAcceptably(methodSignature, each));
        }

        public static Delegate FindDelegate(this FieldInfo[] fields, object actualInstance, MethodInfo signature) {
            return (from field in fields
                let value = field.GetValue(actualInstance) as Delegate
                where DoNamesMatchAcceptably(signature.Name, field.Name) && field.FieldType.IsDelegateAssignableFromMethod(signature) && value != null
                select value).FirstOrDefault();
        }

        public static Delegate FindDelegate(this PropertyInfo[] properties, object actualInstance, MethodInfo signature) {
            return (from property in properties
                let value = property.GetValue(actualInstance) as Delegate
                where DoNamesMatchAcceptably(signature.Name, property.Name) && property.PropertyType.IsDelegateAssignableFromMethod(signature) && value != null
                select value).FirstOrDefault();
        }

        private static bool DoNamesMatchAcceptably(string originalName, string candidateName) {
            if (originalName.EqualsIgnoreCase(candidateName)) {
                return true;
            }
            return false;
        }

        private static bool DoSignaturesMatchAcceptably(MethodInfo member, MethodInfo each) {
            return member.GetParameterTypes().SequenceEqual(each.GetParameterTypes()) && member.ReturnType == each.ReturnType;
        }

        internal static MethodInfo[] GetPublicMethods(this Type type) {
            return _methodCache.GetOrAdd(type, () => {
                if (type != null) {
                    return type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                }
                return new MethodInfo[0];
            });
        }

        internal static MethodInfo[] GetPublicMethods(this Type[] types) {
            return _methodCacheForTypes.GetOrAdd(types, () => types.SelectMany(each => each.GetPublicMethods()).ToArray());
        }

        internal static IEnumerable<FieldInfo> GetPublicFields(this Type type) {
            if (type != null) {
                return type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            }
            return Enumerable.Empty<FieldInfo>();
        }

        internal static FieldInfo[] GetPublicDelegateFields(this Type type) {
            return _delegateFieldsCache.GetOrAdd(type, () => type.GetPublicFields().Where(each => each.FieldType.BaseType == typeof (MulticastDelegate)).ToArray());
        }

        internal static PropertyInfo[] GetPublicDelegateProperties(this Type type) {
            return _delegatePropertiesCache.GetOrAdd(type, () => type.GetPublicProperties().Where(each => each.PropertyType.BaseType == typeof (MulticastDelegate)).ToArray());
        }

        internal static IEnumerable<PropertyInfo> GetPublicProperties(this Type type) {
            if (type != null) {
                return type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            }
            return Enumerable.Empty<PropertyInfo>();
        }

        internal static MethodInfo[] GetRequiredMethods(this Type type) {
            return _requiredMethodsCache.GetOrAdd(type, () => type.GetMethods().Where(each => each.CustomAttributes.Any(attr => attr.AttributeType.Name.Equals("RequiredAttribute", StringComparison.OrdinalIgnoreCase))).ToArray());
        }

        internal static ConstructorInfo GetDefaultConstructor(this Type t) {
            return t.GetConstructor(_emptyTypes);
        }

        internal static string ToSignatureString(this MethodInfo method) {
            return "{0} {1}({2})".format(method.ReturnType.Name, method.Name, method.GetParameters().Select( each => "{0} {1}".format( each.ParameterType.NiceName(), each.Name)).JoinWithComma());
        }

        public static string NiceName(this Type type) {
            if (!type.IsGenericType) {
                return type.Name;
            }
            var typeName = type.GetGenericTypeDefinition().Name;
            typeName = typeName.Substring(0, typeName.IndexOf('`'));
            return typeName + "<" + string.Join(",", type.GetGenericArguments().Select(NiceName).ToArray()) + ">";
        }

        public static string FullNiceName(this Type type) {
            if (!type.IsGenericType) {
                return type.FullName;
            }
            var typeName = type.GetGenericTypeDefinition().FullName;
            typeName = typeName.Substring(0, typeName.IndexOf('`'));
            return typeName + "<" + string.Join(",", type.GetGenericArguments().Select(NiceName).ToArray()) + ">";
        }


        internal static Func<string, bool> GenerateInstancesSupportsMethod(object[] actualInstance) {
            var ism = actualInstance.Select(GenerateInstanceSupportsMethod).ToArray();
            return (s) => ism.Any(each => each(s));
        }

        internal static Func<string, bool> GenerateInstanceSupportsMethod(object actualInstance) {
            // if the object implements an IsMethodImplemented Method, we'll be using that 
            // to see if the method is actually supposed to be used.
            // this enables an implementor to physically implement the function in the class
            // yet treat it as if it didn't. (see the PowerShellPackageProvider)
            var imiMethodInfo = actualInstance.GetType().GetMethod("IsMethodImplemented", new[] {
                typeof (string)
            });
            return imiMethodInfo == null ? (s) => true : actualInstance.CreateProxiedDelegate<Func<string, bool>>(imiMethodInfo);
        }
    }
}