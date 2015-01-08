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

namespace Microsoft.OneGet.Test.Core.Conceptual {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

#if EXCLUDE_FROM_OFFICIAL_BUILD

    // Note: this is just a scratch pad for some thinking I'm doing
    // not intended to test anything.

    public class Pkg {
        
    }

    public class Req {
        private CancellationTokenSource cts;

        public CancellationToken CancellationToken {
            get {
                return cts.Token;
            }
        }
    }

    public class PackageProvider {
        public async Task<IEnumerable<Pkg>> FindPackage(string name, Req req) {
            return await Task.Run(() => {
                Task.Delay(1000);
                return (IEnumerable<Pkg>)new[] { new Pkg() };
            }, req.CancellationToken);
        }
    }

    public class ProofOfConceptTests : Tests {
        public ProofOfConceptTests(ITestOutputHelper outputHelper) : base(outputHelper) {
        }

        [Fact]
        public void UseAsyncForHostApi() {

            var req = new Req();

            var pp = new PackageProvider();
            // foreach( var pkg in pp.FindPackage("test", req) );
            
        }
    }
#endif 
}