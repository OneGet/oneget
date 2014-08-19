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
    using Xunit.Sdk;
   
    [Prioritized]
    public class Class2 {

        [Fact, TestPriority(2)]
        public void testMe() {
            Thread.Sleep(1000);
            Console.WriteLine("First");
        }

        [Fact, TestPriority(1)]
        public void testMeToo() {
            Thread.Sleep(2000);
            Console.WriteLine("Second");
        }
    }
    [Prioritized]
    public class Class3 {

        [Fact, TestPriority(2)]
        public void testMe() {
            Thread.Sleep(1000);
            Console.WriteLine("First");
        }

        [Fact, TestPriority(1)]
        public void testMeToo() {
            Thread.Sleep(2000);
            Console.WriteLine("Second");
        }
    }
    
    class PrioritizedAttribute : RunWithAttribute {
        public PrioritizedAttribute()
            : base(typeof(PrioritizedFixtureClassCommand)) {
        }
    }

    class PrioritizedFixtureClassCommand : ITestClassCommand {
        readonly TestClassCommand _inner = new TestClassCommand();

        public int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun) {
            return 0;
        }

        public Exception ClassFinish() {
            return _inner.ClassFinish();
        }

        public Exception ClassStart() {
            return _inner.ClassStart();
        }

        public IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod) {
            return _inner.EnumerateTestCommands(testMethod);
        }

        public IEnumerable<IMethodInfo> EnumerateTestMethods() {
            return from m in _inner.EnumerateTestMethods()
                   let priority = GetPriority(m)
                   orderby priority
                   select m;
        }

        public bool IsTestMethod(IMethodInfo testMethod) {
            return _inner.IsTestMethod(testMethod);
        }

        public object ObjectUnderTest {
            get { return _inner.ObjectUnderTest; }
        }

        public ITypeInfo TypeUnderTest {
            get { return _inner.TypeUnderTest; }
            set { _inner.TypeUnderTest = value; }
        }

        private static int GetPriority(IMethodInfo method) {
            var priorityAttribute = method
                .GetCustomAttributes(typeof(TestPriorityAttribute))
                .FirstOrDefault();

            return priorityAttribute == null
                ? 0
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
}