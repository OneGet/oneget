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

namespace Microsoft.PackageManagement.Internal.Utility.Platform
{
    using Collections;
    using Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    /// <summary>
    ///     This is a wrapper around the Process class to provide easy access to asynchronous
    ///     stdout/stderr streams.
    ///     TODO: add support for a cancellation token to kill the process when cancelled.
    /// </summary>
    internal class AsyncProcess : IDisposable
    {
        protected Process _process;
        private BlockingCollection<string> _stdError = new BlockingCollection<string>();
        private ManualResetEvent _stdErrStarted = new ManualResetEvent(false);
        private BlockingCollection<string> _stdOut = new BlockingCollection<string>();
        private ManualResetEvent _stdOutStarted = new ManualResetEvent(false);

        protected AsyncProcess(Process process)
        {
            _process = process;
        }

        public IEnumerable<string> StandardError
        {
            get
            {
                _stdErrStarted.WaitOne();
                return _stdError;
            }
        }

        public IEnumerable<string> StandardOutput
        {
            get
            {
                _stdOutStarted.WaitOne();
                return _stdOut;
            }
        }

        public int ExitCode => _process.ExitCode;

        public bool HasExited => _process.HasExited;

        public DateTime ExitTime => _process.ExitTime;

#if !CORECLR

        public IntPtr Handle => _process.Handle;

        public int HandleCount => _process.HandleCount;

        public IntPtr MainWindowHandle => _process.MainWindowHandle;

        public string MainWindowTitle => _process.MainWindowTitle;

        public ISynchronizeInvoke SynchronizingObject => _process.SynchronizingObject;

        public bool Responding => _process.Responding;

        public bool WaitForInputIdle(int milliseconds)
        {
            return _process.WaitForInputIdle(milliseconds);
        }

        public bool WaitForInputIdle()
        {
            return WaitForInputIdle(-1);
        }

        public bool CloseMainWindow()
        {
            return _process.CloseMainWindow();
        }

#endif

        public void Close()
        {
#if !CORECLR
            _process.Close();
#else
            Dispose();
#endif
        }

        public int Id => _process.Id;

        public string MachineName => _process.MachineName;

        public ProcessModule MainModule => _process.MainModule;

        public IntPtr MaxWorkingSet => _process.MaxWorkingSet;

        public IntPtr MinWorkingSet => _process.MinWorkingSet;

        public ProcessModuleCollection Modules => _process.Modules;

        public long NonpagedSystemMemorySize64 => _process.NonpagedSystemMemorySize64;

        public long PagedMemorySize64 => _process.PagedMemorySize64;

        public long PagedSystemMemorySize64 => _process.PagedSystemMemorySize64;

        public long PeakPagedMemorySize64 => _process.PeakPagedMemorySize64;

        public long PeakWorkingSet64 => _process.PeakWorkingSet64;

        public long PeakVirtualMemorySize64 => _process.PeakVirtualMemorySize64;

        public bool PriorityBoostEnabled
        {
            get => _process.PriorityBoostEnabled;
            set => _process.PriorityBoostEnabled = value;
        }

        public ProcessPriorityClass PriorityClass
        {
            get => _process.PriorityClass;
            set => _process.PriorityClass = value;
        }

        public long PrivateMemorySize64 => _process.PrivateMemorySize64;

        public TimeSpan PrivilegedProcessorTime => _process.PrivilegedProcessorTime;

        public string ProcessName => _process.ProcessName;

        public IntPtr ProcessorAffinity
        {
            get => _process.ProcessorAffinity;
            set => _process.ProcessorAffinity = value;
        }

        public int SessionId => _process.SessionId;

        public DateTime StartTime => _process.StartTime;

        public ProcessThreadCollection Threads => _process.Threads;

        public TimeSpan TotalProcessorTime => _process.TotalProcessorTime;

        public TimeSpan UserProcessorTime => _process.UserProcessorTime;

        public long VirtualMemorySize64 => _process.VirtualMemorySize64;

        public bool EnableRaisingEvents
        {
            get => _process.EnableRaisingEvents;
            set => _process.EnableRaisingEvents = value;
        }

        public long WorkingSet64 => _process.WorkingSet64;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static AsyncProcess Start(ProcessStartInfo startInfo)
        {
            return Start(startInfo, null);
        }

        private void ErrorDataReceived(object sender, DataReceivedEventArgs args)
        {
            _stdErrStarted.Set();
            if (_stdError != null)
            {
                _stdError.Add(args.Data ?? string.Empty);
            }
            else
            {
                Console.WriteLine("Attempting to add when collection is null!!!!! (stdErr)");
                _process.ErrorDataReceived -= ErrorDataReceived;
            }
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            _stdOutStarted.Set();
            if (_stdOut != null)
            {
                _stdOut.Add(args.Data ?? string.Empty);
            }
            else
            {
                Console.WriteLine("Attempting to add when collection is null!!!!! (stdOut)");
                _process.OutputDataReceived -= OutputDataReceived;
            }
        }

        private void ProcessExited(object sender, EventArgs args)
        {
            WaitForExit();
            _stdError.CompleteAdding();
            _stdOut.CompleteAdding();
            _stdErrStarted.Set();
            _stdOutStarted.Set();
        }

        public static AsyncProcess Start(ProcessStartInfo startInfo, IDictionary environment)
        {
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            bool redirecting = true;

#if !CORECLR
            if (AdminPrivilege.IsElevated)
            {
                startInfo.Verb = "";
            }
            else
            {
                if ("runas".EqualsIgnoreCase(startInfo.Verb))
                {
                    redirecting = false;
                    startInfo.UseShellExecute = true;
                    startInfo.RedirectStandardError = false;
                    startInfo.RedirectStandardOutput = false;
                }
            }
#endif

            if (environment != null)
            {
                foreach (object i in environment.Keys)
                {
#if !CORECLR
                    startInfo.EnvironmentVariables[(string)i] = (string)environment[i];
#else
                    startInfo.Environment[(string)i] = (string)environment[i];
#endif
                }
            }

            AsyncProcess result = new AsyncProcess(new Process
            {
                StartInfo = startInfo
            });

            result._process.EnableRaisingEvents = true;

            // set up std* access
            if (redirecting)
            {
                result._process.ErrorDataReceived += result.ErrorDataReceived;
                result._process.OutputDataReceived += result.OutputDataReceived;
            }
            result._process.Exited += result.ProcessExited;
            result._process.Start();

            if (redirecting)
            {
                result._process.BeginErrorReadLine();
                result._process.BeginOutputReadLine();
            }
            return result;
        }

        public static AsyncProcess Start(string fileName)
        {
            return Start(new ProcessStartInfo
            {
                FileName = fileName
            });
        }

        public static AsyncProcess Start(string fileName, IDictionary environment)
        {
            return Start(new ProcessStartInfo
            {
                FileName = fileName
            }, environment);
        }

        public static AsyncProcess Start(string fileName, string parameters, IDictionary environment)
        {
            return Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = parameters
            }, environment);
        }

        public static AsyncProcess Start(string fileName, string parameters)
        {
            return Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = parameters
            });
        }

        public void Kill()
        {
            _process.Kill();
        }

        public bool WaitForExit(int milliseconds)
        {
            return _process.WaitForExit(milliseconds);
        }

        public void WaitForExit()
        {
            WaitForExit(-1);
        }

        public event EventHandler Exited
        {
            add
            {
                _process.Exited += value;
            }
            remove
            {
                _process.Exited -= value;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_process != null)
                {
                    if (!_process.HasExited)
                    {
                        // this object is being disposed, yet the process isn't finished.
                        Console.WriteLine("Killing AsyncProcess");
                        _process.Kill();
                    }
                    ((IDisposable)_process).Dispose();
                }
                _process = null;
                if (_stdOut != null)
                {
                    ((IDisposable)_stdOut).Dispose();
                }
                _stdOut = null;
                if (_stdError != null)
                {
                    ((IDisposable)_stdError).Dispose();
                }
                _stdError = null;
                if (_stdOutStarted != null)
                {
                    _stdOutStarted.Dispose();
                }
                _stdOutStarted = null;
                if (_stdErrStarted != null)
                {
                    _stdErrStarted.Dispose();
                }
                _stdErrStarted = null;
            }
        }

        public static void EnterDebugMode()
        {
            Process.EnterDebugMode();
        }

        public static void LeaveDebugMode()
        {
            Process.LeaveDebugMode();
        }

        public static AsyncProcess GetProcessById(int processId, string machineName)
        {
            return new AsyncProcess(Process.GetProcessById(processId, machineName));
        }

        public static AsyncProcess GetProcessById(int processId)
        {
            return new AsyncProcess(Process.GetProcessById(processId));
        }

        public static AsyncProcess[] GetProcessesByName(string processName)
        {
            return Process.GetProcessesByName(processName).Select(each => new AsyncProcess(each)).ToArray();
        }

        public static AsyncProcess[] GetProcessesByName(string processName, string machineName)
        {
            return Process.GetProcessesByName(processName, machineName).Select(each => new AsyncProcess(each)).ToArray();
        }

        public static AsyncProcess[] GetProcesses()
        {
            return Process.GetProcesses().Select(each => new AsyncProcess(each)).ToArray();
        }

        public static AsyncProcess[] GetProcesses(string machineName)
        {
            return Process.GetProcesses(machineName).Select(each => new AsyncProcess(each)).ToArray();
        }

        public static AsyncProcess GetCurrentProcess()
        {
            return new AsyncProcess(Process.GetCurrentProcess());
        }

        public void Refresh()
        {
            _process.Refresh();
        }
    }
}