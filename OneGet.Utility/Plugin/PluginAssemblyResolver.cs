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

namespace Microsoft.OneGet.Utility.Plugin {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Extensions;

    internal class PluginAssemblyResolver : MarshalByRefObject {
        // we don't want these objects being gc's out because they remain unused...

        private static readonly string[] _assemblyFileExtensions = {
            "exe", "dll"
        };

        private Func<string, string> _alternatePathResolver;
        private readonly Dictionary<string, Assembly> _cache = new Dictionary<string, Assembly>();

        private Func<string, Assembly> _loadWhenResolving;
        private string[] _searchPath = new string[0];

        public PluginAssemblyResolver() {
            _loadWhenResolving = LoadFile;
        }

        public override object InitializeLifetimeService() {
            return null;
        }

        public void SetAlternatePathResolver(Func<string, string> f) {
            _alternatePathResolver = f;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Still in development.")]
        public void SetLoadMethodToFile() {
            _loadWhenResolving = LoadFile;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Still in development.")]
        public void SetLoadMethodToFrom() {
            _loadWhenResolving = LoadFrom;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Still in development.")]
        public void SetLoadMethodToBinary() {
            _loadWhenResolving = LoadBinary;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called from friend assembly")]
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
            return _cache.GetOrAdd(args.Name, () => {
                if (_alternatePathResolver != null) {
                    var a = _alternatePathResolver(args.Name);
                    if (a != null) {
                        var assembly = _loadWhenResolving(a);
                        return assembly;
                    }
                }

                if (args.RequestingAssembly != null && !args.RequestingAssembly.IsDynamic) {
                    var folder = args.RequestingAssembly.Location;
                    folder = Path.GetDirectoryName(folder);
                    if (folder != null && Directory.Exists(folder)) {
                        AddPath(folder);
                    }
                }

                var name = new AssemblyName(args.Name).Name;

#if NOT_USED
                Console.WriteLine("RESOLVING ASSEMBLY {0} in {1}", args.Name,AppDomain.CurrentDomain.FriendlyName);
#if DEBUG
            Debug.WriteLine(string.Format("RESOLVING ASSEMBLY {0}", args.Name));
            if (args.RequestingAssembly != null) {
                Debug.WriteLine(string.Format("---> REQUESTING ASSEMBLY {0}", args.RequestingAssembly.GetName()));
            }
#endif
#endif

                foreach (var f in _searchPath) {
                    var folder = f;
                    foreach (var assemblyPath in _assemblyFileExtensions.Select(extension => {return Path.Combine(folder, ("{0}.{1}".format(name, extension)));}).Where(File.Exists)) {
                        var assembly = _loadWhenResolving(assemblyPath);
                        return assembly;
                    }
                }

#if NOT_USED
    // todo: this is a terrible fallback for finding stuff that should be found in the GAC.
    // I'm pretty sure this isn't acutally needed, but I noticed that the powershell resources assemblies were 
    // repeatedly being searched for when a powershell assembly loaded into an appdomain.
    // this did't speed it up really, but it did stop it from not finding it 15 times...
                var gacPath = @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL";
                var p = Path.Combine(gacPath, name);
                if (Directory.Exists(p)) {
                    var dirs = Directory.GetDirectories(p);
                    foreach (var folder in dirs) {
                        foreach (string assemblyPath in _assemblyFileExtensions.Select(extension => {
                            return Path.Combine(folder, ("{0}.{1}".format(name, extension)));
                        }).Where(File.Exists)) {
                            Assembly assembly = _loadWhenResolving(assemblyPath);
                            return assembly;
                        }
                    }
                }

#endif
#if DEEPDEBUG
               Debug.WriteLine(string.Format("FAILED RESOLVING ASSEMBLY {0}", args.Name));

#endif
                return null;
            });
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom", Justification = "This is a plugin loader. It *needs* to do that.")]
        internal Assembly LoadFrom(string path) {
            if (path.FileExists()) {
                AddPath(Path.GetDirectoryName(Path.GetFullPath(path)));
            }
            return Assembly.LoadFrom(path);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile", Justification = "This is a plugin loader. It *needs* to do that.")]
        internal Assembly LoadFile(string path) {
            return Assembly.LoadFile(path);
        }

        internal Assembly LoadBinary(string assemblyPath) {
            var symbolsPath = Path.ChangeExtension(assemblyPath, "pdb");
            // Try to load the PDB for the assembly.
            if (File.Exists(symbolsPath)) {
                return Assembly.Load(
                    File.ReadAllBytes(assemblyPath)
                    , File.ReadAllBytes(symbolsPath)
                    );
            }
            return Assembly.Load(File.ReadAllBytes(assemblyPath));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called From Friend Assembly")]
        internal IList<Assembly> LoadBinaryWithReferences(string assemblyPath) {
            var assembly = LoadBinary(assemblyPath);
            var list = new List<Assembly> {
                assembly
            };
            // list.AddRange(assembly.GetReferencedAssemblies().Select(Assembly.Load));
            list.AddRange(assembly.GetReferencedAssemblies().Select(each => Resolve(this, new ResolveEventArgs(each.Name))));
            return list;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called from Friend Assembly.")]
        internal IList<Assembly> LoadFromWithReferences(string assemblyPath) {
            var assembly = LoadFrom(assemblyPath);
            var list = new List<Assembly> {
                assembly
            };
            // list.AddRange(assembly.GetReferencedAssemblies().Select(Assembly.Load));
            list.AddRange(assembly.GetReferencedAssemblies().Select(each => Resolve(this, new ResolveEventArgs(each.Name))));
            return list;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called From Friend Assembly")]
        internal IList<Assembly> LoadFileWithReferences(string assemblyPath) {
            var assembly = LoadFile(assemblyPath);
            var list = new List<Assembly> {
                assembly
            };
            list.AddRange(assembly.GetReferencedAssemblies().Select(each => Resolve(this, new ResolveEventArgs(each.Name))));
            return list;
        }
    }
}