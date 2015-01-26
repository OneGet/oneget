using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.OneGet.Test.Core.Host {
    using System.Security;
    using Api;
    using Xunit.Abstractions;

    public class HostImpl : IHostApi {
        public bool IsCanceled {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065")]
            get {
                throw new NotImplementedException();
            }
        }

        public string GetMessageString(string messageText, string defaultText) {
            throw new NotImplementedException();
        }

        public bool Warning(string messageText) {
            throw new NotImplementedException();
        }

        public bool Error(string id, string category, string targetObjectValue, string messageText) {
            throw new NotImplementedException();
        }

        public bool Message(string messageText) {
            throw new NotImplementedException();
        }

        public bool Verbose(string messageText) {
            throw new NotImplementedException();
        }

        public bool Debug(string messageText) {
            throw new NotImplementedException();
        }

        public int StartProgress(int parentActivityId, string messageText) {
            throw new NotImplementedException();
        }

        public bool Progress(int activityId, int progressPercentage, string messageText) {
            throw new NotImplementedException();
        }

        public bool CompleteProgress(int activityId, bool isSuccessful) {
            throw new NotImplementedException();
        }

        public IEnumerable<string> OptionKeys {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065")]
            get {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<string> GetOptionValues(string key) {
            throw new NotImplementedException();
        }

        public IEnumerable<string> Sources {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065")]
            get {
                throw new NotImplementedException();
            }
        }

        public string CredentialUsername {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065")]
            get {
                throw new NotImplementedException();
            }
        }

        public SecureString CredentialPassword {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065")]
            get {
                throw new NotImplementedException();
            }
        }

        public bool ShouldBootstrapProvider(string requestor, string providerName, string providerVersion, string providerType, string location, string destination) {
            throw new NotImplementedException();
        }

        public bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource) {
            throw new NotImplementedException();
        }

        public bool AskPermission(string permission) {
            throw new NotImplementedException();
        }

        public bool IsInteractive {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065")]
            get {
                throw new NotImplementedException();
            }
        }

        public int CallCount {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065")]
            get {
                throw new NotImplementedException();
            }
        }
    }

    public class HostTests : Tests {
        public HostTests(ITestOutputHelper outputHelper) : base(outputHelper) {


        }
    }
}
