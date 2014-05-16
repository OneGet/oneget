using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.PowerShell.OneGet.Core {
    using System.Collections;
    using Microsoft.OneGet;
    using Microsoft.OneGet.Core.Api;
    using Microsoft.OneGet.Core.Extensions;
    using Microsoft.OneGet.Core.Tasks;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    public class OneGetCmdlet : AsyncCmdlet {
        public const string PackageNoun = "Package";
        public const string PackageSourceNoun = "PackageSource";
        public const string PackageProviderNoun = "PackageProvider";
        private static readonly object _lockObject = new object();
        private HostMessageDispatcher _hostMessageDispatcher;



        protected override Callback Invoke {
            get {
                return (_hostMessageDispatcher ?? (_hostMessageDispatcher = new HostMessageDispatcher(this, base.Invoke))).Invoke;
            }
        }

        protected override void Init() {
            if (IsCancelled()) {
                return;
            }
            if (!IsInitialized) {
                lock (_lockObject) {
                    if (!IsInitialized) {
                        try {
                            var privateData = MyInvocation.MyCommand.Module.PrivateData as Hashtable;

                            var assemblyProviders = privateData.GetStringCollection("Providers/Assembly");

                            if (assemblyProviders.IsNullOrEmpty()) {
                                Event<Error>.Raise( "PrivateData is null");
                                return;
                            }
                            IsInitialized = PackageManagementService.Initialize(Invoke, assemblyProviders, !IsInvocation);
                        }
                        catch (Exception e) {
                            e.Dump();
                        }
                    }
                }
            }

        }

        public virtual IEnumerable<string> GetPackageSources() {
            return Enumerable.Empty<string>();
        }

        public virtual bool ShouldProcessPackageInstall(string packageName, string version, string source) {
            Event<Message>.Raise("ShouldProcessPackageInstall", new string[] {packageName});
            return false;
        }

        public virtual bool ShouldProcessPackageUninstall(string packageName, string version) {
            Event<Message>.Raise("ShouldProcessPackageUnInstall", new string[] {packageName});
            return false;
        }

        public virtual bool ShouldContinueAfterPackageInstallFailure(string packageName, string version, string source) {
            Event<Message>.Raise("ShouldContinueAfterPackageInstallFailure",new string[] { packageName});
            return false;
        }

        public virtual bool ShouldContinueAfterPackageUninstallFailure(string packageName, string version, string source) {
            Event<Message>.Raise("ShouldContinueAfterPackageUnInstallFailure", new string[] {packageName});
            return false;
        }

        public virtual bool ShouldContinueRunningInstallScript(string packageName, string version, string source, string scriptLocation) {
            Event<Message>.Raise("ShouldContinueRunningInstallScript",new string[] { packageName});
            return false;
        }

        public virtual bool ShouldContinueRunningUninstallScript(string packageName, string version, string source, string scriptLocation) {
            Event<Message>.Raise("ShouldContinueRunningUninstallScript", new string[] {packageName});
            return true;
        }

        public virtual bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource) {
            Event<Message>.Raise("ShouldContinueWithUntrustedPackageSource", new string[] {packageSource});
            return true;
        }

        private class HostMessageDispatcher : MarshalByRefObject {
            private readonly OneGetCmdlet _cmdlet;
            private readonly Callback _base;

            private InvokableDispatcher _dispatcher;

            public HostMessageDispatcher(OneGetCmdlet cmdlet, Callback baseCallback) {
                _cmdlet = cmdlet;
                _base = baseCallback;
            }

            internal Callback Invoke {
                get {
                    return (_dispatcher ?? (_dispatcher = new InvokableDispatcher(_base) {
                        /*
                         * 
                         // These are handled by the base AsyncCmdlet
                        (IsCancelled)(() => _cmdlet.IsCancelled()),
                        (Warning)((s, s1, ie) => Event<Warning>.Raise(s, s1, ie)),
                        (Message)((s, s1, ie) => Event<Message>.Raise(s, s1, ie)),
                        (Error)((s, s1, ie) => Event<Error>.Raise(s, s1, ie)),
                        (Debug)((s, s1, ie) => Event<Debug>.Raise(s, s1, ie)),
                        (Verbose)((s, s1, ie) => Event<Verbose>.Raise(s, s1, ie)),
                        (ExceptionThrown)((s, s1, ie) => Event<ExceptionThrown>.Raise(s, s1, ie)),
                        (Progress)((s, s1, ie, p4, p5) => Event<Progress>.Raise(s, s1, ie, p4, p5)),
                        (CompleteProgress)((s, s1, s2, s3) => Event<CompleteProgress>.Raise(s, s1, s2, s3)),
                        
                         */

                        (AskPermission)((p1) => _cmdlet.ShouldContinue(p1, "RequiresInformation").Result),
                        (WhatIf)(() => _cmdlet.WhatIf),
                        (ShouldProcessPackageInstall)((p1, p2, p3) => _cmdlet.ShouldProcessPackageInstall(p1, p2, p3)),
                        (ShouldProcessPackageUninstall)((p1, p2) => _cmdlet.ShouldProcessPackageUninstall(p1, p2)),
                        (ShouldContinueAfterPackageInstallFailure)((p1, p2, p3) => _cmdlet.ShouldContinueAfterPackageInstallFailure(p1, p2, p3)),
                        (ShouldContinueAfterPackageUninstallFailure)((p1, p2, p3) => _cmdlet.ShouldContinueAfterPackageUninstallFailure(p1, p2, p3)),
                        (ShouldContinueRunningInstallScript)((p1, p2, p3, p4) => _cmdlet.ShouldContinueRunningInstallScript(p1, p2, p3, p4)),
                        (ShouldContinueRunningUninstallScript)((p1, p2, p3, p4) => _cmdlet.ShouldContinueRunningUninstallScript(p1, p2, p3, p4)),
                        (ShouldContinueWithUntrustedPackageSource)((p1, p2) => _cmdlet.ShouldContinueWithUntrustedPackageSource(p1, p2)),
                        (GetOptionKeys)((categeory) => {
                            var m = _cmdlet.GetRequestMetadata();
                            if (m.IsNullOrEmpty()) {
                                return Enumerable.Empty<string>().ByRef();
                            }
                            return m.Keys.Cast<string>().ByRef();
                        }),
                        (GetOptionValues)((category, key) => {
                            var m = _cmdlet.GetRequestMetadata();
                            if (m.IsNullOrEmpty() || !m.ContainsKey(key)) {
                                return Enumerable.Empty<string>().ByRef();
                            }
                            return m[key].ToEnumerable<string>().ByRef();
                        }),
                        (GetConfiguration)((key) => {return (_cmdlet.MyInvocation.MyCommand.Module.PrivateData as Hashtable).GetStringCollection(key).ByRef();}),
                        (PackageSources)(() => {
                            var m = _cmdlet.GetPackageSources();
                            return m.IsNullOrEmpty() ? Enumerable.Empty<string>().ByRef() : m.ByRef();
                        }),
                    }));
                }
            }
        }
    }
}
