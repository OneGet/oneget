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

namespace Microsoft.PowerShell.OneGet.Core {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.OneGet.Core.Extensions;
    using Microsoft.OneGet.Core.Packaging;
    using Microsoft.OneGet.Core.Providers.Package;

    internal class CustomRuntimeDefinedParameter : RuntimeDefinedParameter {

        internal HashSet<DynamicOption> Options = new HashSet<DynamicOption>();
        
        public CustomRuntimeDefinedParameter(DynamicOption option) : base( option.Name, ParameterType(option.Type), new Collection<Attribute>{ new ParameterAttribute() } ) {
            Options.Add(option);
            var values = option.PossibleValues.ToArray();
            if (!values.IsNullOrEmpty()) {
                Attributes.Add(new ValidateSetAttribute(values));
            }
        }

        private static Type ParameterType(OptionType optionType ) {
            switch (optionType) {
                case OptionType.Switch:
                    return typeof(SwitchParameter);
                case OptionType.Uri:
                    return  typeof(Uri);
                case OptionType.StringArray:
                    return typeof(string[]);
                case OptionType.Int:
                    return typeof(int);
                case OptionType.Path:
                    return  typeof(string);
                default:
                    return typeof(string);
            }
        }

        internal IEnumerable<string> Values {
            get {
                if (IsSet && Value != null ) {
                    switch (Options.FirstOrDefault().Type) {
                        case OptionType.Switch:
                            return new string [] {"true" };
                        case OptionType.StringArray:
                            return (string[]) Value;
                    }
                    return new [] { Value.ToString() };
                }
                return new string[0];
            }
        }
    }

    
    
}