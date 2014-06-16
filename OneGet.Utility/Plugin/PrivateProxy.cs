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

namespace Microsoft.OneGet.Plugin {
    using System;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;
    using Extensions;

    internal class PrivateProxy : DynamicObject {
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public;
        private readonly object _actual;

        public PrivateProxy(object o) {
            _actual = o;
        }

        public static dynamic FromType(Assembly assembly, string type, params object[] args) {
            var targetType = assembly.GetTypes().FirstOrDefault(item => item.Name == type);
            if (targetType == null) {
                throw new Exception("Unknown type {0} in Assembly {1}".format(type, assembly.Location));
            }

            var constructor = targetType.GetConstructor(BindingFlags, null, args.Select(a => a.GetType()).ToArray(), null);
            if (constructor != null) {
                return new PrivateProxy(constructor.Invoke(args));
            }

            return null;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            var parameterTypes = args.Select(each => each == null ? typeof (object) : each.GetType());
            var method = _actual.GetType().GetMethod(binder.Name, BindingFlags, null, parameterTypes.ToArray(), null) ?? _actual.GetType().GetMethod(binder.Name, BindingFlags);

            if (method == null) {
                return base.TryInvokeMember(binder, args, out result);
            }

            result = method.Invoke(_actual, args);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            var propertyInfo = _actual.GetType().GetProperty(binder.Name, BindingFlags);
            if (propertyInfo != null) {
                result = propertyInfo.GetValue(_actual, null);
                return true;
            }

            var fieldInfo = _actual.GetType().GetField(binder.Name, BindingFlags);
            if (fieldInfo != null) {
                result = fieldInfo.GetValue(_actual);
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            var propertyInfo = _actual.GetType().GetProperty(binder.Name, BindingFlags);
            if (propertyInfo != null) {
                propertyInfo.SetValue(_actual, value, null);
                return true;
            }

            var fieldInfo = _actual.GetType().GetField(binder.Name, BindingFlags);
            if (fieldInfo != null) {
                fieldInfo.SetValue(_actual, value);
                return true;
            }
            return base.TrySetMember(binder, value);
        }
    }
}