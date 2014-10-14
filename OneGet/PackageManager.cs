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

namespace Microsoft.OneGet {
    using System.Linq;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Ipc;
    using Api;
    using Implementation;
    using Utility.Extensions;
    using Utility.Platform;
    using Utility.Plugin;

    /// <summary>
    ///     The public interface to accessing the fetaures of the Package Management Service
    ///     This offers two possible methods to get the instance of the PackageManagementService.
    ///     If the Host is consuming the PackageManagementService by linking to this assembly, then
    ///     the simplest access is just to use the <code>Instance</code> method.
    ///     If the Host has dynamically loaded this assembly, then it can request a dynamically-generated
    ///     instance of the PackageManagementService that implements an interface of their own choosing.
    ///     <example><![CDATA[
    ///    // Manually load the assembly
    ///    var asm = Assembly.Load("Microsoft.OneGet.Core.dll" )
    ///
    ///    // instantiate this object
    ///    dynamic pms = Assembly.CreateInstance( "Microsoft.OneGet.PackageManager" );
    ///
    ///    // ask this object to genetrate a dynamic implementation of my own interface.
    ///    pms.GetInstance<IMyPackageManagementService>();
    /// ]]>
    ///     </example>
    /// </summary>
    public class PackageManager {
        internal static PackageManagementService _instance;
        private static readonly object _lockObject = new object();

        /// <summary>
        ///     Provides access to the PackageManagenmentService instance
        /// </summary>
        public IPackageManagementService Instance {
            get {
                lock (_lockObject) {
                    if (_instance == null) {
                        _instance = new PackageManagementService();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        ///     Provides Access to the PackageManagementService instance
        /// </summary>
        /// <typeparam name="T">An caller-supplied interface type to dynamically generate a an implementation for.</typeparam>
        /// <returns>The PackageManagementService as an instance of the supplied interface type.</returns>
        public T GetTypedInstance<T>() {
            return new DynamicInterface().Create<T>(Instance);
        }

        public static int Main(string[] args) {
            // this entrypoint is only for use when inter-process remoting to get an elevated host.
            if (args.Length != 3 || !AdminPrivilege.IsElevated) {
                return 1;
            }

            var pms = new PackageManager().Instance;
            var rpc = args[0];
            var provider = args[1];
            var payload = args[2];

            var clientChannel = new IpcClientChannel();
            ChannelServices.RegisterChannel(clientChannel, true);
            var remoteRequest = (IHostApi)RemotingServices.Connect(typeof (IHostApi), rpc, null);
            var localRequest = new RemotableHostApi(remoteRequest);

            pms.Initialize(localRequest);
            var pro = pms.SelectProviders(provider, localRequest).FirstOrDefault();
            pro.ExecuteElevatedAction(payload.FromBase64(), localRequest);
            return 0;
        }
    }
}