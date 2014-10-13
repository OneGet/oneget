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

namespace Microsoft.OneGet.Utility.Extensions {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using Collections;

    public static class StringExtensions {
        private static readonly char[] _wildcardCharacters = new[] {
            '*', '?'
        };

        private static readonly Regex _escapeFilepathCharacters = new Regex(@"([\\|\$|\^|\{|\[|\||\)|\+|\.|\]|\}|\/])");

        private static string FixMeFormat(string formatString, object[] args) {
            return args.Aggregate(formatString.Replace('{', '\u00ab').Replace('}', '\u00bb'), (current, arg) => current + string.Format(CultureInfo.CurrentCulture, " \u00ab{0}\u00bb", arg));
        }

        // ReSharper disable InconsistentNaming
        /// <summary>
        ///     Formats the specified format string.
        /// </summary>
        /// <param name="formatString"> The format string. </param>
        /// <param name="args"> The args. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static string format(this string formatString, params object[] args) {
            if (args == null || args.Length == 0) {
                return formatString;
            }

            try {
                var replacedByName = false;
                // first, try to replace 
                formatString = new Regex(@"\$\{(?<macro>\w*?)\}").Replace(formatString, new MatchEvaluator((m) => {
                    var key = m.Groups["macro"].Value;

                    var p = args[0].GetType().GetProperty(key);
                    if (p != null) {
                        replacedByName = true;
                        return p.GetValue(args[0], null).ToString();
                    }
                    return "${{" + m.Groups["macro"].Value + "}}";
                }));

                // if it looks like it doesn't take parameters, (and yet we have args!)
                // let's return a fixmeformat string.
                if (!replacedByName && formatString.IndexOf('{') < 0) {
                    return FixMeFormat(formatString, args);
                }

                return String.Format(CultureInfo.CurrentCulture, formatString, args);
            } catch (Exception) {
                // if we got an exception, let's at least return a string that we can use to figure out what parameters should have been matched vs what was passed.
                return FixMeFormat(formatString, args);
            }
        }

        public static string formatWithIEnumerable(this string formatString, IEnumerable<object> args) {
            var arguments = args.ReEnumerable();
            if (arguments.IsNullOrEmpty()) {
                return formatString;
            }
            return string.Format(CultureInfo.CurrentCulture, formatString, arguments.ToArray());
        }

        public static bool Is(this string str) {
            return !string.IsNullOrEmpty(str);
        }

        public static bool IsEmptyOrNull(this string str) {
            return string.IsNullOrEmpty(str);
        }

        public static string DashedToCamelCase(this string dashedText, char separator) {
            return dashedText.IndexOf('-') == -1 ? dashedText : new string(dashedToCamelCase(dashedText, separator).ToArray());
        }

        public static string DashedToCamelCase(this string dashedText) {
            return dashedText.DashedToCamelCase('-');
        }

        private static IEnumerable<char> dashedToCamelCase(this string dashedText, char separator = '-', bool pascalCase = false) {
            var nextIsUpper = pascalCase;
            foreach (var ch in dashedText) {
                if (ch == '-') {
                    nextIsUpper = true;
                } else {
                    yield return nextIsUpper ? char.ToUpper(ch) : ch;
                    nextIsUpper = false;
                }
            }
        }

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
            } finally {
                Array.Clear(data, 0, data.Length);
            }
        }

        public static string ToUnicodeString(this IEnumerable<byte> bytes) {
            var data = bytes.ToArray();
            try {
                return Encoding.Unicode.GetString(data);
            } finally {
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

        public static bool IsTrue(this string text) {
            return text.Is() && text.Equals("true", StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        ///     coerces a string to an int32, defaults to zero.
        /// </summary>
        /// <param name="str"> The STR. </param>
        /// <param name="defaultValue"> The default value if the string isn't a valid int. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static int ToInt32(this string str, int defaultValue) {
            int i;
            return Int32.TryParse(str, out i) ? i : defaultValue;
        }

        public static bool EqualsIgnoreCase(this string str, string str2) {
            if (str == null && str2 == null) {
                return true;
            }

            if (str == null || str2 == null) {
                return false;
            }

            return str.Equals(str2, StringComparison.OrdinalIgnoreCase);
        }

        // ReSharper restore InconsistentNaming

        public static IEnumerable<string> Quote(this IEnumerable<string> items) {
            return items.Select(each => "'" + each + "'");
        }

        public static string JoinWithComma(this IEnumerable<string> items) {
            return items.JoinWith(",");
        }

        public static string JoinWith(this IEnumerable<string> items, string delimiter) {
            return items.SafeAggregate((current, each) => current + delimiter + each);
        }

        public static TSource SafeAggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func) {
            var src = source.ReEnumerable();
            if (source != null && src.Any()) {
                return src.Aggregate(func);
            }
            return default(TSource);
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
            } finally {
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
            } finally {
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
            } finally {
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
            } finally {
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
            } catch {
                /* suppress */
            }
            return Enumerable.Empty<byte>();
        }

        /// <summary>
        ///     decrypts the given collection of bytes with the machine key and salt returns an empty collection of bytes on
        ///     failure
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
            } catch {
                /* suppress */
            }
            return Enumerable.Empty<byte>();
        }

        /// <summary>
        ///     decrypts the given collection of bytes with the user key and salt and returns a string from the UTF8 representation
        ///     of the bytes. returns an empty string on failure
        /// </summary>
        /// <param name="binaryData"> The binary data. </param>
        /// <param name="salt"> The salt. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static string UnprotectForUser(this IEnumerable<byte> binaryData, string salt) {
            var data = binaryData.UnprotectBinaryForUser(salt).ReEnumerable();
            return data.Any() ? data.ToUtf8String() : String.Empty;
        }

        /// <summary>
        ///     decrypts the given collection of bytes with the machine key and salt and returns a string from the UTF8
        ///     representation of the bytes. returns an empty string on failure
        /// </summary>
        /// <param name="binaryData"> The binary data. </param>
        /// <param name="salt"> The salt. </param>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        public static string UnprotectForMachine(this IEnumerable<byte> binaryData, string salt) {
            var data = binaryData.UnprotectBinaryForMachine(salt).ReEnumerable();
            return data.Any() ? data.ToUtf8String() : String.Empty;
        }

        public static string ToUnsecureString(this SecureString securePassword) {
            if (securePassword == null) {
                throw new ArgumentNullException("securePassword");
            }

            var unmanagedString = IntPtr.Zero;
            try {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            } finally {
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
            if (securePassword == null) {
                throw new ArgumentNullException("securePassword");
            }

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

        public static bool ContainsIgnoreCase(this IEnumerable<string> collection, string value) {
            if (collection == null) {
                return false;
            }
            return collection.Any(s => s.EqualsIgnoreCase(value));
        }

        public static bool ContainsAnyOfIgnoreCase(this IEnumerable<string> collection, params object[] values) {
            return collection.ContainsAnyOfIgnoreCase(values.Select(value => value == null ? null : value.ToString()));
        }

        public static bool ContainsAnyOfIgnoreCase(this IEnumerable<string> collection, IEnumerable<string> values) {
            if (collection == null) {
                return false;
            }
            var set = values.ReEnumerable();

            return collection.Any(set.ContainsIgnoreCase);
        }

        private static Regex WildcardToRegex(string wildcard, string noEscapePrefix = "^") {
            return new Regex(noEscapePrefix + _escapeFilepathCharacters.Replace(wildcard, "\\$1")
                .Replace("?", @".")
                .Replace("**", @"?")
                .Replace("*", @"[^\\\/\<\>\|]*")
                .Replace("?", @".*") + '$', RegexOptions.IgnoreCase);
        }

        /// <summary>
        ///     Determines whether the specified input has wildcards.
        /// </summary>
        /// <param name="input"> The input. </param>
        /// <returns>
        ///     <c>true</c> if the specified input has wildcards; otherwise, <c>false</c> .
        /// </returns>
        /// <remarks>
        /// </remarks>
        public static bool ContainsWildcards(this string input) {
            return input.IndexOfAny(_wildcardCharacters) > -1;
        }

        public static bool IsWildcardMatch(this string input, string wildcardMask) {
            if (input == null || string.IsNullOrEmpty(wildcardMask)) {
                return false;
            }
            return WildcardToRegex(wildcardMask).IsMatch(input);
        }

        private static byte FromHexChar(this char c) {
            if ((c >= 'a') && (c <= 'f')) {
                return (byte)(c - 'a' + 10);
            }
            if ((c >= 'A') && (c <= 'F')) {
                return (byte)(c - 'A' + 10);
            }
            if ((c >= '0') && (c <= '9')) {
                return (byte)(c - '0');
            }
            throw new ArgumentException("invalid hex char");
        }

        public static byte[] FromHex(this string hex) {
            if (string.IsNullOrEmpty(hex)) {
                return new byte[0];
            }

            if ((hex.Length & 0x1) == 0x1) {
                throw new ArgumentException("Length must be a multiple of 2");
            }
            var input = hex.ToCharArray();
            var result = new byte[hex.Length >> 1];

            for (var i = 0; i < input.Length; i += 2) {
                result[i >> 1] = (byte)(((byte)(FromHexChar(input[i]) << 4)) | FromHexChar(input[i + 1]));
            }

            return result;
        }
    }
}