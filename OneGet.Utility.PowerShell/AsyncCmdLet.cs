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
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Management.Automation;
    using System.Threading.Tasks;
    using Microsoft.OneGet.Core.Api;
    using Microsoft.OneGet.Core.Collections;
    using Microsoft.OneGet.Core.Extensions;
    using Microsoft.OneGet.Core.Tasks;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    public abstract class AsyncCmdlet : PSCmdlet, IDynamicParameters, IDisposable {
        private List<ICancellable> _cancelWhenStopped = new List<ICancellable>();
        private bool _consumed;
        private RuntimeDefinedParameterDictionary _dynamicParameters;
        private HashSet<string> _errors = new HashSet<string>();
        protected bool _failing = false;
        private SystemMessageDispatcher _systemMessageDispatcher;
        private LocalEventSource _localEventSource;

        private BlockingCollection<TaskCompletionSource<bool>> _messages;

        private int? _parentProgressId;

        public AsyncCmdlet() {
        }

        protected bool Confirm {
            get {
                return MyInvocation.BoundParameters.ContainsKey("Confirm") && (SwitchParameter)MyInvocation.BoundParameters["Confirm"];
            }
        }

        public bool WhatIf {
            get {
                return MyInvocation.BoundParameters.ContainsKey("WhatIf") && (SwitchParameter)MyInvocation.BoundParameters["WhatIf"];
            }
        }

        protected static bool IsInitialized {get; set;}

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Still in development.")]
        public virtual RuntimeDefinedParameterDictionary DynamicParameters {
            get {
                return _dynamicParameters ?? (_dynamicParameters = new RuntimeDefinedParameterDictionary());
            }
        }

        protected bool IsInvocation {
            get {
                return MyInvocation.Line.Is();
            }
        }

        protected virtual new Callback Invoke {
            get {
                return (_systemMessageDispatcher ?? (_systemMessageDispatcher = new SystemMessageDispatcher(this))).Invoke;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public object GetDynamicParameters() {
            // CompletionCompleters.
            // CommandCompletion.
            if (IsOverridden("GenerateDynamicParameters")) {
                AsyncRun(GenerateDynamicParameters);
            }

            // if the cmdlet is not actually running a command, just getting the dynamic parameters
            // for tab-completion, the cmdlet doesn't actually call Dispose(), so if we've
            // allocated the _localEventSource, we need to clean it up before we bail.
            // (even if PS did reuse this same instance later, it wouldn't actually hurt, it'd just
            // incurr the startup cost again..)
            if (!IsInvocation) {
                if (_localEventSource != null) {
                    ((IDisposable)_localEventSource).Dispose();
                }
                _localEventSource = null;
            }

            return DynamicParameters;
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
                if (!_messages.IsAddingCompleted) {
                    _messages.Add(message);
                }
            }
            return message.Task;
        }

        private Task<bool> QueueMessage(Func<bool> message) {
            return QueueMessage(new TaskCompletionSource<bool>(message));
        }

        private Task<bool> QueueMessage(Action message) {
            return QueueMessage(() => {
                message();
                return true;
            });
        }

        private void InitLocalEventSource() {
            if (_localEventSource == null) {
                _localEventSource = CurrentTask.Local;

                // this handles calls back to the main thread -- this will block waiting for the main thread to execute (or get thru whatever other msgs are there)
                _localEventSource.Events += new OnMainThread(onMainThreadDelegate => ExecuteOnMainThread(onMainThreadDelegate).Result);

                _localEventSource.Events += new Error(( message, objects) => {
                    message = GetLocalizedMessage(message,objects);

                    _failing = true;
                    // queue the message to run on the main thread.
                    if (IsInvocation) {
                        var error =  message;

                        if (!_errors.Contains(error)) {
                            if (!_errors.Any()) {
                                // todo : this should really have better error types. this is terrible..
                                WriteError(new ErrorRecord(new Exception(error), "errorid", ErrorCategory.OperationStopped, this));
                            }
                            _errors.Add(error);
                        }

                        //QueueMessage(() => Host.UI.WriteErrorLine("{0}:{1}".format(code, message.formatWithIEnumerable(objects))));
                    }
                    // rather than wait on the result of the async'd message,
                    // we'll just return the stopping state.
                    return IsCancelled();
                });

                _localEventSource.Events += new ExceptionThrown((type, message, stacktrace) => {
                    // queue the message to run on the main thread.
                    if (IsInvocation) {
                        // we should probably put this on the veryverbose channel
                        // QueueMessage(() => Host.UI.WriteErrorLine("{0}:{1}\r\n{2}".format(type, message, stacktrace)));
                    }
                    // rather than wait on the result of the async'd message,
                    // we'll just return the stopping state.
                    return IsCancelled();
                });

                _localEventSource.Events += new Debug(( message, objects) => {

                    if (IsInvocation) {
                        WriteDebug( GetLocalizedMessage(message,objects));
                    }

                    // rather than wait on the result of the async WriteVerbose,
                    // we'll just return the stopping state.
                    return IsCancelled();
                });

                _localEventSource.Events += new Verbose((message, objects) => {
                    if (IsInvocation) {
                        // Message is going to go to the verbose channel
                        // and Verbose will only be output if VeryVerbose is true.
                        WriteVerbose(GetLocalizedMessage(message,objects));
                    }
                    // rather than wait on the result of the async WriteVerbose,
                    // we'll just return the stopping state.
                    return IsCancelled();
                });

                _localEventSource.Events += new Progress((activityId, progress, message, objects) => {

                    if (IsInvocation) {
                        if (_parentProgressId == null) {
                            WriteProgress(new ProgressRecord(Math.Abs(activityId) + 1, "todo:activitylookup", GetLocalizedMessage(message, objects)) {
                                PercentComplete = progress
                            });
                        } else {
                            WriteProgress(new ProgressRecord(Math.Abs(activityId) + 1, "todo:activitylookup;", GetLocalizedMessage(message, objects)) {
                                ParentActivityId = (int)_parentProgressId,
                                PercentComplete = progress
                            });
                        }
                    }

                    // rather than wait on the result of the async WriteVerbose,
                    // we'll just return the stopping state.
                    return IsCancelled();
                });

                _localEventSource.Events += new CompleteProgress((activityId, isSuccessful) => {
                    if (IsInvocation) {
                        if (_parentProgressId == null) {
                            WriteProgress(new ProgressRecord(Math.Abs(activityId) + 1, "todo:activitylookup", "") {
                                PercentComplete = 100,
                                RecordType = ProgressRecordType.Completed
                            });
                        } else {
                            WriteProgress(new ProgressRecord(Math.Abs(activityId) + 1, "todo:activitylookup", "") {
                                ParentActivityId = (int)_parentProgressId,
                                PercentComplete = 100,
                                RecordType = ProgressRecordType.Completed
                            });
                        }
                    }
                    // rather than wait on the result of the async WriteVerbose,
                    // we'll just return the stopping state.
                    return IsCancelled();
                });

                _localEventSource.Events += new Warning((message, objects) => {
                    if (IsInvocation) {
                        WriteWarning(GetLocalizedMessage(message, objects));
                    }
                    // rather than wait on the result of the async WriteVerbose,
                    // we'll just return the stopping state.
                    return IsCancelled();
                });

                _localEventSource.Events += new Message(( message, objects) => {
                    // queue the message to run on the main thread.
                    if (IsInvocation) {
                        //  QueueMessage(() => Host.UI.WriteLine("{0}::{1}".format(code, message.formatWithIEnumerable(objects))));
                        // Message is going to go to the verbose channel
                        // and Verbose will only be output if VeryVerbose is true.
                        WriteVerbose(GetLocalizedMessage(message, objects));
                    }
                    // rather than wait on the result of the async WriteVerbose,
                    // we'll just return the stopping state.
                    return IsCancelled();
                });
            }
        }

        private string GetLocalizedMessage(string message, IEnumerable<object> objects) {

            // TODO: lookup message as a message code first.
            // TODO: ie: message = LookupMessage(message).formatWithIEnumerable(objects);

            return message.formatWithIEnumerable(objects);
        }

        public bool IsCancelled() {
            return Stopping || _failing;
        }

        private void AsyncRun(Func<bool> asyncAction) {
            InitLocalEventSource();
            _messages = new BlockingCollection<TaskCompletionSource<bool>>();

            // spawn the activity off in another thread.
            var task = IsInitialized ?
                Task.Factory.StartNew(asyncAction, TaskCreationOptions.LongRunning) :
                Task.Factory.StartNew(Init, TaskCreationOptions.LongRunning).ContinueWith(anteceedent => asyncAction());

            // when the task is done, mark the msg queue as complete
            task.ContinueWith(anteceedent => {
                if (_messages != null) {
                    _messages.CompleteAdding();
                }
            });

            // process the queue of messages back in the main thread so that they
            // can properly access the non-thread-safe-things in cmdlet
            foreach (var message in _messages.GetConsumingEnumerable()) {
                InvokeMessage(message);
            }
            _messages.Dispose();
            _messages = null;
        }

        private bool IsOverridden(string functionName) {
            return GetType().GetMethod(functionName).DeclaringType != typeof (AsyncCmdlet);
        }

        protected override sealed void BeginProcessing() {
            // let's not even bother doing all this if they didn't even
            // override the method.
            if (IsOverridden("BeginProcessingAsync")) {
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
            if (IsOverridden("EndProcessingAsync")) {
                // just before we kick stuff off, let's make sure we consume the dynamicaparmeters
                if (!_consumed) {
                    ConsumeDynamicParameters();
                    _consumed = true;
                }

                // just use our async/message pump to handle this activity
                AsyncRun(EndProcessingAsync);
            }

            // make sure that we mark progress complete.
            if (_parentProgressId != null) {
                MasterCompleteProgress();
            }
        }

        protected T CancelWhenStopped<T>(T cancellable) where T : ICancellable {
            _cancelWhenStopped.Add(cancellable);
            return cancellable;
        }

        protected override sealed void StopProcessing() {
            if (IsCancelled()) {
                foreach (var i in _cancelWhenStopped) {
                    if (i != null) {
                        i.Cancel();
                    }
                }
                _cancelWhenStopped.Clear();
                _cancelWhenStopped = null;
            }
            // let's not even bother doing all this if they didn't even
            // override the method.
            if (IsOverridden("StopProcessingAsync")) {
                // just use our async/message pump to handle this activity
                AsyncRun(StopProcessingAsync);
            }
            if (_parentProgressId != null) {
                MasterCompleteProgress();
            }
        }

        protected override sealed void ProcessRecord() {
            // let's not even bother doing all this if they didn't even
            // override the method.
            if (IsOverridden("ProcessRecordAsync")) {
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
                if (!IsCancelled()) {
                    base.WriteObject(obj);
                }
            });
        }

        public new Task<bool> WriteObject(object sendToPipeline, bool enumerateCollection) {
            return QueueMessage(() => {
                if (!IsCancelled()) {
                    base.WriteObject(sendToPipeline, enumerateCollection);
                }
            });
        }

        public new Task<bool> WriteProgress(ProgressRecord progressRecord) {
            return QueueMessage(() => {
                if (!IsCancelled()) {
                    base.WriteProgress(progressRecord);
                }
            });
        }

        public Task<bool> WriteMasterProgress(string activity, int percent, string format, params object[] args) {
            _parentProgressId = 0;
            return QueueMessage(() => base.WriteProgress(new ProgressRecord(0, activity, format.format(args)) {
                PercentComplete = percent
            }));
        }

        public Task<bool> MasterCompleteProgress() {
            _parentProgressId = null;
            return QueueMessage(() => base.WriteProgress(new ProgressRecord(0, "", "") {
                PercentComplete = 100,
                RecordType = ProgressRecordType.Completed
            }));
        }

        public new Task<bool> WriteWarning(string text) {
            if (!IsInvocation) {
                return false.AsResultTask();
            }
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
            return QueueMessage(() => {
#if DEBUG_BUILD
                NativeMethods.OutputDebugString(text);
#endif
                base.WriteVerbose(text);
            });
        }

        public new Task<bool> ShouldContinue(string query, string caption) {
            if (IsCancelled() || !IsInvocation) {
                return false.AsResultTask();
            }
            return QueueMessage(() => base.ShouldContinue(query, caption));
        }

        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "MYOB.")]
        public new Task<bool> ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll) {
            if (IsCancelled() || !IsInvocation) {
                return false.AsResultTask();
            }

            // todo: Uh, this is gonna be tricky!?
            return QueueMessage(() => base.ShouldContinue(query, caption));
        }

        public new Task<bool> ShouldProcess(string target) {
            if (IsCancelled() || !IsInvocation) {
                return false.AsResultTask();
            }

            return QueueMessage(() => base.ShouldProcess(target));
        }

        public new Task<bool> ShouldProcess(string target, string action) {
            if (IsCancelled() || !IsInvocation) {
                return false.AsResultTask();
            }

            return QueueMessage(() => base.ShouldProcess(target, action));
        }

        public new Task<bool> ShouldProcess(string verboseDescription, string verboseWarning, string caption) {
            if (IsCancelled() || !IsInvocation) {
                return false.AsResultTask();
            }

            return QueueMessage(() => base.ShouldProcess(verboseDescription, verboseWarning, caption));
        }

        public new Task<bool> ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason) {
            if (IsCancelled() || !IsInvocation) {
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

        public virtual Hashtable GetRequestOptions() {
            return null;
        }

        public virtual Hashtable GetRequestMetadata() {
            return null;
        }

   

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                _cancelWhenStopped.Clear();
                _cancelWhenStopped = null;

                // According to http://msdn.microsoft.com/en-us/library/windows/desktop/ms714463(v=vs.85).aspx
                // Powershell will dispose the cmdlet if it implements IDisposable.
                if (_localEventSource != null) {
                    _localEventSource.Dispose();
                }
                _localEventSource = null;

                if (_messages != null) {
                    _messages.Dispose();
                    _messages = null;
                }
            }
        }

        private class SystemMessageDispatcher : MarshalByRefObject {
            private readonly AsyncCmdlet _cmdlet;

            private InvokableDispatcher _dispatcher;

            public SystemMessageDispatcher(AsyncCmdlet cmdlet) {
                _cmdlet = cmdlet;
            }

            internal Callback Invoke {
                get {
                    return (_dispatcher ?? (_dispatcher = new InvokableDispatcher {
                        (IsCancelled)(() => _cmdlet.IsCancelled()),
                        (Warning)((s,  ie) => Event<Warning>.Raise(s, ie)),
                        (Message)((s, ie) => Event<Message>.Raise(s, ie)),
                        (Error)((s, ie) => Event<Error>.Raise(s, ie)),
                        (Debug)((s, ie) => Event<Debug>.Raise(s, ie)),
                        (Verbose)((s, ie) => Event<Verbose>.Raise(s, ie)),
                        (ExceptionThrown)((s, s1, ie) => Event<ExceptionThrown>.Raise(s, s1, ie)),
                        (Progress)((s, ie, p4, p5) => Event<Progress>.Raise(s, ie, p4, p5)),
                        (StartProgress)((p1,p2,p3) => Event<StartProgress>.Raise(p1,p2,p3)),
                        (CompleteProgress)((s, b)=> Event<CompleteProgress>.Raise(s, b)),
                    }));
                }
            }
        }
    }
}