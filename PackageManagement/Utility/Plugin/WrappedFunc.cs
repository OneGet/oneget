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

namespace Microsoft.PackageManagement.Utility.Plugin {
    using System;
    using System.Globalization;

    public class WrappedFunc<TRet> : Invokable {
        private readonly Func<TRet> _func;

        public WrappedFunc(Func<TRet> func) {
            _func = func;
        }

        public TRet Invoke() {
            return _func.Invoke();
        }

        public override object DynamicInvoke(object[] args) {
            return Invoke();
        }
    }

    public class WrappedFunc<T0, TRet> : Invokable {
        private readonly Func<T0, TRet> _func;

        public WrappedFunc() {
        }

        public WrappedFunc(Func<T0, TRet> func) {
            _func = func;
        }

        public TRet Invoke(T0 tVal0) {
            try {
                return _func.Invoke(tVal0);
            } catch (Exception e) {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
            // return default(TRet);
        }

        public override object DynamicInvoke(object[] args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 1) {
                throw new Exception("DynamicInvoke with too few args");
            }
            return Invoke((T0)args[0]);
        }
    }

    public class WrappedFunc<T0, T1, TRet> : Invokable {
        private readonly Func<T0, T1, TRet> _func;

        public WrappedFunc() {
        }

        public WrappedFunc(Func<T0, T1, TRet> func) {
            _func = func;
        }

        public TRet Invoke(T0 tVal0, T1 tVal1) {
            try {
                return _func.Invoke(tVal0, tVal1);
            } catch (Exception e) {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
            // return default(TRet);
        }

        public override object DynamicInvoke(object[] args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 2) {
                throw new Exception("DynamicInvoke with too few args");
            }
            return Invoke((T0)args[0], (T1)args[1]);
        }
    }

    public class WrappedFunc<T0, T1, T2, TRet> : Invokable {
        private readonly Func<T0, T1, T2, TRet> _func;

        public WrappedFunc() {
        }

        public WrappedFunc(Func<T0, T1, T2, TRet> func) {
            _func = func;
        }

        public TRet Invoke(T0 tVal0, T1 tVal1, T2 tVal2) {
            try {
                return _func.Invoke(tVal0, tVal1, tVal2);
            } catch (Exception e) {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
            // return default(TRet);
        }

        public override object DynamicInvoke(object[] args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 3) {
                throw new Exception("DynamicInvoke with too few args");
            }
            return Invoke((T0)args[0], (T1)args[1], (T2)args[2]);
        }
    }

    public class WrappedFunc<T0, T1, T2, T3, TRet> : Invokable {
        private readonly Func<T0, T1, T2, T3, TRet> _func;

        public WrappedFunc() {
        }

        public WrappedFunc(Func<T0, T1, T2, T3, TRet> func) {
            _func = func;
        }

        public TRet Invoke(T0 tVal0, T1 tVal1, T2 tVal2, T3 tVal3) {
            try {
                return _func.Invoke(tVal0, tVal1, tVal2, tVal3);
            } catch (Exception e) {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
            // return default(TRet);
        }

        public override object DynamicInvoke(object[] args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 4) {
                throw new Exception("DynamicInvoke with too few args");
            }
            return Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3]);
        }
    }

    public class WrappedFunc<T0, T1, T2, T3, T4, TRet> : Invokable {
        private readonly Func<T0, T1, T2, T3, T4, TRet> _func;

        public WrappedFunc() {
        }

        public WrappedFunc(Func<T0, T1, T2, T3, T4, TRet> func) {
            _func = func;
        }

        public TRet Invoke(T0 tVal0, T1 tVal1, T2 tVal2, T3 tVal3, T4 tVal4) {
            try {
                return _func.Invoke(tVal0, tVal1, tVal2, tVal3, tVal4);
            } catch (Exception e) {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
            // return default(TRet);
        }

        public override object DynamicInvoke(object[] args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 5) {
                throw new Exception("DynamicInvoke with too few args");
            }
            return Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4]);
        }
    }

    public class WrappedFunc<T0, T1, T2, T3, T4, T5, TRet> : Invokable {
        private readonly Func<T0, T1, T2, T3, T4, T5, TRet> _func;

        public WrappedFunc() {
        }

        public WrappedFunc(Func<T0, T1, T2, T3, T4, T5, TRet> func) {
            _func = func;
        }

        public TRet Invoke(T0 tVal0, T1 tVal1, T2 tVal2, T3 tVal3, T4 tVal4, T5 tVal5) {
            try {
                return _func.Invoke(tVal0, tVal1, tVal2, tVal3, tVal4, tVal5);
            } catch (Exception e) {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
            // return default(TRet);
        }

        public override object DynamicInvoke(object[] args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 6) {
                throw new Exception("DynamicInvoke with too few args");
            }
            return Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5]);
        }
    }

    public class WrappedFunc<T0, T1, T2, T3, T4, T5, T6, TRet> : Invokable {
        private readonly Func<T0, T1, T2, T3, T4, T5, T6, TRet> _func;

        public WrappedFunc() {
        }

        public WrappedFunc(Func<T0, T1, T2, T3, T4, T5, T6, TRet> func) {
            _func = func;
        }

        public TRet Invoke(T0 tVal0, T1 tVal1, T2 tVal2, T3 tVal3, T4 tVal4, T5 tVal5, T6 tVal6) {
            try {
                return _func.Invoke(tVal0, tVal1, tVal2, tVal3, tVal4, tVal5, tVal6);
            } catch (Exception e) {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
            // return default(TRet);
        }

        public override object DynamicInvoke(object[] args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 7) {
                throw new Exception("DynamicInvoke with too few args");
            }
            return Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6]);
        }
    }

    public class WrappedFunc<T0, T1, T2, T3, T4, T5, T6, T7, TRet> : Invokable {
        private readonly Func<T0, T1, T2, T3, T4, T5, T6, T7, TRet> _func;

        public WrappedFunc() {
        }

        public WrappedFunc(Func<T0, T1, T2, T3, T4, T5, T6, T7, TRet> func) {
            _func = func;
        }

        public TRet Invoke(T0 tVal0, T1 tVal1, T2 tVal2, T3 tVal3, T4 tVal4, T5 tVal5, T6 tVal6, T7 tVal7) {
            try {
                return _func.Invoke(tVal0, tVal1, tVal2, tVal3, tVal4, tVal5, tVal6, tVal7);
            } catch (Exception e) {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
            // return default(TRet);
        }

        public override object DynamicInvoke(object[] args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 8) {
                throw new Exception("DynamicInvoke with too few args");
            }
            return Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7]);
        }
    }

    public class WrappedFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet> : Invokable {
        private readonly Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet> _func;

        public WrappedFunc() {
        }

        public WrappedFunc(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRet> func) {
            _func = func;
        }

        public TRet Invoke(T0 tVal0, T1 tVal1, T2 tVal2, T3 tVal3, T4 tVal4, T5 tVal5, T6 tVal6, T7 tVal7, T8 tVal8) {
            try {
                return _func.Invoke(tVal0, tVal1, tVal2, tVal3, tVal4, tVal5, tVal6, tVal7, tVal8);
            } catch (Exception e) {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
            // return default(TRet);
        }

        public override object DynamicInvoke(object[] args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 9) {
                throw new Exception("DynamicInvoke with too few args");
            }
            return Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8]);
        }
    }

    public class WrappedFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet> : Invokable {
        private readonly Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet> _func;

        public WrappedFunc() {
        }

        public WrappedFunc(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet> func) {
            _func = func;
        }

        public TRet Invoke(T0 tVal0, T1 tVal1, T2 tVal2, T3 tVal3, T4 tVal4, T5 tVal5, T6 tVal6, T7 tVal7, T8 tVal8, T9 tVal9) {
            try {
                return _func.Invoke(tVal0, tVal1, tVal2, tVal3, tVal4, tVal5, tVal6, tVal7, tVal8, tVal9);
            } catch (Exception e) {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
            // return default(TRet);
        }

        public override object DynamicInvoke(object[] args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 10) {
                throw new Exception("DynamicInvoke with too few args");
            }
            return Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9]);
        }
    }

    public class WrappedFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet> : Invokable {
        private readonly Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet> _func;

        public WrappedFunc() {
        }

        public WrappedFunc(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet> func) {
            _func = func;
        }

        public TRet Invoke(T0 tVal0, T1 tVal1, T2 tVal2, T3 tVal3, T4 tVal4, T5 tVal5, T6 tVal6, T7 tVal7, T8 tVal8, T9 tVal9, T10 tVal10) {
            try {
                return _func.Invoke(tVal0, tVal1, tVal2, tVal3, tVal4, tVal5, tVal6, tVal7, tVal8, tVal9, tVal10);
            } catch (Exception e) {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
            // return default(TRet);
        }

        public override object DynamicInvoke(object[] args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 11) {
                throw new Exception("DynamicInvoke with too few args");
            }
            return Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9], (T10)args[10]);
        }
    }

    public class WrappedFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet> : Invokable {
        private readonly Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet> _func;

        public WrappedFunc() {
        }

        public WrappedFunc(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet> func) {
            _func = func;
        }

        public TRet Invoke(T0 tVal0, T1 tVal1, T2 tVal2, T3 tVal3, T4 tVal4, T5 tVal5, T6 tVal6, T7 tVal7, T8 tVal8, T9 tVal9, T10 tVal10, T11 tVal11) {
            try {
                return _func.Invoke(tVal0, tVal1, tVal2, tVal3, tVal4, tVal5, tVal6, tVal7, tVal8, tVal9, tVal10, tVal11);
            } catch (Exception e) {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
            // return default(TRet);
        }

        public override object DynamicInvoke(object[] args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 12) {
                throw new Exception("DynamicInvoke with too few args");
            }
            return Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9], (T10)args[10], (T11)args[11]);
        }
    }

    public class WrappedFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet> : Invokable {
        private readonly Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet> _func;

        public WrappedFunc() {
        }

        public WrappedFunc(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet> func) {
            _func = func;
        }

        public TRet Invoke(T0 tVal0, T1 tVal1, T2 tVal2, T3 tVal3, T4 tVal4, T5 tVal5, T6 tVal6, T7 tVal7, T8 tVal8, T9 tVal9, T10 tVal10, T11 tVal11, T12 tVal12) {
            try {
                return _func.Invoke(tVal0, tVal1, tVal2, tVal3, tVal4, tVal5, tVal6, tVal7, tVal8, tVal9, tVal10, tVal11, tVal12);
            } catch (Exception e) {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
            // return default(TRet);
        }

        public override object DynamicInvoke(object[] args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 13) {
                throw new Exception("DynamicInvoke with too few args");
            }
            return Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9], (T10)args[10], (T11)args[11], (T12)args[12]);
        }
    }

    public class WrappedFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet> : Invokable {
        private readonly Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet> _func;

        public WrappedFunc() {
        }

        public WrappedFunc(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet> func) {
            _func = func;
        }

        public TRet Invoke(T0 tVal0, T1 tVal1, T2 tVal2, T3 tVal3, T4 tVal4, T5 tVal5, T6 tVal6, T7 tVal7, T8 tVal8, T9 tVal9, T10 tVal10, T11 tVal11, T12 tVal12, T13 tVal13) {
            try {
                return _func.Invoke(tVal0, tVal1, tVal2, tVal3, tVal4, tVal5, tVal6, tVal7, tVal8, tVal9, tVal10, tVal11, tVal12, tVal13);
            } catch (Exception e) {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
            // return default(TRet);
        }

        public override object DynamicInvoke(object[] args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 14) {
                throw new Exception("DynamicInvoke with too few args");
            }
            return Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9], (T10)args[10], (T11)args[11], (T12)args[12], (T13)args[13]);
        }
    }

    public class WrappedFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet> : Invokable {
        private readonly Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet> _func;

        public WrappedFunc() {
        }

        public WrappedFunc(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet> func) {
            _func = func;
        }

        public TRet Invoke(T0 tVal0, T1 tVal1, T2 tVal2, T3 tVal3, T4 tVal4, T5 tVal5, T6 tVal6, T7 tVal7, T8 tVal8, T9 tVal9, T10 tVal10, T11 tVal11, T12 tVal12, T13 tVal13, T14 tVal14) {
            try {
                return _func.Invoke(tVal0, tVal1, tVal2, tVal3, tVal4, tVal5, tVal6, tVal7, tVal8, tVal9, tVal10, tVal11, tVal12, tVal13, tVal14);
            } catch (Exception e) {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
            // return default(TRet);
        }

        public override object DynamicInvoke(object[] args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 15) {
                throw new Exception("DynamicInvoke with too few args");
            }
            return Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9], (T10)args[10], (T11)args[11], (T12)args[12], (T13)args[13], (T14)args[14]);
        }
    }

    public class WrappedFunc<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet> : Invokable {
        private readonly Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet> _func;

        public WrappedFunc() {
        }

        public WrappedFunc(Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet> func) {
            _func = func;
        }

        public TRet Invoke(T0 tVal0, T1 tVal1, T2 tVal2, T3 tVal3, T4 tVal4, T5 tVal5, T6 tVal6, T7 tVal7, T8 tVal8, T9 tVal9, T10 tVal10, T11 tVal11, T12 tVal12, T13 tVal13, T14 tVal14, T15 tVal15) {
            try {
                return _func.Invoke(tVal0, tVal1, tVal2, tVal3, tVal4, tVal5, tVal6, tVal7, tVal8, tVal9, tVal10, tVal11, tVal12, tVal13, tVal14, tVal15);
            } catch (Exception e) {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "{0}/{1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
            }
            // return default(TRet);
        }

        public override object DynamicInvoke(object[] args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }
            if (args.Length < 16) {
                throw new Exception("DynamicInvoke with too few args");
            }
            return Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4], (T5)args[5], (T6)args[6], (T7)args[7], (T8)args[8], (T9)args[9], (T10)args[10], (T11)args[11], (T12)args[12], (T13)args[13], (T14)args[14], (T15)args[15]);
        }
    }
}