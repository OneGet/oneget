
namespace Microsoft.PackageManagement.Provider.Utility
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    /// <summary>
    /// Convert String  to SemanticVersion type
    /// </summary>
    public class SemanticVersionTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string stringValue && SemanticVersion.TryParse(stringValue, out SemanticVersion semVer))
            {
                return semVer;
            }
            return null;
        }
    }
}
