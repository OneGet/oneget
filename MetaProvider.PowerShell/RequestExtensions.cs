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

namespace Microsoft.OneGet.MetaProvider.PowerShell {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using RequestImpl = System.MarshalByRefObject;
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
                return _remoteDynamicInterface ?? ( _remoteDynamicInterface = AppDomain.CurrentDomain.GetData("DynamicInteface"));
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