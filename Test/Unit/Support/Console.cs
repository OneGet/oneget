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

namespace Microsoft.PackageManagement.Test.Support {
    using System.Collections.Generic;
    using PackageManagement.Utility.Collections;
    using PackageManagement.Utility.Extensions;

    public delegate void WriteLine(string format, params object[] args);

    public static class Console {
        private static List<string> _queue = new List<string>();
        public static void WriteLine(string format, params object[] args) {
            try {
                // System.Console.Beep(3000, 26);
                // Event<WriteLine>.Raise(format, args);
                if (Tests.CurrentOut != null) {
                    // first Flush queue
                    Flush();
                    Tests.CurrentOut.WriteLine(format, args);
                } else {
                    // Event<WriteLine>.Raise(format, args);
                    lock (_queue) {
                        _queue.Add(format.format(args));
                    }
                }
            } catch {

            }
        }

        public static void Flush() {
            lock (_queue) {
                if (Tests.CurrentOut != null) {
                    foreach (var i in _queue) {
                        Tests.CurrentOut.WriteLine(i);
                    }
                    _queue.Clear();
                }
            }
        }

        public static void WriteLine(object output) {
            try {
                // System.Console.Beep(3000, 26);
                if (Tests.CurrentOut != null) {
                    Flush();
                    Tests.CurrentOut.WriteLine((output ?? "«null»").ToString());
                }
                else {
                    lock (_queue) {
                        _queue.Add((output ?? "«null»").ToString());
                    }
                }
                // Event<WriteLine>.Raise((output ?? "«null»").ToString());
            } catch {
                // Event<WriteLine>.Raise((output ?? "«null»").ToString());
            }
        }
    }
}
