namespace Microsoft.OneGet.Packaging {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Utility.Extensions;

    public class DynamicOption : MarshalByRefObject {
        private IEnumerable<string> _values;
        public string ProviderName {get; set;}
        public OptionCategory Category {get; internal set;}
        public string Name {get; internal set;}
        public OptionType Type {get; internal set;}

        public bool IsRequired {get; internal set;}

        public IEnumerable<string> PossibleValues {
            get {
                return _values ?? Enumerable.Empty<string>();
            }
            internal set {
                _values = value.ByRef();
            }
        }

        public override object InitializeLifetimeService() {
            return null;
        }
    }
}