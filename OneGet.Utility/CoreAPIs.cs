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

namespace Microsoft.OneGet {
    using System;
    using System.Collections.Generic;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    public delegate bool OnMainThread(Func<bool> onMainThreadDelegate);


    #region declare core-apis
    // Core Callbacks that we'll both use internally and pass on down to providers.
    public delegate bool Warning( string message, IEnumerable<object> args = null);

    public delegate bool Error(string message, IEnumerable<object> args = null);

    public delegate bool Message(string message, IEnumerable<object> args = null);

    public delegate bool Verbose(string message, IEnumerable<object> args = null);

    public delegate bool Debug(string message, IEnumerable<object> args = null);

    public delegate bool ExceptionThrown(string exceptionType, string message, string stacktrace);

    public delegate int StartProgress(int parentActivityId, string message, IEnumerable<object> args = null);

    public delegate bool Progress(int activityId, int progress, string message, IEnumerable<object> args = null);

    public delegate bool CompleteProgress(int activityId, bool isSuccessful);

    /// <summary>
    ///     The provider can query to see if the operation has been cancelled.
    ///     This provides for a gentle way for the caller to notify the callee that
    ///     they don't want any more results.
    /// </summary>
    /// <returns>returns TRUE if the operation has been cancelled.</returns>
    public delegate bool IsCancelled();
    #endregion 

}