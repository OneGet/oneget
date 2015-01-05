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

namespace Microsoft.OneGet.Utility.Platform {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Extensions;

    public static class EnvironmentUtility {
        private const Int32 HWND_BROADCAST = 0xffff;
        private const Int32 WM_SETTINGCHANGE = 0x001A;
        private const Int32 SMTO_ABORTIFHUNG = 0x0002;

        public static IEnumerable<string> SystemPath {
            get {
                var path = GetSystemEnvironmentVariable("PATH");
                return string.IsNullOrWhiteSpace(path) ? new string[] {} : path.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
            set {
                var newValue = value.ToPathString();
                if (newValue != GetSystemEnvironmentVariable("PATH")) {
                    SetSystemEnvironmentVariable("PATH", newValue);
                }
            }
        }

        public static IEnumerable<string> UserPath {
            get {
                var path = GetUserEnvironmentVariable("PATH");
                return string.IsNullOrWhiteSpace(path) ? new string[] {} : path.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
            set {
                var newValue = value.ToPathString();
                if (newValue != GetUserEnvironmentVariable("PATH")) {
                    SetUserEnvironmentVariable("PATH", newValue);
                }
            }
        }

        public static IEnumerable<string> Path {
            get {
                var path = GetEnvironmentVariable("PATH");
                return string.IsNullOrWhiteSpace(path) ? new string[] {} : path.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
            set {
                var newValue = value.ToPathString();
                if (newValue != GetEnvironmentVariable("PATH")) {
                    SetEnvironmentVariable("PATH", newValue);
                }
            }
        }

        public static void BroadcastChange() {
            Task.Factory.StartNew(() => {NativeMethods.SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, 0, "Environment", SMTO_ABORTIFHUNG, 1000, IntPtr.Zero);}, TaskCreationOptions.LongRunning);
        }

        public static string GetSystemEnvironmentVariable(string name) {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);
        }

        public static void SetSystemEnvironmentVariable(string name, string value) {
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Machine);
        }

        public static string GetUserEnvironmentVariable(string name) {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);
        }

        public static void SetUserEnvironmentVariable(string name, string value) {
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
        }

        public static string GetEnvironmentVariable(string name) {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        public static void SetEnvironmentVariable(string name, string value) {
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
        }

        public static string ToPathString(this IEnumerable<string> value) {
            return value.SafeAggregate((current, each) => current + ";" + each) ?? string.Empty;
        }

        public static string[] Append(this IEnumerable<string> searchPath, string pathToAdd) {
            var p = searchPath.ToArray();

            if (p.Any(s => s.EqualsIgnoreCase(pathToAdd))) {
                return p;
            }
            return p.Union(new[] {pathToAdd}).ToArray();
        }

        public static string[] Prepend(this IEnumerable<string> searchPath, string pathToAdd) {
            var p = searchPath.ToArray();

            if (p.Any(s => s.EqualsIgnoreCase(pathToAdd))) {
                return p;
            }

            return new[] {pathToAdd}.Union(p).ToArray();
        }

        public static string[] Remove(this string[] searchPath, string pathToRemove) {
            return searchPath.Where(s => !s.EqualsIgnoreCase(pathToRemove)).ToArray();
        }

        public static string[] RemoveMissingFolders(this string[] searchPath) {
            return searchPath.Where(Directory.Exists).ToArray();
        }

        public static void Rehash() {
            var system = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);
            var user = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User);

            // do system/shared variables first
            foreach (var key in system.Keys) {
                var value = system[key].ToString();
                if (string.IsNullOrWhiteSpace(value)) {
                    continue;
                }

                // merge path-like variables.
                if (key.ToString().IndexOf("path", StringComparison.OrdinalIgnoreCase) > -1 && user.Contains(key)) {
                    value = value + ";" + user[key];
                    user.Remove(key);
                }

                Environment.SetEnvironmentVariable(key.ToString(), value, EnvironmentVariableTarget.Process);
            }

            // do user variables next
            foreach (var key in user.Keys) {
                var value = user[key].ToString();
                if (string.IsNullOrWhiteSpace(value)) {
                    continue;
                }

                Environment.SetEnvironmentVariable(key.ToString(), value, EnvironmentVariableTarget.Process);
            }
        }
    }
}