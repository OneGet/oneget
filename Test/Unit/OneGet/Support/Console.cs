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

namespace Microsoft.OneGet.Test.Support {
    public delegate void WriteLine(string format, params object[] args);

    public static class Console {
        public static void WriteLine(string format, params object[] args) {
            // System.Console.Beep(3000, 26);
            Event<WriteLine>.Raise(format, args);
        }

        public static void WriteLine(object output) {
            // System.Console.Beep(3000, 26);
            Event<WriteLine>.Raise((output ?? "«null»").ToString());
        }
    }
}