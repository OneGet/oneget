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
    using Microsoft.OneGet;
    using Microsoft.OneGet.Implementation;
    using Microsoft.OneGet.Utility.Extensions;
    using Microsoft.OneGet.Utility.PowerShell;
    using Resources;
    using Utility;
    using Constants = OneGet.Constants;

    public delegate string GetMessageString(string message);

    public abstract class CmdletBase : AsyncCmdlet {
        private static int _globalCallCount = 1;
        private static readonly object _lockObject = new object();
        private static readonly IPackageManagementService _packageManagementService = new PackageManager().Instance;
        private readonly int _callCount;
        private readonly Hashtable _dynamicOptions;

        [Parameter]
        public SwitchParameter Force;

        [Parameter]
        public SwitchParameter ForceBootstrap;

        [Parameter(DontShow = true)]
        public GetMessageString MessageResolver;

        protected CmdletBase() {
            _callCount = _globalCallCount++;
            _dynamicOptions = new Hashtable();
        }

        protected bool IsPackageBySearch {
            get {
                return ParameterSetName == Constants.PackageBySearchSet;
            }
        }

        protected bool IsPackageByObject {
            get {
                return ParameterSetName == Constants.PackageByInputObjectSet;
            }
        }

        protected bool IsSourceByObject {
            get {
                return ParameterSetName == Constants.SourceByInputObjectSet;
            }
        }

        protected internal IPackageManagementService PackageManagementService {
            get {
                lock (_lockObject) {
                    if (!IsCancelled() && !IsInitialized) {
                        try {
                            IsInitialized = _packageManagementService.Initialize(this);
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
                return PackageManagementService.SelectProviders(null, this).Where(each => !each.Features.ContainsKey(Constants.AutomationOnlyFeature));
            }
            // you can manually ask for any provider by name, if it is for automation only.
            return names.SelectMany(each => PackageManagementService.SelectProviders(each, this)); // .Where(each => !each.Features.ContainsKey(Constants.AutomationOnlyFeature));
        }

        protected IEnumerable<PackageProvider> SelectProviders(string name) {
            // you can manually ask for any provider by name, if it is for automation only.
            var result = PackageManagementService.SelectProviders(name, this).ToArray(); //.Where(each => !each.Features.ContainsKey(Constants.AutomationOnlyFeature))
            if (result.Length == 0) {
                Warning(Errors.UnknownProvider, name);
            }
            return result;
        }

        public override string GetMessageString(string messageText) {
            messageText = DropMsgPrefix(messageText);

            string result = null;
            if (MessageResolver != null) {
                // if the consumer has specified a MessageResolver delegate, we need to call it on the main thread
                // because powershell won't let us use the default runspace from another thread.
                ExecuteOnMainThread(() => {
                    result = MessageResolver(messageText);
                    if (string.IsNullOrEmpty(result) || string.IsNullOrEmpty(result.Trim())) {
                        result = null;
                    }
                    return true;
                }).Wait();
            }

            return result ?? Messages.ResourceManager.GetString(messageText);
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
#if DEBUG
            Message(Constants.NotImplemented, packageName);
#endif
            return false;
        }

        public virtual bool ShouldProcessPackageUninstall(string packageName, string version) {
#if DEBUG
            Message(Constants.NotImplemented, packageName);
#endif
            return false;
        }

        public virtual bool ShouldContinueAfterPackageInstallFailure(string packageName, string version, string source) {
#if DEBUG
            Message(Constants.NotImplemented, packageName);
#endif
            return false;
        }

        public virtual bool ShouldContinueAfterPackageUninstallFailure(string packageName, string version, string source) {
#if DEBUG
            Message(Constants.NotImplemented, packageName);
#endif
            return false;
        }

        public virtual bool ShouldContinueRunningInstallScript(string packageName, string version, string source, string scriptLocation) {
#if DEBUG
            Message(Constants.NotImplemented, packageName);
#endif
            return false;
        }

        public virtual bool ShouldContinueRunningUninstallScript(string packageName, string version, string source, string scriptLocation) {
#if DEBUG
            Message(Constants.NotImplemented, packageName);
#endif
            return true;
        }

        public virtual bool AskPermission(string permission) {
#if DEBUG
            Message(Constants.NotImplemented, permission);
#endif
            return true;
        }

        public virtual IEnumerable<string> GetOptionKeys() {
            return DynamicParameterDictionary.Values.OfType<CustomRuntimeDefinedParameter>().Where(each => each.IsSet).Select(each => each.Name).Concat(MyInvocation.BoundParameters.Keys).ByRef();
        }

        public virtual IEnumerable<string> GetOptionValues(string key) {
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
            return DynamicParameterDictionary.Values.OfType<CustomRuntimeDefinedParameter>().Where(each => each.IsSet && each.Name == key).SelectMany(each => each.GetValues(this)).ByRef();
        }

        public virtual string GetCredentialUsername() {
            return null;
        }

        public virtual string GetCredentialPassword() {
            return null;
        }

        public virtual bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource) {
#if DEBUG
            Message(Constants.NotImplemented, packageSource);
#endif
            return true;
        }

        public virtual bool ShouldBootstrapProvider(string requestor, string providerName, string providerVersion, string providerType, string location, string destination) {
            try {
                if (Force || ForceBootstrap) {
                    return true;
                }

                return ShouldContinue(FormatMessageString(Constants.QueryBootstrap, providerName),
                    FormatMessageString(Constants.BootstrapProvider,
                        requestor.Is() ?
                            FormatMessageString(Constants.BootstrapProviderProviderRequested, requestor, providerName, providerVersion) :
                            FormatMessageString(Constants.BootstrapProviderUserRequested, providerName, providerVersion),
                        providerType.Is() && providerType.Equals(Constants.AssemblyProviderType) ?
                            FormatMessageString(Constants.BootstrapManualAssembly, providerName, location, destination) :
                            FormatMessageString(Constants.BootstrapManualInstall, providerName, location))).Result;
            } catch (Exception e) {
                e.Dump();
            }
            return false;
        }

        public virtual bool IsInteractive() {
            return IsInvocation;
        }

        public virtual int CallCount() {
            return _callCount;
        }
    }
}