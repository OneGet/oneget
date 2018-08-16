//---------------------------------------------------------------------
// <copyright file="VersionStringTable.cs" company="Microsoft Corporation">
//   Copyright (c) 1999, Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// Part of the Deployment Tools Foundation project.
// </summary>
//---------------------------------------------------------------------

namespace Microsoft.PackageManagement.Msi.Internal.Deployment.Resources
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Represents a string table of a file version resource.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    internal sealed class VersionStringTable : IDictionary<string, string>
    {
        private readonly VersionResource parent;
        private VersionInfo rawStringVersionInfo;

        internal VersionStringTable(VersionResource parent, VersionInfo rawStringVersionInfo)
        {
            this.parent = parent;
            this.rawStringVersionInfo = rawStringVersionInfo;
        }

        /// <summary>
        /// Gets the locale (LCID) of the string table.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public int Locale
        {
            get => ushort.Parse(rawStringVersionInfo.Key.Substring(0, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            set
            {
                rawStringVersionInfo.Key = ((ushort)value).ToString("x4", CultureInfo.InvariantCulture) + rawStringVersionInfo.Key.Substring(4, 4);
                parent.dirty = true;
            }
        }

        /// <summary>
        /// Gets or sets a string value.
        /// </summary>
        /// <param name="key">Name of the string.</param>
        public string this[string key]
        {
            get
            {
                VersionInfo verValue = rawStringVersionInfo[key];
                if (verValue == null)
                {
                    return null;
                }
                else
                {
                    return Encoding.Unicode.GetString(verValue.Data, 0, verValue.Data.Length - 2);
                }
            }
            set
            {
                if (value == null)
                {
                    rawStringVersionInfo.Remove(key);
                }
                else
                {
                    VersionInfo verValue = rawStringVersionInfo[key];
                    if (verValue == null)
                    {
                        verValue = new VersionInfo(key)
                        {
                            IsString = true
                        };
                        rawStringVersionInfo.Add(verValue);
                    }
                    verValue.Data = new byte[Encoding.Unicode.GetByteCount(value) + 2];
                    Encoding.Unicode.GetBytes(value, 0, value.Length, verValue.Data, 0);
                }
                parent.dirty = true;
            }
        }

        bool ICollection<KeyValuePair<string, string>>.IsReadOnly => false;

        bool IDictionary<string, string>.TryGetValue(string key, out string value)
        {
            value = this[key];
            return value != null;
        }

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
        {
            this[item.Key] = item.Value;
        }

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
        {
            string value = this[item.Key];
            if (value == item.Value)
            {
                this[item.Key] = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
        {
            string value = this[item.Key];
            if (value == item.Value)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        bool IDictionary<string, string>.ContainsKey(string key)
        {
            return this[key] != null;
        }

        void IDictionary<string, string>.Add(string key, string value)
        {
            this[key] = value;
        }

        bool IDictionary<string, string>.Remove(string key)
        {
            if (this[key] != null)
            {
                this[key] = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes all strings from the string table.
        /// </summary>
        public void Clear()
        {
            rawStringVersionInfo.Clear();
        }

        /// <summary>
        /// Gets a collection of all the names of the strings in the table.
        /// </summary>
        public ICollection<string> Keys
        {
            get
            {
                List<string> keys = new List<string>(rawStringVersionInfo.Count);
                foreach (VersionInfo verValue in rawStringVersionInfo)
                {
                    keys.Add(verValue.Key);
                }
                return keys;
            }
        }

        /// <summary>
        /// Gets a collection of all the values in the table.
        /// </summary>
        public ICollection<string> Values
        {
            get
            {
                List<string> values = new List<string>(rawStringVersionInfo.Count);
                foreach (VersionInfo verValue in rawStringVersionInfo)
                {
                    values.Add(Encoding.Unicode.GetString(verValue.Data, 0, verValue.Data.Length - 2));
                }
                return values;
            }
        }

        /// <summary>
        /// Gets the number of strings in the table.
        /// </summary>
        public int Count => rawStringVersionInfo.Count;

        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int index)
        {
            foreach (VersionInfo verValue in rawStringVersionInfo)
            {
                array[index++] = new KeyValuePair<string, string>(verValue.Key, Encoding.Unicode.GetString(verValue.Data, 0, verValue.Data.Length - 2));
            }
        }

        /// <summary>
        /// Gets an enumeration over all strings in the table.
        /// </summary>
        /// <returns>Enumeration of string name and value pairs</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (VersionInfo verValue in rawStringVersionInfo)
            {
                yield return new KeyValuePair<string, string>(verValue.Key, Encoding.Unicode.GetString(verValue.Data, 0, verValue.Data.Length - 2));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}