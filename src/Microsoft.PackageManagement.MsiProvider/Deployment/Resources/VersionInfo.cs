//---------------------------------------------------------------------
// <copyright file="VersionInfo.cs" company="Microsoft Corporation">
//   Copyright (c) 1999, Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// Part of the Deployment Tools Foundation project.
// </summary>
//---------------------------------------------------------------------

namespace Microsoft.PackageManagement.Msi.Internal.Deployment.Resources
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    internal class VersionInfo : ICollection<VersionInfo>
    {
        private string key;
        private bool isString;
        private byte[] data;
        private List<VersionInfo> children;

        public VersionInfo(string key)
            : base()
        {
            this.key = key ?? throw new ArgumentNullException("key");
            children = new List<VersionInfo>();
        }

        public string Key
        {
            get => key;

            set => key = value ?? throw new ArgumentNullException("value");
        }

        public bool IsString
        {
            get => isString;

            set => isString = value;
        }

        public byte[] Data
        {
            get => data;

            set
            {
                data = value;
                isString = false;
            }
        }

        public void Read(BinaryReader reader)
        {
            long basePosition = reader.BaseStream.Position;
            int verInfoSize = reader.ReadUInt16();
            int valueSize = reader.ReadUInt16();
            bool dataIsString = (reader.ReadUInt16() != 0);
            StringBuilder keyStringBuilder = new StringBuilder();
            char c;
            while ((c = (char)reader.ReadUInt16()) != 0)
            {
                keyStringBuilder.Append(c);
            }
            Key = keyStringBuilder.ToString();
            Pad(reader, basePosition);
            if (valueSize == 0)
            {
                data = null;
            }
            else
            {
                if (dataIsString)
                {
                    valueSize *= 2; // Count is # of chars instead of bytes
                }

                data = reader.ReadBytes(valueSize);
                isString = dataIsString;
                Pad(reader, basePosition);
            }

            while (reader.BaseStream.Position - basePosition < verInfoSize)
            {
                Pad(reader, basePosition);
                VersionInfo childVerInfo = new VersionInfo("");
                childVerInfo.Read(reader);
                children.Add(childVerInfo);
            }
        }

        public void Write(BinaryWriter writer)
        {
            long basePosition = writer.BaseStream.Position;
            writer.Write((ushort)Length);
            byte[] valueBytes = data;
            writer.Write((ushort)((valueBytes != null ? valueBytes.Length : 0) / (IsString ? 2 : 1)));
            writer.Write((ushort)(IsString ? 1 : 0));
            byte[] keyBytes = new byte[Encoding.Unicode.GetByteCount(Key) + 2];
            Encoding.Unicode.GetBytes(Key, 0, Key.Length, keyBytes, 0);
            writer.Write(keyBytes);
            Pad(writer, basePosition);
            if (valueBytes != null)
            {
                writer.Write(valueBytes);
                Pad(writer, basePosition);
            }

            foreach (VersionInfo childVersionInfo in children)
            {
                Pad(writer, basePosition);
                childVersionInfo.Write(writer);
            }
        }

        private static void Pad(BinaryReader reader, long basePosition)
        {
            long position = reader.BaseStream.Position;
            int diff = (int)(position - basePosition) % 4;
            if (diff > 0)
            {
                while (diff++ < 4 && reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    reader.ReadByte();
                }
            }
        }

        private static void Pad(BinaryWriter writer, long basePosition)
        {
            long position = writer.BaseStream.Position;
            int diff = (int)(position - basePosition) % 4;
            if (diff > 0)
            {
                while (diff++ < 4)
                {
                    writer.Write((byte)0);
                }
            }
        }

        private int Length
        {
            get
            {
                int len = 6 + Encoding.Unicode.GetByteCount(Key) + 2;
                if (len % 4 > 0)
                {
                    len += (4 - len % 4);
                }

                len += (data != null ? data.Length : 0);
                if (len % 4 > 0)
                {
                    len += (4 - len % 4);
                }

                foreach (VersionInfo childVersionInfo in children)
                {
                    if (len % 4 > 0)
                    {
                        len += (4 - len % 4);
                    }

                    len += childVersionInfo.Length;
                }
                return len;
            }
        }

        public static explicit operator VersionInfo(byte[] bytesValue)
        {
            VersionInfo viValue = new VersionInfo("");
            using (BinaryReader reader = new BinaryReader(new MemoryStream(bytesValue, false)))
            {
                viValue.Read(reader);
            }
            return viValue;
        }

        public static explicit operator byte[] (VersionInfo viValue)
        {
            byte[] bytesValue = new byte[viValue.Length];
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream(bytesValue, true)))
            {
                viValue.Write(writer);
            }
            return bytesValue;
        }

        public VersionInfo this[string itemKey]
        {
            get
            {
                int index = IndexOf(itemKey);
                if (index < 0)
                {
                    return null;
                }

                return children[index];
            }
        }

        public void Add(VersionInfo item)
        {
            children.Add(item);
        }

        public bool Remove(VersionInfo item)
        {
            return children.Remove(item);
        }

        public bool Remove(string itemKey)
        {
            int index = IndexOf(itemKey);
            if (index >= 0)
            {
                children.RemoveAt(index);
                return true;
            }
            else
            {
                return false;
            }
        }

        private int IndexOf(string itemKey)
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].Key == itemKey)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool Contains(VersionInfo item)
        {
            return children.Contains(item);
        }

        public void CopyTo(VersionInfo[] array, int index)
        {
            children.CopyTo(array, index);
        }

        public void Clear()
        {
            children.Clear();
        }

        public int Count => children.Count;

        public bool IsReadOnly => false;

        public IEnumerator<VersionInfo> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}