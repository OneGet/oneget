using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PackageManagement;
using Microsoft.PackageManagement.Implementation;
using Microsoft.PackageManagement.Internal;
using Microsoft.PackageManagement.Internal.Api;
using Microsoft.PackageManagement.MetaProvider.PowerShell;
using Microsoft.PackageManagement.Providers.Internal;

namespace ConsoleApp1
{
    class Program
    {

        public interface IPackageManagementService
        {
            int Version { get; }

            IEnumerable<string> ProviderNames { get; }

            IEnumerable<string> AllProviderNames { get; }

            IEnumerable<PackageProvider> PackageProviders { get; }

            IEnumerable<PackageProvider> GetAvailableProviders(IHostApi requestObject, string[] names);

            IEnumerable<PackageProvider> ImportPackageProvider(IHostApi requestObject, string providerName, Version requiredVersion,
                Version minimumVersion, Version maximumVersion, bool isRooted, bool force);

            bool Initialize(IHostApi requestObject);

            IEnumerable<PackageProvider> SelectProvidersWithFeature(string featureName);

            IEnumerable<PackageProvider> SelectProvidersWithFeature(string featureName, string value);

            IEnumerable<PackageProvider> SelectProviders(string providerName, IHostApi requestObject);

            IEnumerable<SoftwareIdentity> FindPackageByCanonicalId(string packageId, IHostApi requestObject);

            bool RequirePackageProvider(string requestor, string packageProviderName, string minimumVersion, IHostApi requestObject);
        }



        static void Main(string[] args)
        {
            


        }
        //ProgramsProvider provider = new ProgramsProvider();
    }    //IPackageManagementService packageManagement = new IPackageManagementService();
  }

