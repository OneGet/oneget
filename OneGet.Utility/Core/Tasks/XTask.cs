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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensions;

    public static class XTask {
        // necessary evil: I needed the parent task and the current task to do what I did here.
        // since they were private, I went around them and accessed the fields via reflection.
        // if you have a better idea, I'm all for it.
        private static readonly FieldInfo _parentTaskField = typeof (Task).GetField("m_parent", BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);
        private static readonly PropertyInfo _currentTaskProperty = typeof (Task).GetProperty("InternalCurrent", BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Static);
        private static readonly IDictionary<Task, List<Delegate>> _tasks = new Dictionary<Task, List<Delegate>>();
        private static readonly IDictionary<Task, Task> _parentTasks = new Dictionary<Task, Task>();
        private static readonly List<Delegate> _nullTaskDelegates = new List<Delegate>();

        public static Task<T> AsResultTask<T>(this T result) {
            var x = new TaskCompletionSource<T>(TaskCreationOptions.AttachedToParent);
            x.SetResult(result);
            return x.Task;
        }

        public static Task<T> AsCanceledTask<T>(this T result) {
            var x = new TaskCompletionSource<T>(TaskCreationOptions.AttachedToParent);
            x.SetCanceled();
            return x.Task;
        }

        private static bool IsTaskReallyCompleted(Task task) {
            if (!task.IsCompleted) {
                return false;
            }

            return !(from child in _parentTasks.Keys where _parentTasks[child] == task && !IsTaskReallyCompleted(child) select child).Any();
        }

        public static void Collect() {
            lock (_tasks) {
                var completedTasks = (from t in _tasks.Keys where IsTaskReallyCompleted(t) select t).ToArray();
                foreach (var t in completedTasks) {
                    _tasks.Remove(t);
                }
            }

            lock (_parentTasks) {
                var completedTasks = (from t in _parentTasks.Keys where IsTaskReallyCompleted(t) select t).ToArray();
                foreach (var t in completedTasks) {
                    _parentTasks.Remove(t);
                }
            }
        }

        /// <summary>
        ///     This associates a child task with the parent task. This isn't necessary (and will have no effect) when the child
        ///     task is created with AttachToParent in the creation/continuation options, but it does take a few cycles to validate
        ///     that there is actually a parent, so don't call this when not needed.
        /// </summary>
        /// <param name="task"> </param>
        /// <returns> </returns>
        public static Task AutoManage(this Task task) {
            if (task == null) {
                return null;
            }

            // if the task isn't associated with it's parent
            // we can insert a 'cheat'
            if (task.GetParentTask() == null) {
                lock (_parentTasks) {
                    var currentTask = CurrentExecutingTask;
                    if (currentTask != null) {
                        // the given task isn't attached to the parent.
                        // we can fake out attachment, by using the current task
                        _parentTasks.Add(task, currentTask);
                    }
                }
            }
            return task;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "No. I want the generic type passed back.")]
        public static Task<T> AutoManage<T>(this Task<T> task) {
            AutoManage((Task)task);
            return task;
        }

        private static Task _rootTask = new TaskCompletionSource<int>().Task;

        internal static Task CurrentExecutingTask {
            get {
                return (_currentTaskProperty.GetValue(null, null) as Task); // ?? _rootTask;
            }
        }

        internal static Task GetParentTask(this Task task) {
            if (task == null) {
                return null;
            }

            return _parentTaskField.GetValue(task) as Task ?? (_parentTasks.ContainsKey(task) ? _parentTasks[task] : null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Still in development.")]
        internal static Task ParentTask {
            get {
                return CurrentExecutingTask.GetParentTask();
            }
        }

        /// <summary>
        ///     Gets the message handler.
        /// </summary>
        /// <param name="task"> The task to get the message handler for. </param>
        /// <param name="eventDelegateHandlerType"> the delegate handler class </param>
        /// <returns> A delegate handler; null if there isn't one. </returns>
        /// <remarks>
        /// </remarks>
        internal static Delegate GetEventHandler(this Task task, Type eventDelegateHandlerType) {
            if (task == null) {
                return Delegate.Combine((from handlerDelegate in _nullTaskDelegates where eventDelegateHandlerType.IsInstanceOfType(handlerDelegate) select handlerDelegate).ToArray());
            }

            // if the current task has an entry.
            if (_tasks.ContainsKey(task)) {
                var result = Delegate.Combine((from handler in _tasks[task] where handler.GetType().IsAssignableFrom(eventDelegateHandlerType) select handler).ToArray());
                return Delegate.Combine(result, GetEventHandler(task.GetParentTask(), eventDelegateHandlerType));
            }

            // otherwise, check with the parent.
            return GetEventHandler(task.GetParentTask(), eventDelegateHandlerType);
        }

        internal static Delegate AddEventHandler(this Task task, Delegate handler) {
            if (handler == null) {
                return null;
            }

            if (task != null) {
                // if this is in a task, sometimes it's possible to get ahead of the parent
                // This really should be fixed. Not even sure it's still needed at this point.
                // todo: we need a better way for a task to wait for it's parent to be populated.
                for (var count = 10; count > 0 && task.GetParentTask() == null; count--) {
                    Thread.Sleep(3); // yeild for a bit
                }
            }

            lock (_tasks) {
                if (task == null) {
                    _nullTaskDelegates.Insert(0, handler);
                    // _nullTaskDelegates.Add(handler);
                } else {
                    if (!_tasks.ContainsKey(task)) {
                        _tasks.Add(task, new List<Delegate>());
                    }
                    //_tasks[task].Add(handler);
                    _tasks[task].Insert(0, handler);
                }
            }
            return handler;
        }

        internal static void RemoveEventHandler(this Task task, Delegate handler) {
            if (handler != null) {
                lock (_tasks) {
                    if (task == null) {
                        if (_nullTaskDelegates.Contains(handler)) {
                            _nullTaskDelegates.Remove(handler);
                        }
                    } else {
                        if (_tasks.ContainsKey(task) && _tasks[task].Contains(handler)) {
                            _tasks[task].Remove(handler);
                        }
                    }
                }
            }
        }

        public static void Iterate<TResult>(this TaskCompletionSource<TResult> tcs, IEnumerable<Task> asyncIterator) {
            var enumerator = asyncIterator.GetEnumerator();
            Action<Task> recursiveBody = null;
            recursiveBody = completedTask => {
                if (completedTask != null && completedTask.IsFaulted) {
                    tcs.TrySetException(completedTask.Exception.InnerExceptions);
                    enumerator.Dispose();
                } else if (enumerator.MoveNext()) {
                    enumerator.Current.ContinueWith(recursiveBody, TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously);
                } else {
                    enumerator.Dispose();
                }
            };
            recursiveBody(null);
        }
    }
}
