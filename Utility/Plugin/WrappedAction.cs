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

namespace Microsoft.OneGet.Utility.Plugin {
    using System;
    using System.Globalization;

	public abstract class Invokable : MarshalByRefObject {
		public abstract object DynamicInvoke(object[] args);

		// we don't want these objects being gc's out because they remain unused...
        public override object InitializeLifetimeService() {
            return null;
        }
	}

	public class WrappedAction : Invokable {
        private readonly Action _action;

        public WrappedAction() { 

        }

        public WrappedAction(Action action) {
            _action = action;
        }

        public void Invoke() {
            _action.Invoke();
        }

		public override object DynamicInvoke(object[] args) {
			Invoke();
			return null;
		}
    }

    public class WrappedAction< T0 > : Invokable {
        private readonly Action<T0 > _action;

        public WrappedAction() {
        }

        public WrappedAction(Action<T0 > action) {
            _action = action;
        }

        public void Invoke(T0 tVal0 ) {
            try {
                _action.Invoke(tVal0);
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }

		public override object DynamicInvoke(object[] args) {
			if( args == null ) {
				throw new ArgumentNullException("args");
			}
			if( args.Length < 1 ) {
                throw new Exception( "DynamicInvoke with too few args");
			}
			Invoke((T0)args[0]);
			return null;
		}
    }
    public class WrappedAction< T0 ,T1 > : Invokable {
        private readonly Action<T0 ,T1 > _action;

        public WrappedAction() {
        }

        public WrappedAction(Action<T0 ,T1 > action) {
            _action = action;
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ) {
            try {
                _action.Invoke(tVal0 ,tVal1);
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }

		public override object DynamicInvoke(object[] args) {
			if( args == null ) {
				throw new ArgumentNullException("args");
			}
			if( args.Length < 2 ) {
                throw new Exception( "DynamicInvoke with too few args");
			}
			Invoke((T0)args[0] ,(T1)args[1]);
			return null;
		}
    }
    public class WrappedAction< T0 ,T1 ,T2 > : Invokable {
        private readonly Action<T0 ,T1 ,T2 > _action;

        public WrappedAction() {
        }

        public WrappedAction(Action<T0 ,T1 ,T2 > action) {
            _action = action;
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ) {
            try {
                _action.Invoke(tVal0 ,tVal1 ,tVal2);
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }

		public override object DynamicInvoke(object[] args) {
			if( args == null ) {
				throw new ArgumentNullException("args");
			}
			if( args.Length < 3 ) {
                throw new Exception( "DynamicInvoke with too few args");
			}
			Invoke((T0)args[0] ,(T1)args[1] ,(T2)args[2]);
			return null;
		}
    }
    public class WrappedAction< T0 ,T1 ,T2 ,T3 > : Invokable {
        private readonly Action<T0 ,T1 ,T2 ,T3 > _action;

        public WrappedAction() {
        }

        public WrappedAction(Action<T0 ,T1 ,T2 ,T3 > action) {
            _action = action;
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ) {
            try {
                _action.Invoke(tVal0 ,tVal1 ,tVal2 ,tVal3);
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }

		public override object DynamicInvoke(object[] args) {
			if( args == null ) {
				throw new ArgumentNullException("args");
			}
			if( args.Length < 4 ) {
                throw new Exception( "DynamicInvoke with too few args");
			}
			Invoke((T0)args[0] ,(T1)args[1] ,(T2)args[2] ,(T3)args[3]);
			return null;
		}
    }
    public class WrappedAction< T0 ,T1 ,T2 ,T3 ,T4 > : Invokable {
        private readonly Action<T0 ,T1 ,T2 ,T3 ,T4 > _action;

        public WrappedAction() {
        }

        public WrappedAction(Action<T0 ,T1 ,T2 ,T3 ,T4 > action) {
            _action = action;
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ) {
            try {
                _action.Invoke(tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4);
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }

		public override object DynamicInvoke(object[] args) {
			if( args == null ) {
				throw new ArgumentNullException("args");
			}
			if( args.Length < 5 ) {
                throw new Exception( "DynamicInvoke with too few args");
			}
			Invoke((T0)args[0] ,(T1)args[1] ,(T2)args[2] ,(T3)args[3] ,(T4)args[4]);
			return null;
		}
    }
    public class WrappedAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 > : Invokable {
        private readonly Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 > _action;

        public WrappedAction() {
        }

        public WrappedAction(Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 > action) {
            _action = action;
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ) {
            try {
                _action.Invoke(tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5);
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }

		public override object DynamicInvoke(object[] args) {
			if( args == null ) {
				throw new ArgumentNullException("args");
			}
			if( args.Length < 6 ) {
                throw new Exception( "DynamicInvoke with too few args");
			}
			Invoke((T0)args[0] ,(T1)args[1] ,(T2)args[2] ,(T3)args[3] ,(T4)args[4] ,(T5)args[5]);
			return null;
		}
    }
    public class WrappedAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 > : Invokable {
        private readonly Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 > _action;

        public WrappedAction() {
        }

        public WrappedAction(Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 > action) {
            _action = action;
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ) {
            try {
                _action.Invoke(tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6);
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }

		public override object DynamicInvoke(object[] args) {
			if( args == null ) {
				throw new ArgumentNullException("args");
			}
			if( args.Length < 7 ) {
                throw new Exception( "DynamicInvoke with too few args");
			}
			Invoke((T0)args[0] ,(T1)args[1] ,(T2)args[2] ,(T3)args[3] ,(T4)args[4] ,(T5)args[5] ,(T6)args[6]);
			return null;
		}
    }
    public class WrappedAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 > : Invokable {
        private readonly Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 > _action;

        public WrappedAction() {
        }

        public WrappedAction(Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 > action) {
            _action = action;
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ) {
            try {
                _action.Invoke(tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7);
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }

		public override object DynamicInvoke(object[] args) {
			if( args == null ) {
				throw new ArgumentNullException("args");
			}
			if( args.Length < 8 ) {
                throw new Exception( "DynamicInvoke with too few args");
			}
			Invoke((T0)args[0] ,(T1)args[1] ,(T2)args[2] ,(T3)args[3] ,(T4)args[4] ,(T5)args[5] ,(T6)args[6] ,(T7)args[7]);
			return null;
		}
    }
    public class WrappedAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 > : Invokable {
        private readonly Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 > _action;

        public WrappedAction() {
        }

        public WrappedAction(Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 > action) {
            _action = action;
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ,T8 tVal8 ) {
            try {
                _action.Invoke(tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7 ,tVal8);
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }

		public override object DynamicInvoke(object[] args) {
			if( args == null ) {
				throw new ArgumentNullException("args");
			}
			if( args.Length < 9 ) {
                throw new Exception( "DynamicInvoke with too few args");
			}
			Invoke((T0)args[0] ,(T1)args[1] ,(T2)args[2] ,(T3)args[3] ,(T4)args[4] ,(T5)args[5] ,(T6)args[6] ,(T7)args[7] ,(T8)args[8]);
			return null;
		}
    }
    public class WrappedAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 > : Invokable {
        private readonly Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 > _action;

        public WrappedAction() {
        }

        public WrappedAction(Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 > action) {
            _action = action;
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ,T8 tVal8 ,T9 tVal9 ) {
            try {
                _action.Invoke(tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7 ,tVal8 ,tVal9);
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }

		public override object DynamicInvoke(object[] args) {
			if( args == null ) {
				throw new ArgumentNullException("args");
			}
			if( args.Length < 10 ) {
                throw new Exception( "DynamicInvoke with too few args");
			}
			Invoke((T0)args[0] ,(T1)args[1] ,(T2)args[2] ,(T3)args[3] ,(T4)args[4] ,(T5)args[5] ,(T6)args[6] ,(T7)args[7] ,(T8)args[8] ,(T9)args[9]);
			return null;
		}
    }
    public class WrappedAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 > : Invokable {
        private readonly Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 > _action;

        public WrappedAction() {
        }

        public WrappedAction(Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 > action) {
            _action = action;
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ,T8 tVal8 ,T9 tVal9 ,T10 tVal10 ) {
            try {
                _action.Invoke(tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7 ,tVal8 ,tVal9 ,tVal10);
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }

		public override object DynamicInvoke(object[] args) {
			if( args == null ) {
				throw new ArgumentNullException("args");
			}
			if( args.Length < 11 ) {
                throw new Exception( "DynamicInvoke with too few args");
			}
			Invoke((T0)args[0] ,(T1)args[1] ,(T2)args[2] ,(T3)args[3] ,(T4)args[4] ,(T5)args[5] ,(T6)args[6] ,(T7)args[7] ,(T8)args[8] ,(T9)args[9] ,(T10)args[10]);
			return null;
		}
    }
    public class WrappedAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 > : Invokable {
        private readonly Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 > _action;

        public WrappedAction() {
        }

        public WrappedAction(Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 > action) {
            _action = action;
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ,T8 tVal8 ,T9 tVal9 ,T10 tVal10 ,T11 tVal11 ) {
            try {
                _action.Invoke(tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7 ,tVal8 ,tVal9 ,tVal10 ,tVal11);
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }

		public override object DynamicInvoke(object[] args) {
			if( args == null ) {
				throw new ArgumentNullException("args");
			}
			if( args.Length < 12 ) {
                throw new Exception( "DynamicInvoke with too few args");
			}
			Invoke((T0)args[0] ,(T1)args[1] ,(T2)args[2] ,(T3)args[3] ,(T4)args[4] ,(T5)args[5] ,(T6)args[6] ,(T7)args[7] ,(T8)args[8] ,(T9)args[9] ,(T10)args[10] ,(T11)args[11]);
			return null;
		}
    }
    public class WrappedAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 ,T12 > : Invokable {
        private readonly Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 ,T12 > _action;

        public WrappedAction() {
        }

        public WrappedAction(Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 ,T12 > action) {
            _action = action;
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ,T8 tVal8 ,T9 tVal9 ,T10 tVal10 ,T11 tVal11 ,T12 tVal12 ) {
            try {
                _action.Invoke(tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7 ,tVal8 ,tVal9 ,tVal10 ,tVal11 ,tVal12);
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }

		public override object DynamicInvoke(object[] args) {
			if( args == null ) {
				throw new ArgumentNullException("args");
			}
			if( args.Length < 13 ) {
                throw new Exception( "DynamicInvoke with too few args");
			}
			Invoke((T0)args[0] ,(T1)args[1] ,(T2)args[2] ,(T3)args[3] ,(T4)args[4] ,(T5)args[5] ,(T6)args[6] ,(T7)args[7] ,(T8)args[8] ,(T9)args[9] ,(T10)args[10] ,(T11)args[11] ,(T12)args[12]);
			return null;
		}
    }
    public class WrappedAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 ,T12 ,T13 > : Invokable {
        private readonly Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 ,T12 ,T13 > _action;

        public WrappedAction() {
        }

        public WrappedAction(Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 ,T12 ,T13 > action) {
            _action = action;
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ,T8 tVal8 ,T9 tVal9 ,T10 tVal10 ,T11 tVal11 ,T12 tVal12 ,T13 tVal13 ) {
            try {
                _action.Invoke(tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7 ,tVal8 ,tVal9 ,tVal10 ,tVal11 ,tVal12 ,tVal13);
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }

		public override object DynamicInvoke(object[] args) {
			if( args == null ) {
				throw new ArgumentNullException("args");
			}
			if( args.Length < 14 ) {
                throw new Exception( "DynamicInvoke with too few args");
			}
			Invoke((T0)args[0] ,(T1)args[1] ,(T2)args[2] ,(T3)args[3] ,(T4)args[4] ,(T5)args[5] ,(T6)args[6] ,(T7)args[7] ,(T8)args[8] ,(T9)args[9] ,(T10)args[10] ,(T11)args[11] ,(T12)args[12] ,(T13)args[13]);
			return null;
		}
    }
    public class WrappedAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 ,T12 ,T13 ,T14 > : Invokable {
        private readonly Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 ,T12 ,T13 ,T14 > _action;

        public WrappedAction() {
        }

        public WrappedAction(Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 ,T12 ,T13 ,T14 > action) {
            _action = action;
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ,T8 tVal8 ,T9 tVal9 ,T10 tVal10 ,T11 tVal11 ,T12 tVal12 ,T13 tVal13 ,T14 tVal14 ) {
            try {
                _action.Invoke(tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7 ,tVal8 ,tVal9 ,tVal10 ,tVal11 ,tVal12 ,tVal13 ,tVal14);
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }

		public override object DynamicInvoke(object[] args) {
			if( args == null ) {
				throw new ArgumentNullException("args");
			}
			if( args.Length < 15 ) {
                throw new Exception( "DynamicInvoke with too few args");
			}
			Invoke((T0)args[0] ,(T1)args[1] ,(T2)args[2] ,(T3)args[3] ,(T4)args[4] ,(T5)args[5] ,(T6)args[6] ,(T7)args[7] ,(T8)args[8] ,(T9)args[9] ,(T10)args[10] ,(T11)args[11] ,(T12)args[12] ,(T13)args[13] ,(T14)args[14]);
			return null;
		}
    }
    public class WrappedAction< T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 ,T12 ,T13 ,T14 ,T15 > : Invokable {
        private readonly Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 ,T12 ,T13 ,T14 ,T15 > _action;

        public WrappedAction() {
        }

        public WrappedAction(Action<T0 ,T1 ,T2 ,T3 ,T4 ,T5 ,T6 ,T7 ,T8 ,T9 ,T10 ,T11 ,T12 ,T13 ,T14 ,T15 > action) {
            _action = action;
        }

        public void Invoke(T0 tVal0 ,T1 tVal1 ,T2 tVal2 ,T3 tVal3 ,T4 tVal4 ,T5 tVal5 ,T6 tVal6 ,T7 tVal7 ,T8 tVal8 ,T9 tVal9 ,T10 tVal10 ,T11 tVal11 ,T12 tVal12 ,T13 tVal13 ,T14 tVal14 ,T15 tVal15 ) {
            try {
                _action.Invoke(tVal0 ,tVal1 ,tVal2 ,tVal3 ,tVal4 ,tVal5 ,tVal6 ,tVal7 ,tVal8 ,tVal9 ,tVal10 ,tVal11 ,tVal12 ,tVal13 ,tVal14 ,tVal15);
            } catch (Exception e) {
                throw new Exception( string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
        }

		public override object DynamicInvoke(object[] args) {
			if( args == null ) {
				throw new ArgumentNullException("args");
			}
			if( args.Length < 16 ) {
                throw new Exception( "DynamicInvoke with too few args");
			}
			Invoke((T0)args[0] ,(T1)args[1] ,(T2)args[2] ,(T3)args[3] ,(T4)args[4] ,(T5)args[5] ,(T6)args[6] ,(T7)args[7] ,(T8)args[8] ,(T9)args[9] ,(T10)args[10] ,(T11)args[11] ,(T12)args[12] ,(T13)args[13] ,(T14)args[14] ,(T15)args[15]);
			return null;
		}
    }

}
