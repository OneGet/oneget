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

namespace Microsoft.OneGet.Core.Api {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using AppDomains;
    using Extensions;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    internal interface IInvokable {
        object Invoke(string callbackName, IEnumerable<object> args);
    }

    public class InvokableDispatcher : MarshalByRefObject, IDisposable, IEnumerable<Delegate>, IInvokable {
        private readonly Dictionary<Callback, HashSet<string>> _parents = new Dictionary<Callback, HashSet<string>>();
        private Dictionary<string, Call> _functions = new Dictionary<string, Call>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> _methods;

        public InvokableDispatcher() {
        }

        public InvokableDispatcher(params Callback[] parents) {
            parents = parents ?? new Callback[] {
            };
            foreach (var p in parents.WhereNotNull()) {
                var methods = p(null, null) as IEnumerable<string>;
                if (methods != null) {
                    _parents.Add(p, new HashSet<string>(methods, StringComparer.OrdinalIgnoreCase));
                }
            }
        }

        public IEnumerable<string> Methods {
            get {
                return _methods ?? (_methods = new HashSet<string>(_functions.Keys.Union(_parents.Values.SelectMany(p => p)), StringComparer.OrdinalIgnoreCase));
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerator<Delegate> GetEnumerator() {
            return _functions.Values.Select(each => each.Delegate).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public object Invoke(string callbackName, IEnumerable<object> args) {
            try {
                if (string.IsNullOrEmpty(callbackName)) {
                    return Methods;
                }

                // if it's not in our set, forward call to a parent (or return null if nobody knows)
                if (!_functions.ContainsKey(callbackName)) {
                    return (from p in _parents.Keys where _parents[p].Contains(callbackName) select p.Invoke(callbackName, args)).FirstOrDefault();
                }

                // call it
                return args == null ? _functions[callbackName].Delegate : _functions[callbackName].Invokable.DynamicInvoke(args.ToArray());
            } catch (Exception e) {
                e.Dump();
            }
            return null;
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                _functions.Clear();
                _functions = null;
            }
        }

        public void Add<T>(T dlg) {
            if (dlg == null) {
                throw new ArgumentNullException("dlg");
            }
            _functions.AddOrSet(typeof (T).Name, new Call {
                Delegate = dlg as Delegate,
                Invokable = ((Invokable)typeof (T).CreateWrappedProxy(dlg as Delegate))
            });
        }

        public static Callback ToCallback(InvokableDispatcher c) {
            return c.Invoke;
        }

        public static implicit operator Callback(InvokableDispatcher c) {
            return c.Invoke;
        }

        internal class Call {
            internal Delegate Delegate;
            internal Invokable Invokable;
        }
    }

    // <summary>
    //     Is the provider of all other callbacks to the providers.
    //     This operates in two modes.
    //     1. If args is null, it looks up the callback and returns the delegate thru to the client
    //     2. if args is not null (although, it can be empty) it looks up the delegate and calls
    //     the function with the arguments supplied.
    // </summary>
    // <param name="callbackName">the callback requested</param>
    // <param name="args">arguments to pass to the callback function.</param>
    // <returns></returns>
    // public delegate object Invoke(string callbackName, IEnumerable<object> args);

    // There will be more definitions for collection callbacks when the native API gets built.
    // In .NET and Powershell, the provider model already transparently marshals IEnumerable<>
    // via a MarshalByRef interface.
}