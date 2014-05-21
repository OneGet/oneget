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

namespace Microsoft.OneGet.Core.Providers.Package {
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Api;
    using Collections;
    using DuckTyping;
    using Extensions;
    using Packaging;
    using Service;
    using Tasks;
    using Callback = System.Func<string, System.Collections.Generic.IEnumerable<object>, object>;

    public interface IPackageProvider {
        #region pretend-declarre PackageProvider-interface
        [DuckTypedClass.Required]
        string GetPackageProviderName();

        void InitializeProvider(Callback c);
        #endregion
    }

  
#if fale
    public class foo {
        public foo() {
            object xyz;

            if (DuckTyper.IsCompatible<IPackageProvider>(xyz)) {
                var x = DuckTyper.DuckType<IPackageProvider>(xyz);
                x.GetPackageProviderName();
            }

            AppDomain myCurrentDomain = AppDomain.CurrentDomain;
            AssemblyName myAssemblyName = new AssemblyName();
            myAssemblyName.Name = "TempAssembly";

            AssemblyBuilder myAssemblyBuilder;

            myAssemblyBuilder = myCurrentDomain.DefineDynamicAssembly
                     (myAssemblyName, AssemblyBuilderAccess.Run);

            // Define a dynamic module in this assembly.
            ModuleBuilder myModuleBuilder = myAssemblyBuilder.
                                            DefineDynamicModule("TempModule");

            // Define a runtime class with specified name and attributes.
            TypeBuilder myTypeBuilder = myModuleBuilder.DefineType
                                             ("TempClass", TypeAttributes.Public,typeof(IPackageProvider));

            var f = myTypeBuilder.DefineField("_GetPackageProviderName", typeof (Func<string>), FieldAttributes.Public);

            var methodBuilder = myTypeBuilder.DefineMethod("GetPackageProviderName", MethodAttributes.Public);
            var t = myTypeBuilder.CreateType();
            var instance = Activator.CreateInstance(t);
            setDelegates(instance, remoteInstance);
            var pp = instance as IPackageProvider;

        }

        void consume() {

            Assembly asm;
            dynamic duckTyper = asm.CreateInstance("OneGet.DuckTyper");
            IPackageManagementService service = duckTyper.Bind<IPackageManagementService>(asm.CreateInstance("OneGet.PackageManagementService"));
            IPackageManagementService service2 = duckTyper.Create<IPackageManagementService>("OneGet.PackageManagementService");
            Type ppType;
            duckTyper.FindCompatibleTypes<IPackageProvider>();

            IPackageProvider pp = duckTyper.Create<IPackageProvider>(ppType);

            object ppInstance;
            IPackageProvider pp2 = duckTyper.Create<IPackageProvider>(ppInstance);

            /*
            Assembly asm;
            dynamic pms = asm.CreateInstance("OneGet.PackageManagementService");
            IPackageManagementService service = pms.CastTo<IPackageManagementService>();
            service.GetProviders()

            IPackageProvider instance = DuckTyper.CreateInstance<IDisposable>(remote_instance);

            instance.InitializeProvider(null);
            */
        }
    }

        public interface IPackageManagementService {
        IEnumerable<IPackageProvider> GetProviders();
    }

#endif
    internal class PackageProviderImpl {

        private Func<string> _getPackageProviderName;
        private Action<Callback> _initializeProvider;

        private object foo;

        PackageProviderImpl(object instance) {
            var members = instance.GetType().GetMembers();

            //_getPackageProviderName = GenerateDelegate<Func<string>>(instance, members, "GetPackageProviderName");

            //typeof(IPackageProvider).GetMembers()[0].Get
        }

        public string GetPackageProviderName() {
            return _getPackageProviderName != null ?_getPackageProviderName() : default(string);
        }

        public void InitializeProvider(Callback c) {
            if (_initializeProvider != null) {
                _initializeProvider(c);
            }
        }

    }

    public class PackageProviderInstance : DuckTypedClass {

        internal PackageProviderInstance(object instance) : base(instance) {
            #region generate-memberinit PackageProvider-interface
            GetPackageProviderName = GetRequiredDelegate<Interface.GetPackageProviderName>(instance);
            InitializeProvider = GetOptionalDelegate<Interface.InitializeProvider>(instance);
            GetFeatures = GetOptionalDelegate<Interface.GetFeatures>(instance);
            GetDynamicOptions = GetOptionalDelegate<Interface.GetDynamicOptions>(instance);
            GetMagicSignatures = GetOptionalDelegate<Interface.GetMagicSignatures>(instance);
            GetSchemes = GetOptionalDelegate<Interface.GetSchemes>(instance);
            GetFileExtensions = GetOptionalDelegate<Interface.GetFileExtensions>(instance);
            GetIsSourceRequired = GetOptionalDelegate<Interface.GetIsSourceRequired>(instance);
            AddPackageSource = GetOptionalDelegate<Interface.AddPackageSource>(instance);
            GetPackageSources = GetOptionalDelegate<Interface.GetPackageSources>(instance);
            RemovePackageSource = GetOptionalDelegate<Interface.RemovePackageSource>(instance);
            StartFind = GetOptionalDelegate<Interface.StartFind>(instance);
            CompleteFind = GetOptionalDelegate<Interface.CompleteFind>(instance);
            FindPackage = GetOptionalDelegate<Interface.FindPackage>(instance);
            FindPackageByFile = GetOptionalDelegate<Interface.FindPackageByFile>(instance);
            FindPackageByUri = GetOptionalDelegate<Interface.FindPackageByUri>(instance);
            GetInstalledPackages = GetOptionalDelegate<Interface.GetInstalledPackages>(instance);
            DownloadPackage = GetOptionalDelegate<Interface.DownloadPackage>(instance);
            GetPackageDependencies = GetOptionalDelegate<Interface.GetPackageDependencies>(instance);
            GetPackageDetails = GetOptionalDelegate<Interface.GetPackageDetails>(instance);
            InstallPackage = GetOptionalDelegate<Interface.InstallPackage>(instance);
            UninstallPackage = GetOptionalDelegate<Interface.UninstallPackage>(instance);
            #endregion

        }

        internal PackageProviderInstance(Type type)
            : this(Activator.CreateInstance(type)) {
        }

        public static bool IsInstanceCompatible(object instance) {
            return instance != null && IsTypeCompatible(instance.GetType());
        }

        public static bool IsTypeCompatible(Type type) {
            if (type == null) {
                return false;
            }

            var publicMethods = type.GetPublicMethods().ToArray();

            return true
            #region generate-istypecompatible PackageProvider-interface
                && publicMethods.Any(each => DuckTypedExtensions.IsNameACloseEnoughMatch(each.Name, "GetPackageProviderName") && typeof (Interface.GetPackageProviderName).IsDelegateAssignableFromMethod(each))
            #endregion

;
        }

        #region generate-members PackageProvider-interface
        internal readonly Interface.GetPackageProviderName GetPackageProviderName;
        internal readonly Interface.InitializeProvider InitializeProvider;
        internal readonly Interface.GetFeatures GetFeatures;
        internal readonly Interface.GetDynamicOptions GetDynamicOptions;
        internal readonly Interface.GetMagicSignatures GetMagicSignatures;
        internal readonly Interface.GetSchemes GetSchemes;
        internal readonly Interface.GetFileExtensions GetFileExtensions;
        internal readonly Interface.GetIsSourceRequired GetIsSourceRequired;
        internal readonly Interface.AddPackageSource AddPackageSource;
        internal readonly Interface.GetPackageSources GetPackageSources;
        internal readonly Interface.RemovePackageSource RemovePackageSource;
        internal readonly Interface.StartFind StartFind;
        internal readonly Interface.CompleteFind CompleteFind;
        internal readonly Interface.FindPackage FindPackage;
        internal readonly Interface.FindPackageByFile FindPackageByFile;
        internal readonly Interface.FindPackageByUri FindPackageByUri;
        internal readonly Interface.GetInstalledPackages GetInstalledPackages;
        internal readonly Interface.DownloadPackage DownloadPackage;
        internal readonly Interface.GetPackageDependencies GetPackageDependencies;
        internal readonly Interface.GetPackageDetails GetPackageDetails;
        internal readonly Interface.InstallPackage InstallPackage;
        internal readonly Interface.UninstallPackage UninstallPackage;
        #endregion

        public class Interface {
            #region declare PackageProvider-interface
            /// <summary>
            /// Returns the name of the Provider. Doesn't need a callback .
            /// </summary>
            /// <required/>
            /// <returns>the name of the package provider</returns>
            internal delegate string GetPackageProviderName();
            internal delegate void InitializeProvider(Callback c);

            internal delegate void GetFeatures(Callback c);
            internal delegate void GetDynamicOptions(int category, Callback c);

            // --- Optimization features -----------------------------------------------------------------------------------------------------
            internal delegate IEnumerable<string> GetMagicSignatures();
            internal delegate IEnumerable<string> GetSchemes();
            internal delegate IEnumerable<string> GetFileExtensions();
            internal delegate bool GetIsSourceRequired(); // or should we imply this from the GetPackageSources() == null/empty?

            // --- Manages package sources ---------------------------------------------------------------------------------------------------
            internal delegate void AddPackageSource(string name, string location, bool trusted, Callback c);
            internal delegate bool GetPackageSources(Callback c);
            internal delegate void RemovePackageSource(string name, Callback c);

            internal delegate int StartFind(Callback c);

            internal delegate bool CompleteFind(int id, Callback c);

            // --- Finds packages ---------------------------------------------------------------------------------------------------
            /// <summary>
            /// 
            /// 
            /// Notes:
            /// 
            ///  - If a call to GetPackageSources() on this object returns no sources, the cmdlet won't call FindPackage on this source
            ///  - (ie, the expectation is that you have to provide a source in order to use find package)
            /// </summary>
            /// <param name="name"></param>
            /// <param name="requiredVersion"></param>
            /// <param name="minimumVersion"></param>
            /// <param name="maximumVersion"></param>
            /// <param name="c"></param>
            /// <returns></returns>
            internal delegate bool FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Callback c);

            internal delegate bool FindPackageByFile(string file, int id, Callback c);
            internal delegate bool FindPackageByUri(Uri uri, int id, Callback c);

            internal delegate bool GetInstalledPackages(string name, Callback c);

            // --- operations on a package ---------------------------------------------------------------------------------------------------
            internal delegate bool DownloadPackage(string fastPath, string location, Callback c);
            internal delegate bool GetPackageDependencies(string fastPath, Callback c);
            internal delegate bool GetPackageDetails(string fastPath, Callback c);

            internal delegate bool InstallPackage(string fastPath, Callback c);
                // auto-install-dependencies
                // skip-dependency-check
                // continue-on-failure
                // location system/user/folder
                // callback for each package installed when installing dependencies?

            internal delegate bool UninstallPackage(string fastPath, Callback c);

            #endregion
        }

    }

    public enum PackageProviderApi {
        #region generate-enum PackageProvider-interface
        GetPackageProviderName,
        InitializeProvider,
        GetFeatures,
        GetDynamicOptions,
        GetMagicSignatures,
        GetSchemes,
        GetFileExtensions,
        GetIsSourceRequired,
        AddPackageSource,
        GetPackageSources,
        RemovePackageSource,
        StartFind,
        CompleteFind,
        FindPackage,
        FindPackageByFile,
        FindPackageByUri,
        GetInstalledPackages,
        DownloadPackage,
        GetPackageDependencies,
        GetPackageDetails,
        InstallPackage,
        UninstallPackage,
        #endregion

    }

    public class PackageProvider : /* RemoteableDynamicObject  */ MarshalByRefObject {

        private readonly PackageProviderInstance _provider;
        internal PackageProvider(PackageProviderInstance provider) {
            _provider = provider;
        }

        // we don't want these objects being gc's out because they remain unused...
        public override object InitializeLifetimeService() {
            return null;
        }

#if IF_NEEDED_REMOTE_DYNAMIC_OBJECT 
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public;

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            var parameterTypes = args.Select(each => each == null ? typeof(object) : each.GetType());
            var method = _provider.Instance.GetType().GetMethod(binder.Name, BindingFlags, null, parameterTypes.ToArray(), null) ?? _provider.Instance.GetType().GetMethod(binder.Name, BindingFlags);

            if (method == null) {
                return base.TryInvokeMember(binder, args, out result);
            }

            result = method.Invoke(_provider.Instance, args);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            PropertyInfo propertyInfo = _provider.Instance.GetType().GetProperty(binder.Name, BindingFlags);
            if (propertyInfo != null) {
                result = propertyInfo.GetValue(_provider.Instance, null);
                return true;
            }

            var fieldInfo = _provider.Instance.GetType().GetField(binder.Name, BindingFlags);
            if (fieldInfo != null) {
                result = fieldInfo.GetValue(_provider.Instance);
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            PropertyInfo propertyInfo = _provider.Instance.GetType().GetProperty(binder.Name, BindingFlags);
            if (propertyInfo != null) {
                propertyInfo.SetValue(_provider.Instance, value, null);
                return true;
            }

            var fieldInfo = _provider.Instance.GetType().GetField(binder.Name, BindingFlags);
            if (fieldInfo != null) {
                fieldInfo.SetValue(_provider.Instance, value);
                return true;
            }
            return base.TrySetMember(binder, value);
        }
#endif

        public bool IsSupported(PackageProviderApi api) {
            switch (api) {
                #region generate-issupported PackageProvider-interface
                 case PackageProviderApi.GetPackageProviderName:
                    return _provider.GetPackageProviderName.IsSupported();
                 case PackageProviderApi.InitializeProvider:
                    return _provider.InitializeProvider.IsSupported();
                 case PackageProviderApi.GetFeatures:
                    return _provider.GetFeatures.IsSupported();
                 case PackageProviderApi.GetDynamicOptions:
                    return _provider.GetDynamicOptions.IsSupported();
                 case PackageProviderApi.GetMagicSignatures:
                    return _provider.GetMagicSignatures.IsSupported();
                 case PackageProviderApi.GetSchemes:
                    return _provider.GetSchemes.IsSupported();
                 case PackageProviderApi.GetFileExtensions:
                    return _provider.GetFileExtensions.IsSupported();
                 case PackageProviderApi.GetIsSourceRequired:
                    return _provider.GetIsSourceRequired.IsSupported();
                 case PackageProviderApi.AddPackageSource:
                    return _provider.AddPackageSource.IsSupported();
                 case PackageProviderApi.GetPackageSources:
                    return _provider.GetPackageSources.IsSupported();
                 case PackageProviderApi.RemovePackageSource:
                    return _provider.RemovePackageSource.IsSupported();
                 case PackageProviderApi.StartFind:
                    return _provider.StartFind.IsSupported();
                 case PackageProviderApi.CompleteFind:
                    return _provider.CompleteFind.IsSupported();
                 case PackageProviderApi.FindPackage:
                    return _provider.FindPackage.IsSupported();
                 case PackageProviderApi.FindPackageByFile:
                    return _provider.FindPackageByFile.IsSupported();
                 case PackageProviderApi.FindPackageByUri:
                    return _provider.FindPackageByUri.IsSupported();
                 case PackageProviderApi.GetInstalledPackages:
                    return _provider.GetInstalledPackages.IsSupported();
                 case PackageProviderApi.DownloadPackage:
                    return _provider.DownloadPackage.IsSupported();
                 case PackageProviderApi.GetPackageDependencies:
                    return _provider.GetPackageDependencies.IsSupported();
                 case PackageProviderApi.GetPackageDetails:
                    return _provider.GetPackageDetails.IsSupported();
                 case PackageProviderApi.InstallPackage:
                    return _provider.InstallPackage.IsSupported();
                 case PackageProviderApi.UninstallPackage:
                    return _provider.UninstallPackage.IsSupported();
                #endregion

            }
            return false;
        }

        // Friendly APIs

        public void InitializeProvider(Callback c) {
            _provider.InitializeProvider(c);
        }

        public string Name {
            get {
                return _provider.GetPackageProviderName();
            }
        }

        public void AddPackageSource(string name, string location, bool trusted, Callback c) {
            _provider.AddPackageSource(name, location, trusted, new InvokableDispatcher(c, Instance.Service.Invoke));
        }

        public void RemovePackageSource(string name, Callback c) {
            _provider.RemovePackageSource(name, new InvokableDispatcher(c, Instance.Service.Invoke));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackageByUri(Uri u, int id, Callback c) {
            var providerName = Name;

            return CallAndCollectResults<SoftwareIdentity, YieldPackage>(
                c, // inherited callback
                nc => _provider.FindPackageByUri(u, id, nc), // actual call
                (collection, okToContinue) => ((fastpath, name, version, scheme, summary, source) => {
                    collection.Add(new SoftwareIdentity {
                        FastPath = fastpath,
                        Name = name,
                        Version = version,
                        VersionScheme = scheme,
                        Summary = summary,
                        ProviderName = providerName,
                        Source = source,
                        Status = "Available"
                    });
                    return okToContinue();
                }));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackageByFile(string filename, int id, Callback c) {
            var providerName = Name;

            return CallAndCollectResults<SoftwareIdentity, YieldPackage>(
                c, // inherited callback
                nc => _provider.FindPackageByFile(filename,id, nc), // actual call
                (collection, okToContinue) => ((fastpath, name, version, scheme, summary, source) => {
                    collection.Add(new SoftwareIdentity {
                        FastPath = fastpath,
                        Name = name,
                        Version = version,
                        VersionScheme = scheme,
                        Summary = summary,
                        ProviderName = providerName,
                        Source = source,
                        Status = "Available"
                    });
                    return okToContinue();
                }));
        }

        public int StartFind(Callback c) {
            return _provider.StartFind(c);
        }

        public CancellableEnumerable<SoftwareIdentity> CompleteFind(int i, Callback c) {
            var providerName = Name;

            return CallAndCollectResults<SoftwareIdentity, YieldPackage>(
               c, // inherited callback
               nc => _provider.CompleteFind(i, nc), // actual call
               (collection, okToContinue) => ((fastpath, name, version, scheme, summary, source) => {
                   collection.Add(new SoftwareIdentity {
                       FastPath = fastpath,
                       Name = name,
                       Version = version,
                       VersionScheme = scheme,
                       Summary = summary,
                       ProviderName = providerName,
                       Source = source,
                       Status = "Available"
                   });
                   return okToContinue();
               }));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackages(string[] names, string requiredVersion, string minimumVersion, string maximumVersion, Callback c) {
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>( new CancellationTokenSource(), names.SelectMany(each => FindPackage(each, requiredVersion, minimumVersion, maximumVersion, id, c)).Concat(CompleteFind(id, c)));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackagesByUris(Uri[] uris, Callback c) {
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), uris.SelectMany(each => FindPackageByUri(each, id, c)).Concat(CompleteFind(id, c)));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackagesByFiles(string[] filenames, Callback c) {
            var id = StartFind(c);
            return new CancellableEnumerable<SoftwareIdentity>(new CancellationTokenSource(), filenames.SelectMany(each => FindPackageByFile(each, id, c)).Concat(CompleteFind(id, c)));
        }

        public CancellableEnumerable<SoftwareIdentity> FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, Callback c) {
            var providerName = Name;

            return CallAndCollectResults<SoftwareIdentity, YieldPackage>(
                c, // inherited callback
                nc => _provider.FindPackage(name, requiredVersion, minimumVersion, maximumVersion, id, nc), // actual call
                (collection, okToContinue) => ((fastpath, n, version, scheme, summary, source) => {
                    collection.Add(new SoftwareIdentity {
                        FastPath = fastpath,
                        Name = n,
                        Version = version,
                        VersionScheme = scheme,
                        Summary = summary,
                        ProviderName = providerName,
                        Source = source,
                        Status = "Available"
                    });
                    return okToContinue();
                }), false);
        }

        public CancellableEnumerable<SoftwareIdentity> GetInstalledPackages(string name, Callback c) {
            var providerName = Name;

            return CallAndCollectResults<SoftwareIdentity, YieldPackage>(
                c, // inherited callback
                nc => _provider.GetInstalledPackages(name, nc), // actual call
                (collection, okToContinue) => ((fastpath, n, version, scheme, summary, source) => {
                    collection.Add(new SoftwareIdentity {
                        FastPath = fastpath,
                        Name = n,
                        Version = version,
                        VersionScheme = scheme,
                        Summary = summary,
                        ProviderName = providerName,
                        Source = source,
                        Status = "Installed"
                    });
                    return okToContinue();
                }));
        }

        /* CTP */

        public CancellableEnumerable<SoftwareIdentity> InstallPackage(SoftwareIdentity softwareIdentity, Callback c) {
            if (softwareIdentity == null) {
                throw new ArgumentNullException("softwareIdentity");
            }

            if (c == null) {
                throw new ArgumentNullException("c");
            }
            var providerName = Name;

            if (!IsTrustedPackageSource(softwareIdentity.Source, c)) {
                try {
                    if (!(bool)c.DynamicInvoke<ShouldContinueWithUntrustedPackageSource>(softwareIdentity.Name, softwareIdentity.Source)) {
                        c.DynamicInvoke<Error>("Cancelled", "User declined to trust package source ", null);
                        throw new Exception("cancelled");
                    }
                }
                catch {
                    c.DynamicInvoke<Error>("Cancelled", "User declined to trust package source ", null);
                    throw new Exception("cancelled");
                }
            }

            return CallAndCollectResults<SoftwareIdentity, YieldPackage>(
                c, // inherited callback
                nc => _provider.InstallPackage(softwareIdentity.FastPath, nc), // actual call
                (collection, okToContinue) => ((fastpath, n, version, scheme, summary, source) => {
                    collection.Add(new SoftwareIdentity {
                        FastPath = fastpath,
                        Name = n,
                        Version = version,
                        VersionScheme = scheme,
                        Summary = summary,
                        ProviderName = providerName,
                        Source = source,
                        Status = "Installed"
                    });
                    return okToContinue();
                }));
        }

        public CancellableEnumerable<SoftwareIdentity> UninstallPackage(SoftwareIdentity softwareIdentity, Callback c) {
            var providerName = Name;

            return CallAndCollectResults<SoftwareIdentity, YieldPackage>(
                c, // inherited callback
                nc => _provider.UninstallPackage(softwareIdentity.FastPath, nc), // actual call
                (collection, okToContinue) => ((fastpath, n, version, scheme, summary, source) => {
                    collection.Add(new SoftwareIdentity {
                        FastPath = fastpath,
                        Name = n,
                        Version = version,
                        VersionScheme = scheme,
                        Summary = summary,
                        ProviderName = providerName,
                        Source = source,
                        Status = "Not Installed"
                    });
                    return okToContinue();
                }));
        }

        /// <summary>
        ///     I noticed that most of my functions ended up as a pattern that was extremely common.
        ///     I've therefore decided to distill this down to eliminate fat-fingered mistakes when cloning the pattern.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="OnResultFn"></typeparam>
        /// <param name="c"></param>
        /// <param name="action"></param>
        /// <param name="onResultFn"></param>
        /// <returns></returns>
        private CancellableEnumerable<TResult> CallAndCollectResults<TResult, OnResultFn>(Callback c, Action<Callback> action,
            Func<CancellableBlockingCollection<TResult>, OkToContinue, OnResultFn> onResultFn, bool cancelOnException = true) {
            var result = new CancellableBlockingCollection<TResult>();

            Task.Factory.StartNew(() => {
                try {
                    // callback.DynamicInvoke<Verbose>("Hello", "World", null);
                    var isOkToContinueFn = new OkToContinue(() => !(result.IsCancelled || (bool)c.DynamicInvoke<IsCancelled>()));

                    using (var cb = new InvokableDispatcher(c, Instance.Service.Invoke) {
                        isOkToContinueFn,
                        onResultFn(result, isOkToContinueFn)
                    }) {
                        try {
                            action(cb);
                        }
                        catch (Exception e) {
                            if (cancelOnException) {
                                result.Cancel();
                                Event<ExceptionThrown>.Raise(e.GetType().Name, e.Message, e.StackTrace);
                            }
                        }
                    }
                }
                catch (Exception e) {
                    e.Dump();
                }
                finally {
                    result.CompleteAdding();
                }
            });

            return result;
        }

        public CancellableEnumerable<DynamicOption> GetOptionDefinitons(OptionCategory operation, Callback c) {
            return CallAndCollectResults<DynamicOption, YieldOptionDefinition>(
                c,
                nc => _provider.GetDynamicOptions((int)operation, nc),
                (collection, okToContinue) => ((category, name, type, isRequired, values) => {
                    collection.Add(new DynamicOption {
                        Category = category,
                        Name = name,
                        Type = type,
                        IsRequired = isRequired,
                        PossibleValues = values
                    });
                    return okToContinue();
                }));
        }

        public bool IsValidPackageSource(string packageSource, Callback c) {
            return false;
            // return _provider.IsValidPackageSource(packageSource, new InvokableDispatcher(c, Instance.Service.Invoke));
        }

        public bool IsTrustedPackageSource(string packageSource, Callback c) {
            return false;
        }

        public CancellableEnumerable<PackageSource> GetPackageSources(Callback c) {
            return CallAndCollectResults<PackageSource, YieldSource>(
                c,
                nc => _provider.GetPackageSources(nc),
                (collection, okToContinue) => ((name, location, isTrusted) => {
                    collection.Add(new PackageSource {
                        Name = name,
                        Location = location,
                        Provider = Name,
                        IsTrusted = isTrusted
                    });
                    return okToContinue();
                }));
        }

    }

    #region declare PackageProvider-types
    public enum OptionCategory {
        Package = 0,
        Provider = 1,
        Source = 2,
        Install = 3
    }

    public enum OptionType {
        String = 0,
        StringArray = 1,
        Int = 2,
        Switch = 3,
        Path = 4,
        Uri = 5
    }

    #endregion

    internal static class CallbackExt {
        public static T Lookup<T>(this Callback c) where T : class {
            return c(typeof(T).Name, null) as T ?? typeof(T).CreateEmptyDelegate() as T;
        }

        public static object DynamicInvoke<T>(this Callback c, params object[] args) where T : class {
            return c(typeof(T).Name, args);
        }
    }
}