using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.OneGet {
    internal static class Messages {

        

        internal static class Miscellaneous {
            public static string NuGetRequired = "NuGet is Required";
            public static string NuGetNotFound = "NuGet not in place. Attempting to download";

            public static IDictionary<int, string> Codes = new Dictionary<int, string> {
                { 1, NuGetRequired },
                { 2, NuGetNotFound },
            };

        }


    }
}
