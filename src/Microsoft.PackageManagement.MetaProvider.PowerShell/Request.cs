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

using Microsoft.PackageManagement.Internal.Utility.Plugin;

namespace Microsoft.PackageManagement.MetaProvider.PowerShell.Internal
{
    using Microsoft.PackageManagement.Internal.Api;
    using Microsoft.PackageManagement.Internal.Implementation;
    using Microsoft.PackageManagement.Internal.Utility.Async;
    using Microsoft.PackageManagement.Internal.Utility.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.Security;
    using Messages = Microsoft.PackageManagement.MetaProvider.PowerShell.Internal.Resources.Messages;

    public abstract class PsRequest : Request
    {
        internal CommandInfo CommandInfo;
        private PowerShellProviderBase _provider;

        internal bool IsMethodImplemented => CommandInfo != null;

        public IEnumerable<string> PackageSources
        {
            get
            {
                IEnumerable<string> ps = Sources;
                if (ps == null)
                {
                    return new string[] {
                    };
                }
                return ps.ToArray();
            }
        }

        internal IRequest Extend(params object[] objects)
        {
            return objects.ConcatSingleItem(this).As<IRequest>();
        }

        internal string GetMessageStringInternal(string messageText)
        {
            return Messages.ResourceManager.GetString(messageText);
        }

        public PSCredential Credential
        {
            get
            {
                if (CredentialUsername != null && CredentialPassword != null)
                {
                    return new PSCredential(CredentialUsername, CredentialPassword);
                }
                return null;
            }
        }

        private Hashtable _options;

        public Hashtable Options
        {
            get
            {
                if (_options == null)
                {
                    _options = new Hashtable();
                    //quick and dirty, grab all four sets and merge them.
                    IEnumerable<string> keys = OptionKeys ?? new string[0];
                    foreach (string k in keys)
                    {
                        if (_options.ContainsKey(k))
                        {
                            continue;
                        }
                        string[] values = GetOptionValues(k).ToArray();
                        if (values.Length == 1)
                        {
                            if (values[0].IsTrue())
                            {
                                _options.Add(k, true);
                            }
                            else if (values[0].IndexOf("SECURESTRING:", StringComparison.OrdinalIgnoreCase) == 0)
                            {
#if !CORECLR
                                _options.Add(k, values[0].Substring(13).FromProtectedString("salt"));
#endif
                            }
                            else
                            {
                                _options.Add(k, values[0]);
                            }
                        }
                        else
                        {
                            _options.Add(k, values);
                        }
                    }
                }
                return _options;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "This is required for the PowerShell Providers.")]
        public object CloneRequest(Hashtable options = null, ArrayList sources = null, PSCredential credential = null)
        {
            string[] srcs = (sources ?? new ArrayList()).ToArray().Select(each => each.ToString()).ToArray();

            options = options ?? new Hashtable();

            Dictionary<string, string[]> lst = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            foreach (object k in options.Keys)
            {
                if (k != null)
                {
                    object obj = options[k];

                    string[] val = null;

                    if (obj is string)
                    {
                        val = new[] { obj as string };
                    }
                    else
                    {
                        // otherwise, try to cast it to a collection of string-like-things
                        if (obj is IEnumerable collection)
                        {
                            val = collection.Cast<object>().Select(each => each.ToString()).ToArray();
                        }
                        else
                        {
                            // meh. ToString, and goodnight.
                            val = new[] { obj.ToString() };
                        }
                    }

                    lst.Add(k.ToString(), val);
                }
            }

            return Extend(new
            {
                GetOptionKeys = new Func<IEnumerable<string>>(() => { return lst.Keys.ToArray(); }),
                GetOptionValues = new Func<string, IEnumerable<string>>((key) =>
                {
                    if (lst.ContainsKey(key))
                    {
                        return lst[key];
                    }
                    return new string[0];
                }),
                GetSources = new Func<IEnumerable<string>>(() => { return srcs; }),
                GetCredentialUsername = new Func<string>(() => { return credential?.UserName; }),
                GetCredentialPassword = new Func<SecureString>(() => { return credential?.Password; }),
                ShouldContinueWithUntrustedPackageSource = new Func<string, string, bool>((pkgName, pkgSource) =>
                {
                    // chained providers provide locations, and don't rely on 'trusted' flags from the upstream provider.
                    return true;
                })
            });
        }

        public object CallPowerShell(params object[] args)
        {
            if (IsMethodImplemented)
            {
                return _provider.CallPowerShell(this, args);
            }
            return null;
        }

        internal static PsRequest New(object requestObject, PowerShellProviderBase provider, string methodName)
        {
            if (requestObject is IAsyncAction)
            {
                ((IAsyncAction)(requestObject)).OnCancel += provider.CancelRequest;
                ((IAsyncAction)(requestObject)).OnAbort += provider.CancelRequest;
            }
            PsRequest req = requestObject.As<PsRequest>();

            req.CommandInfo = provider.GetMethod(methodName);
            if (req.CommandInfo == null)
            {
                req.Debug("METHOD_NOT_IMPLEMENTED", methodName);
            }
            req._provider = provider;

            if (req.Options == null)
            {
                req.Debug("req.Options is null");
            }
            else
            {
                req.Debug("Calling New() : MethodName = '{0}'", methodName);

                foreach (string key in req.Options.Keys)
                {
                    req.Debug(string.Format(CultureInfo.CurrentCulture, "{0}: {1}", key, req.Options[key]));
                }
            }

            return req;
        }

        public object SelectProvider(string providerName)
        {
            return PackageManagementService.SelectProviders(providerName, Extend()).FirstOrDefault(each => each.Name.EqualsIgnoreCase(providerName));
        }

        public IEnumerable<object> SelectProviders(string providerName)
        {
            return PackageManagementService.SelectProviders(providerName, Extend());
        }

        public object Services => ProviderServices;

        public IEnumerable<object> FindPackageByCanonicalId(string packageId, object requestObject)
        {
            return ProviderServices.FindPackageByCanonicalId(packageId, (requestObject ?? new object()).As<IRequest>());
        }

        public bool RequirePackageProvider(string packageProviderName, string minimumVersion)
        {
            PowerShellPackageProvider pp = (_provider as PowerShellPackageProvider);
            return PackageManagementService.RequirePackageProvider(pp == null ? Constants.ProviderNameUnknown : pp.GetPackageProviderName(), packageProviderName, minimumVersion, Extend());
        }
    }
}