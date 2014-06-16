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

namespace Microsoft.OneGet.Extensions {
    using System;
    using Platform;

    public static class ExceptionExtensions {
        public static void Dump(this Exception e) {
#if DEBUG
            var text = string.Format("{0}//{1}/{2}\r\n{3}", AppDomain.CurrentDomain.FriendlyName, e.GetType().Name, e.Message, e.StackTrace);
             Console.WriteLine(text);
             NativeMethods.OutputDebugString(text);
#endif
        }
#if DETAILED_DEBUG
        private static DateTime startTime = DateTime.Now;
        public static T DumpTime<T>(this T nothing) {
            StackTrace s = new StackTrace(true);
            var f = s.GetFrame(1);
           
            Console.WriteLine("      OFFSET: \r\n          {0}:{1}:\r\n          {2}\r\n          Time:[{3}] ",f.GetFileName(), f.GetFileLineNumber(), f.GetMethod(), DateTime.Now.Subtract(startTime).TotalMilliseconds);
            return nothing;
        }
#endif

    }
}
