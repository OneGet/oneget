//---------------------------------------------------------------------
// <copyright file="ZipFormat.cs" company="Microsoft Corporation">
//   Copyright (c) 1999, Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// Part of the Deployment Tools Foundation project.
// </summary>
//---------------------------------------------------------------------

namespace Microsoft.PackageManagement.Archivers.Internal.Compression.Zip
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Text;

    [Flags]
    internal enum ZipFileFlags : ushort
    {
        None = 0x0000,
        Encrypt = 0x0001,
        CompressOption1 = 0x0002,
        CompressOption2 = 0x0004,
        DataDescriptor = 0x0008,
        StrongEncrypt = 0x0040,
        UTF8 = 0x0800
    }

    internal enum ZipExtraFileFieldType : ushort
    {
        ZIP64 = 0x0001,
        NTFS_TIMES = 0x000A,
        NTFS_ACLS = 0x4453,
        EXTIME = 0x5455
    }

    internal class ZipFileHeader
    {
        public const uint LFHSIG = 0x04034B50;
        public const uint CFHSIG = 0x02014B50;

        public const uint SPANSIG = 0x08074b50;
        public const uint SPANSIG2 = 0x30304b50;

        public const uint LFH_FIXEDSIZE = 30;
        public const uint CFH_FIXEDSIZE = 46;

        public ushort versionMadeBy;
        public ushort versionNeeded;
        public ZipFileFlags flags;
        public ZipCompressionMethod compressionMethod;
        public short lastModTime;
        public short lastModDate;
        public uint crc32;
        public uint compressedSize;
        public uint uncompressedSize;
        public ushort diskStart;
        public ushort internalFileAttrs;
        public uint externalFileAttrs;
        public uint localHeaderOffset;
        public string fileName;
        public ZipExtraFileField[] extraFields;
        public string fileComment;
        public bool zip64;

        public ZipFileHeader()
        {
            versionMadeBy = 20;
            versionNeeded = 20;
        }

        public ZipFileHeader(ZipFileInfo fileInfo, bool zip64)
            : this()
        {
            flags = ZipFileFlags.None;
            compressionMethod = fileInfo.CompressionMethod;
            fileName = Path.Combine(fileInfo.Path, fileInfo.Name);
            CompressionEngine.DateTimeToDosDateAndTime(
                fileInfo.LastWriteTime, out lastModDate, out lastModTime);
            this.zip64 = zip64;

            if (this.zip64)
            {
                compressedSize = uint.MaxValue;
                uncompressedSize = uint.MaxValue;
                diskStart = ushort.MaxValue;
                versionMadeBy = 45;
                versionNeeded = 45;
                ZipExtraFileField field = new ZipExtraFileField
                {
                    fieldType = ZipExtraFileFieldType.ZIP64
                };
                field.SetZip64Data(
                    fileInfo.CompressedLength,
                    fileInfo.Length,
                    0,
                    fileInfo.ArchiveNumber);
                extraFields = new ZipExtraFileField[] { field };
            }
            else
            {
                compressedSize = (uint)fileInfo.CompressedLength;
                uncompressedSize = (uint)fileInfo.Length;
                diskStart = (ushort)fileInfo.ArchiveNumber;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "compressedSize")]
        [SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "uncompressedSize")]
        [SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "crc32")]
        [SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "localHeaderOffset")]
        public void Update(
            long compressedSize,
            long uncompressedSize,
            uint crc32,
            long localHeaderOffset,
            int archiveNumber)
        {
            this.crc32 = crc32;

            if (zip64)
            {
                this.compressedSize = uint.MaxValue;
                this.uncompressedSize = uint.MaxValue;
                this.localHeaderOffset = uint.MaxValue;
                diskStart = ushort.MaxValue;

                if (extraFields != null)
                {
                    foreach (ZipExtraFileField field in extraFields)
                    {
                        if (field.fieldType == ZipExtraFileFieldType.ZIP64)
                        {
                            field.SetZip64Data(
                                compressedSize,
                                uncompressedSize,
                                localHeaderOffset,
                                archiveNumber);
                        }
                    }
                }
            }
            else
            {
                this.compressedSize = (uint)compressedSize;
                this.uncompressedSize = (uint)uncompressedSize;
                this.localHeaderOffset = (uint)localHeaderOffset;
                diskStart = (ushort)archiveNumber;
            }
        }

        public bool Read(Stream stream, bool central)
        {
            long startPos = stream.Position;

            if (stream.Length - startPos <
                (central ? CFH_FIXEDSIZE : LFH_FIXEDSIZE))
            {
                return false;
            }

            BinaryReader reader = new BinaryReader(stream);
            uint sig = reader.ReadUInt32();

            if (sig == SPANSIG || sig == SPANSIG2)
            {
                // Spanned zip files may optionally begin with a special marker.
                // Just ignore it and move on.
                sig = reader.ReadUInt32();
            }

            if (sig != (central ? CFHSIG : LFHSIG))
            {
                return false;
            }

            versionMadeBy = (central ? reader.ReadUInt16() : (ushort)0);
            versionNeeded = reader.ReadUInt16();
            flags = (ZipFileFlags)reader.ReadUInt16();
            compressionMethod = (ZipCompressionMethod)reader.ReadUInt16();
            lastModTime = reader.ReadInt16();
            lastModDate = reader.ReadInt16();
            crc32 = reader.ReadUInt32();
            compressedSize = reader.ReadUInt32();
            uncompressedSize = reader.ReadUInt32();

            zip64 = uncompressedSize == uint.MaxValue;

            int fileNameLength = reader.ReadUInt16();
            int extraFieldLength = reader.ReadUInt16();
            int fileCommentLength;

            if (central)
            {
                fileCommentLength = reader.ReadUInt16();

                diskStart = reader.ReadUInt16();
                internalFileAttrs = reader.ReadUInt16();
                externalFileAttrs = reader.ReadUInt32();
                localHeaderOffset = reader.ReadUInt32();
            }
            else
            {
                fileCommentLength = 0;
                diskStart = 0;
                internalFileAttrs = 0;
                externalFileAttrs = 0;
                localHeaderOffset = 0;
            }

            if (stream.Length - stream.Position <
                fileNameLength + extraFieldLength + fileCommentLength)
            {
                return false;
            }

#if CORECLR
            Encoding headerEncoding = Encoding.UTF8;
#else
            Encoding headerEncoding = ((flags | ZipFileFlags.UTF8) != 0 ?
                Encoding.UTF8 : Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage));
#endif

            byte[] fileNameBytes = reader.ReadBytes(fileNameLength);
            fileName = headerEncoding.GetString(fileNameBytes).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            List<ZipExtraFileField> fields = new List<ZipExtraFileField>();
            while (extraFieldLength > 0)
            {
                ZipExtraFileField field = new ZipExtraFileField();
                if (!field.Read(stream, ref extraFieldLength))
                {
                    return false;
                }
                fields.Add(field);
                if (field.fieldType == ZipExtraFileFieldType.ZIP64)
                {
                    zip64 = true;
                }
            }
            extraFields = fields.ToArray();

            byte[] fileCommentBytes = reader.ReadBytes(fileCommentLength);
            fileComment = headerEncoding.GetString(fileCommentBytes);

            return true;
        }

        public void Write(Stream stream, bool central)
        {
            byte[] fileNameBytes = (fileName != null
                ? Encoding.UTF8.GetBytes(fileName) : new byte[0]);
            byte[] fileCommentBytes = (fileComment != null
                ? Encoding.UTF8.GetBytes(fileComment) : new byte[0]);
            bool useUtf8 =
                (fileName != null && fileNameBytes.Length > fileName.Length) ||
                (fileComment != null && fileCommentBytes.Length > fileComment.Length);
            if (useUtf8)
            {
                flags |= ZipFileFlags.UTF8;
            }

            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(central ? CFHSIG : LFHSIG);
            if (central)
            {
                writer.Write(versionMadeBy);
            }
            writer.Write(versionNeeded);
            writer.Write((ushort)flags);
            writer.Write((ushort)compressionMethod);
            writer.Write(lastModTime);
            writer.Write(lastModDate);
            writer.Write(crc32);
            writer.Write(compressedSize);
            writer.Write(uncompressedSize);

            ushort extraFieldLength = 0;
            if (extraFields != null)
            {
                foreach (ZipExtraFileField field in extraFields)
                {
                    if (field.data != null)
                    {
                        extraFieldLength += (ushort)(4 + field.data.Length);
                    }
                }
            }

            writer.Write((ushort)fileNameBytes.Length);
            writer.Write(extraFieldLength);

            if (central)
            {
                writer.Write((ushort)fileCommentBytes.Length);

                writer.Write(diskStart);
                writer.Write(internalFileAttrs);
                writer.Write(externalFileAttrs);
                writer.Write(localHeaderOffset);
            }

            writer.Write(fileNameBytes);

            if (extraFields != null)
            {
                foreach (ZipExtraFileField field in extraFields)
                {
                    if (field.data != null)
                    {
                        field.Write(stream);
                    }
                }
            }

            if (central)
            {
                writer.Write(fileCommentBytes);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "compressedSize")]
        [SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "uncompressedSize")]
        [SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "crc32")]
        [SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "localHeaderOffset")]
        public void GetZip64Fields(
            out long compressedSize,
            out long uncompressedSize,
            out long localHeaderOffset,
            out int archiveNumber,
            out uint crc)
        {
            compressedSize = this.compressedSize;
            uncompressedSize = this.uncompressedSize;
            localHeaderOffset = this.localHeaderOffset;
            archiveNumber = diskStart;
            crc = crc32;

            foreach (ZipExtraFileField field in extraFields)
            {
                if (field.fieldType == ZipExtraFileFieldType.ZIP64)
                {
                    field.GetZip64Data(
                        out compressedSize,
                        out uncompressedSize,
                        out localHeaderOffset,
                        out archiveNumber);
                }
            }
        }

        public ZipFileInfo ToZipFileInfo()
        {
            string name = fileName;
            GetZip64Fields(
                out long compressedSizeL,
                out long uncompressedSizeL,
                out long localHeaderOffsetL,
                out int archiveNumberL,
                out uint crc);

            CompressionEngine.DosDateAndTimeToDateTime(
                lastModDate,
                lastModTime,
                out DateTime dateTime);
            FileAttributes attrs = FileAttributes.Normal;
            // TODO: look for attrs or times in extra fields

            return new ZipFileInfo(name, archiveNumberL, attrs, dateTime,
                uncompressedSizeL, compressedSizeL, compressionMethod);
        }

        public bool IsDirectory => fileName != null &&
                    (fileName.EndsWith("/", StringComparison.Ordinal) ||
                    fileName.EndsWith("\\", StringComparison.Ordinal));

        public int GetSize(bool central)
        {
            int size = 30;

            int fileNameSize = (fileName != null
                ? Encoding.UTF8.GetByteCount(fileName) : 0);
            size += fileNameSize;

            if (extraFields != null)
            {
                foreach (ZipExtraFileField field in extraFields)
                {
                    if (field.data != null)
                    {
                        size += 4 + field.data.Length;
                    }
                }
            }

            if (central)
            {
                size += 16;

                int fileCommentSize = (fileComment != null
                    ? Encoding.UTF8.GetByteCount(fileComment) : 0);
                size += fileCommentSize;
            }

            return size;
        }
    }

    internal class ZipExtraFileField
    {
        public ZipExtraFileFieldType fieldType;
        public byte[] data;

        public bool Read(Stream stream, ref int bytesRemaining)
        {
            if (bytesRemaining < 4)
            {
                return false;
            }

            BinaryReader reader = new BinaryReader(stream);

            fieldType = (ZipExtraFileFieldType)reader.ReadUInt16();
            ushort dataSize = reader.ReadUInt16();
            bytesRemaining -= 4;

            if (bytesRemaining < dataSize)
            {
                return false;
            }

            data = reader.ReadBytes(dataSize);
            bytesRemaining -= dataSize;

            return true;
        }

        public void Write(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)fieldType);

            byte[] dataBytes = (data ?? (new byte[0]));
            writer.Write((ushort)dataBytes.Length);
            writer.Write(dataBytes);
        }

        public bool GetZip64Data(
            out long compressedSize,
            out long uncompressedSize,
            out long localHeaderOffset,
            out int diskStart)
        {
            uncompressedSize = 0;
            compressedSize = 0;
            localHeaderOffset = 0;
            diskStart = 0;

            if (fieldType != ZipExtraFileFieldType.ZIP64 ||
                data == null || data.Length != 28)
            {
                return false;
            }

            using (MemoryStream dataStream = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(dataStream);
                uncompressedSize = reader.ReadInt64();
                compressedSize = reader.ReadInt64();
                localHeaderOffset = reader.ReadInt64();
                diskStart = reader.ReadInt32();
            }

            return true;
        }

        public bool SetZip64Data(
            long compressedSize,
            long uncompressedSize,
            long localHeaderOffset,
            int diskStart)
        {
            if (fieldType != ZipExtraFileFieldType.ZIP64)
            {
                return false;
            }

            using (MemoryStream dataStream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(dataStream);
                writer.Write(uncompressedSize);
                writer.Write(compressedSize);
                writer.Write(localHeaderOffset);
                writer.Write(diskStart);
                data = dataStream.ToArray();
            }

            return true;
        }
    }

    internal class ZipEndOfCentralDirectory
    {
        public const uint EOCDSIG = 0x06054B50;
        public const uint EOCD64SIG = 0x06064B50;

        public const uint EOCD_RECORD_FIXEDSIZE = 22;
        public const uint EOCD64_RECORD_FIXEDSIZE = 56;

        public ushort versionMadeBy;
        public ushort versionNeeded;
        public uint diskNumber;
        public uint dirStartDiskNumber;
        public long entriesOnDisk;
        public long totalEntries;
        public long dirSize;
        public long dirOffset;
        public string comment;
        public bool zip64;

        public ZipEndOfCentralDirectory()
        {
            versionMadeBy = 20;
            versionNeeded = 20;
        }

        public bool Read(Stream stream)
        {
            long startPos = stream.Position;

            if (stream.Length - startPos < EOCD_RECORD_FIXEDSIZE)
            {
                return false;
            }

            BinaryReader reader = new BinaryReader(stream);
            uint sig = reader.ReadUInt32();

            zip64 = false;
            if (sig != EOCDSIG)
            {
                if (sig == EOCD64SIG)
                {
                    zip64 = true;
                }
                else
                {
                    return false;
                }
            }

            if (zip64)
            {
                if (stream.Length - startPos < EOCD64_RECORD_FIXEDSIZE)
                {
                    return false;
                }

                long recordSize = reader.ReadInt64();
                versionMadeBy = reader.ReadUInt16();
                versionNeeded = reader.ReadUInt16();
                diskNumber = reader.ReadUInt32();
                dirStartDiskNumber = reader.ReadUInt32();
                entriesOnDisk = reader.ReadInt64();
                totalEntries = reader.ReadInt64();
                dirSize = reader.ReadInt64();
                dirOffset = reader.ReadInt64();

                // Ignore any extended zip64 eocd data.
                long exDataSize = recordSize + 12 - EOCD64_RECORD_FIXEDSIZE;

                if (stream.Length - stream.Position < exDataSize)
                {
                    return false;
                }

                stream.Seek(exDataSize, SeekOrigin.Current);

                comment = null;
            }
            else
            {
                diskNumber = reader.ReadUInt16();
                dirStartDiskNumber = reader.ReadUInt16();
                entriesOnDisk = reader.ReadUInt16();
                totalEntries = reader.ReadUInt16();
                dirSize = reader.ReadUInt32();
                dirOffset = reader.ReadUInt32();

                int commentLength = reader.ReadUInt16();

                if (stream.Length - stream.Position < commentLength)
                {
                    return false;
                }

                byte[] commentBytes = reader.ReadBytes(commentLength);
                comment = Encoding.UTF8.GetString(commentBytes);
            }

            return true;
        }

        public void Write(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            if (zip64)
            {
                writer.Write(EOCD64SIG);
                writer.Write((long)EOCD64_RECORD_FIXEDSIZE);
                writer.Write(versionMadeBy);
                writer.Write(versionNeeded);
                writer.Write(diskNumber);
                writer.Write(dirStartDiskNumber);
                writer.Write(entriesOnDisk);
                writer.Write(totalEntries);
                writer.Write(dirSize);
                writer.Write(dirOffset);
            }
            else
            {
                writer.Write(EOCDSIG);
                writer.Write((ushort)Math.Min(ushort.MaxValue, diskNumber));
                writer.Write((ushort)Math.Min(ushort.MaxValue, dirStartDiskNumber));
                writer.Write((ushort)Math.Min(ushort.MaxValue, entriesOnDisk));
                writer.Write((ushort)Math.Min(ushort.MaxValue, totalEntries));
                writer.Write((uint)Math.Min(uint.MaxValue, dirSize));
                writer.Write((uint)Math.Min(uint.MaxValue, dirOffset));

                byte[] commentBytes = (comment != null
                    ? Encoding.UTF8.GetBytes(comment) : new byte[0]);
                writer.Write((ushort)commentBytes.Length);
                writer.Write(commentBytes);
            }
        }

        public int GetSize(bool zip64Size)
        {
            if (zip64Size)
            {
                return 56;
            }
            else
            {
                int commentSize = (comment != null
                    ? Encoding.UTF8.GetByteCount(comment) : 0);
                return 22 + commentSize;
            }
        }
    }

    internal class Zip64EndOfCentralDirectoryLocator
    {
        public const uint EOCDL64SIG = 0x07064B50;

        public const uint EOCDL64_SIZE = 20;

        public uint dirStartDiskNumber;
        public long dirOffset;
        public uint totalDisks;

        public bool Read(Stream stream)
        {
            long startPos = stream.Position;
            if (stream.Length - startPos < EOCDL64_SIZE)
            {
                return false;
            }

            BinaryReader reader = new BinaryReader(stream);
            uint sig = reader.ReadUInt32();

            if (sig != EOCDL64SIG)
            {
                return false;
            }

            dirStartDiskNumber = reader.ReadUInt32();
            dirOffset = reader.ReadInt64();
            totalDisks = reader.ReadUInt32();

            return true;
        }

        public void Write(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(EOCDL64SIG);
            writer.Write(dirStartDiskNumber);
            writer.Write(dirOffset);
            writer.Write(totalDisks);
        }
    }
}