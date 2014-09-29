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
    using Microsoft.OneGet.Utility.Extensions;
    using Microsoft.OneGet.Utility.PowerShell;
    using Xunit;

    public class TestBase {
        private static object lockObject = new object();
        private static dynamic PowerShellSession;

        protected dynamic NewPowerShellSession {
            get {
                lock (lockObject) {
                    if (PowerShellSession == null) {

                        PowerShellSession = new DynamicPowershell();
                        DynamicPowershellResult result = PowerShellSession.ImportModule(".\\oneget.psd1");
                        Assert.False(result.IsFailing, "unable to import '.\\oneget.psd1  (PWD:'{0}')".format(Environment.CurrentDirectory));
                    }
                    return PowerShellSession;
                }
            }
        }

        protected void UnloadOneGet() {
            lock (lockObject) {
                if (PowerShellSession != null) {
                    PowerShellSession.Dispose();
                    PowerShellSession = null;
                }
            }
        }
    }
}