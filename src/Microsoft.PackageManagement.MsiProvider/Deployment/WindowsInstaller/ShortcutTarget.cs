//---------------------------------------------------------------------
// <copyright file="ShortcutTarget.cs" company="Microsoft Corporation">
//   Copyright (c) 1999, Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// Microsoft.Deployment.WindowsInstaller.ShortcutTarget structure.
// </summary>
//---------------------------------------------------------------------

namespace Microsoft.PackageManagement.Msi.Internal.Deployment.WindowsInstaller
{
    /// <summary>
    /// Holds information about the target of a shortcut file.
    /// </summary>
    public struct ShortcutTarget
    {
        private string productCode;
        private string feature;
        private string componentCode;

        internal ShortcutTarget(string productCode, string feature, string componentCode)
        {
            this.productCode = productCode;
            this.feature = feature;
            this.componentCode = componentCode;
        }

        /// <summary>
        /// Gets the target product code of the shortcut, or null if not available.
        /// </summary>
        public string ProductCode => productCode;

        /// <summary>
        /// Gets the name of the target feature of the shortcut, or null if not available.
        /// </summary>
        public string Feature => feature;

        /// <summary>
        /// Gets the target component code of the shortcut, or null if not available.
        /// </summary>
        public string ComponentCode => componentCode;

        /// <summary>
        /// Tests whether two shortcut targets have the same product code, feature, and/or component code.
        /// </summary>
        /// <param name="st1">The first shortcut target to compare.</param>
        /// <param name="st2">The second shortcut target to compare.</param>
        /// <returns>True if all parts of the targets are the same, else false.</returns>
        public static bool operator ==(ShortcutTarget st1, ShortcutTarget st2)
        {
            return st1.Equals(st2);
        }

        /// <summary>
        /// Tests whether two shortcut targets have the same product code, feature, and/or component code.
        /// </summary>
        /// <param name="st1">The first shortcut target to compare.</param>
        /// <param name="st2">The second shortcut target to compare.</param>
        /// <returns>True if any parts of the targets are different, else false.</returns>
        public static bool operator !=(ShortcutTarget st1, ShortcutTarget st2)
        {
            return !st1.Equals(st2);
        }

        /// <summary>
        /// Tests whether two shortcut targets have the same product code, feature, and/or component code.
        /// </summary>
        /// <param name="obj">The shortcut target to compare to the current object.</param>
        /// <returns>True if <paramref name="obj"/> is a shortcut target and all parts of the targets are the same, else false.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(ShortcutTarget))
            {
                return false;
            }
            ShortcutTarget st = (ShortcutTarget)obj;
            return productCode == st.productCode
                && feature == st.feature
                && componentCode == st.componentCode;
        }

        /// <summary>
        /// Generates a hash code using all parts of the shortcut target.
        /// </summary>
        /// <returns>An integer suitable for hashing the shortcut target.</returns>
        public override int GetHashCode()
        {
            return (productCode != null ? productCode.GetHashCode() : 0)
                ^ (feature != null ? feature.GetHashCode() : 0)
                ^ (componentCode != null ? componentCode.GetHashCode() : 0);
        }
    }
}