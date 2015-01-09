using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.OneGet.Test.Utility.DynamicInterface.NameMatching {
    using System.Security.Policy;
    using OneGet.Utility.Plugin;
    using Xunit;
    using Xunit.Abstractions;
    using Console = Support.Console;

    public interface IHasProperty {
        string GetName();

        string OtherName {get;}
    }

    public class HasProperty1 {
        public string GetName() {
            return "Name";
        }

        public string GetOtherName() {
            return "OtherName";
        }
    }

    public class HasProperty2 {
        public string Name {
            get {
                return "Name";            
            }
        }

        public string OtherName {
            get {
                return "OtherName";
            }
        }
    }


    public class NameTests: Tests {

        public NameTests(ITestOutputHelper outputHelper) : base (outputHelper) {
        }


        [Fact]
        public void CheckGetToProperty1() {
            var hp1 = new HasProperty1().As<IHasProperty>();
            Assert.Equal( "Name", hp1.GetName() );
        }
        [Fact]
        public void CheckGetToProperty2() {
            var hp1 = new HasProperty1().As<IHasProperty>();
            Assert.Equal("OtherName", hp1.OtherName);
        }
        [Fact]
        public void CheckGetToProperty3() {
            var hp2 = new HasProperty2().As<IHasProperty>();
            Assert.Equal("Name", hp2.GetName());
        }
        [Fact]
        public void CheckGetToProperty4() {
            var hp2 = new HasProperty2().As<IHasProperty>();
            Assert.Equal("OtherName", hp2.OtherName);
        }
    }
}
