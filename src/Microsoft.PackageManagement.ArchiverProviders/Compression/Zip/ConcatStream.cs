//---------------------------------------------------------------------
// <copyright file="ConcatStream.cs" company="Microsoft Corporation">
//   Copyright (c) 1999, Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// Part of the Deployment Tools Foundation project.
// </summary>
//---------------------------------------------------------------------

namespace Microsoft.PackageManagement.Archivers.Internal.Compression.Zip
{
    using System;
    using System.IO;

    /// <summary>
    /// Used to trick a DeflateStream into reading from or writing to
    /// a series of (chunked) streams instead of a single steream.
    /// </summary>
    internal class ConcatStream : Stream
    {
        private Stream source;
        private long position;
        private long length;
        private readonly Action<ConcatStream> nextStreamHandler;

        public ConcatStream(Action<ConcatStream> nextStreamHandler)
        {
            this.nextStreamHandler = nextStreamHandler ?? throw new ArgumentNullException("nextStreamHandler");
            length = long.MaxValue;
        }

        public Stream Source
        {
            get => source;
            set => source = value;
        }

        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override bool CanSeek => false;

        public override long Length => length;

        public override long Position
        {
            get => position;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (source == null)
            {
                nextStreamHandler(this);
            }

            count = (int)Math.Min(count, length - position);

            int bytesRemaining = count;
            while (bytesRemaining > 0)
            {
                if (source == null)
                {
                    throw new InvalidOperationException();
                }

                int partialCount = (int)Math.Min(bytesRemaining,
                    source.Length - source.Position);

                if (partialCount == 0)
                {
                    nextStreamHandler(this);
                    continue;
                }

                partialCount = source.Read(
                    buffer, offset + count - bytesRemaining, partialCount);
                bytesRemaining -= partialCount;
                position += partialCount;
            }

            return count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (source == null)
            {
                nextStreamHandler(this);
            }

            int bytesRemaining = count;
            while (bytesRemaining > 0)
            {
                if (source == null)
                {
                    throw new InvalidOperationException();
                }

                int partialCount = (int)Math.Min(bytesRemaining,
                    Math.Max(0, length - source.Position));

                if (partialCount == 0)
                {
                    nextStreamHandler(this);
                    continue;
                }

                source.Write(
                    buffer, offset + count - bytesRemaining, partialCount);
                bytesRemaining -= partialCount;
                position += partialCount;
            }
        }

        public override void Flush()
        {
            if (source != null)
            {
                source.Flush();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            length = value;
        }

#if !CORECLR

        /// <summary>
        /// Closes underying stream
        /// </summary>
        public override void Close()
        {
            if (source != null)
            {
                source.Close();
            }
        }

#endif

        /// <summary>
        /// Disposes underlying stream
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                source.Dispose();
            }
        }
    }
}