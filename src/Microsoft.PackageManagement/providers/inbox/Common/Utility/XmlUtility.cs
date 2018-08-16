namespace Microsoft.PackageManagement.Provider.Utility
{
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    public static class XmlUtility
    {
        public static XDocument LoadSafe(string filePath)
        {
            XmlReaderSettings settings = CreateSafeSettings();
            using (XmlReader reader = XmlReader.Create(filePath, settings))
            {
                return XDocument.Load(reader);
            }
        }

        public static XDocument LoadSafe(Stream input, bool ignoreWhiteSpace)
        {
            XmlReaderSettings settings = CreateSafeSettings(ignoreWhiteSpace);
            XmlReader reader = XmlReader.Create(input, settings);
            return XDocument.Load(reader);
        }

        private static XmlReaderSettings CreateSafeSettings(bool ignoreWhiteSpace = false)
        {
            XmlReaderSettings safeSettings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                IgnoreWhitespace = ignoreWhiteSpace
            };

            return safeSettings;
        }
    }
}