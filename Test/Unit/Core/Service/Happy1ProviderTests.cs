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

namespace Microsoft.PackageManagement.Test.Core.Service {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PackageManagement.Packaging;
    using PackageManagement.Utility.Async;
    using PackageManagement.Utility.Extensions;
    using Sdk;
    using TestProviders;
    using Xunit;
    using Xunit.Abstractions;
    using Console = Support.Console;
    using Constants = PackageManagement.Constants;

    public class Happy1ProviderTests : UsingOneProvider {
        public Happy1ProviderTests(ITestOutputHelper outputHelper) : base(outputHelper, "Happy1") {
        }

        private void Reset() {
            // get the provider
            Assert.NotNull(Provider);

            // check to see if the happy instance is initialized.
            Assert.True(Happy1.Initialized);

            Happy1.TestInstance.Reset();
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
                var x = Provider.GetFeatures(Host());
                AssertNoErrors();
                x.Wait();
                Assert.NotNull(x.Value);

                // or use the cached version
                Assert.NotNull(Provider.Features);

                // our test features should tell us they are for testing
                Assert.True(Provider.Features.ContainsKey(Sdk.Constants.Features.Test));
            }
        }

        [Fact]
        public void TestDynamicOptions() {
            using (CaptureConsole) {
                lock (Provider) {
                    Reset();

                    var opt_install = Provider.GetDynamicOptions(OptionCategory.Install, Host());

                    opt_install.Wait();
                    var opts = opt_install.ToArray();

                    Assert.Equal(9, opts.Length);
                    foreach (var o in opts) {
                        // common
                        Assert.Equal(OptionCategory.Install, o.Category);
                        Assert.False(o.IsRequired);
                        Assert.Empty(o.PossibleValues);
                    }

                    // verify names;
                    for (int i = 0; i < opts.Length; i++) {
                        Assert.Equal("install_{0}".format(i + 1), opts[i].Name);
                    }

                    // verify that the types are set correctly
                    Assert.Equal(new[] {
                        OptionType.File,
                        OptionType.Folder,
                        OptionType.Int,
                        OptionType.Path,
                        OptionType.SecureString,
                        OptionType.String,
                        OptionType.StringArray,
                        OptionType.Switch,
                        OptionType.Uri
                    }, opts.Select(each => each.Type));

                    var opt_provider = Provider.GetDynamicOptions(OptionCategory.Provider, Host());
                    AssertNoErrors();
                    opts = opt_provider.ToArray();
                    Assert.Equal(1, opts.Length);
                    Assert.Equal("provider_1", opts[0].Name);
                    Assert.Equal(new[] {"one", "two", "three"}, opts[0].PossibleValues);

                    var opt_source = Provider.GetDynamicOptions(OptionCategory.Source, Host());
                    AssertNoErrors();
                    opts = opt_source.ToArray();
                    Assert.Equal(1, opts.Length);
                    Assert.Equal(new[] {"one", "two", "three"}, opts[0].PossibleValues);
                    Assert.True(opts[0].IsRequired);
                    Assert.Equal("source_1", opts[0].Name);

                    var opt_package = Provider.GetDynamicOptions(OptionCategory.Package, Host());
                    AssertNoErrors();
                    Assert.Empty(opt_package);
                }
            }
        }

        [Fact]
        public void ResolvePackageSourceTest() {
            using (CaptureConsole) {
                lock (Provider) {
                    Reset();

                    Console.WriteLine("hi");

                    Assert.NotNull(Provider);

                    // ask without any parameters
                    var sources = Provider.ResolvePackageSources(Host()).ToArray();
                    AssertNoErrors();
                    Assert.NotEmpty(sources);
                    foreach (var source in sources) {
                        Console.WriteLine("Source {0} => {1}", source.Name, source.Location);
                    }

                    sources = Provider.ResolvePackageSources(Host(new {
                        // override just GetSources()
                        GetSources = new Func<IEnumerable<string>>(() => "source2".SingleItemAsEnumerable())
                    })).ToArray();
                    AssertNoErrors();
                    Assert.Equal(1, sources.Length);
                    Assert.Equal("location2", sources[0].Location);
                    Assert.Equal(_providerName, sources[0].ProviderName);
                    Assert.Equal(Provider, sources[0].Provider);
                }
            }
        }

        [Fact]
        public void AddPackageSourceTest() {
            using (CaptureConsole) {
                lock (Provider) {
                    Reset();

                    // simple add a package source
                    var addedSources = Provider.AddPackageSource("srcname", "srcLocation", false, Host()).ToArray();
                    AssertNoErrors();
                    Assert.Equal(1, addedSources.Length);
                    Assert.Equal("srcname", addedSources[0].Name);
                    Assert.Equal("srcLocation", addedSources[0].Location);
                    Assert.True(addedSources[0].IsRegistered);
                    Assert.False(addedSources[0].IsTrusted);
                    Assert.True(addedSources[0].IsValidated);

                    // simple update a package source
                    addedSources = Provider.AddPackageSource("srcname", "srcLocation2", true, Host(new {
                        // tell the client that it's an update
                        GetOptionValues = new Func<string, IEnumerable<string>>(key => {
                            if (key == Constants.Parameters.IsUpdate) {
                                return "true".SingleItemAsEnumerable();
                            }
                            return null;
                        })
                    })).ToArray();
                    AssertNoErrors();

                    Assert.Equal(1, addedSources.Length);
                    Assert.Equal("srcname", addedSources[0].Name);
                    Assert.Equal("srcLocation2", addedSources[0].Location);
                    Assert.True(addedSources[0].IsRegistered);
                    Assert.True(addedSources[0].IsTrusted);
                    Assert.True(addedSources[0].IsValidated);

                    // should error on overwrite without isUpdate
                    addedSources = Provider.AddPackageSource("srcname", "srcLocation2", true, Host()).ToArray();
                    Assert.Equal(1, ErrorCount);
                    Assert.Equal(0, addedSources.Length);
                }
            }
        }

        [Fact]
        public void RemovePackageSourceTest() {
            using (CaptureConsole) {
                Reset();

                // simple add a package source
                var addedSources = Provider.AddPackageSource("srcname", "srcLocation", false, Host()).ToArray();
                AssertNoErrors();
                Assert.Equal(1, addedSources.Length);
                Assert.Equal("srcname", addedSources[0].Name);
                Assert.Equal("srcLocation", addedSources[0].Location);
                Assert.True(addedSources[0].IsRegistered);
                Assert.False(addedSources[0].IsTrusted);
                Assert.True(addedSources[0].IsValidated);

                // now, remove it.
                var removedSources = Provider.RemovePackageSource("srcname", Host()).ToArray();
                AssertNoErrors();
                Assert.Equal(1, removedSources.Length);

                // make sure that the package source doesn't exist in the list anymore
                var sources = Provider.ResolvePackageSources(Host()).ToArray();
                AssertNoErrors();
                Assert.DoesNotContain(sources, each => each.Name.EqualsIgnoreCase("srcname"));

                // shoudl error on source not found
                removedSources = Provider.RemovePackageSource("srcname", Host()).ToArray();
                Assert.Equal(1, ErrorCount);
                Assert.Equal(0, removedSources.Length);
            }
        }

        [Fact]
        public void FindPackageTest() {
            using (CaptureConsole) {
                Reset();

                var packages = Provider.FindPackage(null, null, null, null, 0, Host()).ToArray();
                AssertNoErrors();
                Assert.Equal(5, packages.Length);
            }
        }

        [Fact]
        public void FindPackageByUriTest() {
            using (CaptureConsole) {
                Reset();
            }
        }

        [Fact]
        public void FindPackageByFileTest() {
            using (CaptureConsole) {
                Reset();
            }
        }

        [Fact]
        public void GetInstalledPackagesTest() {
            using (CaptureConsole) {
                Reset();
            }
        }

        [Fact]
        public void InstallPackageTest() {
            using (CaptureConsole) {
                Reset();
            }
        }

        [Fact]
        public void UninstallPackageTest() {
            using (CaptureConsole) {
                Reset();
            }
        }

        [Fact]
        public void IsSupportedFile() {
            using (CaptureConsole) {
                Reset();
            }
        }

        [Fact]
        public void IsSupportedSchemeTest() {
            using (CaptureConsole) {
                Reset();
            }
        }

        [Fact]
        public void FirstTest() {
            using (CaptureConsole) {
                Reset();
            }
        }
    }
}