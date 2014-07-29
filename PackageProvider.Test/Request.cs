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

namespace Microsoft.OneGet.PackageProvider.Test {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Proxies;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using Callback = System.Object;
    public abstract class Request : IDisposable {

        #region copy core-apis

        /* Synced/Generated code =================================================== */
        /// <summary>
        ///     The provider can query to see if the operation has been cancelled.
        ///     This provides for a gentle way for the caller to notify the callee that
        ///     they don't want any more results.
        /// </summary>
        /// <returns>returns TRUE if the operation has been cancelled.</returns>
        public abstract bool IsCancelled();

        /// <summary>
        ///     Returns a reference to the PackageManagementService API
        ///     The consumer of this function should either use this as a dynamic object
        ///     Or DuckType it to an interface that resembles IPacakgeManagementService
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public abstract object GetPackageManagementService(Object c);

        /// <summary>
        ///     Returns the type for a Request/Callback that the OneGet Core is expecting
        ///     This is (currently) neccessary to provide an appropriately-typed version
        ///     of the Request to the core when a Plugin is calling back into the core
        ///     and has to pass a Callback.
        /// </summary>
        /// <returns></returns>
        public abstract Type GetIRequestInterface();

        public abstract bool NotifyBeforePackageInstall(string packageName, string version, string source, string destination);

        public abstract bool NotifyPackageInstalled(string packageName, string version, string source, string destination);

        public abstract bool NotifyBeforePackageUninstall(string packageName, string version, string source, string destination);

        public abstract bool NotifyPackageUninstalled(string packageName, string version, string source, string destination);

        public abstract string GetCanonicalPackageId(string providerName, string packageName, string version);

        public abstract string ParseProviderName(string canonicalPackageId);

        public abstract string ParsePackageName(string canonicalPackageId);

        public abstract string ParsePackageVersion(string canonicalPackageId);
        #endregion

        #region copy host-apis

        /* Synced/Generated code =================================================== */
        public abstract string GetMessageString(string messageText);

        public abstract bool Warning(string messageText);

        public abstract bool Error(string id, string category, string targetObjectValue, string messageText);

        public abstract bool Message(string messageText);

        public abstract bool Verbose(string messageText);

        public abstract bool Debug(string messageText);

        public abstract int StartProgress(int parentActivityId, string messageText);

        public abstract bool Progress(int activityId, int progressPercentage, string messageText);

        public abstract bool CompleteProgress(int activityId, bool isSuccessful);

        /// <summary>
        ///     Used by a provider to request what metadata keys were passed from the user
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> GetOptionKeys(int category);

        public abstract IEnumerable<string> GetOptionValues(int category, string key);

        public abstract IEnumerable<string> GetSources();

        public abstract string GetCredentialUsername();

        public abstract string GetCredentialPassword();

        public abstract bool ShouldContinueWithUntrustedPackageSource(string package, string packageSource);

        public abstract bool ShouldProcessPackageInstall(string packageName, string version, string source);

        public abstract bool ShouldProcessPackageUninstall(string packageName, string version);

        public abstract bool ShouldContinueAfterPackageInstallFailure(string packageName, string version, string source);

        public abstract bool ShouldContinueAfterPackageUninstallFailure(string packageName, string version, string source);

        public abstract bool ShouldContinueRunningInstallScript(string packageName, string version, string source, string scriptLocation);

        public abstract bool ShouldContinueRunningUninstallScript(string packageName, string version, string source, string scriptLocation);

        public abstract bool AskPermission(string permission);
        #endregion

        #region copy service-apis

        /* Synced/Generated code =================================================== */
        public abstract void DownloadFile(Uri remoteLocation, string localFilename, Object c);

        public abstract bool IsSupportedArchive(string localFilename, Object c);

        public abstract IEnumerable<string> UnpackArchive(string localFilename, string destinationFolder, Object c);

        public abstract void AddPinnedItemToTaskbar(string item, Object c);

        public abstract void RemovePinnedItemFromTaskbar(string item, Object c);

        public abstract void CreateShortcutLink(string linkPath, string targetPath, string description, string workingDirectory, string arguments, Object c);

        public abstract void SetEnvironmentVariable(string variable, string value, int context, Object c);

        public abstract void RemoveEnvironmentVariable(string variable, int context, Object c);

        public abstract void CopyFile(string sourcePath, string destinationPath, Object c);

        public abstract void Delete(string path, Object c);

        public abstract void DeleteFolder(string folder, Object c);

        public abstract void CreateFolder(string folder, Object c);

        public abstract void DeleteFile(string filename, Object c);

        public abstract string GetKnownFolder(string knownFolder, Object c);

        public abstract bool IsElevated(Object c);
        #endregion

        #region copy response-apis

        /* Synced/Generated code =================================================== */

        /// <summary>
        ///     The provider can query to see if the operation has been cancelled.
        ///     This provides for a gentle way for the caller to notify the callee that
        ///     they don't want any more results. It's essentially just !IsCancelled
        /// </summary>
        /// <returns>returns FALSE if the operation has been cancelled.</returns>
        public abstract bool OkToContinue();

        /// <summary>
        ///     Used by a provider to return fields for a SoftwareIdentity.
        /// </summary>
        /// <param name="fastPath"></param>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="versionScheme"></param>
        /// <param name="summary"></param>
        /// <param name="source"></param>
        /// <param name="searchKey"></param>
        /// <param name="fullPath"></param>
        /// <param name="packageFileName"></param>
        /// <returns></returns>
        public abstract bool YieldSoftwareIdentity(string fastPath, string name, string version, string versionScheme, string summary, string source, string searchKey, string fullPath, string packageFileName);

        public abstract bool YieldSoftwareMetadata(string parentFastPath, string name, string value);

        public abstract bool YieldEntity(string parentFastPath, string name, string regid, string role, string thumbprint);

        public abstract bool YieldLink(string parentFastPath, string referenceUri, string relationship, string mediaType, string ownership, string use, string appliesToMedia, string artifact);

        #if M2
        public abstract bool YieldSwidtag(string fastPath, string xmlOrJsonDoc);

        public abstract bool YieldMetadata(string fieldId, string @namespace, string name, string value);

        #endif 

        /// <summary>
        ///     Used by a provider to return fields for a package source (repository)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="location"></param>
        /// <param name="isTrusted"></param>
        /// <param name="isRegistered"></param>
        /// <param name="isValidated"></param>
        /// <returns></returns>
        public abstract bool YieldPackageSource(string name, string location, bool isTrusted,bool isRegistered, bool isValidated);

        /// <summary>
        ///     Used by a provider to return the fields for a Metadata Definition
        ///     The cmdlets can use this to supply tab-completion for metadata to the user.
        /// </summary>
        /// <param name="category"> one of ['provider', 'source', 'package', 'install']</param>
        /// <param name="name">the provider-defined name of the option</param>
        /// <param name="expectedType"> one of ['string','int','path','switch']</param>
        /// <param name="isRequired">if the parameter is mandatory</param>
        /// <returns></returns>
        public abstract bool YieldDynamicOption(int category, string name, int expectedType, bool isRequired);

        public abstract bool YieldKeyValuePair(string key, string value);

        public abstract bool YieldValue(string value);
        #endregion

        #region copy Request-implementation
/* Synced/Generated code =================================================== */

        public bool Warning(string messageText, params object[] args) {
            return Warning(FormatMessageString(messageText,args));
        }

        internal bool Error( ErrorCategory category, string targetObjectValue, string messageText, params object[] args) {
            return Error(messageText, category.ToString(), targetObjectValue, FormatMessageString(messageText, args));
        }

        internal bool ThrowError(ErrorCategory category, string targetObjectValue, string messageText, params object[] args) {
            Error(messageText, category.ToString(), targetObjectValue, FormatMessageString(messageText, args));
            throw new Exception("MSG:TerminatingError");
        }

        public bool Message(string messageText, params object[] args) {
            return Message(FormatMessageString(messageText,args));
        }

        public bool Verbose(string messageText, params object[] args) {
            return Verbose(FormatMessageString(messageText,args));
        } 

        public bool Debug(string messageText, params object[] args) {
            return Debug(FormatMessageString(messageText,args));
        }

        public int StartProgress(int parentActivityId, string messageText, params object[] args) {
            return StartProgress(parentActivityId, FormatMessageString(messageText,args));
        }

        public bool Progress(int activityId, int progressPercentage, string messageText, params object[] args) {
            return Progress(activityId, progressPercentage, FormatMessageString(messageText,args));
        }

        private static string FixMeFormat(string formatString, object[] args) {
            if (args == null || args.Length == 0 ) {
                // not really any args, and not really expectng any
                return formatString.Replace('{', '\u00ab').Replace('}', '\u00bb');
            }
            return System.Linq.Enumerable.Aggregate(args, "FIXME/Format:" + formatString.Replace('{', '\u00ab').Replace('}', '\u00bb'), (current, arg) => current + string.Format(CultureInfo.CurrentCulture," \u00ab{0}\u00bb", arg));
        }

        internal string GetMessageStringInternal(string messageText) {
            return Resources.ResourceManager.GetString(messageText);
        }

        internal string FormatMessageString(string messageText, object[] args) {
            if (string.IsNullOrEmpty(messageText)) {
                return string.Empty;
            }

            if (messageText.StartsWith(Constants.MSGPrefix, true, CultureInfo.CurrentCulture)) {
                // check with the caller first, then with the local resources, and fallback to using the messageText itself.
                messageText = GetMessageString(messageText.Substring(Constants.MSGPrefix.Length)) ?? GetMessageStringInternal(messageText) ?? messageText;    
            }

            // if it doesn't look like we have the correct number of parameters
            // let's return a fixmeformat string.
            var c = System.Linq.Enumerable.Count( System.Linq.Enumerable.Where(messageText.ToCharArray(), each => each == '{'));
            if (c < args.Length) {
                return FixMeFormat(messageText, args);
            }
            return string.Format(CultureInfo.CurrentCulture, messageText, args);
        }

        public SecureString Password {
            get {
                var p = GetCredentialPassword();
                if (p == null) {
                    return null;
                }
                return p.FromProtectedString("salt");
            }
        }

        public string Username {
            get {
                return  GetCredentialUsername();
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing) {

        }

        public static implicit operator MarshalByRefObject(Request req) {
            return req.RemoteThis;
        }

        public static MarshalByRefObject ToMarshalByRefObject(Request request) {
            return request.RemoteThis;
        }

        internal MarshalByRefObject RemoteThis {
            get {
                return Extend();
            }
        }

        internal MarshalByRefObject Extend(params object[] objects) {
            return RequestExtensions.Extend(this, GetIRequestInterface(), objects);
        }

        #endregion

    }

    #region copy requestextension-implementation
/* Synced/Generated code =================================================== */

    public static class RequestExtensions {
        private static dynamic _remoteDynamicInterface;
        private static dynamic _localDynamicInterface;

        /// <summary>
        ///  This is the Instance for DynamicInterface that we use when we're giving another AppDomain a remotable object.
        /// </summary>
        public static dynamic LocalDynamicInterface {
            get {
                return _localDynamicInterface ?? (_localDynamicInterface = Activator.CreateInstance(RemoteDynamicInterface.GetType()));
            }
        }

        /// <summary>
        /// The is the instance of the DynamicInteface service from the calling AppDomain
        /// </summary>
        public static dynamic RemoteDynamicInterface {
            get {
                return _remoteDynamicInterface;
            }
            set {
                if (_remoteDynamicInterface == null) {
                    _remoteDynamicInterface = value;
                }
            }
        }

        /// <summary>
        /// This is called to adapt an object from a foreign app domain to a known interface
        /// In this appDomain
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static T As<T>(this object instance) {
            return RemoteDynamicInterface.Create<T>(instance);
        }

        /// <summary>
        ///  This is called to adapt and extend an object that we wish to pass to a foreign app domain
        /// </summary>
        /// <param name="obj">The base object that we are passing</param>
        /// <param name="tInterface">the target interface (from the foreign appdomain)</param>
        /// <param name="objects">the overriding objects (may be anonymous objects with Delegates, or an object with methods)</param>
        /// <returns></returns>
        public static MarshalByRefObject Extend(this object obj, Type tInterface, params object[] objects) {
            return LocalDynamicInterface.Create(tInterface, objects, obj);
        }

        // more extensions

        /// <summary>
        ///     Encodes the string as an array of UTF8 bytes.
        /// </summary>
        /// <param name="text"> The text. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static byte[] ToByteArray(this string text) {
            return Encoding.UTF8.GetBytes(text);
        }

        /// <summary>
        ///     Creates a string from a collection of UTF8 bytes
        /// </summary>
        /// <param name="bytes"> The bytes. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static string ToUtf8String(this IEnumerable<byte> bytes) {
            var data = bytes.ToArray();
            try {
                return Encoding.UTF8.GetString(data);
            }
            finally {
                Array.Clear(data, 0, data.Length);
            }
        }

        public static string ToUnicodeString(this IEnumerable<byte> bytes) {
            var data = bytes.ToArray();
            try {
                return Encoding.Unicode.GetString(data);
            }
            finally {
                Array.Clear(data, 0, data.Length);
            }
        }

        public static string ToBase64(this string text) {
            if (text == null) {
                return null;
            }
            return Convert.ToBase64String(text.ToByteArray());
        }

        public static string FromBase64(this string text) {
            if (text == null) {
                return null;
            }
            return Convert.FromBase64String(text).ToUtf8String();
        }

        public static bool Is(this string str) {
            return !string.IsNullOrEmpty(str);
        }

        public static bool IsEmptyOrNull(this string str) {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsTrue(this string text) {
            return text.Is() && text.Equals("true", StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        ///     encrypts the given collection of bytes with the machine key and salt 
        /// </summary>
        /// <param name="binaryData"> The binary data. </param>
        /// <param name="salt"> The salt. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static IEnumerable<byte> ProtectBinaryForMachine(this IEnumerable<byte> binaryData, string salt) {
            var data = binaryData.ToArray();
            var s = salt.ToByteArray();
            try {
                return ProtectedData.Protect(data, s, DataProtectionScope.LocalMachine);
            }
            finally {
                Array.Clear(data, 0, data.Length);
                Array.Clear(s, 0, s.Length);
            }
        }

        /// <summary>
        ///     encrypts the given collection of bytes with the user key and salt
        /// </summary>
        /// <param name="binaryData"> The binary data. </param>
        /// <param name="salt"> The salt. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static IEnumerable<byte> ProtectBinaryForUser(this IEnumerable<byte> binaryData, string salt) {
            var data = binaryData.ToArray();
            var s = salt.ToByteArray();
            try {
                return ProtectedData.Protect(data, s, DataProtectionScope.CurrentUser);
            }
            finally {
                Array.Clear(data, 0, data.Length);
                Array.Clear(s, 0, s.Length);
            }
        }

        /// <summary>
        ///     encrypts the given string with the machine key and salt
        /// </summary>
        /// <param name="text"> The text. </param>
        /// <param name="salt"> The salt. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static IEnumerable<byte> ProtectForMachine(this string text, string salt) {
            var data = (text ?? String.Empty).ToByteArray();
            try {
                return ProtectBinaryForMachine(data, salt);
            }
            finally {
                Array.Clear(data, 0, data.Length);
            }
        }

        /// <summary>
        ///     encrypts the given string with the machine key and salt
        /// </summary>
        /// <param name="text"> The text. </param>
        /// <param name="salt"> The salt. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static IEnumerable<byte> ProtectForUser(this string text, string salt) {
            var data = (text ?? String.Empty).ToByteArray();
            try {
                return ProtectBinaryForUser(data, salt);
            }
            finally {
                Array.Clear(data, 0, data.Length);
            }
        }

        /// <summary>
        ///     decrypts the given collection of bytes with the user key and salt returns an empty collection of bytes on failure
        /// </summary>
        /// <param name="binaryData"> The binary data. </param>
        /// <param name="salt"> The salt. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static IEnumerable<byte> UnprotectBinaryForUser(this IEnumerable<byte> binaryData, string salt) {
            if (binaryData == null) {
                return Enumerable.Empty<byte>();
            }

            try {
                return ProtectedData.Unprotect(binaryData.ToArray(), salt.ToByteArray(), DataProtectionScope.CurrentUser);
            }
            catch {
                /* suppress */
            }
            return Enumerable.Empty<byte>();
        }

        /// <summary>
        ///     decrypts the given collection of bytes with the machine key and salt returns an empty collection of bytes on failure
        /// </summary>
        /// <param name="binaryData"> The binary data. </param>
        /// <param name="salt"> The salt. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static IEnumerable<byte> UnprotectBinaryForMachine(this IEnumerable<byte> binaryData, string salt) {
            if (binaryData == null) {
                return Enumerable.Empty<byte>();
            }

            try {
                return ProtectedData.Unprotect(binaryData.ToArray(), salt.ToByteArray(), DataProtectionScope.LocalMachine);
            }
            catch {
                /* suppress */
            }
            return Enumerable.Empty<byte>();
        }

        /// <summary>
        ///     decrypts the given collection of bytes with the user key and salt and returns a string from the UTF8 representation of the bytes. returns an empty string on failure
        /// </summary>
        /// <param name="binaryData"> The binary data. </param>
        /// <param name="salt"> The salt. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static string UnprotectForUser(this IEnumerable<byte> binaryData, string salt) {
            var data = binaryData.UnprotectBinaryForUser(salt).ToArray();
            return data.Any() ? data.ToUtf8String() : String.Empty;
        }

        /// <summary>
        ///     decrypts the given collection of bytes with the machine key and salt and returns a string from the UTF8 representation of the bytes. returns an empty string on failure
        /// </summary>
        /// <param name="binaryData"> The binary data. </param>
        /// <param name="salt"> The salt. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static string UnprotectForMachine(this IEnumerable<byte> binaryData, string salt) {
            var data = binaryData.UnprotectBinaryForMachine(salt).ToArray();
            return data.Any() ? data.ToUtf8String() : String.Empty;
        }

        public static string ToUnsecureString(this SecureString securePassword) {
            if (securePassword == null)
                throw new ArgumentNullException("securePassword");

            IntPtr unmanagedString = IntPtr.Zero;
            try {

                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public static SecureString ToSecureString(this string password) {
            if (password == null) {
                throw new ArgumentNullException("password");
            }

            var ss = new SecureString();
            foreach (var ch in password.ToCharArray()) {
                ss.AppendChar(ch);
            }

            return ss;
        }

        public static string ToProtectedString(this SecureString secureString, string salt) {
            return Convert.ToBase64String(secureString.ToBytes().ProtectBinaryForUser(salt).ToArray());
        }

        public static SecureString FromProtectedString(this string str, string salt) {
            return Convert.FromBase64String(str).UnprotectBinaryForUser(salt).ToUnicodeString().ToSecureString();
        }

        public static IEnumerable<byte> ToBytes(this SecureString securePassword) {
            if (securePassword == null)
                throw new ArgumentNullException("securePassword");

            var unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
            var ofs = 0;

            do {
                var x = Marshal.ReadByte(unmanagedString, ofs++);
                var y = Marshal.ReadByte(unmanagedString, ofs++);
                if (x == 0 && y == 0) {
                    break;
                }
                // now we have two bytes!
                yield return x;
                yield return y;
            } while (true);

            Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
        }
    }

    #endregion

}