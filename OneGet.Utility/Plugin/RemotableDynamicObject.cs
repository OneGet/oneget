namespace Microsoft.OneGet.Plugin {
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides a base class for specifying dynamic behavior at run time. This class must be inherited from; you cannot instantiate it directly.
    /// </summary>
    
    public class RemoteableDynamicObject : MarshalByRefObject, IDynamicMetaObjectProvider {
        /// <summary>
        /// Enables derived types to initialize a new instance of the <see cref="T:System.Dynamic.DynamicObject"/> type.
        /// </summary>

        protected RemoteableDynamicObject() {
        }

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// 
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)
        /// </returns>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param><param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result"/>.</param>
        
        public virtual bool TryGetMember(GetMemberBinder binder, out object result) {
            result = (object)null;
            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that set member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as setting a value for a property.
        /// </summary>
        /// 
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member to which the value is being assigned. For example, for the statement sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param><param name="value">The value to set to the member. For example, for sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, the <paramref name="value"/> is "Test".</param>
        
        public virtual bool TrySetMember(SetMemberBinder binder, object value) {
            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that delete an object member. This method is not intended for use in C# or Visual Basic.
        /// </summary>
        /// 
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        /// <param name="binder">Provides information about the deletion.</param>
        
        public virtual bool TryDeleteMember(DeleteMemberBinder binder) {
            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that invoke a member. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as calling a method.
        /// </summary>
        /// 
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        /// <param name="binder">Provides information about the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the statement sampleObject.SampleMethod(100), where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleMethod". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param><param name="args">The arguments that are passed to the object member during the invoke operation. For example, for the statement sampleObject.SampleMethod(100), where sampleObject is derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, <paramref name="args[0]"/> is equal to 100.</param><param name="result">The result of the member invocation.</param>
        
        public virtual bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            result = (object)null;
            return false;
        }

        /// <summary>
        /// Provides implementation for type conversion operations. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations that convert an object from one type to another.
        /// </summary>
        /// 
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        /// <param name="binder">Provides information about the conversion operation. The binder.Type property provides the type to which the object must be converted. For example, for the statement (String)sampleObject in C# (CType(sampleObject, Type) in Visual Basic), where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Type returns the <see cref="T:System.String"/> type. The binder.Explicit property provides information about the kind of conversion that occurs. It returns true for explicit conversion and false for implicit conversion.</param><param name="result">The result of the type conversion operation.</param>
        
        public virtual bool TryConvert(ConvertBinder binder, out object result) {
            result = (object)null;
            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that initialize a new instance of a dynamic object. This method is not intended for use in C# or Visual Basic.
        /// </summary>
        /// 
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        /// <param name="binder">Provides information about the initialization operation.</param><param name="args">The arguments that are passed to the object during initialization. For example, for the new SampleType(100) operation, where SampleType is the type derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, <paramref name="args[0]"/> is equal to 100.</param><param name="result">The result of the initialization.</param>
        
        public virtual bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result) {
            result = (object)null;
            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that invoke an object. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as invoking an object or a delegate.
        /// </summary>
        /// 
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.
        /// </returns>
        /// <param name="binder">Provides information about the invoke operation.</param><param name="args">The arguments that are passed to the object during the invoke operation. For example, for the sampleObject(100) operation, where sampleObject is derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, <paramref name="args[0]"/> is equal to 100.</param><param name="result">The result of the object invocation.</param>
        
        public virtual bool TryInvoke(InvokeBinder binder, object[] args, out object result) {
            result = (object)null;
            return false;
        }

        /// <summary>
        /// Provides implementation for binary operations. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as addition and multiplication.
        /// </summary>
        /// 
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        /// <param name="binder">Provides information about the binary operation. The binder.Operation property returns an <see cref="T:System.Linq.Expressions.ExpressionType"/> object. For example, for the sum = first + second statement, where first and second are derived from the DynamicObject class, binder.Operation returns ExpressionType.Add.</param><param name="arg">The right operand for the binary operation. For example, for the sum = first + second statement, where first and second are derived from the DynamicObject class, <paramref name="arg"/> is equal to second.</param><param name="result">The result of the binary operation.</param>
        
        public virtual bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result) {
            result = (object)null;
            return false;
        }

        /// <summary>
        /// Provides implementation for unary operations. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as negation, increment, or decrement.
        /// </summary>
        /// 
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        /// <param name="binder">Provides information about the unary operation. The binder.Operation property returns an <see cref="T:System.Linq.Expressions.ExpressionType"/> object. For example, for the negativeNumber = -number statement, where number is derived from the DynamicObject class, binder.Operation returns "Negate".</param><param name="result">The result of the unary operation.</param>
        
        public virtual bool TryUnaryOperation(UnaryOperationBinder binder, out object result) {
            result = (object)null;
            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that get a value by index. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for indexing operations.
        /// </summary>
        /// 
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)
        /// </returns>
        /// <param name="binder">Provides information about the operation. </param><param name="indexes">The indexes that are used in the operation. For example, for the sampleObject[3] operation in C# (sampleObject(3) in Visual Basic), where sampleObject is derived from the DynamicObject class, <paramref name="indexes[0]"/> is equal to 3.</param><param name="result">The result of the index operation.</param>
        
        public virtual bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
            result = (object)null;
            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that set a value by index. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations that access objects by a specified index.
        /// </summary>
        /// 
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.
        /// </returns>
        /// <param name="binder">Provides information about the operation. </param><param name="indexes">The indexes that are used in the operation. For example, for the sampleObject[3] = 10 operation in C# (sampleObject(3) = 10 in Visual Basic), where sampleObject is derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, <paramref name="indexes[0]"/> is equal to 3.</param><param name="value">The value to set to the object that has the specified index. For example, for the sampleObject[3] = 10 operation in C# (sampleObject(3) = 10 in Visual Basic), where sampleObject is derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, <paramref name="value"/> is equal to 10.</param>
        
        public virtual bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that delete an object by index. This method is not intended for use in C# or Visual Basic.
        /// </summary>
        /// 
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        /// <param name="binder">Provides information about the deletion.</param><param name="indexes">The indexes to be deleted.</param>
        
        public virtual bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes) {
            return false;
        }

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// 
        /// <returns>
        /// A sequence that contains dynamic member names.
        /// </returns>
        
        public virtual IEnumerable<string> GetDynamicMemberNames() {
            return (IEnumerable<string>)new string[0];
        }

        /// <summary>
        /// Provides a <see cref="T:System.Dynamic.DynamicMetaObject"/> that dispatches to the dynamic virtual methods. The object can be encapsulated inside another <see cref="T:System.Dynamic.DynamicMetaObject"/> to provide custom behavior for individual actions. This method supports the Dynamic Language Runtime infrastructure for language implementers and it is not intended to be used directly from your code.
        /// </summary>
        /// 
        /// <returns>
        /// An object of the <see cref="T:System.Dynamic.DynamicMetaObject"/> type.
        /// </returns>
        /// <param name="parameter">The expression that represents <see cref="T:System.Dynamic.DynamicMetaObject"/> to dispatch to the dynamic virtual methods.</param>

        public virtual DynamicMetaObject GetMetaObject(Expression parameter) {
            return (DynamicMetaObject)new RemoteableDynamicObject.MetaDynamic(parameter, this);
        }

        private sealed class MetaDynamic : DynamicMetaObject {
            private static readonly Expression[] NoArgs = new Expression[0];

            private DynamicObject Value {
                get {
                    return (DynamicObject)base.Value;
                }
            }

            static MetaDynamic() {
            }

            internal MetaDynamic(Expression expression, RemoteableDynamicObject value)
                : base(expression, BindingRestrictions.Empty, (object)value) {
            }

            public override IEnumerable<string> GetDynamicMemberNames() {
                return this.Value.GetDynamicMemberNames();
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
                if (this.IsOverridden("TryGetMember"))
                    return this.CallMethodWithResult("TryGetMember", (DynamicMetaObjectBinder)binder, RemoteableDynamicObject.MetaDynamic.NoArgs, (RemoteableDynamicObject.MetaDynamic.Fallback)(e => binder.FallbackGetMember((DynamicMetaObject)this, e)));
                else
                    return base.BindGetMember(binder);
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
                if (this.IsOverridden("TrySetMember"))
                    return this.CallMethodReturnLast("TrySetMember", (DynamicMetaObjectBinder)binder, RemoteableDynamicObject.MetaDynamic.NoArgs, value.Expression, (RemoteableDynamicObject.MetaDynamic.Fallback)(e => binder.FallbackSetMember((DynamicMetaObject)this, value, e)));
                else
                    return base.BindSetMember(binder, value);
            }

            public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) {
                if (this.IsOverridden("TryDeleteMember"))
                    return this.CallMethodNoResult("TryDeleteMember", (DynamicMetaObjectBinder)binder, RemoteableDynamicObject.MetaDynamic.NoArgs, (RemoteableDynamicObject.MetaDynamic.Fallback)(e => binder.FallbackDeleteMember((DynamicMetaObject)this, e)));
                else
                    return base.BindDeleteMember(binder);
            }

            public override DynamicMetaObject BindConvert(ConvertBinder binder) {
                if (this.IsOverridden("TryConvert"))
                    return this.CallMethodWithResult("TryConvert", (DynamicMetaObjectBinder)binder, RemoteableDynamicObject.MetaDynamic.NoArgs, (RemoteableDynamicObject.MetaDynamic.Fallback)(e => binder.FallbackConvert((DynamicMetaObject)this, e)));
                else
                    return base.BindConvert(binder);
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
                RemoteableDynamicObject.MetaDynamic.Fallback fallback = (RemoteableDynamicObject.MetaDynamic.Fallback)(e => binder.FallbackInvokeMember((DynamicMetaObject)this, args, e));
                DynamicMetaObject errorSuggestion = this.BuildCallMethodWithResult("TryInvokeMember", (DynamicMetaObjectBinder)binder, dmo.GetExpressions(args), this.BuildCallMethodWithResult("TryGetMember", (DynamicMetaObjectBinder)new RemoteableDynamicObject.MetaDynamic.GetBinderAdapter(binder), RemoteableDynamicObject.MetaDynamic.NoArgs, fallback((DynamicMetaObject)null), (RemoteableDynamicObject.MetaDynamic.Fallback)(e => binder.FallbackInvoke(e, args, (DynamicMetaObject)null))), (RemoteableDynamicObject.MetaDynamic.Fallback)null);
                return fallback(errorSuggestion);
            }

            public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args) {
                if (this.IsOverridden("TryCreateInstance"))
                    return this.CallMethodWithResult("TryCreateInstance", (DynamicMetaObjectBinder)binder, dmo.GetExpressions(args), (RemoteableDynamicObject.MetaDynamic.Fallback)(e => binder.FallbackCreateInstance((DynamicMetaObject)this, args, e)));
                else
                    return base.BindCreateInstance(binder, args);
            }

            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
                if (this.IsOverridden("TryInvoke"))
                    return this.CallMethodWithResult("TryInvoke", (DynamicMetaObjectBinder)binder, dmo.GetExpressions(args), (RemoteableDynamicObject.MetaDynamic.Fallback)(e => binder.FallbackInvoke((DynamicMetaObject)this, args, e)));
                else
                    return base.BindInvoke(binder, args);
            }

            public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg) {
                if (!this.IsOverridden("TryBinaryOperation"))
                    return base.BindBinaryOperation(binder, arg);
                return this.CallMethodWithResult("TryBinaryOperation", (DynamicMetaObjectBinder)binder, dmo.GetExpressions(new DynamicMetaObject[1]
        {
          arg
        }), (RemoteableDynamicObject.MetaDynamic.Fallback)(e => binder.FallbackBinaryOperation((DynamicMetaObject)this, arg, e)));
            }

            public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder) {
                if (this.IsOverridden("TryUnaryOperation"))
                    return this.CallMethodWithResult("TryUnaryOperation", (DynamicMetaObjectBinder)binder, RemoteableDynamicObject.MetaDynamic.NoArgs, (RemoteableDynamicObject.MetaDynamic.Fallback)(e => binder.FallbackUnaryOperation((DynamicMetaObject)this, e)));
                else
                    return base.BindUnaryOperation(binder);
            }

            public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) {
                if (this.IsOverridden("TryGetIndex"))
                    return this.CallMethodWithResult("TryGetIndex", (DynamicMetaObjectBinder)binder, dmo.GetExpressions(indexes), (RemoteableDynamicObject.MetaDynamic.Fallback)(e => binder.FallbackGetIndex((DynamicMetaObject)this, indexes, e)));
                else
                    return base.BindGetIndex(binder, indexes);
            }

            public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) {
                if (this.IsOverridden("TrySetIndex"))
                    return this.CallMethodReturnLast("TrySetIndex", (DynamicMetaObjectBinder)binder, dmo.GetExpressions(indexes), value.Expression, (RemoteableDynamicObject.MetaDynamic.Fallback)(e => binder.FallbackSetIndex((DynamicMetaObject)this, indexes, value, e)));
                else
                    return base.BindSetIndex(binder, indexes, value);
            }

            public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes) {
                if (this.IsOverridden("TryDeleteIndex"))
                    return this.CallMethodNoResult("TryDeleteIndex", (DynamicMetaObjectBinder)binder, dmo.GetExpressions(indexes), (RemoteableDynamicObject.MetaDynamic.Fallback)(e => binder.FallbackDeleteIndex((DynamicMetaObject)this, indexes, e)));
                else
                    return base.BindDeleteIndex(binder, indexes);
            }

            private static Expression[] GetConvertedArgs(params Expression[] args) {
                ReadOnlyCollectionBuilder<Expression> collectionBuilder = new ReadOnlyCollectionBuilder<Expression>(args.Length);
                for (int index = 0; index < args.Length; ++index)
                    collectionBuilder.Add((Expression)Expression.Convert(args[index], typeof(object)));
                return collectionBuilder.ToArray();
            }

            private static Expression ReferenceArgAssign(Expression callArgs, Expression[] args) {
                ReadOnlyCollectionBuilder<Expression> collectionBuilder = (ReadOnlyCollectionBuilder<Expression>)null;
                for (int index = 0; index < args.Length; ++index) {
                    ContractUtils.Requires(args[index] is ParameterExpression);
                    if (((ParameterExpression)args[index]).IsByRef) {
                        if (collectionBuilder == null)
                            collectionBuilder = new ReadOnlyCollectionBuilder<Expression>();
                        collectionBuilder.Add((Expression)Expression.Assign(args[index], (Expression)Expression.Convert((Expression)Expression.ArrayIndex(callArgs, (Expression)Expression.Constant((object)index)), args[index].Type)));
                    }
                }
                if (collectionBuilder != null)
                    return (Expression)Expression.Block((IEnumerable<Expression>)collectionBuilder);
                else
                    return (Expression)Expression.Empty();
            }

            private static Expression[] BuildCallArgs(DynamicMetaObjectBinder binder, Expression[] parameters, Expression arg0, Expression arg1) {
                if (!object.ReferenceEquals((object)parameters, (object)RemoteableDynamicObject.MetaDynamic.NoArgs)) {
                    if (arg1 == null)
                        return new Expression[2]
            {
              (Expression) RemoteableDynamicObject.MetaDynamic.Constant(binder),
              arg0
            };
                    else
                        return new Expression[3]
            {
              (Expression) RemoteableDynamicObject.MetaDynamic.Constant(binder),
              arg0,
              arg1
            };
                }
                else if (arg1 == null)
                    return new Expression[1]
          {
            (Expression) RemoteableDynamicObject.MetaDynamic.Constant(binder)
          };
                else
                    return new Expression[2]
          {
            (Expression) RemoteableDynamicObject.MetaDynamic.Constant(binder),
            arg1
          };
            }

            private static ConstantExpression Constant(DynamicMetaObjectBinder binder) {
                Type type = binder.GetType();
                while (!type.IsVisible)
                    type = type.BaseType;
                return Expression.Constant((object)binder, type);
            }

            private DynamicMetaObject CallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, RemoteableDynamicObject.MetaDynamic.Fallback fallback) {
                return this.CallMethodWithResult(methodName, binder, args, fallback, (RemoteableDynamicObject.MetaDynamic.Fallback)null);
            }

            private DynamicMetaObject CallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, RemoteableDynamicObject.MetaDynamic.Fallback fallback, RemoteableDynamicObject.MetaDynamic.Fallback fallbackInvoke) {
                DynamicMetaObject fallbackResult = fallback((DynamicMetaObject)null);
                DynamicMetaObject errorSuggestion = this.BuildCallMethodWithResult(methodName, binder, args, fallbackResult, fallbackInvoke);
                return fallback(errorSuggestion);
            }

            private DynamicMetaObject BuildCallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, DynamicMetaObject fallbackResult, RemoteableDynamicObject.MetaDynamic.Fallback fallbackInvoke) {
                if (!this.IsOverridden(methodName))
                    return fallbackResult;
                ParameterExpression parameterExpression1 = Expression.Parameter(typeof(object), (string)null);
                ParameterExpression parameterExpression2 = methodName != "TryBinaryOperation" ? Expression.Parameter(typeof(object[]), (string)null) : Expression.Parameter(typeof(object), (string)null);
                Expression[] convertedArgs = RemoteableDynamicObject.MetaDynamic.GetConvertedArgs(args);
                DynamicMetaObject errorSuggestion = new DynamicMetaObject((Expression)parameterExpression1, BindingRestrictions.Empty);
                if (binder.ReturnType != typeof(object)) {
                    UnaryExpression unaryExpression = Expression.Convert(errorSuggestion.Expression, binder.ReturnType);
                    // string str = Strings.DynamicObjectResultNotAssignable((object)"{0}", (object)this.Value.GetType(), (object)binder.GetType(), (object)binder.ReturnType);
                    String str = "DynamicObjectResultNotAssignable";
                    errorSuggestion = new DynamicMetaObject((Expression)Expression.Condition(!binder.ReturnType.IsValueType || !(Nullable.GetUnderlyingType(binder.ReturnType) == (Type)null) ? (Expression)Expression.OrElse((Expression)Expression.Equal(errorSuggestion.Expression, (Expression)Expression.Constant((object)null)), (Expression)Expression.TypeIs(errorSuggestion.Expression, binder.ReturnType)) : (Expression)Expression.TypeIs(errorSuggestion.Expression, binder.ReturnType), (Expression)unaryExpression, (Expression)Expression.Throw((Expression)Expression.New(typeof(InvalidCastException).GetConstructor(new Type[1]
          {
            typeof (string)
          }), new Expression[1]
          {
            (Expression) Expression.Call(typeof (string).GetMethod("Format", new Type[2]
            {
              typeof (string),
              typeof (object[])
            }), (Expression) Expression.Constant((object) str), (Expression) Expression.NewArrayInit(typeof (object), new Expression[1]
            {
              (Expression) Expression.Condition((Expression) Expression.Equal(errorSuggestion.Expression, (Expression) Expression.Constant((object) null)), (Expression) Expression.Constant((object) "null"), (Expression) Expression.Call(errorSuggestion.Expression, typeof (object).GetMethod("GetType")), typeof (object))
            }))
          }), binder.ReturnType), binder.ReturnType), errorSuggestion.Restrictions);
                }
                if (fallbackInvoke != null)
                    errorSuggestion = fallbackInvoke(errorSuggestion);
                return new DynamicMetaObject((Expression)Expression.Block((IEnumerable<ParameterExpression>)new ParameterExpression[2]
        {
          parameterExpression1,
          parameterExpression2
        }, new Expression[2]
        {
          methodName != "TryBinaryOperation" ? (Expression) Expression.Assign((Expression) parameterExpression2, (Expression) Expression.NewArrayInit(typeof (object), convertedArgs)) : (Expression) Expression.Assign((Expression) parameterExpression2, convertedArgs[0]),
          (Expression) Expression.Condition((Expression) Expression.Call(this.GetLimitedSelf(), typeof (DynamicObject).GetMethod(methodName), RemoteableDynamicObject.MetaDynamic.BuildCallArgs(binder, args, (Expression) parameterExpression2, (Expression) parameterExpression1)), (Expression) Expression.Block(methodName != "TryBinaryOperation" ? RemoteableDynamicObject.MetaDynamic.ReferenceArgAssign((Expression) parameterExpression2, args) : (Expression) Expression.Empty(), errorSuggestion.Expression), fallbackResult.Expression, binder.ReturnType)
        }), this.GetRestrictions().Merge(errorSuggestion.Restrictions).Merge(fallbackResult.Restrictions));
            }

            private DynamicMetaObject CallMethodReturnLast(string methodName, DynamicMetaObjectBinder binder, Expression[] args, Expression value, RemoteableDynamicObject.MetaDynamic.Fallback fallback) {
                DynamicMetaObject dynamicMetaObject = fallback((DynamicMetaObject)null);
                ParameterExpression parameterExpression1 = Expression.Parameter(typeof(object), (string)null);
                ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object[]), (string)null);
                Expression[] convertedArgs = RemoteableDynamicObject.MetaDynamic.GetConvertedArgs(args);
                DynamicMetaObject errorSuggestion = new DynamicMetaObject((Expression)Expression.Block((IEnumerable<ParameterExpression>)new ParameterExpression[2]
        {
          parameterExpression1,
          parameterExpression2
        }, new Expression[2]
        {
          (Expression) Expression.Assign((Expression) parameterExpression2, (Expression) Expression.NewArrayInit(typeof (object), convertedArgs)),
          (Expression) Expression.Condition((Expression) Expression.Call(this.GetLimitedSelf(), typeof (DynamicObject).GetMethod(methodName), RemoteableDynamicObject.MetaDynamic.BuildCallArgs(binder, args, (Expression) parameterExpression2, (Expression) Expression.Assign((Expression) parameterExpression1, (Expression) Expression.Convert(value, typeof (object))))), (Expression) Expression.Block(RemoteableDynamicObject.MetaDynamic.ReferenceArgAssign((Expression) parameterExpression2, args), (Expression) parameterExpression1), dynamicMetaObject.Expression, typeof (object))
        }), this.GetRestrictions().Merge(dynamicMetaObject.Restrictions));
                return fallback(errorSuggestion);
            }

            private DynamicMetaObject CallMethodNoResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, RemoteableDynamicObject.MetaDynamic.Fallback fallback) {
                DynamicMetaObject dynamicMetaObject = fallback((DynamicMetaObject)null);
                ParameterExpression parameterExpression = Expression.Parameter(typeof(object[]), (string)null);
                Expression[] convertedArgs = RemoteableDynamicObject.MetaDynamic.GetConvertedArgs(args);
                DynamicMetaObject errorSuggestion = new DynamicMetaObject((Expression)Expression.Block((IEnumerable<ParameterExpression>)new ParameterExpression[1]
        {
          parameterExpression
        }, new Expression[2]
        {
          (Expression) Expression.Assign((Expression) parameterExpression, (Expression) Expression.NewArrayInit(typeof (object), convertedArgs)),
          (Expression) Expression.Condition((Expression) Expression.Call(this.GetLimitedSelf(), typeof (DynamicObject).GetMethod(methodName), RemoteableDynamicObject.MetaDynamic.BuildCallArgs(binder, args, (Expression) parameterExpression, (Expression) null)), (Expression) Expression.Block(RemoteableDynamicObject.MetaDynamic.ReferenceArgAssign((Expression) parameterExpression, args), (Expression) Expression.Empty()), dynamicMetaObject.Expression, typeof (void))
        }), this.GetRestrictions().Merge(dynamicMetaObject.Restrictions));
                return fallback(errorSuggestion);
            }

            private bool IsOverridden(string method) {
                foreach (MethodInfo methodInfo in this.Value.GetType().GetMember(method, MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public)) {
                    if (methodInfo.DeclaringType != typeof(DynamicObject) && methodInfo.GetBaseDefinition().DeclaringType == typeof(DynamicObject))
                        return true;
                }
                return false;
            }

            internal static BindingRestrictions GetTypeRestriction(DynamicMetaObject obj) {
                if (obj.Value == null && obj.HasValue)
                    return BindingRestrictions.GetInstanceRestriction(obj.Expression, (object)null);
                else
                    return BindingRestrictions.GetTypeRestriction(obj.Expression, obj.LimitType);
            }


            private BindingRestrictions GetRestrictions() {
                return GetTypeRestriction((DynamicMetaObject)this);
            }

            private Expression GetLimitedSelf() {
                if (ContractUtils.AreEquivalent(this.Expression.Type, typeof(DynamicObject)))
                    return this.Expression;
                else
                    return (Expression)Expression.Convert(this.Expression, typeof(DynamicObject));
            }

            private delegate DynamicMetaObject Fallback(DynamicMetaObject errorSuggestion);

            private sealed class GetBinderAdapter : GetMemberBinder {
                internal GetBinderAdapter(InvokeMemberBinder binder)
                    : base(binder.Name, binder.IgnoreCase) {
                }

                public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
                    throw new NotSupportedException();
                }
            }
        }

        public static class dmo {
            internal static Expression[] GetExpressions(DynamicMetaObject[] objects) {
                ContractUtils.RequiresNotNull((object)objects, "objects");
                Expression[] expressionArray = new Expression[objects.Length];
                for (int index = 0; index < objects.Length; ++index) {
                    DynamicMetaObject dynamicMetaObject = objects[index];
                    ContractUtils.RequiresNotNull((object)dynamicMetaObject, "objects");
                    Expression expression = dynamicMetaObject.Expression;
                    ContractUtils.RequiresNotNull((object)expression, "objects");
                    expressionArray[index] = expression;
                }
                return expressionArray;
            }
        }

        internal static class ContractUtils {
            internal static Exception Unreachable {
                get {
                    return (Exception)new InvalidOperationException("Code supposed to be unreachable");
                }
            }

            internal static void Requires(bool precondition, string paramName) {
                if (!precondition)
                    throw new ArgumentException("InvalidArgumentValue", paramName);
            }

            internal static void RequiresNotNull(object value, string paramName) {
                if (value == null)
                    throw new ArgumentNullException(paramName);
            }

            internal static bool AreEquivalent(Type t1, Type t2) {
                if (!(t1 == t2))
                    return t1.IsEquivalentTo(t2);
                else
                    return true;
            }
            internal static void RequiresNotEmpty<T>(ICollection<T> collection, string paramName) {
                ContractUtils.RequiresNotNull((object)collection, paramName);
                if (collection.Count == 0)
                    throw new ArgumentException("NonEmptyCollectionRequired", paramName);
            }

            internal static void RequiresArrayRange<T>(IList<T> array, int offset, int count, string offsetName, string countName) {
                if (count < 0)
                    throw new ArgumentOutOfRangeException(countName);
                if (offset < 0 || array.Count - offset < count)
                    throw new ArgumentOutOfRangeException(offsetName);
            }

            internal static void RequiresNotNullItems<T>(IList<T> array, string arrayName) {
                ContractUtils.RequiresNotNull((object)array, arrayName);
                for (int index = 0; index < array.Count; ++index) {
                    if ((object)array[index] == null)
                        throw new ArgumentNullException("array/index");
                }
            }

            internal static void Requires(bool precondition) {
                if (!precondition)
                    throw new ArgumentException("MethodPreconditionViolated");
            }
        }
    }
}
