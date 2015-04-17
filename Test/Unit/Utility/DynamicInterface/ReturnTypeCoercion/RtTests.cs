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

namespace Microsoft.PackageManagement.Test.Utility.DynamicInterface.ReturnTypeCoercion {
    using System.Collections.Generic;
    using System.Linq;
    using PackageManagement.Utility.Plugin;
    using Xunit;
    using Xunit.Abstractions;

    public class MyPluginImplementation {
        public int ReturnAString() {
            return 100;
        }

        public IEnumerable<string> ReturnAStringCollection() {
            return new[] {"One", "Two", "Three"};
        }

        public SomeType ReturnSomeObject() {
            return new SomeType("One");
        }

        public IEnumerable<SomeType> ReturnSomeObjects() {
            return new[] {new SomeType("One"), new SomeType("Two"), new SomeType("Three")};
        }

        public IEnumerable<SomeType> ReturnSomeObjectsToArray() {
            yield return new SomeType("Three");
            yield return new SomeType("Two");
            yield return new SomeType("One");
        }
    }

    public class SomeType {
        private readonly string _name;

        public SomeType(string name) {
            _name = name;
        }

        public string Name {
            get {
                return _name;
            }
        }
    }

    public interface IConsumerImplementation {
        string ReturnAString();
        IEnumerable<object> ReturnAStringCollection();
        ConsumerSomeType ReturnSomeObject();
        IEnumerable<ConsumerSomeType> ReturnSomeObjects();
        ConsumerSomeType[] ReturnSomeObjectsToArray();
    }

    public abstract class ConsumerSomeType {
        public abstract string Name {get;}
    }

    public interface IPluginInterface {
        string ReturnAString();
        IEnumerable<string> ReturnAStringCollection();
        SomeType ReturnSomeObject();
        IEnumerable<SomeType> ReturnSomeObjects();
        IEnumerable<SomeType> ReturnSomeObjectsToArray();
    }

    public class RtTests : Tests {
        public RtTests(ITestOutputHelper outputHelper)
            : base(outputHelper) {
        }

        [Fact]
        public void TestChangingReturnType() {
            // create the service object
            var plugin = new MyPluginImplementation();

            // create the duck-typed wrapper
            var dPlugin = plugin.As<IConsumerImplementation>();

            // check that the returned value for string.
            Assert.Equal("100", dPlugin.ReturnAString());

            var items = dPlugin.ReturnAStringCollection();
            Assert.NotNull(items);
            Assert.Equal(3, items.Count());
            Assert.Equal("One", items.First());
            Assert.Equal("Two", items.Skip(1).First());
            Assert.Equal("Three", items.Skip(2).First());

            var someObject = dPlugin.ReturnSomeObject();
            Assert.NotNull(someObject);
            Assert.Equal("One", someObject.Name);

            var someObjects = dPlugin.ReturnSomeObjects();
            Assert.NotNull(someObjects);
            Assert.Equal(3, someObjects.Count());
            Assert.Equal("One", someObjects.First().Name);
            Assert.Equal("Two", someObjects.Skip(1).First().Name);
            Assert.Equal("Three", someObjects.Skip(2).First().Name);

            var moreObjects = dPlugin.ReturnSomeObjectsToArray();
            Assert.NotNull(moreObjects);
            Assert.Equal(3, moreObjects.Length);
            Assert.Equal("Three", moreObjects[0].Name);
            Assert.Equal("Two", moreObjects[1].Name);
            Assert.Equal("One", moreObjects[2].Name);
        }
    }
}