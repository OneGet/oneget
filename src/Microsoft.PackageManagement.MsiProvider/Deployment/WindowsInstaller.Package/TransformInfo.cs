//---------------------------------------------------------------------
// <copyright file="TransformInfo.cs" company="Microsoft Corporation">
//   Copyright (c) 1999, Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// Part of the Deployment Tools Foundation project.
// </summary>
//---------------------------------------------------------------------

namespace Microsoft.PackageManagement.Msi.Internal.Deployment.WindowsInstaller.Package
{
    using System;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Contains properties of a transform package (.MST).
    /// </summary>
    internal class TransformInfo
    {
        /// <summary>
        /// Reads transform information from a transform package.
        /// </summary>
        /// <param name="mstFile">Path to a transform package (.MST file).</param>
        public TransformInfo(string mstFile)
        {
            name = Path.GetFileName(mstFile);
            using (SummaryInfo transformSummInfo = new SummaryInfo(mstFile, false))
            {
                DecodeSummaryInfo(transformSummInfo);
            }
        }

        /// <summary>
        /// Reads transform information from the summary information of a transform package.
        /// </summary>
        /// <param name="name">Filename of the transform (optional).</param>
        /// <param name="transformSummaryInfo">Handle to the summary information of a transform package (.MST file).</param>
        public TransformInfo(string name, SummaryInfo transformSummaryInfo)
        {
            this.name = name;
            DecodeSummaryInfo(transformSummaryInfo);
        }

        private void DecodeSummaryInfo(SummaryInfo transformSummaryInfo)
        {
            try
            {
                string[] rev = transformSummaryInfo.RevisionNumber.Split(new char[] { ';' }, 3);
                targetProductCode = rev[0].Substring(0, 38);
                targetProductVersion = rev[0].Substring(38);
                upgradeProductCode = rev[1].Substring(0, 38);
                upgradeProductVersion = rev[1].Substring(38);
                upgradeCode = rev[2];

                string[] templ = transformSummaryInfo.Template.Split(new char[] { ';' }, 2);
                targetPlatform = templ[0];
                targetLanguage = 0;
                if (templ.Length >= 2 && templ[1].Length > 0)
                {
                    targetLanguage = int.Parse(templ[1], CultureInfo.InvariantCulture.NumberFormat);
                }

                validateFlags = (TransformValidations)transformSummaryInfo.CharacterCount;
            }
            catch (Exception ex)
            {
                throw new InstallerException("Invalid transform summary info", ex);
            }
        }

        /// <summary>
        /// Gets the filename of the transform.
        /// </summary>
        public string Name => name;

        private readonly string name;

        /// <summary>
        /// Gets the target product code of the transform.
        /// </summary>
        public string TargetProductCode => targetProductCode;

        private string targetProductCode;

        /// <summary>
        /// Gets the target product version of the transform.
        /// </summary>
        public string TargetProductVersion => targetProductVersion;

        private string targetProductVersion;

        /// <summary>
        /// Gets the upgrade product code of the transform.
        /// </summary>
        public string UpgradeProductCode => upgradeProductCode;

        private string upgradeProductCode;

        /// <summary>
        /// Gets the upgrade product version of the transform.
        /// </summary>
        public string UpgradeProductVersion => upgradeProductVersion;

        private string upgradeProductVersion;

        /// <summary>
        /// Gets the upgrade code of the transform.
        /// </summary>
        public string UpgradeCode => upgradeCode;

        private string upgradeCode;

        /// <summary>
        /// Gets the target platform of the transform.
        /// </summary>
        public string TargetPlatform => targetPlatform;

        private string targetPlatform;

        /// <summary>
        /// Gets the target language of the transform, or 0 if the transform is language-neutral.
        /// </summary>
        public int TargetLanguage => targetLanguage;

        private int targetLanguage;

        /// <summary>
        /// Gets the validation flags specified when the transform was generated.
        /// </summary>
        public TransformValidations Validations => validateFlags;

        private TransformValidations validateFlags;

        /// <summary>
        /// Returns the name of the transform.
        /// </summary>
        public override string ToString()
        {
            return Name ?? "MST";
        }
    }
}