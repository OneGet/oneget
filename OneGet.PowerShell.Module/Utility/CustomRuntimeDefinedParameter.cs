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

namespace Microsoft.PowerShell.OneGet.Utility {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Security;
    using Microsoft.OneGet;
    using Microsoft.OneGet.Extensions;
    using Microsoft.OneGet.Packaging;
    using Microsoft.OneGet.Providers.Package;
    using System.IO;

    internal class CustomRuntimeDefinedParameter : RuntimeDefinedParameter {
        internal HashSet<DynamicOption> Options = new HashSet<DynamicOption>();

        internal bool IsRequiredForProvider(string name) {
            return Options.Any(each => each.ProviderName.EqualsIgnoreCase(name) && each.IsRequired);
        }

        public CustomRuntimeDefinedParameter(DynamicOption option) : base(option.Name, ParameterType(option.Type), new Collection<Attribute> {
            new ParameterAttribute()
        }) {
            Options.Add(option);
            var values = option.PossibleValues.ToArray();
            if (!values.IsNullOrEmpty()) {
                Attributes.Add(new ValidateSetAttribute(values));
            }
        }

        internal IEnumerable<string> GetValues(AsyncCmdlet cmdlet) {
            if (IsSet && Value != null) {
                switch (Options.FirstOrDefault().Type) {
                    case OptionType.Switch:
                        return new string[] {
                            ((SwitchParameter)Value).IsPresent.ToString()
                        };
                    case OptionType.StringArray:
                        return (string[])Value;

                    case OptionType.File:
                        return new[] {cmdlet.ResolveExistingFilePath(Value.ToString()) };
                        
                    case OptionType.Folder:
                        return new[] { cmdlet.ResolveExistingFolderPath(Value.ToString()) };

                    case OptionType.Path:
                        return new[] { cmdlet.ResolvePath(Value.ToString()) };
                    
                    case OptionType.SecureString:
                        return new[] { "SECURESTRING:"+((SecureString)Value).ToProtectedString("salt") };
                }
                return new[] {
                    Value.ToString()
                };
            }
            return new string[0];
        }

        private static Type ParameterType(OptionType optionType) {
            switch (optionType) {
                case OptionType.Switch:
                    return typeof (SwitchParameter);
                case OptionType.Uri:
                    return typeof (Uri);
                case OptionType.StringArray:
                    return typeof (string[]);
                case OptionType.Int:
                    return typeof (int);
                case OptionType.Path:
                    return typeof(string);
                case OptionType.File:
                    return typeof(string);
                case OptionType.Folder:
                    return typeof(string);
                case OptionType.SecureString:
                    return typeof (SecureString);

                default:
                    return typeof (string);
            }
        }
    }
}