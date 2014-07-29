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

namespace Microsoft.PowerShell.OneGet.CmdLets {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using Microsoft.OneGet;
    using Microsoft.OneGet.Extensions;
    using Microsoft.OneGet.Providers.Package;
    using Utility;
    using Constants = OneGet.Constants;

    public delegate string GetMessageString(string message);

    public abstract class CmdletBase : AsyncCmdlet {
        private static readonly object _lockObject = new object();
        private readonly Hashtable _dynamicOptions;
        private readonly IPackageManagementService _packageManagementService = new PackageManagementService().Instance;

        [Parameter(DontShow = true)]
        public GetMessageString MessageResolver;

        protected CmdletBase() {
            _dynamicOptions = new Hashtable();
        }

        protected bool IsPackageBySearch {
            get {
                return ParameterSetName == Constants.PackageBySearchSet;
            }
        }

        protected bool IsPackageByObject {
            get {
                return ParameterSetName == Constants.PackageByObjectSet;
            }
        }

        protected bool IsSourceByObject {
            get {
                return ParameterSetName == Constants.SourceByObjectSet;
            }
        }

        protected bool IsProviderByObject {
            get {
                return ParameterSetName == Constants.ProviderByObjectSet;
            }
        }

        protected bool IsOverwriteExistingSource {
            get {
                return ParameterSetName == Constants.OverwriteExistingSourceSet;
            }
        }

        protected internal IPackageManagementService PackageManagementService {
            get {
                lock (_lockObject) {
                    if (!IsCancelled() && !IsInitialized) {
                        try {
                            IsInitialized = _packageManagementService.Initialize(this, !IsInvocation);
                        } catch (Exception e) {
                            e.Dump();
                        }
                    }
                }
                return _packageManagementService;
            }
        }

        public Hashtable DynamicOptions {
            get {
                return _dynamicOptions;
            }
        }

        public virtual IEnumerable<string> Sources {
            get {
                return null;
            }
            set {
            }
        }

        protected override void Init() {
            if (!IsInitialized) {
                // get the service ( forces initialization )
                var x = PackageManagementService;
            }
        }

        protected IEnumerable<PackageProvider> SelectProviders(string[] names) {
            if (names.IsNullOrEmpty()) {
                return PackageManagementService.SelectProviders(null).Where(each => !each.Features.ContainsKey(Constants.AutomationOnlyFeature));
            }
            return names.SelectMany(each => PackageManagementService.SelectProviders(each)).Where(each => !each.Features.ContainsKey(Constants.AutomationOnlyFeature));
        }

        protected IEnumerable<PackageProvider> SelectProviders(string name) {
            var result = PackageManagementService.SelectProviders(name).Where(each => !each.Features.ContainsKey(Constants.AutomationOnlyFeature)).ToArray();
            if (result.Length == 0) {
                Warning(Messages.UnknownProvider, name);
            }
            return result;
        }

        public override string GetMessageString(string messageText) {

            if (MessageResolver != null) {
                // if the consumer has specified a MessageResolver delegate, we need to call it on the main thread
                // beacuse powershell won't let us use the default runspace from another thread.
                ExecuteOnMainThread(() => {
                    messageText = MessageResolver(messageText) ?? messageText;
                    return true;
                }).Wait();
            }
            return Resources.ResourceManager.GetString(messageText) ?? messageText;
        }

        public override bool ConsumeDynamicParameters() {
            // pull data from dynamic parameters and place them into the DynamicOptions collection.
            foreach (var rdp in DynamicParameterDictionary.Keys.Select(d => DynamicParameterDictionary[d]).Where(rdp => rdp.IsSet)) {
                if (rdp.ParameterType == typeof (SwitchParameter)) {
                    _dynamicOptions[rdp.Name] = true;
                } else {
                    _dynamicOptions[rdp.Name] = rdp.Value;
                }
            }
            return true;
        }

        public virtual bool ShouldProcessPackageInstall(string packageName, string version, string source) {
            Message(Constants.ShouldProcessPackageInstall, packageName);
            return false;
        }

        public virtual bool ShouldProcessPackageUninstall(string packageName, string version) {
            Message(Constants.ShouldProcessPackageUninstall, packageName);
            return false;
        }

        public virtual bool ShouldContinueAfterPackageInstallFailure(string packageName, string version, string source) {
            Message(Constants.ShouldContinueAfterPackageInstallFailure, packageName);
            return false;
        }

        public virtual bool ShouldContinueAfterPackageUninstallFailure(string packageName, string version, string source) {
            Message(Constants.ShouldContinueAfterPackageUnInstallFailure, packageName);
            return false;
        }

        public virtual bool ShouldContinueRunningInstallScript(string packageName, string version, string source, string scriptLocation) {
            Message(Constants.ShouldContinueRunningInstallScript, packageName);
            return false;
        }

        public virtual bool ShouldContinueRunningUninstallScript(string packageName, string version, string source, string scriptLocation) {
            Message(Constants.ShouldContinueRunningUninstallScript, packageName);
            return true;
        }

        public virtual bool AskPermission(string permission) {
            Message(Constants.AskPermission, permission);
            return true;
        }

        public IEnumerable<string> GetOptionKeys(int category) {
            return DynamicParameterDictionary.Values.OfType<CustomRuntimeDefinedParameter>().Where(each => each.IsSet && each.Options.Any(o => (int)o.Category == category)).Select(each => each.Name).Concat(MyInvocation.BoundParameters.Keys).ByRef();
        }

        public IEnumerable<string> GetOptionValues(int category, string key) {
            if (MyInvocation.BoundParameters.ContainsKey(key)) {
                var value = MyInvocation.BoundParameters[key];
                if (value is string || value is int) {
                    return new[] {
                        MyInvocation.BoundParameters[key].ToString()
                    }.ByRef();
                }
                if (value is SwitchParameter) {
                    return new[] {
                        ((SwitchParameter)MyInvocation.BoundParameters[key]).IsPresent.ToString()
                    }.ByRef();
                }
                if (value is string[]) {
                    return ((string[])value).ByRef();
                }
                return new[] {
                    MyInvocation.BoundParameters[key].ToString()
                }.ByRef();
            }
            return DynamicParameterDictionary.Values.OfType<CustomRuntimeDefinedParameter>().Where(each => each.IsSet && each.Options.Any(o => (int)o.Category == category) && each.Name == key).SelectMany(each => each.GetValues(this)).ByRef();
        }

        public virtual string GetCredentialUsername() {
            return null;
        }

        public virtual string GetCredentialPassword() {
            return null;
        }

        public virtual bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource) {
            Message(Constants.ShouldContinueWithUntrustedPackageSource, packageSource);
            return true;
        }
    }
}