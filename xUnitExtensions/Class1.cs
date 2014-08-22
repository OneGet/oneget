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

namespace xUnitExtensions {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Xunit;
    using Xunit.Extensions;
    using Xunit.Sdk;


    internal static class Number {
        internal static int Value;
    }

    [Priority(2)]
    public class SecondClass {

        [Fact]
        public void B_SecondMethod() {
            Thread.Sleep(100);
            Console.WriteLine("ran {0}", Number.Value++);
        }

        [Fact(Priority = 3)]
        public void A_ThirdMethod() {
            Thread.Sleep(200);
            Console.WriteLine("ran {0}", Number.Value++);
        }


        [Fact(Priority = 1)]
        public void Z_FirstMethod() {
            Thread.Sleep(200);
            Console.WriteLine("ran {0}", Number.Value++);
        }
    }


    [Priority(1)]
    public class FirstClass {

        [Fact(Priority = 2)]
        public void SecondMethod() {
            Thread.Sleep(100);
            Console.WriteLine("ran {0}", Number.Value++);
        }

        [Fact(Priority = 1)]
        public void FirstMethod() {
            Thread.Sleep(200);
            Console.WriteLine("ran {0}", Number.Value++);
        }

        [Fact(Priority = 5)]
        public void FifthMethod() {
            Thread.Sleep(200);
            Console.WriteLine("ran {0}", Number.Value++);
        }
    }


    [Priority(0)]
    public class ZeroClass {
        [Fact(Priority = 0)]
        public void ZeroMethod() {
            Thread.Sleep(200);
            Console.WriteLine("ran {0}", Number.Value++);
        }

        [Fact(Priority = 0)]
        public void SecondMethod() {
            Thread.Sleep(100);
            Console.WriteLine("ran {0}", Number.Value++);
        }

        [Fact(Priority = 0)]
        public void FirstMethod() {
            Thread.Sleep(200);
            Console.WriteLine("ran {0}", Number.Value++);
        }

        [Fact(Priority = 0)]
        public void FifthMethod() {
            Thread.Sleep(2000);
            Console.WriteLine("ran {0}", Number.Value++);
        }
    }
    /*
    public class PrioritizedAttribute : RunWithAttribute {
        public PrioritizedAttribute()
            : base(typeof(PrioritizedFixtureClassCommand)) {
        }
    }

    class PrioritizedFixtureClassCommand : ITestClassCommand {
        readonly TestClassCommand _base = new TestClassCommand();

        public int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun) {
            return 0;
        }

        public Exception ClassFinish() {
            return _base.ClassFinish();
        }

        public Exception ClassStart() {
            return _base.ClassStart();
        }

        public IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod) {
            return _base.EnumerateTestCommands(testMethod);
        }

        public IEnumerable<IMethodInfo> EnumerateTestMethods() {
            return from m in _base.EnumerateTestMethods()
                   let priority = GetPriority(m)
                   orderby  priority ascending 
                   select m;
        }

        public bool IsTestMethod(IMethodInfo testMethod) {
            return _base.IsTestMethod(testMethod);
        }

        public object ObjectUnderTest {
            get { return _base.ObjectUnderTest; }
        }

        public ITypeInfo TypeUnderTest {
            get { return _base.TypeUnderTest; }
            set { _base.TypeUnderTest = value; }
        }

        private static int GetPriority(IMethodInfo method) {
            var priorityAttribute = method
                .GetCustomAttributes(typeof(TestPriorityAttribute))
                .FirstOrDefault();

            return priorityAttribute == null
                ? 100000
                : priorityAttribute.GetPropertyValue<int>("Priority");
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    class TestPriorityAttribute : Attribute {
        readonly int _priority;

        public TestPriorityAttribute(int priority) {
            _priority = priority;
        }

        public int Priority {
            get { return _priority; }
        }
    }
     */
}