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

namespace Microsoft.OneGet.Core.Extensions {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public static class TypeExtensions {
        private static readonly IDictionary<TwoTypes, Func<object, object>> _typeCoercers = new Dictionary<TwoTypes, Func<object, object>>();
        private static readonly ParameterExpression _parameter = Expression.Parameter(typeof (object), "value");

        /// <summary>
        ///     Attempts to turn one type into another using a convert expression.
        ///     Falls back to trying to do it via a string if it didn't work.
        ///     todo: This is kindof a hack, we should rethink this.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object CoerceCast(this Type type, object obj) {
            try {
                return GetConverter(type, obj == null ? typeof (object) : obj.GetType())(obj);
            } catch {
                return GetConverter(type, typeof (string))((obj ?? "").ToString());
            }
        }

        // This is the method with the "guts" of the implementation
        [MethodImpl(MethodImplOptions.Synchronized)]
        private static Func<object, object> GetConverter(Type targetType, Type fromType) {
            return _typeCoercers.GetOrAdd(new TwoTypes(fromType, targetType), () => (Func<object, object>)Expression.Lambda(
                Expression.Convert(Expression.Convert(Expression.Convert(_parameter, fromType), targetType), typeof (object)), _parameter).Compile());
        }
    }

    internal class TwoTypes {
        private readonly Type _first;
        private readonly Type _second;

        public TwoTypes(Type first, Type second) {
            _first = first;
            _second = second;
        }

        public override int GetHashCode() {
            return 31 * _first.GetHashCode() + _second.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            var other = obj as TwoTypes;
            return other != null && (_first == other._first && _second == other._second);
        }
    }

    internal class Types {
        private readonly Type _first;
        private readonly Type[] _second;

        public Types(Type first, params Type[] second) {
            _first = first;
            _second = second;
        }

        public override int GetHashCode() {
            return  _second.Aggregate(_first.FullName.GetHashCode(), (current,each)=> current ^ each.GetHashCode() );
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            var other = obj as Types;
            return other != null && (_first == other._first && _second.SequenceEqual(other._second));
        }
    }
}