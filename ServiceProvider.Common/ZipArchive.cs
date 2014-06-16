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


#if FRAMEWORKv40


namespace Microsoft.OneGet.Core.Api.Compression {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.IO.Packaging;
    using System.Linq;
    using System.Reflection;
    using Extensions;



    internal class ReflectedClass : IDisposable {
        protected readonly object _actual;
        protected readonly Type _actualType;

        internal ReflectedClass(object actual) {
            _actual = actual;
            _actualType = _actual.GetType();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal MethodInfo GetMethod(string name) {
            return _actualType.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        internal object GetProperty(string name) {
            return _actualType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(_actual, null);
        }

        public void Dispose(bool disposing) {
            if (disposing) {
                if (_actual is IDisposable) {
                    (_actual as IDisposable).Dispose();
                }
            }
        }
    }

    internal static class PrivateProxyExtensions {
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public;

        internal static dynamic AsPrivateProxy(this object obj) {
            return new PrivateProxy(obj);
        }

        internal static object CallPrivateStaticMethod(this Type type, string methodName, params object[] args) {
            var parameterTypes = args.Select(each => each == null ? typeof (object) : each.GetType());
            var method = type.GetMethod(methodName, BindingFlags, null, parameterTypes.ToArray(), null) ?? type.GetMethod(methodName, BindingFlags);
            if (method != null) {
                return method.Invoke(null, args);
            }
            return null;
        }
    }

    internal class PrivateProxy : DynamicObject {
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public;
        private readonly object _actual;

        public PrivateProxy(object o) {
            _actual = o;
        }

        public static dynamic FromType(Assembly assembly, string type, params object[] args) {
            var targetType = assembly.GetTypes().FirstOrDefault(item => item.Name == type);
            if (targetType == null) {
                throw new Exception("Unknown type {0} in Assembly {1}".format(type, assembly.Location));
            }

            var constructor = targetType.GetConstructor(BindingFlags, null, args.Select(a => a.GetType()).ToArray(), null);
            if (constructor != null) {
                return new PrivateProxy(constructor.Invoke(args));
            }

            return null;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            var parameterTypes = args.Select(each => each == null ? typeof (object) : each.GetType());
            var method = _actual.GetType().GetMethod(binder.Name, BindingFlags, null, parameterTypes.ToArray(), null) ?? _actual.GetType().GetMethod(binder.Name, BindingFlags);

            if (method == null) {
                return base.TryInvokeMember(binder, args, out result);
            }

            result = method.Invoke(_actual, args);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            var propertyInfo = _actual.GetType().GetProperty(binder.Name, BindingFlags);
            if (propertyInfo != null) {
                result = propertyInfo.GetValue(_actual, null);
                return true;
            }

            var fieldInfo = _actual.GetType().GetField(binder.Name, BindingFlags);
            if (fieldInfo != null) {
                result = fieldInfo.GetValue(_actual);
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            var propertyInfo = _actual.GetType().GetProperty(binder.Name, BindingFlags);
            if (propertyInfo != null) {
                propertyInfo.SetValue(_actual, value, null);
                return true;
            }

            var fieldInfo = _actual.GetType().GetField(binder.Name, BindingFlags);
            if (fieldInfo != null) {
                fieldInfo.SetValue(_actual, value);
                return true;
            }
            return base.TrySetMember(binder, value);
        }
    }

    internal static class ReflectionExtensions {
        internal static MethodInfo GetMethod(this Type type, string name) {
            return type.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }

    internal class ZipFileInfo {
        private readonly dynamic _info;

        internal ZipFileInfo(object o) {
            _info = o.AsPrivateProxy();
        }

        public string Name {
            get {
                return _info.Name;
            }
        }

        public DateTime LastModFileDateTime {
            get {
                return _info.LastModFileDateTime;
            }
        }

        public bool FolderFlag {
            get {
                return _info.FolderFlag;
            }
        }

        public bool VolumeLabelFlag {
            get {
                return _info.VolumeLabelFlag;
            }
        }

        internal Stream GetStream(FileMode mode, FileAccess access) {
            return _info.GetStream(mode, access);
        }

        public override string ToString() {
            return Name;
        }
    }

    internal class ZipArchive : IDisposable {
        private static readonly Type _zipArchiveType = typeof (Package).Assembly.GetType("MS.Internal.IO.Zip.ZipArchive");
        private readonly dynamic _archive;

        private ZipArchive(object o) {
            _archive = o.AsPrivateProxy();
        }

        public IEnumerable<ZipFileInfo> Files {
            get {
                return from object f in GetFiles() ?? Enumerable.Empty<object>() select new ZipFileInfo(f);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static ZipArchive OpenOnFile(string path, FileMode mode, FileAccess access, FileShare share, bool streaming) {
            return new ZipArchive(_zipArchiveType.CallPrivateStaticMethod("OpenOnFile", path, mode, access, share, streaming));
        }

        public static ZipArchive OpenOnStream(Stream stream, FileMode mode = FileMode.OpenOrCreate, FileAccess access = FileAccess.ReadWrite, bool streaming = false) {
            return new ZipArchive(_zipArchiveType.CallPrivateStaticMethod("OpenOnStream", stream, mode, access, streaming));
        }

        // internal ZipFileInfo AddFile(string zipFileName, CompressionMethodEnum compressionMethod, DeflateOptionEnum deflateOption);

        internal void Close() {
            _archive.Close();
        }

        internal void DeleteFile(string zipFileName) {
            _archive.DeleteFile(zipFileName);
        }

        private void Dispose(bool disposing) {
            if (disposing) {
                _archive.Dispose();
            }
        }

        internal bool FileExists(string zipFileName) {
            return _archive.FileExists(zipFileName);
        }

        internal void Flush() {
            _archive.Flush();
        }

        internal ZipFileInfo GetFile(string zipFileName) {
            return new ZipFileInfo(_archive.GetFile(zipFileName));
        }

        internal IEnumerable GetFiles() {
            return _archive.GetFiles() as IEnumerable;
        }
    }
}

#endif