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
    using System.Linq;
    using System.Reflection;
    using Extensions;

    internal static class DynamicInterfaceExtensions {
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
    }
}