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
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Threading;

    public class DynamicPowershellCommand : IDisposable {
        internal Command Command;
        internal Pipeline CommandPipeline;
        internal DynamicPowershellResult Result = new DynamicPowershellResult();

        internal DynamicPowershellCommand(Pipeline pipeline) {
            CommandPipeline = pipeline;
        }

        public void Dispose() {
            Dispose(true); 
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (CommandPipeline != null) {
                    ((IDisposable)CommandPipeline).Dispose();
                }
                CommandPipeline = null;
                
                Result = null;
                
                // we're not really interesting in disposing of this object since someone else
                // might have it, and it's ok. 
                if (Result != null) {
                    Result.Dispose();
                }
            }
        }

        private void DropPipeline() {
            lock (this) {
                if (CommandPipeline != null ) {
                    ((IDisposable)CommandPipeline).Dispose();
                }
                CommandPipeline = null;
            }
        }

        internal void Wait() {
            lock (this) {
                if (Result != null) {
                    Result.CompletedEvent.WaitOne();
                    Result = null;
                }
            }
        }

        internal void SetParameters(IEnumerable<object> unnamedArguments, IEnumerable<KeyValuePair<string, object>> namedArguments) {
            foreach (var arg in unnamedArguments) {
                Command.Parameters.Add(null, arg);
            }
            foreach (var arg in namedArguments) {
                Command.Parameters.Add(arg.Key, arg.Value);
            }
        }

        internal DynamicPowershellResult InvokeAsyncIfPossible() {
            

            CommandPipeline.Commands.Add(Command);
            CommandPipeline.Input.Close();

            CommandPipeline.Output.DataReady += (sender, args) => {
                lock (Result) {
                    if (Result.Output.IsCompleted) {
                        throw new Exception("Attempted to add to completed collection");
                    }

                    var items = CommandPipeline.Output.NonBlockingRead();
                    foreach (var item in items) {
                        Result.Output.Add(item.ImmediateBaseObject);
                    }
                    Result.StartedEvent.Set();
                }
            };

            
            CommandPipeline.Error.DataReady += (sender, args) => {
                lock (Result.Errors) {
                    if (Result.Errors.IsCompleted) {
                        throw new Exception("Attempted to add to completed collection");
                    }
                    Result.IsFailing = true;

                    var items = CommandPipeline.Error.NonBlockingRead();
                    foreach (var item in items) {
                        if (item is PSObject) {
                            var record = (item as PSObject).ImmediateBaseObject;
                            if (record is ErrorRecord) {
                                Result.Errors.Add(record as ErrorRecord);
                            }
                        }
                    }

                    if (CommandPipeline.PipelineStateInfo.State == PipelineState.Failed) {
                        Result.Errors.Add(new ErrorRecord(CommandPipeline.PipelineStateInfo.Reason, "", ErrorCategory.InvalidArgument, null));
                    }
                }
            };

            CommandPipeline.StateChanged += (x, y) => {
                switch (CommandPipeline.PipelineStateInfo.State) {
                    case PipelineState.NotStarted:
                        break;

                    case PipelineState.Completed:
                        // case PipelineState.Disconnected:

                    case PipelineState.Failed:
                        while (!CommandPipeline.Output.EndOfPipeline) {
                            // poor-man's wait for the Result collection to finish reading the data
                            // todo: fix this correctly
                            Thread.Sleep(1);
                        }
                        while (!CommandPipeline.Error.EndOfPipeline) {
                            // poor-man's wait for the Result collection to finish reading the data
                            // todo: fix this correctly
                            Thread.Sleep(1);
                        }
                        lock (Result) {
                            Result.Errors.CompleteAdding();
                            Result.Output.CompleteAdding();

                            Result.StartedEvent.Set();
                            Result.CompletedEvent.Set();

                            if (CommandPipeline.PipelineStateInfo.State == PipelineState.Failed) {
                                // the last error was a terminating error
                                Result.LastIsTerminatingError = true;
                            }
                            DropPipeline();
                        }
                        break;
                        
                    case PipelineState.Stopped:

                        while (!CommandPipeline.Output.EndOfPipeline) {
                            // poor-man's wait for the Result collection to finish reading the data
                            // todo: fix this correctly
                            Thread.Sleep(1);
                        }

                        while (!CommandPipeline.Error.EndOfPipeline) {
                            // poor-man's wait for the Result collection to finish reading the data
                            // todo: fix this correctly
                            Thread.Sleep(1);
                        }

                        lock (Result) {
                            Result.Errors.CompleteAdding();
                            Result.Output.CompleteAdding();

                            Result.StartedEvent.Set();
                            Result.CompletedEvent.Set();

                            DropPipeline();
                        }

                        break;

                    case PipelineState.Stopping:
                        break;

                    case PipelineState.Running:
                        break;
                }
            };

            if (CommandPipeline.IsNested) {
                // goofy-powershell doesn't let nested pipelines async.
                CommandPipeline.Invoke();
            } else {
                CommandPipeline.InvokeAsync();
                Result.StartedEvent.WaitOne();
            }

            return Result;
        }
    }
}