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

namespace Microsoft.PackageManagement.Test.Core.Service {
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using Api;
    using PackageManagement.Utility.Extensions;
    using Support;
    using Utility.Misc;

    public class BasicHostImpl : IHostApi {
        private static object lockObject = new object();
        private int _callCount;
        private int _count;
        internal List<string> Errors = new List<string>();
        internal List<string> Warnings = new List<string>();

        public BasicHostImpl() {
            _callCount = NextNumber;
        }


        private int NextNumber {
            get {
                lock (lockObject) {
                    return ++_count;
                }
            }
        }

        public virtual bool IsCanceled {
            get {
                Console.WriteLine("[IsCancelled]");
                return false;
            }
        }

        public virtual string GetMessageString(string messageText, string defaultText) {
            Console.WriteLine("[GetMessageString],<{0}>,<{1}>", messageText, defaultText);
            return null;
        }

        public virtual bool Warning(string messageText) {
            Warnings.Add(messageText);
            Console.WriteLine("[Warning],<{0}>", messageText);
            return false;
        }

        public virtual bool Error(string id, string category, string targetObjectValue, string messageText) {
            Errors.Add("<{0}>,<{1}>,<{2}>,<{3}>".format( id, category, targetObjectValue, messageText));
            Console.WriteLine("[Error],<{0}>,<{1}>,<{2}>,<{3}>", id, category, targetObjectValue, messageText);
            return false;
        }

        public virtual bool Message(string messageText) {
            Console.WriteLine("[Message],<{0}>", messageText);
            return false;
        }

        public virtual bool Verbose(string messageText) {
            Console.WriteLine("[Verbose],<{0}>", messageText);
            return false;
        }

        public virtual bool Debug(string messageText) {
            Console.WriteLine("[Debug],<{0}>", messageText);
            return false;
        }

        public virtual int StartProgress(int parentActivityId, string messageText) {
            Console.WriteLine("[StartProgress],<{0}>,<{1}>", parentActivityId, messageText);
            return 0;
        }

        public virtual bool Progress(int activityId, int progressPercentage, string messageText) {
            Console.WriteLine("[Progress],<{0}>,<{1}>,<{2}>", activityId, progressPercentage, messageText);
            return false;
        }

        public virtual bool CompleteProgress(int activityId, bool isSuccessful) {
            Console.WriteLine("[CompleteProgress],<{0}>,<{1}>", activityId, isSuccessful);
            return false;
        }

        public virtual IEnumerable<string> OptionKeys {
            get {
                Console.WriteLine("[OptionKeys]");
                return null;
            }
        }

        public virtual IEnumerable<string> GetOptionValues(string key) {
            Console.WriteLine("[GetOptionValues],<{0}>", key);
            return null;
        }

        public virtual IEnumerable<string> Sources {
            get {
                Console.WriteLine("[Sources]");
                return null;
            }
        }

        public virtual string CredentialUsername {
            get {
                Console.WriteLine("[CredentialUsername]");
                return null;
            }
        }

        public virtual SecureString CredentialPassword {
            get {
                Console.WriteLine("[CredentialPassword]");
                return null;
            }
        }

        public virtual bool ShouldBootstrapProvider(string requestor, string providerName, string providerVersion, string providerType, string location, string destination) {
            Console.WriteLine("[ShouldBootstrapProvider],<{0}>,<{1}>,<{2}>,<{3}>,<{4}>,<{5}>", requestor, providerName, providerVersion, providerType, location, destination);
            return true;
        }

        public virtual bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource) {
            Console.WriteLine("[ShouldContinueWithUntrustedPackageSource],<{0}>,<{1}>", package, packageSource);
            return true;
        }

        public virtual bool AskPermission(string permission) {
            Console.WriteLine("[AskPermission],<{0}>", permission);
            return false;
        }

        public virtual bool IsInteractive {
            get {
                Console.WriteLine("[IsInteractive]");
                return true; // this way, it tries to bootstrap when asked.
            }
        }

        public virtual int CallCount {
            get {
                Console.WriteLine("[CallCount]");
                return _callCount;
            }
        }
    }
}
