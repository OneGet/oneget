//---------------------------------------------------------------------
// <copyright file="GroupIconInfo.cs" company="Microsoft Corporation">
//   Copyright (c) 1999, Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// Part of the Deployment Tools Foundation project.
// </summary>
//---------------------------------------------------------------------

namespace Microsoft.PackageManagement.Msi.Internal.Deployment.Resources
{
    using System.IO;

    internal enum GroupIconType
    {
        Unknown,
        Icon,
        Cursor,
    }

    internal struct GroupIconDirectoryInfo
    {
        public byte width;
        public byte height;
        public byte colors;
        public byte reserved;
        public ushort planes;
        public ushort bitsPerPixel;
        public uint imageSize;
        public uint imageOffset; // only valid when icon group is read from .ico file.
        public ushort imageIndex; // only valid when icon group is read from PE resource.
    }

    internal class GroupIconInfo
    {
        private ushort reserved;
        private GroupIconType type;
        private GroupIconDirectoryInfo[] images;

        public GroupIconInfo()
        {
            images = new GroupIconDirectoryInfo[0];
        }

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public GroupIconDirectoryInfo[] DirectoryInfo => images;

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void ReadFromFile(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            Read(reader, true);
        }

        public void ReadFromResource(byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data, false)))
            {
                Read(reader, false);
            }
        }

        public byte[] GetResourceData()
        {
            byte[] data = null;

            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(reserved);
                writer.Write((ushort)type);
                writer.Write((ushort)images.Length);
                for (int i = 0; i < images.Length; ++i)
                {
                    writer.Write(images[i].width);
                    writer.Write(images[i].height);
                    writer.Write(images[i].colors);
                    writer.Write(images[i].reserved);
                    writer.Write(images[i].planes);
                    writer.Write(images[i].bitsPerPixel);
                    writer.Write(images[i].imageSize);
                    writer.Write(images[i].imageIndex);
                }

                data = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, data.Length);
            }

            return data;
        }

        private void Read(BinaryReader reader, bool readFromFile)
        {
            reserved = reader.ReadUInt16();
            type = (GroupIconType)reader.ReadUInt16();

            int imageCount = reader.ReadUInt16();
            images = new GroupIconDirectoryInfo[imageCount];
            for (int i = 0; i < imageCount; ++i)
            {
                images[i].width = reader.ReadByte();
                images[i].height = reader.ReadByte();
                images[i].colors = reader.ReadByte();
                images[i].reserved = reader.ReadByte();
                images[i].planes = reader.ReadUInt16();
                images[i].bitsPerPixel = reader.ReadUInt16();
                images[i].imageSize = reader.ReadUInt32();
                if (readFromFile)
                {
                    images[i].imageOffset = reader.ReadUInt32();
                    images[i].imageIndex = (ushort)(i + 1);
                }
                else
                {
                    images[i].imageIndex = reader.ReadUInt16();
                }
            }
        }
    }
}