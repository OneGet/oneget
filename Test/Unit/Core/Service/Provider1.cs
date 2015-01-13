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
namespace Microsoft.OneGet.Test.Core.Service {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api;
    using Implementation;
    using OneGet.Utility.Async;
    using OneGet.Utility.Extensions;
    using OneGet.Utility.Plugin;
    using Packaging;
    using TestProviders;
    using Xunit;
    using Xunit.Abstractions;
    using Console = Support.Console;

    public class Happy1ProviderTests : BasePMSServiceTests {
        public Happy1ProviderTests(ITestOutputHelper outputHelper) : base(outputHelper) {
        }

        private PackageProvider _provider;
        public PackageProvider Provider {
            get {
                if (_provider == null) {
                    var packageProviders = PackageManagementService.SelectProviders("Happy1", new BasicHostImpl()).ToArray();
                    Assert.NotNull(packageProviders);
                    Assert.Equal(1, packageProviders.Length);
                    _provider = packageProviders[0];
                }
                Assert.NotNull(_provider);
                return _provider;
            }
        }
        
        [Fact]
        public void TestInitialized() {
            using (CaptureConsole) {
                // get the provider
                Assert.NotNull(Provider);

                // check to see if the happy instance is initialized.
                Assert.True(Happy1.Initialized);
            }
        }

        [Fact]
        public void TestFeatures() {
            using (CaptureConsole) {
                // use async call.
                var x = Provider.GetFeatures(new BasicHostImpl());
                x.Wait();
                Assert.NotNull(x.Value);

                // or use the cached version
                Assert.NotNull(Provider.Features);

                // our test features should tell us they are for testing
                Assert.True( Provider.Features.ContainsKey(Sdk.Constants.Features.Test));
            }
        }

        [Fact]
        public void TestDynamicOptions() {
            using (CaptureConsole) {
                var opt_install = Provider.GetDynamicOptions(OptionCategory.Install, new BasicHostImpl());
                opt_install.Wait();
                var opts = opt_install.ToArray();

                Assert.Equal(9,opts.Length );
                foreach (var o in opts) {
                    // common 
                    Assert.Equal(OptionCategory.Install, o.Category);
                    Assert.False(o.IsRequired);
                    Assert.Empty(o.PossibleValues);
                }

                // verify names;
                for (int i = 0; i < opts.Length; i++) {
                    Assert.Equal("install_{0}".format(i+1), opts[i].Name);
                }
                
                // verify that the types are set correctly
                Assert.Equal(new [] { 
                    OptionType.File,
                    OptionType.Folder, 
                    OptionType.Int, 
                    OptionType.Path,
                    OptionType.SecureString,
                    OptionType.String, 
                    OptionType.StringArray,
                    OptionType.Switch, 
                    OptionType.Uri 
                }, opts.Select( each => each.Type ));

                var opt_provider = Provider.GetDynamicOptions(OptionCategory.Provider, new BasicHostImpl());
                opts = opt_provider.ToArray();
                Assert.Equal(1, opts.Length );
                Assert.Equal("provider_1" ,opts[0].Name);
                Assert.Equal(new[] { "one", "two", "three"} ,opts[0].PossibleValues);

                
                var opt_source = Provider.GetDynamicOptions(OptionCategory.Source, new BasicHostImpl());
                opts = opt_source.ToArray();
                Assert.Equal(1, opts.Length);
                Assert.Equal(new[] { "one", "two", "three" }, opts[0].PossibleValues);
                Assert.True(opts[0].IsRequired);
                Assert.Equal("source_1", opts[0].Name);

                var opt_package = Provider.GetDynamicOptions(OptionCategory.Package, new BasicHostImpl());
                Assert.Empty(opt_package); 
            }
        }

        [Fact]
        public void ResolvePackageSourceTest() {
            using (CaptureConsole) {
                Console.WriteLine("hi");

                Assert.NotNull(Provider);

                // ask without any parameters
                var sources = Provider.ResolvePackageSources(new BasicHostImpl()).ToArray();
                Assert.NotEmpty(sources);
                foreach (var source in sources) {
                    Console.WriteLine("Source {0} => {1}", source.Name, source.Location);
                }

                sources = Provider.ResolvePackageSources(new object[] {
                    // override just GetSources()
                    new { GetSources = new Func<IEnumerable<string>> (() => "source2".SingleItemAsEnumerable()) },
                    // everything else just pull from the basic impl
                    new BasicHostImpl()
                }.As<IHostApi>()).ToArray();

                Assert.Equal( 1 , sources.Length );
                Assert.Equal( "location2",sources[0].Location);
            }
        }

        [Fact]
        public void AddPackageSourceTest() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void RemovePackageSourceTest() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void FindPackageTest() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void FindPackageByUriTest() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void FindPackageByFileTest() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void GetInstalledPackagesTest() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void GetPackageDependenciesTest() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void InstallPackageTest() {
            using (CaptureConsole) {

            }
        }


        [Fact]
        public void UninstallPackageTest() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void IsSupportedFile() {
            using (CaptureConsole) {
               
            }
        }

        [Fact]
        public void IsSupportedSchemeTest() {
            using (CaptureConsole) {

            }
        }

        [Fact]
        public void FirstTest() {
            using (CaptureConsole) {
                
            }
            /*
            
            
             * 
            PackageProvider a;
            a.AddPackageSource();
            a.RemovePackageSource();

            a.StartFind();
            a.CompleteFind();
             
            a.DownloadPackage();
             
            a.ExecuteElevatedAction();
            
            
            a.FindPackage();
            a.FindPackageByFile();
            a.FindPackageByUri();
             
            a.FindPackages();
            a.FindPackagesByFiles();
            a.FindPackagesByUris();
             
            a.GetInstalledPackages();
             
            a.GetPackageDependencies();
            
            a.InstallPackage();
            a.UninstallPackage();
             
            a.IsSupportedFile();
             
            a.IsSupportedFile();
             
            a.IsSupportedFileName();
             
            a.IsSupportedScheme();
            
            
            
            
            
            a.Features;
            a.DynamicOptions;
            a.Initialize();
            a.ResolvePackageSources();
            a.Name;
            a.ProviderName;

            */
        }
    }
}