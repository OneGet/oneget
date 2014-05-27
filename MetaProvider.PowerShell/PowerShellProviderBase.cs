namespace Microsoft.OneGet.MetaProvider.PowerShell {
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using Core;
    using Core.Extensions;

    internal class PowerShellProviderBase : IDisposable {
        private readonly Dictionary<string, CommandInfo> _methods = new Dictionary<string, CommandInfo>(StringComparer.OrdinalIgnoreCase);
        private PSModuleInfo _module;
        private DynamicPowershell _powershell;
        private DynamicPowershellResult _result;

        public PowerShellProviderBase(DynamicPowershell ps, PSModuleInfo module) {
            _powershell = ps;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_powershell != null) {
                    _powershell.Dispose();
                    _powershell = null;
                }
                if (_result != null) {
                    _result.Dispose();
                    _result = null;
                }
                _module = null;
            }
        }

        internal CommandInfo GetMethod(string methodName) {
            return _methods.GetOrAdd(methodName, () => {
                // look for a matching function in the module

                // can't find one, return null.
                return null;
            });

            // hmm, it is possible to get the parameter types to match better when binding.
            // module.ExportedFunctions.FirstOrDefault().Value.Parameters.Values.First().ParameterType
        }

        protected object CallPowerShell(Request request, params object[] args) {
            _powershell["request"] = request;

            try {
                request.Debug("INVOKING", request.CommandInfo.Name);
                // make sure we don't pass the callback to the function.
                var result = _powershell.NewTryInvokeMemberEx(request.CommandInfo.Name, new string[0], args);

                // instead, loop thru results and get 
                if (result == null) {
                    // failure! 
                    throw new Exception("Powershell script/function failed.");
                }

                object finalValue = null;

                foreach (var value in result) {
                    var y = value as IYieldable;
                    if (y != null) {
                        y.YieldResult(request);
                    } else {
                        finalValue = result;
                    }
                }
                return finalValue;
            } catch (Exception e) {
                e.Dump();
            } finally {
                _powershell["request"] = null;
            }
            return null;
        }
    }
}