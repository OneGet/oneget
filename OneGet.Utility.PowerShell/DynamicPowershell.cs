﻿// 
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
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Threading;
    using Extensions;

    public class DynamicPowershell : DynamicObject, IDisposable {
        private readonly ManualResetEvent _availableEvent;
        private readonly ManualResetEvent _opened;
        private readonly bool _runspaceIsOwned;
        private IDictionary<string, PSObject> _commands;
        private DynamicPowershellCommand _currentCommand;
        private Runspace _runspace;
        private bool _runspaceWasLikeThatWhenIGotHere;

        public DynamicPowershell() {
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.StateChanged += CheckIfRunspaceIsOpening;
            _runspace.AvailabilityChanged += CheckIfRunspaceIsAvailable;

            if (_runspace.RunspaceStateInfo.State == RunspaceState.BeforeOpen) {
                _runspace.OpenAsync();
            }
            _runspaceIsOwned = true;

            _availableEvent = new ManualResetEvent(Runspace.RunspaceAvailability == RunspaceAvailability.Available);
            _opened = new ManualResetEvent(Runspace.RunspaceStateInfo.State != RunspaceState.Opening);
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
                        _runspaceWasLikeThatWhenIGotHere = true;
                    }
                }
                return _runspace;
            }
        }

        public void Dispose() {
            _runspace.AvailabilityChanged -= CheckIfRunspaceIsAvailable;
            _runspace.StateChanged -= CheckIfRunspaceIsOpening;
            WaitForAvailable();

            if (_runspaceIsOwned) {
                if (_runspace != null) {
                    ((IDisposable)_runspace).Dispose();
                }
                _runspace = null;
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

            if (_runspaceWasLikeThatWhenIGotHere) {
                return Runspace.CreateNestedPipeline();
            }
            try {
                TestIfInNestedPipeline();
                return Runspace.CreatePipeline();
            } catch (Exception) {
                _runspaceWasLikeThatWhenIGotHere = true;
                return Runspace.CreateNestedPipeline();
            }
        }

        private void TestIfInNestedPipeline() {
            var pipeline = Runspace.CreatePipeline();
            //we're running a short command to verify that we're not in a nested pipeline
            pipeline.Commands.Add("get-alias");
            pipeline.Invoke();

            // PowerShell.Create();
        }

        private void AddCommandNames(IEnumerable<PSObject> cmdsOrAliases) {
            foreach (var item in cmdsOrAliases) {
                var cmdName = GetPropertyValue(item, "Name").ToLower(CultureInfo.CurrentCulture);
                var name = cmdName.Replace("-", "");
                if (name.Is()) {
                    _commands.AddOrSet(name, item);
                }
            }
        }

        private string GetPropertyValue(PSObject obj, string propName) {
            var property = obj.Properties.FirstOrDefault(prop => prop.Name == propName);
            return property != null ? property.Value.ToString() : null;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            result = NewTryInvokeMemberEx(binder.Name, binder.CallInfo.ArgumentNames.ToArray(), args);
            return result != null;
        }

        public DynamicPowershellResult NewTryInvokeMemberEx(string name, string[] argumentNames, params object[] args) {
            // make sure that we're clear to drop the last DPSC.
            WaitForAvailable();

            try {
                // command
                _currentCommand = new DynamicPowershellCommand(CreatePipeline(), new Command(GetPropertyValue(LookupCommand(name), "Name")));

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
            }
        }

        internal PSObject LookupCommand(string commandName) {
            var name = commandName.DashedToCamelCase().ToLower(CultureInfo.CurrentCulture);
            if (_commands == null || !_commands.ContainsKey(name)) {
                _commands = new Dictionary<string, PSObject>();

                using (var pipeline = CreatePipeline()) {
                    pipeline.Commands.Add("get-command");
                    AddCommandNames(pipeline.Invoke());
                }

                using (var pipeline = CreatePipeline()) {
                    pipeline.Commands.Add("get-alias");
                    AddCommandNames(pipeline.Invoke());
                }
            }
            var item = _commands.ContainsKey(name) ? _commands[name] : null;
            if (item == null) {
                throw new Exception("Unable to find appropriate cmdlet.");
            }
            return item;
        }

#if DEPRECATING
        
        public bool TryInvokeMemberEx(string name, out object result, string[] argumentNames, params object[] args) {
            // make sure that we're clear to drop the last DPSC.
            WaitForAvailable();

            try {
                // command
                _currentCommand = new DynamicPowershellCommand(CreatePipeline(), new Command(GetPropertyValue(LookupCommand(name), "Name")));

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
                result = _currentCommand.InvokeAsyncIfPossible();

                return true;
            } catch (Exception e) {
                e.Dump();
                result = null;
                return false;
            }
        }

// not used in this app. leftover from where Interface used this before.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Still in development!")]
        public DynamicPowershell(Runspace runspace) {
            _runspace = runspace;
            _runspace.StateChanged += CheckIfRunspaceIsOpening;
            _runspace.AvailabilityChanged += CheckIfRunspaceIsAvailable;

            if (_runspace.RunspaceAvailability == RunspaceAvailability.AvailableForNestedCommand ||
                _runspace.RunspaceAvailability == RunspaceAvailability.Busy) {
                _runspaceWasLikeThatWhenIGotHere = true;
            }

            _availableEvent = new ManualResetEvent(Runspace.RunspaceAvailability == RunspaceAvailability.Available);
            _opened = new ManualResetEvent(Runspace.RunspaceStateInfo.State != RunspaceState.Opening);

            _runspaceIsOwned = false;
        }
#endif
    }
}