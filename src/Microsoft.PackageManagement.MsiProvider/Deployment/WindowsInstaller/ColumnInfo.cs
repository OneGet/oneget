//---------------------------------------------------------------------
// <copyright file="ColumnInfo.cs" company="Microsoft Corporation">
//   Copyright (c) 1999, Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// Part of the Deployment Tools Foundation project.
// </summary>
//---------------------------------------------------------------------

namespace Microsoft.PackageManagement.Msi.Internal.Deployment.WindowsInstaller
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Defines a single column of a table in an installer database.
    /// </summary>
    /// <remarks>Once created, a ColumnInfo object is immutable.</remarks>
    internal class ColumnInfo
    {
        private readonly string name;
        private readonly Type type;
        private readonly int size;
        private readonly bool isRequired;
        private readonly bool isTemporary;
        private readonly bool isLocalizable;

        /// <summary>
        /// Creates a new ColumnInfo object from a column definition.
        /// </summary>
        /// <param name="name">name of the column</param>
        /// <param name="columnDefinition">column definition string</param>
        /// <seealso cref="ColumnDefinitionString"/>
        public ColumnInfo(string name, string columnDefinition)
            : this(name, typeof(string), 0, false, false, false)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (columnDefinition == null)
            {
                throw new ArgumentNullException("columnDefinition");
            }

            switch (char.ToLower(columnDefinition[0], CultureInfo.InvariantCulture))
            {
                case 'i':
                    type = typeof(int);
                    break;

                case 'j':
                    type = typeof(int); isTemporary = true;
                    break;

                case 'g':
                    type = typeof(string); isTemporary = true;
                    break;

                case 'l':
                    type = typeof(string); isLocalizable = true;
                    break;

                case 's':
                    type = typeof(string);
                    break;

                case 'v':
                    type = typeof(Stream);
                    break;

                default: throw new InstallerException();
            }

            isRequired = char.IsLower(columnDefinition[0]);
            size = int.Parse(
                columnDefinition.Substring(1),
                CultureInfo.InvariantCulture.NumberFormat);
            if (type == typeof(int) && size <= 2)
            {
                type = typeof(short);
            }
        }

        /// <summary>
        /// Creates a new ColumnInfo object from a list of parameters.
        /// </summary>
        /// <param name="name">name of the column</param>
        /// <param name="type">type of the column; must be one of the following:
        /// Int16, Int32, String, or Stream</param>
        /// <param name="size">the maximum number of characters for String columns;
        /// ignored for other column types</param>
        /// <param name="isRequired">true if the column is required to have a non-null value</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public ColumnInfo(string name, Type type, int size, bool isRequired)
            : this(name, type, size, isRequired, false, false)
        {
        }

        /// <summary>
        /// Creates a new ColumnInfo object from a list of parameters.
        /// </summary>
        /// <param name="name">name of the column</param>
        /// <param name="type">type of the column; must be one of the following:
        /// Int16, Int32, String, or Stream</param>
        /// <param name="size">the maximum number of characters for String columns;
        /// ignored for other column types</param>
        /// <param name="isRequired">true if the column is required to have a non-null value</param>
        /// <param name="isTemporary">true to if the column is only in-memory and
        /// not persisted with the database</param>
        /// <param name="isLocalizable">for String columns, indicates the column
        /// is localizable; ignored for other column types</param>
        public ColumnInfo(string name, Type type, int size, bool isRequired, bool isTemporary, bool isLocalizable)
        {
            if (type == typeof(int))
            {
                size = 4;
                isLocalizable = false;
            }
            else if (type == typeof(short))
            {
                size = 2;
                isLocalizable = false;
            }
            else if (type == typeof(string))
            {
            }
            else if (type == typeof(Stream))
            {
                isLocalizable = false;
            }
            else
            {
                throw new ArgumentOutOfRangeException("type");
            }

            this.name = name ?? throw new ArgumentNullException("name");
            this.type = type;
            this.size = size;
            this.isRequired = isRequired;
            this.isTemporary = isTemporary;
            this.isLocalizable = isLocalizable;
        }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>name of the column</value>
        public string Name => name;

        /// <summary>
        /// Gets the type of the column as a System.Type.  This is one of the following: Int16, Int32, String, or Stream
        /// </summary>
        /// <value>type of the column</value>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type => type;

        /// <summary>
        /// Gets the type of the column as an integer that can be cast to a System.Data.DbType.  This is one of the following: Int16, Int32, String, or Binary
        /// </summary>
        /// <value>equivalent DbType of the column as an integer</value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public int DBType
        {
            get
            {
                if (type == typeof(short))
                {
                    return 10;
                }
                else if (type == typeof(int))
                {
                    return 11;
                }
                else if (type == typeof(Stream))
                {
                    return 1;
                }
                else
                {
                    return 16;
                }
            }
        }

        /// <summary>
        /// Gets the size of the column.
        /// </summary>
        /// <value>The size of integer columns this is either 2 or 4.  For string columns this is the maximum
        /// recommended length of the string, or 0 for unlimited length.  For stream columns, 0 is returned.</value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public int Size => size;

        /// <summary>
        /// Gets a value indicating whether the column must be non-null when inserting a record.
        /// </summary>
        /// <value>required status of the column</value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool IsRequired => isRequired;

        /// <summary>
        /// Gets a value indicating whether the column is temporary. Temporary columns are not persisted
        /// when the database is saved to disk.
        /// </summary>
        /// <value>temporary status of the column</value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool IsTemporary => isTemporary;

        /// <summary>
        /// Gets a value indicating whether the column is a string column that is localizable.
        /// </summary>
        /// <value>localizable status of the column</value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool IsLocalizable => isLocalizable;

        /// <summary>
        /// Gets an SQL fragment that can be used to create this column within a CREATE TABLE statement.
        /// </summary>
        /// <value>SQL fragment to be used for creating the column</value>
        /// <remarks><p>
        /// Examples:
        /// <list type="bullet">
        /// <item>LONG</item>
        /// <item>SHORT TEMPORARY</item>
        /// <item>CHAR(0) LOCALIZABLE</item>
        /// <item>CHAR(72) NOT NULL LOCALIZABLE</item>
        /// <item>OBJECT</item>
        /// </list>
        /// </p></remarks>
        public string SqlCreateString
        {
            get
            {
                StringBuilder s = new StringBuilder();
                s.AppendFormat("`{0}` ", name);
                if (type == typeof(short))
                {
                    s.Append("SHORT");
                }
                else if (type == typeof(int))
                {
                    s.Append("LONG");
                }
                else if (type == typeof(string))
                {
                    s.AppendFormat("CHAR({0})", size);
                }
                else
                {
                    s.Append("OBJECT");
                }

                if (isRequired)
                {
                    s.Append(" NOT NULL");
                }

                if (isTemporary)
                {
                    s.Append(" TEMPORARY");
                }

                if (isLocalizable)
                {
                    s.Append(" LOCALIZABLE");
                }

                return s.ToString();
            }
        }

        /// <summary>
        /// Gets a short string defining the type and size of the column.
        /// </summary>
        /// <value>
        /// The definition string consists
        /// of a single letter representing the data type followed by the width of the column (in characters
        /// when applicable, bytes otherwise). A width of zero designates an unbounded width (for example,
        /// long text fields and streams). An uppercase letter indicates that null values are allowed in
        /// the column.
        /// </value>
        /// <remarks><p>
        /// <list>
        /// <item>s? - String, variable length (?=1-255)</item>
        /// <item>s0 - String, variable length</item>
        /// <item>i2 - Short integer</item>
        /// <item>i4 - Long integer</item>
        /// <item>v0 - Binary Stream</item>
        /// <item>g? - Temporary string (?=0-255)</item>
        /// <item>j? - Temporary integer (?=0,1,2,4)</item>
        /// <item>l? - Localizable string, variable length (?=1-255)</item>
        /// <item>l0 - Localizable string, variable length</item>
        /// </list>
        /// </p></remarks>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string ColumnDefinitionString
        {
            get
            {
                char t;
                if (type == typeof(short) || type == typeof(int))
                {
                    t = (isTemporary ? 'j' : 'i');
                }
                else if (type == typeof(string))
                {
                    t = (isTemporary ? 'g' : isLocalizable ? 'l' : 's');
                }
                else
                {
                    t = 'v';
                }
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}{1}",
                    (isRequired ? t : char.ToUpper(t, CultureInfo.InvariantCulture)),
                    size);
            }
        }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <returns>Name of the column.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}