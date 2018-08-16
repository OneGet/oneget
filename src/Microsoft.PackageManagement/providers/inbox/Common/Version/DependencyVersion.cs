using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.PackageManagement.Provider.Utility
{
    /// <summary>
    /// Represents dependency version returned by nuget api call
    /// </summary>
    public class DependencyVersion
    {
        public SemanticVersion MinVersion { get; set; }
        /// <summary>
        /// True if the version we are looking for includes min version
        /// </summary>

        public bool IsMinInclusive { get; set; }
        public SemanticVersion MaxVersion { get; set; }

        /// <summary>
        /// True if the version we are looking for includes the max version
        /// </summary>
        public bool IsMaxInclusive { get; set; }

        /// <summary>
        /// Parse and return a dependency version
        /// The version string is either a simple version or an arithmetic range
        /// e.g.
        ///      1.0         --> 1.0 ≤ x
        ///      (,1.0]      --> x ≤ 1.0
        ///      (,1.0)      --> x lt 1.0
        ///      [1.0]       --> x == 1.0
        ///      (1.0,)      --> 1.0 lt x
        ///      (1.0, 2.0)   --> 1.0 lt x lt 2.0
        ///      [1.0, 2.0]   --> 1.0 ≤ x ≤ 2.0
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DependencyVersion ParseDependencyVersion(string value)
        {
            DependencyVersion depVers = new DependencyVersion();

            if (string.IsNullOrWhiteSpace(value))
            {
                return depVers;
            }

            value = value.Trim();

            char first = value.First();
            char last = value.Last();

            if (first != '(' && first != '[' && last != ']' && last != ')')
            {
                // Stand alone version
                depVers.IsMinInclusive = true;
                depVers.MinVersion = new SemanticVersion(value);
                return depVers;
            }

            // value must have length greater than 3
            if (value.Length < 3)
            {
                return depVers;
            }

            // The first character must be [ or (
            switch (value.First())
            {
                case '[':
                    depVers.IsMinInclusive = true;
                    break;

                case '(':
                    depVers.IsMinInclusive = false;
                    break;

                default:
                    // If not, return without setting anything
                    return depVers;
            }

            // The last character must be ] or )
            switch (value.Last())
            {
                case ']':
                    depVers.IsMaxInclusive = true;
                    break;

                case ')':
                    depVers.IsMaxInclusive = false;
                    break;

                default:
                    // If not, return without setting anything
                    return depVers;
            }

            // Get rid of the two brackets
            value = value.Substring(1, value.Length - 2);

            // Split by comma, and make sure we don't get more than two pieces
            string[] parts = value.Split(',');

            // Wrong format if we have more than 2 parts or all the parts are empty
            if (parts.Length > 2 || parts.All(string.IsNullOrEmpty))
            {
                return depVers;
            }

            // First part is min
            string minVersionString = parts[0];

            // If there is only 1 part then first part will also be max
            string maxVersionString = (parts.Length == 2) ? parts[1] : parts[0];

            // Get min version if we have it
            if (!string.IsNullOrWhiteSpace(minVersionString))
            {
                depVers.MinVersion = new SemanticVersion(minVersionString);
            }

            // Get max version if we have it
            if (!string.IsNullOrWhiteSpace(maxVersionString))
            {
                depVers.MaxVersion = new SemanticVersion(maxVersionString);
            }

            return depVers;
        }

        public override string ToString()
        {
            // Returns nothing if no min or max
            if (MinVersion == null && MaxVersion == null)
            {
                return null;
            }

            // If we have min and minInclusive but no max, then return min string
            if (MinVersion != null && IsMinInclusive && MaxVersion == null && !IsMaxInclusive)
            {
                return MinVersion.ToString();
            }

            // MinVersion and MaxVersion is the same and both inclusives then return the value
            if (MinVersion == MaxVersion && IsMinInclusive && IsMaxInclusive)
            {
                return string.Format(CultureInfo.InvariantCulture, "[{0}]", MinVersion);
            }

            char lhs = IsMinInclusive ? '[' : '(';
            char rhs = IsMaxInclusive ? ']' : ')';

            return string.Format(CultureInfo.InvariantCulture, "{0}{1}, {2}{3}", lhs, MinVersion, MaxVersion, rhs);
        }
    }

    public class DependencyVersionComparerBasedOnMinVersion : IComparer<DependencyVersion>
    {
        /// <summary>
        /// Return 1 if dep1.minversion gt dep2.minversion
        /// Return 0 if dep1.minversion eq dep2.minversion
        /// Return -1 if dep.minversion lt dep2.minversion
        /// We consider null min version as the smallest possible value
        /// so if dep1.minversion = null and dep2.minversion = 0.1 then we return -1 since dep1.minversion lt dep2.minversion
        /// </summary>
        /// <param name="dep1"></param>
        /// <param name="dep2"></param>
        /// <returns></returns>
        public int Compare(DependencyVersion dep1, DependencyVersion dep2)
        {
            if (dep1.MinVersion == null)
            {
                if (dep2.MinVersion == null)
                {
                    return 0;
                }

                return -1;
            }

            // get here means dep1.minversion is not null
            if (dep2.MinVersion == null)
            {
                return 1;
            }

            // if they are the same, the one with min inclusive is smaller
            if (dep1.MinVersion.Equals(dep2.MinVersion))
            {
                if (dep1.IsMinInclusive)
                {
                    if (dep2.IsMinInclusive)
                    {
                        return 0;
                    }

                    return -1;
                }

                // reach here means dep1 is not min inclusive
                if (dep2.IsMinInclusive)
                {
                    return 1;
                }

                // here means both are not mean inclusive
                return 0;
            }

            // reach here means both are not null
            return dep1.MinVersion.CompareTo(dep2.MinVersion);
        }
    }
}