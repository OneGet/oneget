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

namespace Microsoft.OneGet.Core {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Threading;
    using OneGet.Core.Extensions;

    public class DynamicPowershell : DynamicObject, IDisposable {
        private readonly bool _runspaceIsOwned;
        private IDictionary<string, PSObject> _commands;
        private DynamicPowershellCommand _currentCommand;
        private Runspace _runspace;
        private bool _runspaceWasLikeThatWhenIGotHere;

        public DynamicPowershell() {
            _runspace = RunspaceFactory.CreateRunspace();
            if (_runspace.RunspaceStateInfo.State == RunspaceState.BeforeOpen) {
                _runspace.OpenAsync();
            }
            _runspaceIsOwned = true;
        }

        // not used in this app. leftover from where Interface used this before.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Still in development!")]
        public DynamicPowershell(Runspace runspace) {
            _runspace = runspace;

            if (_runspace.RunspaceAvailability == RunspaceAvailability.AvailableForNestedCommand ||
                _runspace.RunspaceAvailability == RunspaceAvailability.Busy) {
                _runspaceWasLikeThatWhenIGotHere = true;
            }

            _runspaceIsOwned = false;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Still in development!")]
        public object this[string variableName] {
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Still in development!")]
            get {
                EnsureRunspaceAvailable();
                return _runspace.SessionStateProxy.GetVariable(variableName);
            }
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Still in development!")]
            set {
                EnsureRunspaceAvailable();
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
            Wait();

            if (_runspaceIsOwned) {
                if (_runspace != null) {
                    ((IDisposable)_runspace).Dispose();
                }
                _runspace = null;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Still in development!")]
        internal void SetVariable(string variableName, object value) {
            _runspace.SessionStateProxy.SetVariable(variableName, value);
        }

        private void EnsureRunspaceAvailable() {
            while (Runspace.RunspaceStateInfo.State == RunspaceState.Opening) {
                // sigh. sometimes, the runspace is still opening when we get here.
                // we can't proceed until it's ready.
                // todo: think of a better way to handle this.
                // IIRC, this happens because Interface opened the runspace async'ly to get
                // it going earlier with the hopes that it can do it's startup before
                // Interface actually get around to using it.
                Thread.Sleep(3);
            }
        }

        internal Pipeline CreatePipeline() {
            EnsureRunspaceAvailable();

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

            var ps = PowerShell.Create();
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

        public bool TryInvokeMemberEx(string name, out object result, string[] argumentNames,  params object[] args ) {
            // make sure that we're clear to drop the last DPSC.
            Wait();

            try {
                
                // command
                _currentCommand = new DynamicPowershellCommand(CreatePipeline()) {
                    Command = new Command(GetPropertyValue(LookupCommand(name), "Name"))
                };

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
            }
            catch (Exception e) {
                e.Dump();
                result = null;
                return false;
            }
        }


        public DynamicPowershellResult NewTryInvokeMemberEx(string name, string[] argumentNames, params object[] args) {
            // make sure that we're clear to drop the last DPSC.
            Wait();

            try {
                // command
                _currentCommand = new DynamicPowershellCommand(CreatePipeline()) {
                    Command = new Command(GetPropertyValue(LookupCommand(name), "Name"))
                };

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
            }
            catch (Exception e) {
                e.Dump();
                return null;
            }
        }

        internal void Wait() {
            lock (this) {
                if (_currentCommand != null) {
                    _currentCommand.Wait();
                    _currentCommand = null;
                }
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
    }
}