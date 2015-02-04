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
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Threading;
    using Extensions;

    internal class DynamicPowershellCommand : IDisposable {
        private readonly Command _command;
        private readonly ManualResetEvent _endOfPipelines = new ManualResetEvent(false);
        private Pipeline _commandPipeline;
        private DynamicPowershellResult _result = new DynamicPowershellResult();

        internal DynamicPowershellCommand(Pipeline pipeline, Command command) {
            _commandPipeline = pipeline;
            _command = command;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Stop() {
            try {
                if (_commandPipeline != null && _commandPipeline.PipelineStateInfo.State == PipelineState.Running) {
                    _commandPipeline.StopAsync();
                }
            } catch {
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_commandPipeline != null) {
                    ((IDisposable)_commandPipeline).Dispose();
                }
                _commandPipeline = null;

                // we're not really interesting in disposing of this object since someone else
                // might have it, and it's ok.
                _result = null;
                if (_result != null) {
                    _result.Dispose();
                }

                if (_endOfPipelines != null) {
                    _endOfPipelines.Dispose();
                }
            }
        }

        internal void SetParameters(IEnumerable<object> unnamedArguments, IEnumerable<KeyValuePair<string, object>> namedArguments) {
            foreach (var arg in unnamedArguments) {
                if (arg is DynamicPowershellResult) {
                    // pipelining arguments into command.
                    _commandPipeline.Input.Write(arg, true);
                    continue;
                }
                _command.Parameters.Add(null, arg);
            }
            foreach (var arg in namedArguments) {
                _command.Parameters.Add(arg.Key, arg.Value);
            }
        }

        internal object this[string index] {
            set {
                _command.Parameters.Add(index, value);
            }
            get {
                return _command.Parameters.Where(p => p.Name.EqualsIgnoreCase(index)).Select(p => p.Value).FirstOrDefault();
            }
        }

        private void CheckForPipelineCompletion() {
            // if we're finished draining both pipes.
            if (_commandPipeline.Error.EndOfPipeline && _commandPipeline.Output.EndOfPipeline) {
                _endOfPipelines.Set();
            }
        }

        internal DynamicPowershellResult InvokeAsyncIfPossible() {
            _commandPipeline.Commands.Add(_command);
            _commandPipeline.Input.Close();

            _commandPipeline.Output.DataReady += (sender, args) => {
                if (_result.IsCompleted) {
                    throw new Exception("MSG:ATTEMPTED_TO_ADD_TO_COMPLETED_COLLECTION");
                }

                var items = _commandPipeline.Output.NonBlockingRead();
                foreach (var item in items) {
                    _result.Add(item.ImmediateBaseObject);
                }
                _result.Started();

                CheckForPipelineCompletion();
            };

            _commandPipeline.Error.DataReady += (sender, args) => {
                if (_result.Errors.IsCompleted) {
                    throw new Exception("MSG:ATTEMPTED_TO_ADD_TO_COMPLETED_COLLECTION");
                }

                var items = _commandPipeline.Error.NonBlockingRead();
                foreach (var item in items) {
                    if (item is PSObject) {
                        var record = (item as PSObject).ImmediateBaseObject;
                        if (record is ErrorRecord) {
                            _result.ContainsErrors = true;
                            _result.Errors.Add(record as ErrorRecord);
                        }
                    }
                }

                if (_commandPipeline.PipelineStateInfo.State == PipelineState.Failed) {
                    _result.ContainsErrors = true;

                    var cie = _commandPipeline.PipelineStateInfo.Reason as RuntimeException;
                    if (cie != null && cie.ErrorRecord != null) {
                        _result.Errors.Add(cie.ErrorRecord);
                    } else {
                        _result.Errors.Add(new ErrorRecord(_commandPipeline.PipelineStateInfo.Reason, "Unknown Exception type [{0}]".format(_commandPipeline.PipelineStateInfo.Reason.GetType()), ErrorCategory.NotSpecified, null));
                    }
                }
                CheckForPipelineCompletion();
            };

            _commandPipeline.StateChanged += (x, y) => {
                switch (_commandPipeline.PipelineStateInfo.State) {
                    case PipelineState.NotStarted:
                        break;

                        // case PipelineState.Disconnected:

                    case PipelineState.Completed:
                    case PipelineState.Stopped:
                    case PipelineState.Failed:
                        // make sure we're done with both pipes.
                        _endOfPipelines.WaitOne();

                        // the last error was a terminating error
                        _result.LastIsTerminatingError = (_commandPipeline.PipelineStateInfo.State == PipelineState.Failed);

                        // mark the result object complete.
                        _result.Completed();

                        // clean up the pipeline, we're done with it.
                        if (_commandPipeline != null) {
                            ((IDisposable)_commandPipeline).Dispose();
                        }
                        _commandPipeline = null;
                        break;

                    case PipelineState.Stopping:
                        break;

                    case PipelineState.Running:
                        break;
                }
            };

            if (_commandPipeline.IsNested) {
                // goofy-powershell doesn't let nested pipelines async.
                _commandPipeline.Invoke();
            } else {
                _commandPipeline.InvokeAsync();
                _result.WaitForStart();
            }

            return _result;
        }
    }
}