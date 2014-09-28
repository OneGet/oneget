namespace Microsoft.OneGet.Packaging {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Utility.Extensions;

    public class SoftwareIdentityVersionComparer : IComparer<SoftwareIdentity> {
        public static SoftwareIdentityVersionComparer Instance = new SoftwareIdentityVersionComparer();

        public int Compare(SoftwareIdentity x, SoftwareIdentity y) {
            if (x == null || y == null) {
                // can't compare vs null.
                return 0;
            }
            var xVersionScheme = x.VersionScheme ?? string.Empty;
            var yVersionScheme = y.VersionScheme ?? string.Empty;

            if (!x.VersionScheme.EqualsIgnoreCase(yVersionScheme)) {
                // can't compare versions between different version schemes
                return 0;
            }

            return CompareVersions(xVersionScheme, x.Version, y.Version);
        }

        public static int CompareVersions(string versionScheme, string xVersion, string yVersion) {
            xVersion = xVersion ?? string.Empty;
            yVersion = yVersion ?? string.Empty;

            switch ((versionScheme ?? "unknown").ToLowerInvariant()) {
                case "alphanumeric":
                    // string sort
                    return String.Compare(xVersion, yVersion, StringComparison.Ordinal);

                case "decimal":
                    double xDouble;
                    double yDouble;
                    if (double.TryParse(xVersion, out xDouble) && double.TryParse(yVersion, out yDouble)) {
                        return xDouble.CompareTo(yDouble);
                    }
                    return 0;

                case "multipartnumeric":
                    return CompareMultipartNumeric(xVersion, yVersion);

                case "multipartnumeric+suffix":
                    return CompareMultipartNumericSuffix(xVersion, yVersion);

                case "semver":
                    return CompareSemVer(xVersion, yVersion);

                case "unknown":
                    // can't sort what we don't know
                    return 0;

                default:
                    // can't sort what we don't know
                    return 0;
            }
        }

        private static int CompareMultipartNumeric(string xVersion, string yVersion) {
            var xs = xVersion.Split('.');
            var ys = yVersion.Split('.');
            for (var i = 0; i < xs.Length; i++) {
                ulong xLong;
                ulong yLong;

                if (ulong.TryParse(xs[i], out xLong) && ulong.TryParse(ys.Length > i ? ys[i] : "0", out yLong)) {
                    var compare = xLong.CompareTo(yLong);
                    if (compare != 0) {
                        return compare;
                    }
                    continue;
                }
                return 0;
            }
            return 0;
        }

        private static int CompareMultipartNumericSuffix(string xVersion, string yVersion) {
            var xPos = IndexOfNotAny(xVersion);
            var yPos = IndexOfNotAny(yVersion);
            var xMulti = xPos == -1 ? xVersion : xVersion.Substring(0, xPos);
            var yMulti = yPos == -1 ? yVersion : yVersion.Substring(0, yPos);
            var compare = CompareMultipartNumeric(xMulti, yMulti);
            if (compare != 0) {
                return compare;
            }

            if (xPos == -1 && yPos == -1) {
                // no suffixes?
                return 0;
            }

            if (xPos == -1) {
                // x has no suffix, y does
                // y is later.
                return -1;
            }

            if (yPos == -1) {
                // x has suffix, y doesn't
                // x is later.
                return 1;
            }

            return String.Compare(xVersion.Substring(xPos), yVersion.Substring(yPos), StringComparison.Ordinal);
        }

        private static int CompareSemVer(string xVersion, string yVersion) {
            var xPos = IndexOfNotAny(xVersion);
            var yPos = IndexOfNotAny(yVersion);
            var xMulti = xPos == -1 ? xVersion : xVersion.Substring(0, xPos);
            var yMulti = yPos == -1 ? yVersion : yVersion.Substring(0, yPos);
            var compare = CompareMultipartNumeric(xMulti, yMulti);
            if (compare != 0) {
                return compare;
            }

            if (xPos == -1 && yPos == -1) {
                // no suffixes?
                return 0;
            }

            if (xPos == -1) {
                // x has no suffix, y does
                // x is later.
                return 1;
            }

            if (yPos == -1) {
                // x has suffix, y doesn't
                // y is later.
                return -1;
            }

            return String.Compare(xVersion.Substring(xPos), yVersion.Substring(yPos), StringComparison.Ordinal);
        }

        private static int IndexOfNotAny(string version, params char[] chars) {
            if (string.IsNullOrEmpty(version)) {
                return -1;
            }
            var n = 0;
            foreach (var ch in version) {
                if (chars.Contains(ch)) {
                    return n;
                }
                n++;
            }
            return -1;
        }
    }
}