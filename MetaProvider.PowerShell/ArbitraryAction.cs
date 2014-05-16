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

namespace Microsoft.OneGet.MetaProvider.PowerShell {
    using System;
    using System.Globalization;


	public abstract class Arbitrary : MarshalByRefObject {
		protected readonly Func<object[],object> _func;
		// we don't want these objects being gc's out because they remain unused...
        public override object InitializeLifetimeService() {
            return null;
        }

		public Arbitrary() {
        }

        public Arbitrary(Func<object[],object> func){
            _func= func;
        }

	}

	public class ArbitraryAction : Arbitrary {
        public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke() {
            _func.Invoke(null);
        }
    }


    public class ArbitraryAction< T0 > : Arbitrary {

		public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke(T0 tVal0 ) {
            try {
                _func.Invoke(new object[] {tVal0});
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }

    public class ArbitraryAction< T0 ,T1 > : Arbitrary {

		public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ) {
            try {
                _func.Invoke(new object[] {tVal0 ,tVal1});
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }

    public class ArbitraryAction< T0 ,T1 ,T2 > : Arbitrary {

		public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ) {
            try {
                _func.Invoke(new object[] {tVal0 ,tVal1 ,tVal2});
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }

    public class ArbitraryAction< T0 ,T1 ,T2 ,T3 > : Arbitrary {

		public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ) {
            try {
                _func.Invoke(new object[] {tVal0 ,tVal1 ,tVal2 ,tVal3});
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }

    public class ArbitraryAction< T0 ,T1 ,T2 ,T3 ,T4 > : Arbitrary {

		public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ) {
            try {
                _func.Invoke(new object[] {tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4});
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }

    public class ArbitraryAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 > : Arbitrary {

		public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ) {
            try {
                _func.Invoke(new object[] {tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5});
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }

    public class ArbitraryAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 > : Arbitrary {

		public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ) {
            try {
                _func.Invoke(new object[] {tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6});
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }

    public class ArbitraryAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 > : Arbitrary {

		public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ) {
            try {
                _func.Invoke(new object[] {tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7});
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }

    public class ArbitraryAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 > : Arbitrary {

		public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ,T8 tVal8 ) {
            try {
                _func.Invoke(new object[] {tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7 ,tVal8});
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }

    public class ArbitraryAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 > : Arbitrary {

		public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ,T8 tVal8 ,T9 tVal9 ) {
            try {
                _func.Invoke(new object[] {tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7 ,tVal8 ,tVal9});
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }

    public class ArbitraryAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 > : Arbitrary {

		public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ,T8 tVal8 ,T9 tVal9 ,T10 tVal10 ) {
            try {
                _func.Invoke(new object[] {tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7 ,tVal8 ,tVal9 ,tVal10});
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }

    public class ArbitraryAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 > : Arbitrary {

		public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ,T8 tVal8 ,T9 tVal9 ,T10 tVal10 ,T11 tVal11 ) {
            try {
                _func.Invoke(new object[] {tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7 ,tVal8 ,tVal9 ,tVal10 ,tVal11});
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }

    public class ArbitraryAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 ,T12 > : Arbitrary {

		public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ,T8 tVal8 ,T9 tVal9 ,T10 tVal10 ,T11 tVal11 ,T12 tVal12 ) {
            try {
                _func.Invoke(new object[] {tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7 ,tVal8 ,tVal9 ,tVal10 ,tVal11 ,tVal12});
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }

    public class ArbitraryAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 ,T12 ,T13 > : Arbitrary {

		public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ,T8 tVal8 ,T9 tVal9 ,T10 tVal10 ,T11 tVal11 ,T12 tVal12 ,T13 tVal13 ) {
            try {
                _func.Invoke(new object[] {tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7 ,tVal8 ,tVal9 ,tVal10 ,tVal11 ,tVal12 ,tVal13});
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }

    public class ArbitraryAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 ,T12 ,T13 ,T14 > : Arbitrary {

		public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ,T8 tVal8 ,T9 tVal9 ,T10 tVal10 ,T11 tVal11 ,T12 tVal12 ,T13 tVal13 ,T14 tVal14 ) {
            try {
                _func.Invoke(new object[] {tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7 ,tVal8 ,tVal9 ,tVal10 ,tVal11 ,tVal12 ,tVal13 ,tVal14});
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }

    public class ArbitraryAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 ,T12 ,T13 ,T14 ,T15 > : Arbitrary {

		public ArbitraryAction() {
        }

        public ArbitraryAction(Func<object[],object> func): base(func) {
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ,T8 tVal8 ,T9 tVal9 ,T10 tVal10 ,T11 tVal11 ,T12 tVal12 ,T13 tVal13 ,T14 tVal14 ,T15 tVal15 ) {
            try {
                _func.Invoke(new object[] {tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7 ,tVal8 ,tVal9 ,tVal10 ,tVal11 ,tVal12 ,tVal13 ,tVal14 ,tVal15});
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }
    }
}    
