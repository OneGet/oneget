//---------------------------------------------------------------------
// <copyright file="ZipPacker.cs" company="Microsoft Corporation">
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
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using CompressionLevel = Compression.CompressionLevel;

    public partial class ZipEngine
    {
        /// <summary>
        /// Creates a zip archive or chain of zip archives.
        /// </summary>
        /// <param name="streamContext">A context interface to handle opening
        /// and closing of archive and file streams.</param>
        /// <param name="files">An array of file lists.  Each list is
        /// compressed into one stream in the archive.</param>
        /// <param name="maxArchiveSize">The maximum number of bytes for one archive
        /// before the contents are chained to the next archive, or zero for unlimited
        /// archive size.</param>
        /// <exception cref="ArchiveException">The archive could not be
        /// created.</exception>
        /// <remarks>
        /// The stream context implementation may provide a mapping from the file
        /// paths within the archive to the external file paths.
        /// </remarks>
        public override void Pack(
            IPackStreamContext streamContext,
            IEnumerable<string> files,
            long maxArchiveSize)
        {
            if (streamContext == null)
            {
                throw new ArgumentNullException("streamContext");
            }

            if (files == null)
            {
                throw new ArgumentNullException("files");
            }

            lock (this)
            {
                Stream archiveStream = null;
                try
                {
                    ResetProgressData();
                    totalArchives = 1;

                    object forceZip64Value = streamContext.GetOption("forceZip64", null);
                    bool forceZip64 = Convert.ToBoolean(
                        forceZip64Value, CultureInfo.InvariantCulture);

                    // Count the total number of files and bytes to be compressed.
                    foreach (string file in files)
                    {
                        Stream fileStream = streamContext.OpenFileReadStream(
                            file,
                            out FileAttributes attributes,
                            out DateTime lastWriteTime);
                        if (fileStream != null)
                        {
                            totalFileBytes += fileStream.Length;
                            totalFiles++;
                            streamContext.CloseFileReadStream(file, fileStream);
                        }
                    }

                    List<ZipFileHeader> fileHeaders = new List<ZipFileHeader>();
                    currentFileNumber = -1;

                    if (currentArchiveName == null)
                    {
                        mainArchiveName = streamContext.GetArchiveName(0);
                        currentArchiveName = mainArchiveName;

                        if (string.IsNullOrWhiteSpace(currentArchiveName))
                        {
                            throw new FileNotFoundException("No name provided for archive.");
                        }
                    }

                    OnProgress(ArchiveProgressType.StartArchive);

                    // Compress files one by one, saving header info for each.
                    foreach (string file in files)
                    {
                        ZipFileHeader fileHeader = PackOneFile(
                                streamContext,
                                file,
                                maxArchiveSize,
                                forceZip64,
                                ref archiveStream);

                        if (fileHeader != null)
                        {
                            fileHeaders.Add(fileHeader);
                        }

                        currentArchiveTotalBytes = (archiveStream != null ?
                            archiveStream.Position : 0);
                        currentArchiveBytesProcessed = currentArchiveTotalBytes;
                    }

                    bool zip64 = forceZip64 || totalFiles > ushort.MaxValue;

                    // Write the central directory composed of all the file headers.
                    uint centralDirStartArchiveNumber = 0;
                    long centralDirStartPosition = 0;
                    long centralDirSize = 0;
                    for (int i = 0; i < fileHeaders.Count; i++)
                    {
                        ZipFileHeader fileHeader = fileHeaders[i];

                        int headerSize = fileHeader.GetSize(true);
                        centralDirSize += headerSize;

                        CheckArchiveWriteStream(
                            streamContext,
                            maxArchiveSize,
                            headerSize,
                            ref archiveStream);

                        if (i == 0)
                        {
                            centralDirStartArchiveNumber = (uint)currentArchiveNumber;
                            centralDirStartPosition = archiveStream.Position;
                        }

                        fileHeader.Write(archiveStream, true);
                        if (fileHeader.zip64)
                        {
                            zip64 = true;
                        }
                    }

                    currentArchiveTotalBytes =
                        (archiveStream != null ? archiveStream.Position : 0);
                    currentArchiveBytesProcessed = currentArchiveTotalBytes;

                    ZipEndOfCentralDirectory eocd = new ZipEndOfCentralDirectory
                    {
                        dirStartDiskNumber = centralDirStartArchiveNumber,
                        entriesOnDisk = fileHeaders.Count,
                        totalEntries = fileHeaders.Count,
                        dirSize = centralDirSize,
                        dirOffset = centralDirStartPosition,
                        comment = comment
                    };

                    Zip64EndOfCentralDirectoryLocator eocdl =
                        new Zip64EndOfCentralDirectoryLocator();

                    int maxFooterSize = eocd.GetSize(false);
                    if (archiveStream != null && (zip64 || archiveStream.Position >
                        uint.MaxValue - eocd.GetSize(false)))
                    {
                        maxFooterSize += eocd.GetSize(true) + (int)
                            Zip64EndOfCentralDirectoryLocator.EOCDL64_SIZE;
                        zip64 = true;
                    }

                    CheckArchiveWriteStream(
                        streamContext,
                        maxArchiveSize,
                        maxFooterSize,
                        ref archiveStream);
                    eocd.diskNumber = (uint)currentArchiveNumber;

                    if (zip64)
                    {
                        eocd.versionMadeBy = 45;
                        eocd.versionNeeded = 45;
                        eocd.zip64 = true;
                        eocdl.dirOffset = archiveStream.Position;
                        eocdl.dirStartDiskNumber = (uint)currentArchiveNumber;
                        eocdl.totalDisks = (uint)currentArchiveNumber + 1;
                        eocd.Write(archiveStream);
                        eocdl.Write(archiveStream);

                        eocd.dirOffset = uint.MaxValue;
                        eocd.dirStartDiskNumber = ushort.MaxValue;
                    }

                    eocd.zip64 = false;
                    eocd.Write(archiveStream);

                    currentArchiveTotalBytes = archiveStream.Position;
                    currentArchiveBytesProcessed = currentArchiveTotalBytes;
                }
                finally
                {
                    if (archiveStream != null)
                    {
                        streamContext.CloseArchiveWriteStream(
                            currentArchiveNumber, mainArchiveName, archiveStream);
                        OnProgress(ArchiveProgressType.FinishArchive);
                    }
                }
            }
        }

        /// <summary>
        /// Moves to the next archive in the sequence if necessary.
        /// </summary>
        private void CheckArchiveWriteStream(
            IPackStreamContext streamContext,
            long maxArchiveSize,
            long requiredSize,
            ref Stream archiveStream)
        {
            if (archiveStream != null &&
                archiveStream.Length > 0 && maxArchiveSize > 0)
            {
                long sizeRemaining = maxArchiveSize - archiveStream.Length;
                if (sizeRemaining < requiredSize)
                {
                    string nextArchiveName = streamContext.GetArchiveName(
                        currentArchiveNumber + 1);

                    if (string.IsNullOrWhiteSpace(nextArchiveName))
                    {
                        throw new FileNotFoundException("No name provided for archive #" +
                            currentArchiveNumber + 1);
                    }

                    currentArchiveTotalBytes = archiveStream.Position;
                    currentArchiveBytesProcessed = currentArchiveTotalBytes;

                    streamContext.CloseArchiveWriteStream(
                        currentArchiveNumber,
                        nextArchiveName,
                        archiveStream);
                    archiveStream = null;

                    OnProgress(ArchiveProgressType.FinishArchive);

                    currentArchiveNumber++;
                    totalArchives++;
                    currentArchiveBytesProcessed = 0;
                    currentArchiveTotalBytes = 0;
                }
            }

            if (archiveStream == null)
            {
                if (currentArchiveNumber > 0)
                {
                    OnProgress(ArchiveProgressType.StartArchive);
                }

                archiveStream = streamContext.OpenArchiveWriteStream(
                    currentArchiveNumber, mainArchiveName, true, this);

                if (archiveStream == null)
                {
                    throw new FileNotFoundException("Stream not provided for archive #" +
                        currentArchiveNumber);
                }
            }
        }

        /// <summary>
        /// Adds one file to a zip archive in the process of being created.
        /// </summary>
        private ZipFileHeader PackOneFile(
            IPackStreamContext streamContext,
            string file,
            long maxArchiveSize,
            bool forceZip64,
            ref Stream archiveStream)
        {
            Stream fileStream = null;
            int headerArchiveNumber = 0;
            try
            {
                // TODO: call GetOption to get compression method for the specific file
                ZipCompressionMethod compressionMethod = ZipCompressionMethod.Deflate;
                if (CompressionLevel == CompressionLevel.None)
                {
                    compressionMethod = ZipCompressionMethod.Store;
                }

                if (!ZipEngine.compressionStreamCreators.TryGetValue(
                    compressionMethod, out Func<Stream, Stream> compressionStreamCreator))
                {
                    return null;
                }
                fileStream = streamContext.OpenFileReadStream(
                    file, out FileAttributes attributes, out DateTime lastWriteTime);
                if (fileStream == null)
                {
                    return null;
                }

                currentFileName = file;
                currentFileNumber++;

                currentFileTotalBytes = fileStream.Length;
                currentFileBytesProcessed = 0;
                OnProgress(ArchiveProgressType.StartFile);

                ZipFileInfo fileInfo = new ZipFileInfo(
                    file,
                    currentArchiveNumber,
                    attributes,
                    lastWriteTime,
                    fileStream.Length,
                    0,
                    compressionMethod);

                bool zip64 = forceZip64 || fileStream.Length >= uint.MaxValue;
                ZipFileHeader fileHeader = new ZipFileHeader(fileInfo, zip64);

                CheckArchiveWriteStream(
                    streamContext,
                    maxArchiveSize,
                    fileHeader.GetSize(false),
                    ref archiveStream);

                long headerPosition = archiveStream.Position;
                fileHeader.Write(archiveStream, false);
                headerArchiveNumber = currentArchiveNumber;

                long bytesWritten = PackFileBytes(
                    streamContext,
                    fileStream,
                    maxArchiveSize,
                    compressionStreamCreator,
                    ref archiveStream,
                    out uint crc);

                fileHeader.Update(
                    bytesWritten,
                    fileStream.Length,
                    crc,
                    headerPosition,
                    headerArchiveNumber);

                streamContext.CloseFileReadStream(file, fileStream);
                fileStream = null;

                // Go back and rewrite the updated file header.
                if (currentArchiveNumber == headerArchiveNumber)
                {
                    long fileEndPosition = archiveStream.Position;
                    archiveStream.Seek(headerPosition, SeekOrigin.Begin);
                    fileHeader.Write(archiveStream, false);
                    archiveStream.Seek(fileEndPosition, SeekOrigin.Begin);
                }
                else
                {
                    // The file spanned archives, so temporarily reopen
                    // the archive where it started.
                    string headerArchiveName = streamContext.GetArchiveName(
                        headerArchiveNumber + 1);
                    Stream headerStream = null;
                    try
                    {
                        headerStream = streamContext.OpenArchiveWriteStream(
                            headerArchiveNumber, headerArchiveName, false, this);
                        headerStream.Seek(headerPosition, SeekOrigin.Begin);
                        fileHeader.Write(headerStream, false);
                    }
                    finally
                    {
                        if (headerStream != null)
                        {
                            streamContext.CloseArchiveWriteStream(
                                headerArchiveNumber, headerArchiveName, headerStream);
                        }
                    }
                }

                OnProgress(ArchiveProgressType.FinishFile);

                return fileHeader;
            }
            finally
            {
                if (fileStream != null)
                {
                    streamContext.CloseFileReadStream(
                        currentFileName, fileStream);
                }
            }
        }

        /// <summary>
        /// Writes compressed bytes of one file to the archive,
        /// keeping track of the CRC and number of bytes written.
        /// </summary>
        private long PackFileBytes(
            IPackStreamContext streamContext,
            Stream fileStream,
            long maxArchiveSize,
            Func<Stream, Stream> compressionStreamCreator,
            ref Stream archiveStream,
            out uint crc)
        {
            long writeStartPosition = archiveStream.Position;
            long bytesWritten = 0;
            CrcStream fileCrcStream = new CrcStream(fileStream);

            ConcatStream concatStream = new ConcatStream(
                delegate (ConcatStream s)
                {
                    Stream sourceStream = s.Source;
                    bytesWritten += sourceStream.Position - writeStartPosition;

                    CheckArchiveWriteStream(
                        streamContext,
                        maxArchiveSize,
                        1,
                        ref sourceStream);

                    writeStartPosition = sourceStream.Position;
                    s.Source = sourceStream;
                })
            {
                Source = archiveStream
            };

            if (maxArchiveSize > 0)
            {
                concatStream.SetLength(maxArchiveSize);
            }

            Stream compressionStream = compressionStreamCreator(concatStream);

            try
            {
                byte[] buf = new byte[4096];
                long bytesRemaining = fileStream.Length;
                int counter = 0;
                while (bytesRemaining > 0)
                {
                    int count = (int)Math.Min(
                        bytesRemaining, buf.Length);

                    count = fileCrcStream.Read(buf, 0, count);
                    if (count <= 0)
                    {
                        throw new ZipException(
                            "Failed to read file: " + currentFileName);
                    }

                    compressionStream.Write(buf, 0, count);
                    bytesRemaining -= count;

                    fileBytesProcessed += count;
                    currentFileBytesProcessed += count;
                    currentArchiveTotalBytes = concatStream.Source.Position;
                    currentArchiveBytesProcessed = currentArchiveTotalBytes;

                    if (++counter % 16 == 0) // Report every 64K
                    {
                        OnProgress(ArchiveProgressType.PartialFile);
                    }
                }

                if (compressionStream is DeflateStream)
                {
#if CORECLR
                    compressionStream.Dispose();
#else
                    compressionStream.Close();
#endif
                }
                else
                {
                    compressionStream.Flush();
                }
            }
            finally
            {
                archiveStream = concatStream.Source;
            }

            bytesWritten += archiveStream.Position - writeStartPosition;

            crc = fileCrcStream.Crc;

            return bytesWritten;
        }
    }
}