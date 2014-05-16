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

namespace Microsoft.OneGet.Core.Tasks {
    using System.Linq;
    using Extensions;

    public static class Event<T> where T : class {
        private static T EmptyDelegate {
            get {
                return typeof (T).CreateEmptyDelegate() as T;
            }
        }

        public static T Raise {
            get {
                return (XTask.CurrentExecutingTask.GetEventHandler(typeof (T)) as T) ?? EmptyDelegate;
            }
        }

        public static T RaiseFirst {
            get {
                var dlg = XTask.CurrentExecutingTask.GetEventHandler(typeof (T));
                return dlg != null ? dlg.GetInvocationList().FirstOrDefault() as T : EmptyDelegate;
            }
        }
    }
}