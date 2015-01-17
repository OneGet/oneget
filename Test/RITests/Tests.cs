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

namespace Microsoft.OneGet.Test {
    using System;
    using System.Collections.Generic;
    using Support;
    using OneGet.Utility.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class Tests {
        private static readonly HashSet<string> _flags = new HashSet<string>();
        public readonly ITestOutputHelper Out;

        public Tests(ITestOutputHelper outputHelper) {
            Out = outputHelper;
        }

        public IDisposable CaptureConsole {
            get {
                var lve = CurrentTask.Local;
                lve.Events += new WriteLine((format, args) => {Out.WriteLine(format.format(args));});
                return lve;
            }
        }

        public static void Set(string flag) {
            lock (_flags) {
                Assert.False(_flags.Contains(flag), string.Format("Set Same Flag '{0}' Twice!", flag));
                _flags.Add(flag);
            }
        }

        public static void Ensure(string flag, string message) {
            lock (_flags) {
                Assert.True(_flags.Contains(flag), message);
            }
        }
    }
}