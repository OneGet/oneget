namespace Microsoft.PowerShell.OneGet.CmdLets {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Core;
    using Microsoft.OneGet;
    using Microsoft.OneGet.Core.Api;
    using Microsoft.OneGet.Core.Extensions;
    using Microsoft.OneGet.Core.Providers.Package;

    public class PackagingCmdlet : OneGetCmdlet {
        private Hashtable _metadata;
        protected internal Lazy<IEnumerable<PackageProvider>> _providers;

        [Parameter(Position = 0, ValueFromPipeline = true, ParameterSetName = "PackageBySearch")]
        public string[] Name { get; set; }



        [Parameter(ParameterSetName = "PackageBySearch")]
        public string Provider { get; set; }

        [Parameter(ParameterSetName = "PackageBySearch")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "go away")]
        public Hashtable Metadata {
            get {
                if (_metadata == null) {
                    _metadata = new Hashtable();
                }

                return _metadata;
            }
            set {
                // ensure that storage is created
                if (_metadata == null) {
                    _metadata = new Hashtable();
                }

                // add values from passed-in collection (so that if we have other values we don't loose them)

                if (value != null) {
                    foreach (var k in value.Keys) {
                        _metadata[k] = value[k];
                    }
                }
            }
        }

        internal PackagingCmdlet() {
            // populate the matching providers at first request.
            _providers = new Lazy<IEnumerable<PackageProvider>>(() => PackageManagementService.SelectProviders(Provider, null));
        }

        public override Hashtable GetRequestMetadata() {
            return Metadata;
        }

        public override bool EndProcessingAsync() {
            // clean up _providers?
            _providers = null;
            return true;
        }


        public override bool ConsumeDynamicParameters() {
            // pull data from dynamic parameters and place them into the Metadata collection.
            // Todo: WARNING: all the dynamic parameters will end up in metadata and installation options right now.

            foreach (var rdp in DynamicParameters.Keys.Select(d => DynamicParameters[d]).Where(rdp => rdp.IsSet)) {
                if (rdp.ParameterType == typeof(SwitchParameter)) {
                    Metadata[rdp.Name] = true;
                }
                else {
                    Metadata[rdp.Name] = rdp.Value;
                }
            }
            return true;
        }
    }
}