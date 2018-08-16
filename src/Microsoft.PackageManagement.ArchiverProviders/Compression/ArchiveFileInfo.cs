﻿//---------------------------------------------------------------------
// <copyright file="ArchiveFileInfo.cs" company="Microsoft Corporation">
//   Copyright (c) 1999, Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// Part of the Deployment Tools Foundation project.
// </summary>
//---------------------------------------------------------------------

namespace Microsoft.PackageManagement.Archivers.Internal.Compression
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;


    /// <summary>
    /// Abstract object representing a compressed file within an archive;
    /// provides operations for getting the file properties and unpacking
    /// the file.
    /// </summary>
    public abstract class ArchiveFileInfo : FileSystemInfo
    {
        private ArchiveInfo archiveInfo;
        private readonly string name;
        private readonly string path;

        private readonly bool initialized;
        private bool exists;
        private readonly int archiveNumber;
        private FileAttributes attributes;
        private DateTime lastWriteTime;
        private long length;

        /// <summary>
        /// Creates a new ArchiveFileInfo object representing a file within
        /// an archive in a specified path.
        /// </summary>
        /// <param name="archiveInfo">An object representing the archive
        /// containing the file.</param>
        /// <param name="filePath">The path to the file within the archive.
        /// Usually, this is a simple file name, but if the archive contains
        /// a directory structure this may include the directory.</param>
        protected ArchiveFileInfo(ArchiveInfo archiveInfo, string filePath)
            : base()
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }

            Archive = archiveInfo;

            name = System.IO.Path.GetFileName(filePath);
            path = System.IO.Path.GetDirectoryName(filePath);

            attributes = FileAttributes.Normal;
            lastWriteTime = DateTime.MinValue;
        }

        /// <summary>
        /// Creates a new ArchiveFileInfo object with all parameters specified;
        /// used by subclasses when reading the metadata out of an archive.
        /// </summary>
        /// <param name="filePath">The internal path and name of the file in
        /// the archive.</param>
        /// <param name="archiveNumber">The archive number where the file
        /// starts.</param>
        /// <param name="attributes">The stored attributes of the file.</param>
        /// <param name="lastWriteTime">The stored last write time of the
        /// file.</param>
        /// <param name="length">The uncompressed size of the file.</param>
        protected ArchiveFileInfo(
            string filePath,
            int archiveNumber,
            FileAttributes attributes,
            DateTime lastWriteTime,
            long length)
            : this(null, filePath)
        {
            exists = true;
            this.archiveNumber = archiveNumber;
            this.attributes = attributes;
            this.lastWriteTime = lastWriteTime;
            this.length = length;
            initialized = true;
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>The name of the file, not including any path.</value>
        public override string Name => name;

        /// <summary>
        /// Gets the internal path of the file in the archive.
        /// </summary>
        /// <value>The internal path of the file in the archive, not including
        /// the file name.</value>
        public string Path => path;

        /// <summary>
        /// Gets the full path to the file.
        /// </summary>
        /// <value>The full path to the file, including the full path to the
        /// archive, the internal path in the archive, and the file name.</value>
        /// <remarks>
        /// For example, the path <c>"C:\archive.cab\file.txt"</c> refers to
        /// a file "file.txt" inside the archive "archive.cab".
        /// </remarks>
        public override string FullName
        {
            get
            {
                string fullName = System.IO.Path.Combine(Path, Name);

                if (Archive != null)
                {
                    fullName = System.IO.Path.Combine(ArchiveName, fullName);
                }

                return fullName;
            }
        }

        public string FullNameExtension
        {
            get
            {
                // GetFullPathInternal would have already stripped out the terminating "." if present.
                int len = FullName.Length;
                for (int i = len; --i >= 0;)
                {
                    char ch = FullName[i];
                    if (ch == '.')
                    {
                        return FullName.Substring(i, len - i);
                    }

                    if (ch == System.IO.Path.DirectorySeparatorChar || ch == System.IO.Path.AltDirectorySeparatorChar || ch == System.IO.Path.VolumeSeparatorChar)
                    {
                        break;
                    }
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the archive that contains this file.
        /// </summary>
        /// <value>
        /// The ArchiveInfo instance that retrieved this file information -- this
        /// may be null if the ArchiveFileInfo object was returned directly from
        /// a stream.
        /// </value>
        public ArchiveInfo Archive
        {
            get => archiveInfo;

            internal set
            {
                archiveInfo = value;

                // protected instance members inherited from FileSystemInfo:
                OriginalPath = value?.FullName;
                FullPath = OriginalPath;
            }
        }

        /// <summary>
        /// Gets the full path of the archive that contains this file.
        /// </summary>
        /// <value>The full path of the archive that contains this file.</value>
        public string ArchiveName => Archive?.FullName;

        /// <summary>
        /// Gets the number of the archive where this file starts.
        /// </summary>
        /// <value>The number of the archive where this file starts.</value>
        /// <remarks>A single archive or the first archive in a chain is
        /// numbered 0.</remarks>
        public int ArchiveNumber => archiveNumber;

        /// <summary>
        /// Checks if the file exists within the archive.
        /// </summary>
        /// <value>True if the file exists, false otherwise.</value>
        public override bool Exists
        {
            get
            {
                if (!initialized)
                {
                    Refresh();
                }

                return exists;
            }
        }

        /// <summary>
        /// Gets the uncompressed size of the file.
        /// </summary>
        /// <value>The uncompressed size of the file in bytes.</value>
        public long Length
        {
            get
            {
                if (!initialized)
                {
                    Refresh();
                }

                return length;
            }
        }

        /// <summary>
        /// Gets the attributes of the file.
        /// </summary>
        /// <value>The attributes of the file as stored in the archive.</value>
        public new FileAttributes Attributes
        {
            get
            {
                if (!initialized)
                {
                    Refresh();
                }

                return attributes;
            }
        }

        /// <summary>
        /// Gets the last modification time of the file.
        /// </summary>
        /// <value>The last modification time of the file as stored in the
        /// archive.</value>
        public new DateTime LastWriteTime
        {
            get
            {
                if (!initialized)
                {
                    Refresh();
                }

                return lastWriteTime;
            }
        }

        /// <summary>
        /// Gets the full path to the file.
        /// </summary>
        /// <returns>The same as <see cref="FullName"/></returns>
        public override string ToString()
        {
            return FullName;
        }

        /// <summary>
        /// Deletes the file. NOT SUPPORTED.
        /// </summary>
        /// <exception cref="NotSupportedException">Files cannot be deleted
        /// from an existing archive.</exception>
        public override void Delete()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Refreshes the attributes and other cached information about the file,
        /// by re-reading the information from the archive.
        /// </summary>
        public new void Refresh()
        {
            base.Refresh();

            if (Archive != null)
            {
                string filePath = System.IO.Path.Combine(Path, Name);
                ArchiveFileInfo updatedFile = Archive.GetFile(filePath);
                if (updatedFile == null)
                {
                    throw new FileNotFoundException(
                            "File not found in archive.", filePath);
                }

                Refresh(updatedFile);
            }
        }

        /// <summary>
        /// Extracts the file.
        /// </summary>
        /// <param name="destFileName">The destination path where the file
        /// will be extracted.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dest")]
        public void CopyTo(string destFileName)
        {
            CopyTo(destFileName, false);
        }

        /// <summary>
        /// Extracts the file, optionally overwriting any existing file.
        /// </summary>
        /// <param name="destFileName">The destination path where the file
        /// will be extracted.</param>
        /// <param name="overwrite">If true, <paramref name="destFileName"/>
        /// will be overwritten if it exists.</param>
        /// <exception cref="IOException"><paramref name="overwrite"/> is false
        /// and <paramref name="destFileName"/> exists.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dest")]
        public void CopyTo(string destFileName, bool overwrite)
        {
            if (destFileName == null)
            {
                throw new ArgumentNullException("destFileName");
            }

            if (!overwrite && File.Exists(destFileName))
            {
                throw new IOException();
            }

            if (Archive == null)
            {
                throw new InvalidOperationException();
            }

            Archive.UnpackFile(
                System.IO.Path.Combine(Path, Name), destFileName);
        }

        /// <summary>
        /// Opens the archive file for reading without actually extracting the
        /// file to disk.
        /// </summary>
        /// <returns>
        /// A stream for reading directly from the packed file. Like any stream
        /// this should be closed/disposed as soon as it is no longer needed.
        /// </returns>
        public Stream OpenRead()
        {
            return Archive.OpenRead(System.IO.Path.Combine(Path, Name));
        }

        /// <summary>
        /// Opens the archive file reading text with UTF-8 encoding without
        /// actually extracting the file to disk.
        /// </summary>
        /// <returns>
        /// A reader for reading text directly from the packed file. Like any reader
        /// this should be closed/disposed as soon as it is no longer needed.
        /// </returns>
        /// <remarks>
        /// To open an archived text file with different encoding, use the
        /// <see cref="OpenRead" /> method and pass the returned stream to one of
        /// the <see cref="StreamReader" /> constructor overloads.
        /// </remarks>
        public StreamReader OpenText()
        {
            return Archive.OpenText(System.IO.Path.Combine(Path, Name));
        }

        /// <summary>
        /// Refreshes the information in this object with new data retrieved
        /// from an archive.
        /// </summary>
        /// <param name="newFileInfo">Fresh instance for the same file just
        /// read from the archive.</param>
        /// <remarks>
        /// Subclasses may override this method to refresh sublcass fields.
        /// However they should always call the base implementation first.
        /// </remarks>
        protected virtual void Refresh(ArchiveFileInfo newFileInfo)
        {
            exists = newFileInfo.exists;
            length = newFileInfo.length;
            attributes = newFileInfo.attributes;
            lastWriteTime = newFileInfo.lastWriteTime;
        }
    }
}