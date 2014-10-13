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

namespace Microsoft.OneGet.Utility.PowerShell {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Threading;
    using System.Threading.Tasks;
    using Collections;
    using Extensions;

    public delegate bool OnMainThread(Func<bool> onMainThreadDelegate);

    public abstract class AsyncCmdlet : PSCmdlet, IDynamicParameters, IDisposable {
        private readonly HashSet<string> _errors = new HashSet<string>();
        private readonly HashSet<string> _warnings = new HashSet<string>();
        // private List<ICancellable> _cancelWhenStopped = new List<ICancellable>();
        private bool _consumed;
        private RuntimeDefinedParameterDictionary _dynamicParameters;
        // private ManualResetEvent _cancellationEvent = new ManualResetEvent(false);
        protected CancellationTokenSource _cancellationEvent = new CancellationTokenSource();
        private BlockingCollection<TaskCompletionSource<bool>> _messages;

        private Stopwatch _stopwatch;

#if DEBUG
        [Parameter()]
        public SwitchParameter IsTesting;
#endif

        private static int _ignoreDepth = 0;

        protected static bool IgnoreErrors {
            get {
                return _ignoreDepth != 0;
            }
            set {
                if (value) {
                    _ignoreDepth++;
                } else {
                    _ignoreDepth--;
                }
            }
        }

        protected bool Confirm {
            get {
                return MyInvocation.BoundParameters.ContainsKey(Constants.ConfirmParameter) && (SwitchParameter)MyInvocation.BoundParameters[Constants.ConfirmParameter];
            }
        }

        public bool WhatIf {
            get {
                return MyInvocation.BoundParameters.ContainsKey(Constants.WhatIfParameter) && (SwitchParameter)MyInvocation.BoundParameters[Constants.WhatIfParameter];
            }
        }

        protected static bool IsInitialized {get; set;}

        public virtual RuntimeDefinedParameterDictionary DynamicParameterDictionary {
            get {
                return _dynamicParameters ?? (_dynamicParameters = new RuntimeDefinedParameterDictionary());
            }
        }

        protected bool IsInvocation {
            get {
#if DEBUG
                if (IsTesting) {
                    return true;
                }
#endif
                return MyInvocation.Line.Is();
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public object GetDynamicParameters() {
            // CompletionCompleters.
            // CommandCompletion.
            if (DynamicParameterDictionary.IsNullOrEmpty()) {
                if (IsOverridden(Constants.GenerateDynamicParametersMethod)) {
                    AsyncRun(GenerateDynamicParameters);
                }
            }

            return DynamicParameterDictionary;
        }

        public virtual bool BeginProcessingAsync() {
            return false;
        }

        public virtual bool EndProcessingAsync() {
            return false;
        }

        public virtual bool StopProcessingAsync() {
            return false;
        }

        public virtual bool ProcessRecordAsync() {
            return false;
        }

        public string ResolveExistingFilePath(string filePath) {
            ProviderInfo providerInfo = null;
            var files = GetResolvedProviderPathFromPSPath(filePath, out providerInfo).ToArray();
            switch (files.Length) {
                case 0:
                    // none found 
                    Error(Errors.FileNotFound, filePath);
                    break;

                case 1:
                    if (File.Exists(files[0])) {
                        return files[0];
                    }
                    Error(Errors.FileNotFound, filePath);
                    break;

                default:
                    Error(Errors.MoreThanOneFileMatched, filePath, files.JoinWithComma());
                    break;
            }
            return null;
        }

        public string ResolveExistingFolderPath(string folderPath) {
            ProviderInfo providerInfo = null;
            var files = GetResolvedProviderPathFromPSPath(folderPath, out providerInfo).ToArray();
            switch (files.Length) {
                case 0:
                    // none found 
                    Error(Errors.FolderNotFound, folderPath);
                    break;

                case 1:
                    if (Directory.Exists(files[0])) {
                        return files[0];
                    }
                    Error(Errors.FolderNotFound, folderPath);
                    break;

                default:
                    Error(Errors.MoreThanOneFolderMatched, folderPath, files.JoinWithComma());
                    break;
            }
            return null;
        }

        public string ResolvePath(string path) {
            return GetUnresolvedProviderPathFromPSPath(path);
        }

        private void InvokeMessage(TaskCompletionSource<bool> message) {
            var func = message.Task.AsyncState as Func<bool>;
            if (func != null) {
                try {
                    message.SetResult(func());
                } catch (Exception e) {
                    message.SetException(e);
                }
            } else {
                // this should have been a Func<bool>.
                // cancel it.
                message.SetCanceled();
            }
        }

        private Task<bool> QueueMessage(TaskCompletionSource<bool> message) {
            if (_messages == null || _messages.IsCompleted) {
                // message queue isn't active. Just run the message now.
                InvokeMessage(message);
            } else {
                if (!_messages.IsCompleted) {
                    _messages.Add(message);
                }
            }
            return message.Task;
        }

        private Task<bool> QueueMessage(Func<bool> action) {
            return QueueMessage(new TaskCompletionSource<bool>(action));
        }

        private Task<bool> QueueMessage(Action action) {
            return QueueMessage(() => {
                action();
                return true;
            });
        }

        public bool Warning(string messageText) {
            return Warning(messageText, Constants.NoParameters);
        }

        public bool Warning(ErrorMessage message) {
            if (message == null) {
                throw new ArgumentNullException("message");
            }
            return Warning(message.Resource, Constants.NoParameters);
        }

        public bool Warning(ErrorMessage message, params object[] args) {
            if (message == null) {
                throw new ArgumentNullException("message");
            }

            return Warning(message.Resource, args);
        }

        public bool Warning(string messageText, params object[] args) {
            if (IsInvocation) {
                WriteWarning(FormatMessageString(messageText, args));
            }
            // rather than wait on the result of the async WriteVerbose,
            // we'll just return the stopping state.
            return IsCanceled;
        }

        protected bool Error(ErrorMessage errorMessage) {
            return Error(errorMessage.Resource, errorMessage.Category.ToString(), null, errorMessage.Resource);
        }

        protected bool Error(ErrorMessage errorMessage, params object[] args) {
            return Error(errorMessage.Resource, errorMessage.Category.ToString(), null, FormatMessageString(errorMessage.Resource, args));
        }

        public bool Error(string id, string category, string targetObjectValue, string messageText) {
            return Error(id, category, targetObjectValue, messageText, Constants.NoParameters);
        }

        public string DropMsgPrefix(string messageText) {
            if (string.IsNullOrEmpty(messageText)) {
                return messageText;
            }
            return messageText.StartsWith("MSG:", StringComparison.OrdinalIgnoreCase) ? messageText.Substring(4) : messageText;
        }

        public bool Error(string id, string category, string targetObjectValue, string messageText, params object[] args) {
            if (!IgnoreErrors) {
                if (IsInvocation) {
                    var errorMessage = FormatMessageString(messageText, args);

                    if (!_errors.Contains(errorMessage)) {
                        if (!_errors.Any()) {
                            ErrorCategory errorCategory;
                            if (!Enum.TryParse(category, true, out errorCategory)) {
                                errorCategory = ErrorCategory.NotSpecified;
                            }
                            try {
                                WriteError(new ErrorRecord(new Exception(errorMessage), DropMsgPrefix(id), errorCategory, string.IsNullOrEmpty(targetObjectValue) ? (object)this : targetObjectValue)).Wait();
                            } catch {
                                // this will throw if the provider thread abends before we get back our result.
                            }
                        }
                        _errors.Add(errorMessage);
                    }
                }
                Cancel();
            }
            // rather than wait on the result of the async'd message,
            // we'll just return the stopping state.
            return IsCanceled;
        }

        public bool Message(string messageText) {
            return Message(messageText, Constants.NoParameters);
        }

        public bool Message(string messageText, params object[] args) {
            // queue the message to run on the main thread.
            if (IsInvocation) {
                //  QueueMessage(() => Host.UI.WriteLine("{0}::{1}".format(code, message.formatWithIEnumerable(objects))));
                // Message is going to go to the verbose channel
                // and Verbose will only be output if VeryVerbose is true.
                WriteVerbose(FormatMessageString(messageText, args));
            }
            // rather than wait on the result of the async WriteVerbose,
            // we'll just return the stopping state.
            return IsCanceled;
        }

        public bool Verbose(string messageText) {
            return Verbose(messageText, Constants.NoParameters);
        }

        public bool Verbose(string messageText, params object[] args) {
            if (IsInvocation) {
                // Message is going to go to the verbose channel
                // and Verbose will only be output if VeryVerbose is true.
                WriteVerbose(FormatMessageString(messageText, args));
            }
            // rather than wait on the result of the async WriteVerbose,
            // we'll just return the stopping state.
            return IsCanceled;
        }

        public bool Debug(string messageText) {
            return Debug(messageText, Constants.NoParameters);
        }

        public bool Debug(string messageText, params object[] args) {
            if (IsInvocation) {
                if (_stopwatch == null) {
                    _stopwatch = new Stopwatch();
                    _stopwatch.Start();
                }

                WriteDebug("{0} {1}".format(_stopwatch.Elapsed, FormatMessageString(messageText, args)));
            }

            // rather than wait on the result of the async WriteVerbose,
            // we'll just return the stopping state.
            return IsCanceled;
        }

        public int StartProgress(int parentActivityId, string message) {
            return StartProgress(parentActivityId, message, Constants.NoParameters);
        }

        private List<ProgressTracker> _progressTrackers = new List<ProgressTracker>();

        private ProgressTracker _activeProgressId;
        private int _nextProgressId = 1;

        public int StartProgress(int parentActivityId, string message, params object[] args) {
            if (IsInvocation) {
                lock (_progressTrackers) {
                    ProgressTracker parent = null;

                    if (parentActivityId <= 0) {
                        if (_activeProgressId != null) {
                            parent = _activeProgressId;
                        }
                    } else {
                        parent = _progressTrackers.FirstOrDefault(each => each.Id == parentActivityId);
                    }
                    var p = new ProgressTracker() {
                        Activity = FormatMessageString(message, args),
                        Id = _nextProgressId++,
                        Parent = parent
                    };
                    if (parent != null) {
                        parent.Children.Add(p);
                    }
                    _progressTrackers.Add(p);

                    WriteProgress(new ProgressRecord(p.Id, p.Activity, " ") {
                        PercentComplete = 0,
                        RecordType = ProgressRecordType.Processing
                    });
                    return p.Id;
                }
            }
            return 0;
        }

        public bool Progress(int activityId, int progressPercentage, string messageText) {
            return Progress(activityId, progressPercentage, messageText, Constants.NoParameters);
        }

        public bool Progress(int activityId, int progressPercentage, string messageText, params object[] args) {
            lock (_progressTrackers) {
                if (IsInvocation) {
                    var p = _progressTrackers.FirstOrDefault(each => each.Id == activityId);
                    if (p != null) {
                        if (progressPercentage >= 100) {
                            progressPercentage = 100;
                        }

                        WriteProgress(new ProgressRecord(p.Id, p.Activity, FormatMessageString(messageText, args)) {
                            ParentActivityId = p.Parent != null ? p.Parent.Id : 0,
                            PercentComplete = progressPercentage,
                            RecordType = ProgressRecordType.Processing
                        });

                        if (progressPercentage >= 100) {
                            return CompleteProgress(activityId, true);
                        }
                    }
                }
            }
            // rather than wait on the result of the async WriteVerbose,
            // we'll just return the stopping state.
            return IsCanceled;
        }

        public bool CompleteProgress(int activityId, bool isSuccessful) {
            lock (_progressTrackers) {
                if (IsInvocation) {
                    var p = _progressTrackers.FirstOrDefault(each => each.Id == activityId);
                    if (p != null) {
                        // complete all of this trackers kids.
                        foreach (var child in p.Children) {
                            CompleteProgress(child.Id, isSuccessful);
                        }
                        if (p.Parent != null) {
                            p.Parent.Children.Remove(p);
                        }
                        _progressTrackers.Remove(p);
                        if (_messages == null) {
                            base.WriteProgress(new ProgressRecord(p.Id, p.Activity, "Completed.") {
                                ParentActivityId = p.Parent != null ? p.Parent.Id : 0,
                                PercentComplete = 100,
                                RecordType = ProgressRecordType.Completed
                            });
                        } else {
                            WriteProgress(new ProgressRecord(p.Id, p.Activity, "Completed.") {
                                ParentActivityId = p.Parent != null ? p.Parent.Id : 0,
                                PercentComplete = 100,
                                RecordType = ProgressRecordType.Completed
                            });
                        }
                    }
                }
            }

            // rather than wait on the result of the async WriteVerbose,
            // we'll just return the stopping state.
            return IsCanceled;
        }

        /// <summary>
        ///     The provider can query to see if the operation has been cancelled.
        ///     This provides for a gentle way for the caller to notify the callee that
        ///     they don't want any more results.
        /// </summary>
        /// <value>returns TRUE if the operation has been cancelled.</value>
        public bool IsCanceled {
            get {
                return Stopping || _cancellationEvent == null || _cancellationEvent.IsCancellationRequested;
            }
        }

        public virtual string GetMessageString(string messageText) {
            return null;
        }

        public string FormatMessageString(string messageText, params object[] args) {
            if (string.IsNullOrEmpty(messageText)) {
                return string.Empty;
            }

            if (messageText.StartsWith(Constants.MSGPrefix, true, CultureInfo.CurrentCulture)) {
                messageText = GetMessageString(messageText.Substring(Constants.MSGPrefix.Length)) ?? messageText;
            }

            return args == null || args.Length == 0 ? messageText : messageText.format(args);
        }

        private void AsyncRun(Func<bool> asyncAction) {
            using (_messages = new BlockingCollection<TaskCompletionSource<bool>>()) {
                // spawn the activity off in another thread.
                var task = IsInitialized ?
                    Task.Factory.StartNew(asyncAction, TaskCreationOptions.LongRunning) :
                    Task.Factory.StartNew(Init, TaskCreationOptions.LongRunning).ContinueWith(antecedent => {
                        try {
                            asyncAction();
                        } catch (Exception e) {
                            e.Dump();
                        }
                    })
                    ;

                // when the task is done, mark the msg queue as complete
                task.ContinueWith(antecedent => {
                    if (_messages != null) {
                        _messages.Complete();
                    }
                });

                // process the queue of messages back in the main thread so that they
                // can properly access the non-thread-safe-things in cmdlet
                foreach (var message in _messages) {
                    InvokeMessage(message);
                }
            }
            _messages = null;
        }

        private bool IsOverridden(string functionName) {
            return GetType().GetMethod(functionName).DeclaringType != typeof (AsyncCmdlet);
        }

        protected override sealed void BeginProcessing() {
            // let's not even bother doing all this if they didn't even
            // override the method.
            if (IsOverridden(Constants.BeginProcessingAsyncMethod)) {
                // just before we kick stuff off, let's make sure we consume the dynamicaparmeters
                if (!_consumed) {
                    ConsumeDynamicParameters();
                    _consumed = true;
                }
                // just use our async/message pump to handle this activity
                AsyncRun(BeginProcessingAsync);
            }
        }

        protected override sealed void EndProcessing() {
            // let's not even bother doing all this if they didn't even
            // override the method.
            if (IsOverridden(Constants.EndProcessingAsyncMethod)) {
                // just before we kick stuff off, let's make sure we consume the dynamicaparmeters
                if (!_consumed) {
                    ConsumeDynamicParameters();
                    _consumed = true;
                }

                // just use our async/message pump to handle this activity
                AsyncRun(EndProcessingAsync);
            }

            // make sure that we mark progress complete.
            if (_progressTrackers.Any()) {
                AllProgressComplete();
            }
        }

        /*
        protected T CancelWhenStopped<T>(T cancellable) where T : ICancellable {
            if (IsCanceled) {
                cancellable.Cancel();
                return cancellable;
            }

            lock (_cancelWhenStopped) {
                _cancelWhenStopped.Add(cancellable);
            }

            return cancellable;
        }
        */

        public void Cancel() {
            // notify anyone listening that we're stopping this call.
            _cancellationEvent.Cancel();

            /*
            // actively cancel any calls in progress
            foreach (var i in _cancelWhenStopped.Where(i => i != null)) {
                i.Cancel();
            }
            _cancelWhenStopped.Clear();
            _cancelWhenStopped = null;
             * */
        }

        protected override sealed void StopProcessing() {
            // Console.WriteLine("===============================================================CTRL-C PRESSED");
            Cancel();
            // let's not even bother doing all this if they didn't even
            // override the method.
            if (IsOverridden(Constants.StopProcessingAsyncMethod)) {
                // just use our async/message pump to handle this activity
                AsyncRun(StopProcessingAsync);
            }
            if (_progressTrackers.Any()) {
                AllProgressComplete();
            }
        }

        protected override sealed void ProcessRecord() {
            // let's not even bother doing all this if they didn't even
            // override the method.
            if (IsOverridden(Constants.ProcessRecordAsyncMethod)) {
                // just before we kick stuff off, let's make sure we consume the dynamicaparmeters
                if (!_consumed) {
                    ConsumeDynamicParameters();
                    _consumed = true;
                }

                // just use our async/message pump to handle this activity
                AsyncRun(ProcessRecordAsync);
            }
        }

        public Task<bool> ExecuteOnMainThread(Func<bool> onMainThreadDelegate) {
            return QueueMessage(onMainThreadDelegate);
        }

        public new Task<bool> WriteObject(object obj) {
            return QueueMessage(() => {
                if (!IsCanceled) {
                    base.WriteObject(obj);
                }
            });
        }

        public new Task<bool> WriteObject(object sendToPipeline, bool enumerateCollection) {
            return QueueMessage(() => {
                if (!IsCanceled) {
                    base.WriteObject(sendToPipeline, enumerateCollection);
                }
            });
        }

        public new Task<bool> WriteProgress(ProgressRecord progressRecord) {
            return QueueMessage(() => {
                if (!IsCanceled) {
                    base.WriteProgress(progressRecord);
                }
            });
        }

        public Task<bool> AllProgressComplete() {
            lock (_progressTrackers) {
                while (_progressTrackers.Any()) {
                    CompleteProgress(_progressTrackers.FirstOrDefault().Id, true);
                }
            }
            return IsCanceled.AsResultTask();
        }

        public new Task<bool> WriteWarning(string text) {
            if (!IsInvocation) {
                return false.AsResultTask();
            }
            // ensure the same warning doesn't get played repeatedly.
            if (_warnings.Contains(text)) {
                return true.AsResultTask();
            }
            _warnings.Add(text);
            return QueueMessage(() => base.WriteWarning(text));
        }

        public new Task<bool> WriteDebug(string text) {
            if (!IsInvocation) {
                return false.AsResultTask();
            }
            return QueueMessage(() => base.WriteDebug(text));
        }

        public new Task<bool> WriteError(ErrorRecord errorRecord) {
            if (!IsInvocation) {
                return false.AsResultTask();
            }
            return QueueMessage(() => base.WriteError(errorRecord));
        }

        public new Task<bool> WriteVerbose(string text) {
            if (!IsInvocation) {
                return false.AsResultTask();
            }
            return QueueMessage(() => base.WriteVerbose(text));
        }

        public new Task<bool> ShouldContinue(string query, string caption) {
            if (IsCanceled || !IsInvocation) {
                return false.AsResultTask();
            }
            return QueueMessage(() => base.ShouldContinue(query, caption));
        }

        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "MYOB.")]
        public new Task<bool> ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll) {
            if (IsCanceled || !IsInvocation) {
                return false.AsResultTask();
            }

            // todo: Uh, this is gonna be tricky!?
            return QueueMessage(() => base.ShouldContinue(query, caption));
        }

        public new Task<bool> ShouldProcess(string target) {
            if (IsCanceled || !IsInvocation) {
                return false.AsResultTask();
            }

            return QueueMessage(() => base.ShouldProcess(target));
        }

        public new Task<bool> ShouldProcess(string target, string action) {
            if (IsCanceled || !IsInvocation) {
                return false.AsResultTask();
            }

            return QueueMessage(() => base.ShouldProcess(target, action));
        }

        public new Task<bool> ShouldProcess(string verboseDescription, string verboseWarning, string caption) {
            if (IsCanceled || !IsInvocation) {
                return false.AsResultTask();
            }

            return QueueMessage(() => base.ShouldProcess(verboseDescription, verboseWarning, caption));
        }

        public new Task<bool> ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason) {
            if (IsCanceled || !IsInvocation) {
                shouldProcessReason = ShouldProcessReason.None;
                return false.AsResultTask();
            }

            // todo: Uh, this is gonna be tricky!?
            shouldProcessReason = ShouldProcessReason.None;
            return QueueMessage(() => base.ShouldProcess(verboseDescription, verboseWarning, caption));
        }

        protected virtual void Init() {
        }

        public virtual bool GenerateDynamicParameters() {
            return true;
        }

        public virtual bool ConsumeDynamicParameters() {
            return true;
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                /*
                if (_cancelWhenStopped != null) {
                    _cancelWhenStopped.Clear();
                    _cancelWhenStopped = null;
                }
*/
                if (_cancellationEvent != null) {
                    _cancellationEvent.Dispose();
                    _cancellationEvent = null;
                }

                // According to http://msdn.microsoft.com/en-us/library/windows/desktop/ms714463(v=vs.85).aspx
                // Powershell will dispose the cmdlet if it implements IDisposable.

                if (_messages != null) {
                    _messages.Dispose();
                    _messages = null;
                }
            }
        }
    }
}