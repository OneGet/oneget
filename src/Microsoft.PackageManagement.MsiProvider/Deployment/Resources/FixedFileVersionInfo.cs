//---------------------------------------------------------------------
// <copyright file="FixedFileVersionInfo.cs" company="Microsoft Corporation">
//   Copyright (c) 1999, Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// Part of the Deployment Tools Foundation project.
// </summary>
//---------------------------------------------------------------------

namespace Microsoft.PackageManagement.Msi.Internal.Deployment.Resources
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    internal class FixedFileVersionInfo
    {
        public FixedFileVersionInfo()
        {
            // Set reasonable defaults
            signature = 0xFEEF04BD;
            structVersion = 0x00010000; // v1.0
            FileVersion = new Version(0, 0, 0, 0);
            ProductVersion = new Version(0, 0, 0, 0);
            FileFlagsMask = VersionBuildTypes.Debug | VersionBuildTypes.Prerelease;
            FileFlags = VersionBuildTypes.None;
            FileOS = VersionFileOS.NT_WINDOWS32;
            FileType = VersionFileType.Application;
            FileSubtype = VersionFileSubtype.Unknown;
            Timestamp = DateTime.MinValue;
        }

        private uint signature;
        private uint structVersion;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public Version FileVersion
        {
            get => fileVersion;

            set => fileVersion = value ?? throw new InvalidOperationException();
        }

        private Version fileVersion;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public Version ProductVersion
        {
            get => productVersion;

            set => productVersion = value ?? throw new InvalidOperationException();
        }

        private Version productVersion;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public VersionBuildTypes FileFlagsMask
        {
            get => fileFlagsMask;
            set => fileFlagsMask = value;
        }

        private VersionBuildTypes fileFlagsMask;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public VersionBuildTypes FileFlags
        {
            get => fileFlags;
            set => fileFlags = value;
        }

        private VersionBuildTypes fileFlags;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public VersionFileOS FileOS
        {
            get => fileOS;
            set => fileOS = value;
        }

        private VersionFileOS fileOS;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public VersionFileType FileType
        {
            get => fileType;
            set => fileType = value;
        }

        private VersionFileType fileType;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public VersionFileSubtype FileSubtype
        {
            get => fileSubtype;
            set => fileSubtype = value;
        }

        private VersionFileSubtype fileSubtype;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public DateTime Timestamp
        {
            get => timestamp;
            set => timestamp = value;
        }

        private DateTime timestamp;

        public void Read(BinaryReader reader)
        {
            signature = reader.ReadUInt32();
            structVersion = reader.ReadUInt32();
            fileVersion = UInt64ToVersion(reader.ReadUInt64());
            productVersion = UInt64ToVersion(reader.ReadUInt64());
            fileFlagsMask = (VersionBuildTypes)reader.ReadInt32();
            fileFlags = (VersionBuildTypes)reader.ReadInt32();
            fileOS = (VersionFileOS)reader.ReadInt32();
            fileType = (VersionFileType)reader.ReadInt32();
            fileSubtype = (VersionFileSubtype)reader.ReadInt32();
            timestamp = UInt64ToDateTime(reader.ReadUInt64());
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(signature);
            writer.Write(structVersion);
            writer.Write(VersionToUInt64(fileVersion));
            writer.Write(VersionToUInt64(productVersion));
            writer.Write((int)fileFlagsMask);
            writer.Write((int)fileFlags);
            writer.Write((int)fileOS);
            writer.Write((int)fileType);
            writer.Write((int)fileSubtype);
            writer.Write(DateTimeToUInt64(timestamp));
        }

        public static explicit operator FixedFileVersionInfo(byte[] bytesValue)
        {
            FixedFileVersionInfo ffviValue = new FixedFileVersionInfo();
            using (BinaryReader reader = new BinaryReader(new MemoryStream(bytesValue, false)))
            {
                ffviValue.Read(reader);
            }
            return ffviValue;
        }

        public static explicit operator byte[] (FixedFileVersionInfo ffviValue)
        {
            const int FFVI_LENGTH = 52;

            byte[] bytesValue = new byte[FFVI_LENGTH];
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream(bytesValue, true)))
            {
                ffviValue.Write(writer);
            }
            return bytesValue;
        }

        private static Version UInt64ToVersion(ulong version)
        {
            return new Version((int)((version >> 16) & 0xFFFF), (int)(version & 0xFFFF), (int)(version >> 48), (int)((version >> 32) & 0xFFFF));
        }

        private static ulong VersionToUInt64(Version version)
        {
            return (((ulong)(ushort)version.Major) << 16) | (ushort)version.Minor
                | (((ulong)(ushort)version.Build) << 48) | (((ulong)(ushort)version.Revision) << 32);
        }

        private static DateTime UInt64ToDateTime(ulong dateTime)
        {
            return (dateTime == 0 ? DateTime.MinValue : DateTime.FromFileTime((long)dateTime));
        }

        private static ulong DateTimeToUInt64(DateTime dateTime)
        {
            return (dateTime == DateTime.MinValue ? 0 : (ulong)dateTime.ToFileTime());
        }
    }
}