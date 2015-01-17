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

namespace Microsoft.OneGet.Test.PowerShell {
    using OneGet.Utility.PowerShell;
    using Xunit;
    using Xunit.Abstractions;
    using Console = Support.Console;

    public class DynamicPowerShellTests : Tests {
        public DynamicPowerShellTests(ITestOutputHelper output) : base(output) {
             
        }

        [Fact]
        public void TestPipeline() {

            using (CaptureConsole) {
                dynamic ps = new DynamicPowershell();
                DynamicPowershellResult result = ps.Dir(@"c:\");

                DynamicPowershellResult result2 = ps.TestPath(result);

                foreach (var r in result2) {
                    Console.WriteLine(r);
                }
            }
        }
    }
}