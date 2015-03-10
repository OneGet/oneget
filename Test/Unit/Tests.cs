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
    using System.Threading;
    using Support;
    using OneGet.Utility.Extensions;
    using Xunit;
    using Xunit.Abstractions;
    using TestCodeAttribute = System.CodeDom.Compiler.GeneratedCodeAttribute;

    [TestCode("TestCode","")]
    public class Tests {
        private static readonly HashSet<string> _flags = new HashSet<string>();
        public readonly ITestOutputHelper Out;

        public Tests(ITestOutputHelper outputHelper) {
            Out = outputHelper;
        }

        public class OnDispose : IDisposable {
            public void Dispose() {
                Support.Console.Flush();
                // CurrentOut.WriteLine("Setting Null!");
                // CurrentOut = null;
            }
        }

        static Tests() {
            /* XTask.AddEventHandler(null, new WriteLine((format, args) => {
                try {
                    if (CurrentOut != null) {
                        CurrentOut.WriteLine(format.format(args));
                    }
                }
                catch { }
            }));
             * */
        }

        private static ThreadLocal<ITestOutputHelper> _out = new ThreadLocal<ITestOutputHelper>();
        private static ITestOutputHelper _tmp;
        internal static ITestOutputHelper CurrentOut {
            get {
                if (_out.IsValueCreated) {
                    return _out.Value;
                }
                
                return _tmp;
            }
        }

        public IDisposable CaptureConsole {
            get {
                _out.Value = Out;
              
                // var lve = CurrentTask.Local;
                //var writeLine = new WriteLine((format, args) => {Out.WriteLine(format.format(args));});

                //lve.Events += writeLine;

                // XTask.AddEventHandler(null, writeLine);
                // return lve;
                return new OnDispose();
            }
        }

        public static void Set(string flag) {
            lock (_flags) {
                Assert.False(_flags.Contains(flag), String.Format("Set Same Flag '{0}' Twice!", flag));
                _flags.Add(flag);
            }
        }

        public static void Ensure(string flag, string message) {
            lock (_flags) {
                Assert.True(_flags.Contains(flag), message);
            }
        }

        public class CaseInsensitiveEqualityComparer : IEqualityComparer<string> {
            internal static CaseInsensitiveEqualityComparer Instance = new CaseInsensitiveEqualityComparer();
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "#pw26506")]
            public bool Equals(string x, string y) {
                return x.EqualsIgnoreCase(y);
            }

            public int GetHashCode(string obj) {
                return -1;
            }
        }

        protected static CaseInsensitiveEqualityComparer IgnoreCase {
            get {
                return CaseInsensitiveEqualityComparer.Instance;
            }
        }
    }
}