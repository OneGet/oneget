// 
//  Copyright (c) Microsoft Corporation. All rights reserved. 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  

namespace Microsoft.PackageManagement.Test.Utility.DynamicInterface.NameMatching {
    using PackageManagement.Utility.Plugin;
    using Xunit;
    using Xunit.Abstractions;

    public interface IHasProperty {
        string OtherName {get;}
        string GetName();
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

    public class NameTests : Tests {
        public NameTests(ITestOutputHelper outputHelper) : base(outputHelper) {
        }

        [Fact]
        public void CheckGetToProperty1() {
            var hp1 = new HasProperty1().As<IHasProperty>();
            Assert.Equal("Name", hp1.GetName());
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