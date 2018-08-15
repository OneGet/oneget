//---------------------------------------------------------------------
// <copyright file="CabPacker.cs" company="Microsoft Corporation">
//   Copyright (c) 1999, Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// Part of the Deployment Tools Foundation project.
// </summary>
//---------------------------------------------------------------------

namespace Microsoft.PackageManagement.Archivers.Internal.Compression.Cab
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;

#if !CORECLR

    using System.Security.Permissions;

#endif

    using System.Text;

    internal class CabPacker : CabWorker
    {
        private const string TempStreamName = "%%TEMP%%";

        private NativeMethods.FCI.Handle fciHandle;

        // These delegates need to be saved as member variables
        // so that they don't get GC'd.
        private readonly NativeMethods.FCI.PFNALLOC fciAllocMemHandler;

        private readonly NativeMethods.FCI.PFNFREE fciFreeMemHandler;
        private readonly NativeMethods.FCI.PFNOPEN fciOpenStreamHandler;
        private readonly NativeMethods.FCI.PFNREAD fciReadStreamHandler;
        private readonly NativeMethods.FCI.PFNWRITE fciWriteStreamHandler;
        private readonly NativeMethods.FCI.PFNCLOSE fciCloseStreamHandler;
        private readonly NativeMethods.FCI.PFNSEEK fciSeekStreamHandler;
        private readonly NativeMethods.FCI.PFNFILEPLACED fciFilePlacedHandler;
        private readonly NativeMethods.FCI.PFNDELETE fciDeleteFileHandler;
        private readonly NativeMethods.FCI.PFNGETTEMPFILE fciGetTempFileHandler;

        private readonly NativeMethods.FCI.PFNGETNEXTCABINET fciGetNextCabinet;
        private readonly NativeMethods.FCI.PFNSTATUS fciCreateStatus;
        private readonly NativeMethods.FCI.PFNGETOPENINFO fciGetOpenInfo;

        private IPackStreamContext context;

        private FileAttributes fileAttributes;
        private DateTime fileLastWriteTime;

        private int maxCabBytes;

        private long totalFolderBytesProcessedInCurrentCab;

        private CompressionLevel compressionLevel;
        private bool dontUseTempFiles;
        private IList<Stream> tempStreams;

        public CabPacker(CabEngine cabEngine)
            : base(cabEngine)
        {
            fciAllocMemHandler = CabAllocMem;
            fciFreeMemHandler = CabFreeMem;
            fciOpenStreamHandler = CabOpenStreamEx;
            fciReadStreamHandler = CabReadStreamEx;
            fciWriteStreamHandler = CabWriteStreamEx;
            fciCloseStreamHandler = CabCloseStreamEx;
            fciSeekStreamHandler = CabSeekStreamEx;
            fciFilePlacedHandler = CabFilePlaced;
            fciDeleteFileHandler = CabDeleteFile;
            fciGetTempFileHandler = CabGetTempFile;
            fciGetNextCabinet = CabGetNextCabinet;
            fciCreateStatus = CabCreateStatus;
            fciGetOpenInfo = CabGetOpenInfo;
            tempStreams = new List<Stream>();
            compressionLevel = CompressionLevel.Normal;
        }

        public bool UseTempFiles
        {
            get => !dontUseTempFiles;

            set => dontUseTempFiles = !value;
        }

        public CompressionLevel CompressionLevel
        {
            get => compressionLevel;

            set => compressionLevel = value;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        private void CreateFci(long maxArchiveSize)
        {
            NativeMethods.FCI.CCAB ccab = new NativeMethods.FCI.CCAB();
            if (maxArchiveSize > 0 && maxArchiveSize < ccab.cb)
            {
                ccab.cb = Math.Max(
                    NativeMethods.FCI.MIN_DISK, (int)maxArchiveSize);
            }

            object maxFolderSizeOption = context.GetOption(
                "maxFolderSize", null);
            if (maxFolderSizeOption != null)
            {
                long maxFolderSize = Convert.ToInt64(
                    maxFolderSizeOption, CultureInfo.InvariantCulture);
                if (maxFolderSize > 0 && maxFolderSize < ccab.cbFolderThresh)
                {
                    ccab.cbFolderThresh = (int)maxFolderSize;
                }
            }

            maxCabBytes = ccab.cb;
            ccab.szCab = context.GetArchiveName(0);
            if (ccab.szCab == null)
            {
                throw new FileNotFoundException(
                    "Cabinet name not provided by stream context.");
            }
            ccab.setID = (short)new Random().Next(
                short.MinValue, short.MaxValue + 1);
            CabNumbers[ccab.szCab] = 0;
            currentArchiveName = ccab.szCab;
            totalArchives = 1;
            CabStream = null;

            Erf.Clear();
            fciHandle = NativeMethods.FCI.Create(
                ErfHandle.AddrOfPinnedObject(),
                fciFilePlacedHandler,
                fciAllocMemHandler,
                fciFreeMemHandler,
                fciOpenStreamHandler,
                fciReadStreamHandler,
                fciWriteStreamHandler,
                fciCloseStreamHandler,
                fciSeekStreamHandler,
                fciDeleteFileHandler,
                fciGetTempFileHandler,
                ccab,
                IntPtr.Zero);
            CheckError(false);
        }

        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts")]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
#if !CORECLR
        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
#endif
        public void Pack(
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
                try
                {
                    context = streamContext;

                    ResetProgressData();

                    CreateFci(maxArchiveSize);

                    foreach (string file in files)
                    {
                        Stream fileStream = context.OpenFileReadStream(
                            file,
                            out FileAttributes attributes,
                            out DateTime lastWriteTime);
                        if (fileStream != null)
                        {
                            totalFileBytes += fileStream.Length;
                            totalFiles++;
                            context.CloseFileReadStream(file, fileStream);
                        }
                    }

                    long uncompressedBytesInFolder = 0;
                    currentFileNumber = -1;

                    foreach (string file in files)
                    {
                        Stream fileStream = context.OpenFileReadStream(
                            file, out FileAttributes attributes, out DateTime lastWriteTime);
                        if (fileStream == null)
                        {
                            continue;
                        }

                        if (fileStream.Length >= NativeMethods.FCI.MAX_FOLDER)
                        {
                            throw new NotSupportedException(string.Format(
                                CultureInfo.InvariantCulture,
                                "File {0} exceeds maximum file size " +
                                "for cabinet format.",
                                file));
                        }

                        if (uncompressedBytesInFolder > 0)
                        {
                            // Automatically create a new folder if this file
                            // won't fit in the current folder.
                            bool nextFolder = uncompressedBytesInFolder
                                + fileStream.Length >= NativeMethods.FCI.MAX_FOLDER;

                            // Otherwise ask the client if it wants to
                            // move to the next folder.
                            if (!nextFolder)
                            {
                                object nextFolderOption = streamContext.GetOption(
                                    "nextFolder",
                                    new object[] { file, currentFolderNumber });
                                nextFolder = Convert.ToBoolean(
                                    nextFolderOption, CultureInfo.InvariantCulture);
                            }

                            if (nextFolder)
                            {
                                FlushFolder();
                                uncompressedBytesInFolder = 0;
                            }
                        }

                        if (currentFolderTotalBytes > 0)
                        {
                            currentFolderTotalBytes = 0;
                            currentFolderNumber++;
                            uncompressedBytesInFolder = 0;
                        }

                        currentFileName = file;
                        currentFileNumber++;

                        currentFileTotalBytes = fileStream.Length;
                        currentFileBytesProcessed = 0;
                        OnProgress(ArchiveProgressType.StartFile);

                        uncompressedBytesInFolder += fileStream.Length;

                        AddFile(
                            file,
                            fileStream,
                            attributes,
                            lastWriteTime,
                            false,
                            CompressionLevel);
                    }

                    FlushFolder();
                    FlushCabinet();
                }
                finally
                {
                    if (CabStream != null)
                    {
                        context.CloseArchiveWriteStream(
                            currentArchiveNumber,
                            currentArchiveName,
                            CabStream);
                        CabStream = null;
                    }

                    if (FileStream != null)
                    {
                        context.CloseFileReadStream(
                            currentFileName, FileStream);
                        FileStream = null;
                    }
                    context = null;

                    if (fciHandle != null)
                    {
                        fciHandle.Dispose();
                        fciHandle = null;
                    }
                }
            }
        }

        internal override int CabOpenStreamEx(string path, int openFlags, int shareMode, out int err, IntPtr pv)
        {
            if (CabNumbers.ContainsKey(path))
            {
                Stream stream = CabStream;
                if (stream == null)
                {
                    short cabNumber = CabNumbers[path];

                    currentFolderTotalBytes = 0;

                    stream = context.OpenArchiveWriteStream(cabNumber, path, true, CabEngine);
                    currentArchiveName = path;

                    currentArchiveTotalBytes = Math.Min(
                        totalFolderBytesProcessedInCurrentCab, maxCabBytes);
                    currentArchiveBytesProcessed = 0;

                    OnProgress(ArchiveProgressType.StartArchive);
                    CabStream = stream ?? throw new FileNotFoundException(
                            string.Format(CultureInfo.InvariantCulture, "Cabinet {0} not provided.", cabNumber));
                }
                path = CabWorker.CabStreamName;
            }
            else if (path == CabPacker.TempStreamName)
            {
                // Opening memory stream for a temp file.
                Stream stream = new MemoryStream();
                tempStreams.Add(stream);
                int streamHandle = StreamHandles.AllocHandle(stream);
                err = 0;
                return streamHandle;
            }
            else if (path != CabWorker.CabStreamName)
            {
                // Opening a file on disk for a temp file.
                path = Path.Combine(Path.GetTempPath(), path);
                Stream stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
                tempStreams.Add(stream);
                stream = new DuplicateStream(stream);
                int streamHandle = StreamHandles.AllocHandle(stream);
                err = 0;
                return streamHandle;
            }
            return base.CabOpenStreamEx(path, openFlags, shareMode, out err, pv);
        }

        internal override int CabWriteStreamEx(int streamHandle, IntPtr memory, int cb, out int err, IntPtr pv)
        {
            int count = base.CabWriteStreamEx(streamHandle, memory, cb, out err, pv);
            if (count > 0 && err == 0)
            {
                Stream stream = StreamHandles[streamHandle];
                if (DuplicateStream.OriginalStream(stream) ==
                    DuplicateStream.OriginalStream(CabStream))
                {
                    currentArchiveBytesProcessed += cb;
                    if (currentArchiveBytesProcessed > currentArchiveTotalBytes)
                    {
                        currentArchiveBytesProcessed = currentArchiveTotalBytes;
                    }
                }
            }
            return count;
        }

        internal override int CabCloseStreamEx(int streamHandle, out int err, IntPtr pv)
        {
            Stream stream = DuplicateStream.OriginalStream(StreamHandles[streamHandle]);

            if (stream == DuplicateStream.OriginalStream(FileStream))
            {
                context.CloseFileReadStream(currentFileName, stream);
                FileStream = null;
                long remainder = currentFileTotalBytes - currentFileBytesProcessed;
                currentFileBytesProcessed += remainder;
                fileBytesProcessed += remainder;
                OnProgress(ArchiveProgressType.FinishFile);

                currentFileTotalBytes = 0;
                currentFileBytesProcessed = 0;
                currentFileName = null;
            }
            else if (stream == DuplicateStream.OriginalStream(CabStream))
            {
                if (stream.CanWrite)
                {
                    stream.Flush();
                }

                currentArchiveBytesProcessed = currentArchiveTotalBytes;
                OnProgress(ArchiveProgressType.FinishArchive);
                currentArchiveNumber++;
                totalArchives++;

                context.CloseArchiveWriteStream(
                    currentArchiveNumber,
                    currentArchiveName,
                    stream);

                currentArchiveName = NextCabinetName;
                currentArchiveBytesProcessed = currentArchiveTotalBytes = 0;
                totalFolderBytesProcessedInCurrentCab = 0;

                CabStream = null;
            }
            else  // Must be a temp stream
            {
#if CORECLR
                stream.Dispose();
#else
                stream.Close();
#endif
                tempStreams.Remove(stream);
            }
            return base.CabCloseStreamEx(streamHandle, out err, pv);
        }

        /// <summary>
        /// Disposes of resources allocated by the cabinet engine.
        /// </summary>
        /// <param name="disposing">If true, the method has been called directly or indirectly by a user's code,
        /// so managed and unmanaged resources will be disposed. If false, the method has been called by the
        /// runtime from inside the finalizer, and only unmanaged resources will be disposed.</param>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (fciHandle != null)
                    {
                        fciHandle.Dispose();
                        fciHandle = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private static NativeMethods.FCI.TCOMP GetCompressionType(CompressionLevel compLevel)
        {
            if (compLevel < CompressionLevel.Min)
            {
                return NativeMethods.FCI.TCOMP.TYPE_NONE;
            }
            else
            {
                if (compLevel > CompressionLevel.Max)
                {
                    compLevel = CompressionLevel.Max;
                }

                int lzxWindowMax =
                    ((int)NativeMethods.FCI.TCOMP.LZX_WINDOW_HI >> (int)NativeMethods.FCI.TCOMP.SHIFT_LZX_WINDOW) -
                    ((int)NativeMethods.FCI.TCOMP.LZX_WINDOW_LO >> (int)NativeMethods.FCI.TCOMP.SHIFT_LZX_WINDOW);
                int lzxWindow = lzxWindowMax *
                    (compLevel - CompressionLevel.Min) / (CompressionLevel.Max - CompressionLevel.Min);

                return (NativeMethods.FCI.TCOMP)((int)NativeMethods.FCI.TCOMP.TYPE_LZX |
                    ((int)NativeMethods.FCI.TCOMP.LZX_WINDOW_LO +
                    (lzxWindow << (int)NativeMethods.FCI.TCOMP.SHIFT_LZX_WINDOW)));
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        private void AddFile(
            string name,
            Stream stream,
            FileAttributes attributes,
            DateTime lastWriteTime,
            bool execute,
            CompressionLevel compLevel)
        {
            FileStream = stream;
            fileAttributes = attributes &
                (FileAttributes.Archive | FileAttributes.Hidden | FileAttributes.ReadOnly | FileAttributes.System);
            fileLastWriteTime = lastWriteTime;
            currentFileName = name;

            NativeMethods.FCI.TCOMP tcomp = CabPacker.GetCompressionType(compLevel);

            IntPtr namePtr = IntPtr.Zero;
            try
            {
                Encoding nameEncoding = Encoding.ASCII;
                if (Encoding.UTF8.GetByteCount(name) > name.Length)
                {
                    nameEncoding = Encoding.UTF8;
                    fileAttributes |= FileAttributes.Normal;  // _A_NAME_IS_UTF
                }

                byte[] nameBytes = nameEncoding.GetBytes(name);
                namePtr = Marshal.AllocHGlobal(nameBytes.Length + 1);
                Marshal.Copy(nameBytes, 0, namePtr, nameBytes.Length);
                Marshal.WriteByte(namePtr, nameBytes.Length, 0);

                Erf.Clear();
                int result = NativeMethods.FCI.AddFile(
                    fciHandle,
                    string.Empty,
                    namePtr,
                    execute,
                    fciGetNextCabinet,
                    fciCreateStatus,
                    fciGetOpenInfo,
                    tcomp);
                if (result == 0)
                {
                    // Stop compiler from complaining
                    CheckError(false);
                    FileStream = null;
                    currentFileName = null;
                    return;
                }
            }
            finally
            {
                if (namePtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(namePtr);
                }
            }

            CheckError(false);
            FileStream = null;
            currentFileName = null;
        }

        private void FlushFolder()
        {
            Erf.Clear();
            int result = NativeMethods.FCI.FlushFolder(fciHandle, fciGetNextCabinet, fciCreateStatus);
            if (result == 0)
            {
                // Stop compiler from complaining
                CheckError(false);
                return;
            }
            CheckError(false);
        }

        private void FlushCabinet()
        {
            Erf.Clear();
            int result = NativeMethods.FCI.FlushCabinet(fciHandle, false, fciGetNextCabinet, fciCreateStatus);
            if (result == 0)
            {
                // Stop compiler from complaining
                CheckError(false);
                return;
            }
            CheckError(false);
        }

        private int CabGetOpenInfo(
            string path,
            out short date,
            out short time,
            out short attribs,
            out int err,
            IntPtr pv)
        {
            CompressionEngine.DateTimeToDosDateAndTime(fileLastWriteTime, out date, out time);
            attribs = (short)fileAttributes;

            Stream stream = FileStream;
            FileStream = new DuplicateStream(stream);
            int streamHandle = StreamHandles.AllocHandle(stream);
            err = 0;
            return streamHandle;
        }

        private int CabFilePlaced(
            IntPtr pccab,
            string filePath,
            long fileSize,
            int continuation,
            IntPtr pv)
        {
            return 0;
        }

        private int CabGetNextCabinet(IntPtr pccab, uint prevCabSize, IntPtr pv)
        {
            NativeMethods.FCI.CCAB nextCcab = new NativeMethods.FCI.CCAB();
            Marshal.PtrToStructure(pccab, nextCcab);

            nextCcab.szDisk = string.Empty;
            nextCcab.szCab = context.GetArchiveName(nextCcab.iCab);
            CabNumbers[nextCcab.szCab] = (short)nextCcab.iCab;
            NextCabinetName = nextCcab.szCab;

            Marshal.StructureToPtr(nextCcab, pccab, false);
            return 1;
        }

        private int CabCreateStatus(NativeMethods.FCI.STATUS typeStatus, uint cb1, uint cb2, IntPtr pv)
        {
            switch (typeStatus)
            {
                case NativeMethods.FCI.STATUS.FILE:
                    if (cb2 > 0 && currentFileBytesProcessed < currentFileTotalBytes)
                    {
                        if (currentFileBytesProcessed + cb2 > currentFileTotalBytes)
                        {
                            cb2 = (uint)currentFileTotalBytes - (uint)currentFileBytesProcessed;
                        }
                        currentFileBytesProcessed += cb2;
                        fileBytesProcessed += cb2;

                        OnProgress(ArchiveProgressType.PartialFile);
                    }
                    break;

                case NativeMethods.FCI.STATUS.FOLDER:
                    if (cb1 == 0)
                    {
                        currentFolderTotalBytes = cb2 - totalFolderBytesProcessedInCurrentCab;
                        totalFolderBytesProcessedInCurrentCab = cb2;
                    }
                    else if (currentFolderTotalBytes == 0)
                    {
                        OnProgress(ArchiveProgressType.PartialArchive);
                    }
                    break;

                case NativeMethods.FCI.STATUS.CABINET:
                    break;
            }
            return 0;
        }

        private int CabGetTempFile(IntPtr tempNamePtr, int tempNameSize, IntPtr pv)
        {
            string tempFileName;
            if (UseTempFiles)
            {
                tempFileName = Path.GetFileName(Path.GetTempFileName());
            }
            else
            {
                tempFileName = CabPacker.TempStreamName;
            }

            byte[] tempNameBytes = Encoding.ASCII.GetBytes(tempFileName);
            if (tempNameBytes.Length >= tempNameSize)
            {
                return -1;
            }

            Marshal.Copy(tempNameBytes, 0, tempNamePtr, tempNameBytes.Length);
            Marshal.WriteByte(tempNamePtr, tempNameBytes.Length, 0);  // null-terminator
            return 1;
        }

        private int CabDeleteFile(string path, out int err, IntPtr pv)
        {
            try
            {
                // Deleting a temp file - don't bother if it is only a memory stream.
                if (path != CabPacker.TempStreamName)
                {
                    path = Path.Combine(Path.GetTempPath(), path);
                    File.Delete(path);
                }
            }
            catch (IOException)
            {
                // Failure to delete a temp file is not fatal.
            }
            err = 0;
            return 1;
        }
    }
}