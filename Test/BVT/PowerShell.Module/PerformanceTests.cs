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

namespace OneGet.PowerShell.Module.Test {
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Microsoft.OneGet.Utility.Extensions;
    using Microsoft.OneGet.Utility.PowerShell;
    using Xunit;
    public class PerformanceTests : TestBase {
        
        [Fact(Timeout = 60000), Trait("Test", "Performance")]
        public void TestPerformanceFindPackageSelectObjectPipePsModule()
        {
            dynamic ps = NewPowerShellSession;

            var watch = new Stopwatch();
            DynamicPowershellResult warmup = ps.FindPackage(Provider: "PSModule", Source: "PSGallery", Name: "AppDomainConfig", IsTesting: true);
            warmup.WaitForCompletion();
            watch.Start();
            DynamicPowershellResult result = ps.FindPackage(Provider: "PSModule", Source: "PSGallery", Name: "AppDomainConfig", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.SelectObject(result, First: "1");
            result2.WaitForCompletion();
            watch.Stop();
            Console.WriteLine(@"Time elapsed: {0}", watch.Elapsed);
            Assert.False(result2.ContainsErrors);
        }

        [Fact(Timeout = 60000, Skip = "Disabled. AppDomainConfig does not exist at the moment."), Trait("Test", "Performance")]
        public void TestPerformanceFindPackagePsModule()
        {
            dynamic ps = NewPowerShellSession;

            var watch = new Stopwatch();
            DynamicPowershellResult warmup = ps.FindPackage(Provider: "PSModule", Source: "PSGallery", Name: "AppDomainConfig", IsTesting: true);
            warmup.WaitForCompletion();
            watch.Start();
            DynamicPowershellResult result = ps.FindPackage(Provider: "PSModule", Source: "PSGallery", Name: "AppDomainConfig", IsTesting: true);
            result.WaitForCompletion();
            watch.Stop();
            Console.WriteLine(@"Time elapsed: {0}", watch.Elapsed);
            Assert.False(result.ContainsErrors);
        }

        [Fact(Timeout = 60000), Trait("Test", "Performance")]
        public void TestPerformanceFindPackageSelectObjectRepositoryPsGalleryPipe()
        {
            dynamic ps = NewPowerShellSession;

            var watch = new Stopwatch();
            DynamicPowershellResult warmup = ps.FindModule(Repository: "PSGallery", IsTesting: true);
            warmup.WaitForCompletion();
            watch.Start();
            DynamicPowershellResult result = ps.FindModule(Repository: "PSGallery", IsTesting: true);
            result.WaitForCompletion();
            DynamicPowershellResult result2 = ps.SelectObject(result, First: "1");
            result2.WaitForCompletion();
            watch.Stop();
            Console.WriteLine(@"Time elapsed: {0}", watch.Elapsed);
            Assert.False(result2.ContainsErrors);
        }

        [Fact(Timeout = 60000, Skip = "Disabled. AppDomainConfig does not exist at the moment."), Trait("Test", "Performance")]
        public void TestPerformanceFindPackageRepositoryPsGallery()
        {
            dynamic ps = NewPowerShellSession;

            var watch = new Stopwatch();
            DynamicPowershellResult warmup = ps.FindModule(Repository: "PSGallery", Name: "AppDomainConfig");
            warmup.WaitForCompletion();
            watch.Start();
            DynamicPowershellResult result = ps.FindModule(Repository: "PSGallery", Name: "AppDomainConfig");
            result.WaitForCompletion();
            watch.Stop();
            Console.WriteLine(@"Time elapsed: {0}", watch.Elapsed);
            Assert.False(result.ContainsErrors);
        }

        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */
        /* ---------------------------------------------------------------------------     HELPER FUNCTIONS     ---------------------------------------------------------------------------------- */
        /* --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- */

        private void CleanFolder(String path)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                file.TryHardToDelete();
            }
            foreach (string directory in Directory.GetDirectories(path))
            {
                directory.TryHardToDelete();
            }
        }

        private bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
    }
}