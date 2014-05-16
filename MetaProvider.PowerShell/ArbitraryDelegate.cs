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

namespace Microsoft.OneGet.MetaProvider.PowerShell {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Core.Extensions;

    internal static class ArbitraryDelegate {
        public static Type GetFuncOrActionType(IEnumerable<Type> argTypes, Type returnType) {
            return returnType == typeof (void) ? Expression.GetActionType(argTypes.ToArray()) : Expression.GetFuncType(argTypes.ConcatSingleItem(returnType).ToArray());
        }

        public static Type GetArbitraryActionType(IEnumerable<Type> argTypes) {
            var types = argTypes.ToArray();

            switch (types.Length) {
                case 0:
                    return typeof (ArbitraryAction);
                case 1:
                    return typeof (ArbitraryAction<>).MakeGenericType(types);
                case 2:
                    return typeof (ArbitraryAction<,>).MakeGenericType(types);
                case 3:
                    return typeof (ArbitraryAction<,,>).MakeGenericType(types);
                case 4:
                    return typeof (ArbitraryAction<,,,>).MakeGenericType(types);
                case 5:
                    return typeof (ArbitraryAction<,,,,>).MakeGenericType(types);
                case 6:
                    return typeof (ArbitraryAction<,,,,,>).MakeGenericType(types);
                case 7:
                    return typeof (ArbitraryAction<,,,,,,>).MakeGenericType(types);
                case 8:
                    return typeof (ArbitraryAction<,,,,,,,>).MakeGenericType(types);
                case 9:
                    return typeof (ArbitraryAction<,,,,,,,,>).MakeGenericType(types);
                case 10:
                    return typeof (ArbitraryAction<,,,,,,,,,>).MakeGenericType(types);
                case 11:
                    return typeof (ArbitraryAction<,,,,,,,,,,>).MakeGenericType(types);
                case 12:
                    return typeof (ArbitraryAction<,,,,,,,,,,,>).MakeGenericType(types);
                case 13:
                    return typeof (ArbitraryAction<,,,,,,,,,,,,>).MakeGenericType(types);
                case 14:
                    return typeof (ArbitraryAction<,,,,,,,,,,,,,>).MakeGenericType(types);
                case 15:
                    return typeof (ArbitraryAction<,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 16:
                    return typeof (ArbitraryAction<,,,,,,,,,,,,,,,>).MakeGenericType(types);
                default:
                    return (Type)null;
            }
        }

        public static Type GetArbitraryFunctionType(IEnumerable<Type> argTypes, Type returnType) {
            var types = argTypes.ConcatSingleItem(returnType).ToArray();
            switch (types.Length) {
                case 1:
                    return typeof (ArbitraryFunc<>).MakeGenericType(types);
                case 2:
                    return typeof (ArbitraryFunc<,>).MakeGenericType(types);
                case 3:
                    return typeof (ArbitraryFunc<,,>).MakeGenericType(types);
                case 4:
                    return typeof (ArbitraryFunc<,,,>).MakeGenericType(types);
                case 5:
                    return typeof (ArbitraryFunc<,,,,>).MakeGenericType(types);
                case 6:
                    return typeof (ArbitraryFunc<,,,,,>).MakeGenericType(types);
                case 7:
                    return typeof (ArbitraryFunc<,,,,,,>).MakeGenericType(types);
                case 8:
                    return typeof (ArbitraryFunc<,,,,,,,>).MakeGenericType(types);
                case 9:
                    return typeof (ArbitraryFunc<,,,,,,,,>).MakeGenericType(types);
                case 10:
                    return typeof (ArbitraryFunc<,,,,,,,,,>).MakeGenericType(types);
                case 11:
                    return typeof (ArbitraryFunc<,,,,,,,,,,>).MakeGenericType(types);
                case 12:
                    return typeof (ArbitraryFunc<,,,,,,,,,,,>).MakeGenericType(types);
                case 13:
                    return typeof (ArbitraryFunc<,,,,,,,,,,,,>).MakeGenericType(types);
                case 14:
                    return typeof (ArbitraryFunc<,,,,,,,,,,,,,>).MakeGenericType(types);
                case 15:
                    return typeof (ArbitraryFunc<,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 16:
                    return typeof (ArbitraryFunc<,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 17:
                    return typeof (ArbitraryFunc<,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                default:
                    return (Type)null;
            }
        }
    }
}