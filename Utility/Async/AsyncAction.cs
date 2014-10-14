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

namespace Microsoft.OneGet.Utility.Async {
    using System;
    using System.Threading;

    public abstract class AsyncAction : MarshalByRefObject, IAsyncAction {
        private object _lock = new Object();
        private static readonly TimeSpan DefaultCallTimeout = TimeSpan.FromMinutes(60);
        private static readonly TimeSpan DefaultResponsiveness = TimeSpan.FromMinutes(1);
        protected readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly ManualResetEventSlim _completed = new ManualResetEventSlim(false);
        private ActionState _actionState;
        protected DateTime _callStart = DateTime.Now;
        private DisposalState _disposalState;
        protected Thread _invocationThread;
        private DateTime _lastActivity = DateTime.Now;

        private TimeSpan _responsiveness = DefaultResponsiveness;
        private TimeSpan _timeout = DefaultCallTimeout;
        private Timer _timer;

        protected AsyncAction() {
            _timer = new Timer(Signalled, this, -1, -1);
        }

        private TimeSpan TimeLeft {
            get {
                if (_actionState >= ActionState.Aborting) {
                    return TimeSpan.FromMilliseconds(-1);
                }

                if (_actionState >= ActionState.Cancelling) {
                    var timeToAbort = _responsiveness.Subtract(DateTime.Now.Subtract(_lastActivity));
                    return timeToAbort < TimeSpan.Zero ? TimeSpan.Zero : timeToAbort;
                }

                var timeToRespond = _responsiveness.Subtract(DateTime.Now.Subtract(_lastActivity));
                if (timeToRespond < TimeSpan.Zero) {
                    timeToRespond = TimeSpan.Zero;
                }

                var timeToTimeout = _timeout.Subtract(DateTime.Now.Subtract(_callStart));
                if (timeToTimeout < TimeSpan.Zero) {
                    timeToTimeout = TimeSpan.Zero;
                }
                return timeToRespond < timeToTimeout ? timeToRespond : timeToTimeout;
            }
        }

        public event Action OnComplete;
        public event Action OnCancel;
        public event Action OnAbort;

        public virtual void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Cancel() {
            // if it's already done, this is a no-op
            lock (_lock) {
                if (_actionState >= ActionState.Cancelling) {
                    return;
                }
                _actionState = ActionState.Cancelling;
            }

            // activate the cancellation token for those who are watching for that.
#if DEEP_DEBUG
            Console.WriteLine("CANCELLING {0} {1}", _invocationThread.Name, DateTime.Now.Subtract(_callStart).TotalSeconds);
#endif
            _cancellationTokenSource.Cancel();
#if DEEP_DEBUG
            Console.WriteLine("Passed Cancel Token {0} {1}", _invocationThread.Name, DateTime.Now.Subtract(_callStart).TotalSeconds);
#endif
            // actively tell anyone who is listening that we're trying to cancel this.
            if (OnCancel != null) {
                OnCancel();
            }
#if DEEP_DEBUG
            Console.WriteLine("Waiting to complete cancellation for {0} {1}", _invocationThread.Name, DateTime.Now.Subtract(_callStart).TotalSeconds);
#endif
            lock (_lock) {
#if DEEP_DEBUG
                Console.WriteLine("CANCELLED {0} {1}", _invocationThread.Name, DateTime.Now.Subtract(_callStart).TotalSeconds);
#endif
                if (_actionState < ActionState.Canceled) {
                    _actionState = ActionState.Canceled;
                }
            }
        }

        public virtual void Abort() {
            // make sure we're at least cancelled first!
            Cancel();

            // if it's already done, this is a no-op
            lock (_lock) {
                if (_actionState >= ActionState.Aborting) {
                    return;
                }
                _actionState = ActionState.Aborting;
            }

            // we have no need left for this.
            DisposeTimer();

            // notify any listeners that we're about to kill this.
            if (OnAbort != null) {
                OnAbort();
            }

            // now, foreably kill this thing
            if (_invocationThread.IsAlive) {
                _invocationThread.Abort();
            }

            lock (_lock) {
                if (_actionState < ActionState.Aborted) {
                    _actionState = ActionState.Aborted;
                }
            }

            // and make sure this is marked as complete
            Complete();
        }

        public WaitHandle CompleteEvent {
            get {
                return _completed.WaitHandle;
            }
        }

        public TimeSpan Timeout {
            get {
                return _timeout;
            }
            set {
                _timeout = value;
                ResetTimer();
            }
        }

        public TimeSpan Responsiveness {
            get {
                return _responsiveness;
            }
            set {
                _responsiveness = value;
                ResetTimer();
            }
        }

        public bool IsCanceled {
            get {
                return _cancellationTokenSource.IsCancellationRequested;
            }
        }

        public bool IsAborted {
            get {
                return _actionState == ActionState.Aborting || _actionState == ActionState.Aborted;
            }
        }

        public bool IsCompleted {
            get {
                return _completed.IsSet;
            }
        }

        public virtual void Dispose(bool disposing) {
#if DEEP_DEBUG
            Console.WriteLine("START DISPOSING OF TASK {0} {1}", _invocationThread.Name, DateTime.Now.Subtract(_callStart).TotalSeconds);
#endif

            lock (_lock) {
                // make sure this kind of thing doesn't happen twice.
                if (_disposalState > DisposalState.None) {
                    return;
                }
                _disposalState = DisposalState.Disposing;
            }

            if (disposing) {
                // Ensure we're cancelled first.
                Cancel();

                if (_actionState < ActionState.Aborting) {
                    // if we're not already in the process of aborting, we'll kick that off in a few seconds.

#if DEEP_DEBUG
                    Console.WriteLine("GIVING 5 seconds to die for TASK {0} {1}", _invocationThread.Name, DateTime.Now.Subtract(_callStart).TotalSeconds);
#endif
                    _timer.Change(5000, -1);
                } else {
                    // stop timer activity
                    DisposeTimer();
                }

                // for all intents, we're completed...even if the abort will run after this.
                _completed.Set();
                _disposalState = DisposalState.Disposed;
                if (_actionState >= ActionState.Completed)
                {
                    _completed.Dispose();
                    _cancellationTokenSource.Dispose();
                }
            }
#if DEEP_DEBUG
            Console.WriteLine("DONE TASK {0} {1}", _invocationThread.Name, DateTime.Now.Subtract(_callStart).TotalSeconds);
#endif
        }

        private void DisposeTimer() {
            lock (_lock) {
                if (_timer != null) {
                    _timer.Change(-1, -1);
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }

        protected virtual void Complete() {
            lock (_lock) {
                if (_actionState == ActionState.Completed) {
                    return;
                }
                _actionState = ActionState.Completed;
            }

            DisposeTimer();

            if (OnComplete != null) {
                OnComplete();
            }
            _completed.Set();
        }

        private void Signalled(object obj) {
            if (_actionState > ActionState.Aborting) {
                // we don't have anything to do here.
                return;
            }

            if (_actionState < ActionState.Cancelling) {
#if DEEP_DEBUG
                Console.WriteLine("Signalled to Cancel ================================== {0} : {1}", _invocationThread.Name, DateTime.Now.Subtract(_callStart).TotalSeconds);
#endif
                Cancel();
                return;
            }

            if (_actionState == ActionState.Cancelling) {
                // we were in a cancelled state when we noticed the timer hit zero.
#if DEEP_DEBUG
                Console.WriteLine("ARE WE SUPPOSED TO ABORT HERE? ================================== {0} : {1}", _invocationThread.Name, DateTime.Now.Subtract(_callStart).TotalSeconds);
#endif
                return;
            }

            if (_actionState == ActionState.Canceled) {
#if DEEP_DEBUG
                Console.WriteLine("Signalled to Abort ================================== {0} : {1}", _invocationThread.Name, DateTime.Now.Subtract(_callStart).TotalSeconds);
#endif
                // we were in a cancelled state when we noticed the timer hit zero.
                Abort();
            }
        }

        protected void Activity() {
            _lastActivity = DateTime.Now;
            ResetTimer();
        }

        protected void StartCall() {
            _callStart = DateTime.Now;
            ResetTimer();
        }

        private void ResetTimer() {
            lock (_lock) {
                if (_actionState <= ActionState.Canceled && _timer != null) {
                    _timer.Change(TimeLeft, TimeSpan.FromMilliseconds(-1));
                }
            }
        }

        private enum ActionState {
            None,
            Called,
            Cancelling,
            Canceled,
            Aborting,
            Aborted,
            Completed,
        }

        private enum DisposalState {
            None,
            Disposing,
            Disposed
        }
    }
}