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

namespace Microsoft.PackageManagement.Test.Utility.DynamicInterface.ParameterCoercion {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using PackageManagement.Utility.Plugin;
    using Xunit;
    using Xunit.Abstractions;
    using Console = Support.Console;

    public class HostImplementationOfRequest : IRequestInterface {
        public string CallBackToTheHost(string someText) {
            Console.WriteLine(someText);
            return "Success";
        }
    }

    /// <summary>
    ///     the host's idea of what the plugin should look like.
    /// </summary>
    public interface IPluginInterface {
        string DoSomething(string someText, object theRequest);
    }

    public interface IPluginInterfaceDiscardingValue {
        void DoSomething(string someText, object theRequest);
    }

    public interface IPluginInterfaceWithHandler {
        string DoSomething(string someText, object theRequest);
        void OnUnhandledException(string methodName, Exception e);
    }

    public class ThePluginImplementation {
        [SuppressMessage("Microsoft.Usage", "#pw26506")]
        public string DoSomething(string someText, ThePluginImplementationOfTheRequest theRequest) {
            // since we're able to use the request object as the plugin's strongly type version
            // we can call methods on it directly.
            Tests.Set(someText);

            return theRequest.CallBackToTheHost("You should see: some text.");
        }
    }

    public abstract class ThePluginImplementationOfTheRequest {
        // when the host calls our method that takes this, it should create a duck-typed version on the fly.
        public abstract string CallBackToTheHost(string someText);
    }

    /// <summary>
    ///     the host's idea of what the request should look like.
    /// </summary>
    public interface IRequestInterface {
        string CallBackToTheHost(string someText);
    }

    public class BadImplementation {
        public string DoSomething(string someText, ThePluginImplementationOfTheRequest theRequest) {
            // since we're able to use the request object as the plugin's strongly type version
            // we can call methods on it directly.
            Tests.Set("BadImplementationCalled");

            Console.WriteLine("We get into the call, but we're going to throw.");
            throw new Exception("ha ha");
        }
    }

    public class BadImplementationwithHandler {
        public string DoSomething(string someText, ThePluginImplementationOfTheRequest theRequest) {
            // since we're able to use the request object as the plugin's strongly type version
            // we can call methods on it directly.
            Tests.Set("BadImplementationwithHandlerCalled");

            Console.WriteLine("We get into the call, but we're going to throw.");
            throw new Exception("ha ha");
        }

        [SuppressMessage("Microsoft.Usage", "#pw26506")]
        public void OnUnhandledException(string methodName, Exception e) {
            Console.WriteLine("Method : {0}", methodName);
            Console.WriteLine("Exception: {0}/{1}/{2}", e.GetType().Name, e.Message, e.StackTrace);
            Tests.Set("UnhandledExceptionCalled");
        }
    }

    public class AdvancedDynamicInterfaceTests : Tests {
        public AdvancedDynamicInterfaceTests(ITestOutputHelper outputHelper) : base(outputHelper) {
        }

        [Fact]
        public void CreateDuckTypedBindingForMethodWithDuckTypedParameters() {
            using (CaptureConsole) {
                // create the plugin object:
                var plugin = new ThePluginImplementation();

                // create the host's duck typed version of the plugin:
                var duckTypedPlugin = plugin.As<IPluginInterface>();

                // create the host's actual implementation of the request
                var request = new HostImplementationOfRequest();

                // the host should call the method on the plugin:
                var result = duckTypedPlugin.DoSomething("sample call 1", request);

                Ensure("sample call 1", "The method didn't get called.");

                Assert.True("Success".Equals(result));
            }
        }

        [Fact]
        public void ExceptionHandlingWithoutHandler() {
            using (CaptureConsole) {
                // create the plugin object:
                var plugin = new BadImplementation();

                // create the host's duck typed version of the plugin:
                var duckTypedPlugin = plugin.As<IPluginInterface>();

                // create the host's actual implementation of the request
                var request = new HostImplementationOfRequest();

                // the host should call the method on the plugin:
                var result = duckTypedPlugin.DoSomething("sample call 2", request);

                Assert.Equal(null, result);
                Ensure("BadImplementationCalled", "The method failed to get called, up to the point of the exception.");
            }
        }

        [Fact]
        public void DiscardingValue() {
            using (CaptureConsole) {
                // create the plugin object:
                var plugin = new ThePluginImplementation();

                // create the host's duck typed version of the plugin:
                var duckTypedPlugin = plugin.As<IPluginInterfaceDiscardingValue>();

                // create the host's actual implementation of the request
                var request = new HostImplementationOfRequest();

                // the host should call the method on the plugin:
                duckTypedPlugin.DoSomething("sample call 3", request);

                Ensure("sample call 3", "ducktyped object where host's interface was void (and implementation didn't) failed to call correctly");
            }
        }

        [Fact]
        public void ExceptionWithHandler() {
            using (CaptureConsole) {
                // create the plugin object:
                var plugin = new BadImplementationwithHandler();

                // create the host's duck typed version of the plugin:
                var duckTypedPlugin = plugin.As<IPluginInterfaceWithHandler>();

                // create the host's actual implementation of the request
                var request = new HostImplementationOfRequest();

                // the host should call the method on the plugin:
                var result = duckTypedPlugin.DoSomething("try this", request);

                // result from a duck typed object throwing an exception should be null.
                Assert.Null(result);

                Ensure("BadImplementationwithHandlerCalled", "The method should get called, up to the point of the exception.");

                Ensure("UnhandledExceptionCalled", "When we specify an OnUnhandledException method in the host interface, and the client implements it, we should make sure it gets called.");
            }
        }

        [Fact]
        public void testx() {
            // Assert.True(DynamicInterface.CanCreateFrom(typeof(int), typeof(string)));
            // Assert.True( DynamicInterface.CanCreateFrom(typeof (string), typeof (int)));
        }
    }
}