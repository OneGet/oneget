﻿// 
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
namespace Microsoft.PackageManagement.MetaProvider.PowerShell {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Threading;

    internal static class PowerShellExtensions {
        internal static PowerShell Clear(this PowerShell powershell) {
            if (powershell != null) {
                powershell.Commands.Clear();
            }
            return powershell;
        }

        internal static PSModuleInfo ImportModule(this PowerShell powershell, string name) {
            if (powershell != null) {
                powershell.Commands.Clear();
                return powershell.AddCommand("Import-Module")
                    .AddParameter("Name", name)
                    .AddParameter("PassThru")
                    .Invoke<PSModuleInfo>().FirstOrDefault();
            }
            return null;
        }

        internal static IEnumerable<PSModuleInfo> TestModuleManifest(this PowerShell powershell, string path) {
            if (powershell != null) {
                return powershell
                    .Clear()
                    .AddCommand("Test-ModuleManifest")
                    .AddParameter("Path", path)
                    .Invoke<PSModuleInfo>().ToArray();
            }
            return Enumerable.Empty<PSModuleInfo>();
        }

        internal static PowerShell SetVariable(this PowerShell powershell, string variable, object value) {
            if (powershell != null) {
                powershell
                    .Clear()
                    .AddCommand("Set-Variable")
                    .AddParameter("Name", variable)
                    .AddParameter("Value", value)
                    .Invoke();
            }
            return powershell;
        }

        internal static IEnumerable<T> InvokeFunction<T>(this PowerShell powershell, string command, params object[] args) {
            if (powershell != null) {
                powershell.Clear().AddCommand(command);
                foreach (var arg in args) {
                    powershell.AddArgument(arg);
                }

                IAsyncResult async = powershell.BeginInvoke();
                var result = powershell.EndInvoke(async);
                if (result != null) {
                    return result.Select(each => each.ImmediateBaseObject).Cast<T>();
                }
            }
            return Enumerable.Empty<T>();
        }

        internal static PowerShell WaitForReady(this PowerShell powershell) {
            if (powershell != null) {
                switch (powershell.InvocationStateInfo.State) {
                    case PSInvocationState.Completed:
                        break;
                    case PSInvocationState.Failed:
                        break;
                    case PSInvocationState.Stopped:
                        break;
                    case PSInvocationState.Stopping:
                        while (powershell.InvocationStateInfo.State == PSInvocationState.Stopping) {
                            Thread.Sleep(10);
                        }
                        break;
                    case PSInvocationState.Running:
                        powershell.Stop();
                        while (powershell.InvocationStateInfo.State == PSInvocationState.Stopping) {
                            Thread.Sleep(10);
                        }
                        break;

                    case PSInvocationState.NotStarted:
                        break;
                    case PSInvocationState.Disconnected:
                        break;
                }
            }
            return powershell;
        }
    }
}