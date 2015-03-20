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

namespace Microsoft.PackageManagement.Utility.PowerShell {
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Threading;
    using Extensions;

    public class DynamicPowershell : DynamicObject, IDisposable {
        private IDictionary<string, CommandInfo> _commands;
        private DynamicPowershellCommand _currentCommand;
        private Runspace _runspace;
        private readonly ManualResetEvent _availableEvent;
        private readonly ManualResetEvent _opened;
        private readonly bool _runspaceIsOwned;

        public DynamicPowershell() {
            _runspace = RunspaceFactory.CreateRunspace();

            _availableEvent = new ManualResetEvent(Runspace.RunspaceAvailability == RunspaceAvailability.Available);
            _opened = new ManualResetEvent(Runspace.RunspaceStateInfo.State != RunspaceState.Opening);

            _runspace.StateChanged += CheckIfRunspaceIsOpening;
            _runspace.AvailabilityChanged += CheckIfRunspaceIsAvailable;

            if (_runspace.RunspaceStateInfo.State == RunspaceState.BeforeOpen) {
                _runspace.OpenAsync();
            }
            _runspaceIsOwned = true;
        }

        public object this[string variableName] {
            get {
                WaitForAvailable();
                return _runspace.SessionStateProxy.GetVariable(variableName);
            }
            set {
                WaitForAvailable();
                _runspace.SessionStateProxy.SetVariable(variableName, value);
            }
        }

        private Runspace Runspace {
            get {
                lock (this) {
                    if (_runspace.RunspaceStateInfo.State == RunspaceState.BeforeOpen) {
                        _runspace.OpenAsync();
                    }

                    if (_runspace.RunspaceAvailability == RunspaceAvailability.AvailableForNestedCommand ||
                        _runspace.RunspaceAvailability == RunspaceAvailability.Busy) {
                    }
                }
                return _runspace;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Stop() {
            _currentCommand.Stop();
        }

        public virtual void Dispose(bool disposing) {
            if (disposing) {
                _runspace.AvailabilityChanged -= CheckIfRunspaceIsAvailable;
                _runspace.StateChanged -= CheckIfRunspaceIsOpening;

                // note: WHY WOULD THIS HAVE BEEN HERE?
                // WaitForAvailable();

                if (_currentCommand != null) {
                    _currentCommand.Dispose();
                    _currentCommand = null;
                }

                if (_runspaceIsOwned) {
                    if (_runspace != null) {
                        ((IDisposable)_runspace).Dispose();
                    }
                    _runspace = null;
                }

                if (_availableEvent != null) {
                    _availableEvent.Dispose();
                }
                if (_opened != null) {
                    _opened.Dispose();
                }
            }
        }

        public void WaitForAvailable() {
            _opened.WaitOne();
            _availableEvent.WaitOne();
        }

        private void CheckIfRunspaceIsAvailable(object sender, RunspaceAvailabilityEventArgs runspaceAvailabilityEventArgs) {
            if (runspaceAvailabilityEventArgs.RunspaceAvailability == RunspaceAvailability.Available) {
                _availableEvent.Set();
            } else {
                _availableEvent.Reset();
            }
        }

        private void CheckIfRunspaceIsOpening(object sender, RunspaceStateEventArgs runspaceStateEventArgs) {
            if (runspaceStateEventArgs.RunspaceStateInfo.State != RunspaceState.Opening) {
                _opened.Set();
            } else {
                _opened.Reset();
            }
        }

        internal Pipeline CreatePipeline() {
            WaitForAvailable();

            if (!_runspaceIsOwned) {
                try {
                    //we're running a short command to verify that we're not in a nested pipeline
                    var pipeline = Runspace.CreatePipeline();
                    pipeline.Commands.Add("get-module");
                    pipeline.Invoke();

                    // if we got here, we're not in a nested pipeline
                    return Runspace.CreatePipeline();
                } catch {
                    // must be in a nested pipeline.
                    return Runspace.CreateNestedPipeline();
                }
            }
            // if we own this runspace, it shouldn't ever need to be nested
            return Runspace.CreatePipeline();
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            if (binder == null) {
                throw new ArgumentNullException("binder");
            }

            result = NewTryInvokeMemberEx(binder.Name, binder.CallInfo.ArgumentNames.ToArray(), args);
            return result != null;
        }

        public DynamicPowershellResult ImportModule(string name, bool passThru) {
            var cmd = new DynamicPowershellCommand(CreatePipeline(), new Command("Import-Module"));
            cmd["Name"] = name;
            if (passThru) {
                cmd["PassThru"] = true;
            }

            return cmd.InvokeAsyncIfPossible();
        }

        public IEnumerable<PSModuleInfo> TestModuleManifest(string path) {
            var cmd = new DynamicPowershellCommand(CreatePipeline(), new Command("Test-ModuleManifest"));
            cmd["Path"] = path;

            // this call needs to be synchronous.
            return cmd.InvokeAsyncIfPossible().Select(each => each as PSModuleInfo).Where(each => each != null).ToArray();
        }

        public DynamicPowershellResult NewTryInvokeMemberEx(string name, string[] argumentNames, params object[] args) {
            // make sure that we're clear to drop the last DPSC.
            WaitForAvailable();

            try {
                // command
                // FYI: This is the most expensive part of the call (the first time.)
                // I've thought about up-fronting this, but it's probably just not worth the effort.
                _currentCommand = new DynamicPowershellCommand(CreatePipeline(), new Command(LookupCommand(name).Name));

                // parameters
                var unnamedCount = args.Length - argumentNames.Length;
                var namedArguments = argumentNames.Select((each, index) => new KeyValuePair<string, object>(each, args[index + unnamedCount]));
                _currentCommand.SetParameters(args.Take(unnamedCount), namedArguments);

#if DETAILED_DEBUG
                try {
                    Console.WriteLine("[DynamicInvoke] {0} {1}", _currentCommand.Command.CommandText,
                        _currentCommand.Command.Parameters != null && _currentCommand.Command.Parameters.Any()
                            ? _currentCommand.Command.Parameters.Select(each => (each.Name.Is() ? "-"+each.Name : " ") + " " + each.Value.ToString()).Aggregate((each, current) => current + " " + each) : "");
                } catch  {

                }
#endif
                // invoke
                return _currentCommand.InvokeAsyncIfPossible();
            } catch (Exception e) {
                e.Dump();
                return null;
            } finally {
            }
        }

        public IEnumerable<CommandInfo> GetCommand() {
            var cmd = new DynamicPowershellCommand(CreatePipeline(), new Command("Get-Command"));
            cmd["ListImported"] = true;
            cmd["CommandType"] = new[] {"Cmdlet", "Alias", "Function"};

            // this call needs to be synchronous.
            return cmd.InvokeAsyncIfPossible().Select(each => each as CommandInfo).Where(each => each != null).ToArray();
        }

        internal CommandInfo LookupCommand(string commandName) {
            var name = commandName.DashedToCamelCase();

            if (_commands == null || !_commands.ContainsKey(name)) {
                // if we haven't loaded the commands, or we can't find it (maybe you've imported a module)
                // we should load the commands again.
                _commands = GetCommand().ToDictionaryNicely(each => each.Name.Replace("-", ""), each => each, StringComparer.OrdinalIgnoreCase);
            }

            var item = _commands.ContainsKey(name) ? _commands[name] : null;

            if (item == null) {
                throw new Exception("MSG:UNABLE_TO_FIND_PS_COMMAND:{0}".format(commandName));
            }
            return item;
        }
    }
}
