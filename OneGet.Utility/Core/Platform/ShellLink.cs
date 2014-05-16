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

namespace Microsoft.OneGet.Core.Platform {
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;

    public class ShellLink : IDisposable {
        private const int MaxPath = 260;
        private const int MaxArguments = 1024;
        private const int MaxDescription = 1024;

        private IShellLink _shellLink;
        private ShellLinkClass _shellLinkObject;

        public ShellLink() {
            _shellLinkObject = new ShellLinkClass();
            _shellLink = _shellLinkObject as IShellLink;
        }

        public ShellLink(string filename) : this() {
            if (File.Exists(filename)) {
                ((IPersistFile)_shellLink).Load(filename, (int)Stgm.Read);
            }
        }

        public String TargetPath {
            get {
                var findData = new Win32FindData();
                var sb = new StringBuilder(MaxPath);
                _shellLink.GetPath(sb, sb.Capacity, ref findData, Slgp.RawPath);
                return sb.ToString();
            }

            set {
                _shellLink.SetPath(value);
            }
        }

        public String WorkingDirectory {
            get {
                var sb = new StringBuilder(MaxPath);
                _shellLink.GetWorkingDirectory(sb, sb.Capacity);
                return sb.ToString();
            }

            set {
                _shellLink.SetWorkingDirectory(value);
            }
        }

        public String Description {
            get {
                var sb = new StringBuilder(MaxDescription);
                _shellLink.GetDescription(sb, sb.Capacity);
                return sb.ToString();
            }
            set {
                _shellLink.SetDescription(value);
            }
        }

        public String Arguments {
            get {
                var sb = new StringBuilder(MaxArguments);
                _shellLink.GetArguments(sb, sb.Capacity);
                return sb.ToString();
            }

            set {
                _shellLink.SetArguments(value);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_shellLink != null) {
                    Marshal.ReleaseComObject(_shellLink);
                    _shellLink = null;
                }

                if (_shellLinkObject != null) {
                    Marshal.ReleaseComObject(_shellLinkObject);
                    _shellLinkObject = null;
                }
            }
        }

        public static ShellLink CreateShortcut(string linkFilename, string targetFilename, string description, string workingDirectory, string arguments) {
            linkFilename = Path.GetFullPath(linkFilename);
            targetFilename = Path.GetFullPath(targetFilename);
            linkFilename = Path.HasExtension(linkFilename) ? linkFilename : linkFilename + ".lnk";

            return new ShellLink(linkFilename) {
                TargetPath = targetFilename,
                WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(targetFilename),
                Description = description ?? string.Empty,
                Arguments = arguments ?? string.Empty
            }.Save(linkFilename);
        }

        public ShellLink Save(string filename) {
            ((IPersistFile)_shellLink).Save(filename, true);
            return this;
        }
    }
}