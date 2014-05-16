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

namespace Microsoft.OneGet.Core.AppDomains {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Extensions;

    internal class PluginAssemblyResolver : MarshalByRefObject {
        // we don't want these objects being gc's out because they remain unused...
        public override object InitializeLifetimeService() {
            return null;
        }


        private static readonly string[] _assemblyFileExtensions = {
            "exe", "dll"
        };

        private Func<string, Assembly> _loadWhenResolving;
        private string[] _searchPath = new string[0];

        public PluginAssemblyResolver() {
            _loadWhenResolving = LoadBinary;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Still in development.")]
        public void SetLoadMethodToFile() {
            _loadWhenResolving = LoadFile;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Still in development.")]
        public void SetLoadMethodToFrom() {
            _loadWhenResolving = LoadFrom;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Still in development.")]
        public void SetLoadMethodToBinary() {
            _loadWhenResolving = LoadBinary;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called from friend assembly")]
        internal void AddPath(string path) {
            
            if (!string.IsNullOrEmpty(path)) {
                if (path.Contains(Path.PathSeparator)) {
                    _searchPath = _searchPath.Union(path.Split(new[] {
                        Path.PathSeparator
                    }, StringSplitOptions.RemoveEmptyEntries).Select(Path.GetFullPath)).ToArray();
                } else {
                    _searchPath = _searchPath.Union(new[] {
                        Path.GetFullPath(path)
                    }).ToArray();
                }
            }
        }

        public Assembly Resolve(object sender, ResolveEventArgs args) {
            var name = new AssemblyName(args.Name).Name;
            // Console.WriteLine("Resolving: {0}", name);
            foreach (string folder in _searchPath) {
                foreach (string assemblyPath in _assemblyFileExtensions.Select(extension => {
                    return Path.Combine(folder, ("{0}.{1}".format(name, extension)));
                }).Where(File.Exists)) {
                    Assembly assembly = _loadWhenResolving(assemblyPath);
                    return assembly;
                }
            }
            return null;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom", Justification = "This is a plugin loader. It *needs* to do that.")]
        internal Assembly LoadFrom(string path) {
            if (path.FileExists()) {
                AddPath(Path.GetDirectoryName(Path.GetFullPath(path)));
            }
            return Assembly.LoadFrom(path);
        }
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods",MessageId = "System.Reflection.Assembly.LoadFile",Justification = "This is a plugin loader. It *needs* to do that.")]
        internal Assembly LoadFile(string path) {
            return Assembly.LoadFile(path);
        }

        internal Assembly LoadBinary(string assemblyPath) {
            var symbolsPath = Path.ChangeExtension(assemblyPath, "pdb");

            // Try to load the PDB for the assembly.
            if (File.Exists(symbolsPath)) {
                return Assembly.Load(
                    File.ReadAllBytes(assemblyPath),
                    File.ReadAllBytes(symbolsPath));
            }
            return Assembly.Load(File.ReadAllBytes(assemblyPath));
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called From Friend Assembly")]
        internal IList<Assembly> LoadBinaryWithReferences(string assemblyPath) {
            var assembly = LoadBinary(assemblyPath);
            var list = new List<Assembly> {
                assembly
            };
            list.AddRange(assembly.GetReferencedAssemblies().Select(Assembly.Load));
            return list;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called from Friend Assembly.")]
        internal IList<Assembly> LoadFromWithReferences(string assemblyPath) {
            var assembly = LoadFrom(assemblyPath);
            var list = new List<Assembly> {
                assembly
            };
            list.AddRange(assembly.GetReferencedAssemblies().Select(Assembly.Load));
            return list;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called From Friend Assembly")]
        internal IList<Assembly> LoadFileWithReferences(string assemblyPath) {
            var assembly = LoadFile(assemblyPath);
            var list = new List<Assembly> {
                assembly
            };
            list.AddRange(assembly.GetReferencedAssemblies().Select(Assembly.Load));
            return list;
        }
    }
}
