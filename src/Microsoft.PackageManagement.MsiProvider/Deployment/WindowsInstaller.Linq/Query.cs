//---------------------------------------------------------------------
// <copyright file="Query.cs" company="Microsoft Corporation">
//   Copyright (c) 1999, Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
// Part of the Deployment Tools Foundation project.
// </summary>
//---------------------------------------------------------------------

namespace Microsoft.PackageManagement.Msi.Internal.Deployment.WindowsInstaller.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Implements the LINQ to MSI query functionality.
    /// </summary>
    /// <typeparam name="T">the result type of the current query --
    /// either some kind of QRecord, or some projection of record data</typeparam>
    internal sealed class Query<T> : IOrderedQueryable<T>, IQueryProvider
    {
        private QDatabase db;
        private readonly Expression queryableExpression;
        private List<TableInfo> tables;
        private List<Type> recordTypes;
        private List<string> selectors;
        private string where;
        private List<object> whereParameters;
        private List<TableColumn> orderbyColumns;
        private List<TableColumn> selectColumns;
        private List<TableColumn> joinColumns;
        private List<Delegate> projectionDelegates;

        internal Query(QDatabase db, Expression expression)
        {
            this.db = db;
            queryableExpression = expression ?? throw new ArgumentNullException("expression");
            tables = new List<TableInfo>();
            recordTypes = new List<Type>();
            selectors = new List<string>();
            whereParameters = new List<object>();
            orderbyColumns = new List<TableColumn>();
            selectColumns = new List<TableColumn>();
            joinColumns = new List<TableColumn>();
            projectionDelegates = new List<Delegate>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (selectColumns.Count == 0)
            {
                AddAllColumns(tables[0], selectColumns);
            }

            string query = CompileQuery();
            return InvokeQuery(query);
        }

        private string CompileQuery()
        {
            bool explicitTables = tables.Count > 1;

            StringBuilder queryBuilder = new StringBuilder("SELECT");

            for (int i = 0; i < selectColumns.Count; i++)
            {
                queryBuilder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    (explicitTables ? "{0} `{1}`.`{2}`" : "{0} `{2}`"),
                    (i > 0 ? "," : string.Empty),
                    selectColumns[i].Table.Name,
                    selectColumns[i].Column.Name);
            }

            for (int i = 0; i < tables.Count; i++)
            {
                queryBuilder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "{0} `{1}`",
                    (i == 0 ? " FROM" : ","),
                    tables[i].Name);
            }

            bool startedWhere = false;
            for (int i = 0; i < joinColumns.Count - 1; i += 2)
            {
                queryBuilder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "{0} `{1}`.`{2}` = `{3}`.`{4}` ",
                    (i == 0 ? " WHERE" : "AND"),
                    joinColumns[i].Table,
                    joinColumns[i].Column,
                    joinColumns[i + 1].Table,
                    joinColumns[i + 1].Column);
                startedWhere = true;
            }

            if (where != null)
            {
                queryBuilder.Append(startedWhere ? "AND " : " WHERE");
                queryBuilder.Append(where);
            }

            for (int i = 0; i < orderbyColumns.Count; i++)
            {
                VerifyOrderByColumn(orderbyColumns[i]);

                queryBuilder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    (explicitTables ? "{0} `{1}`.`{2}`" : "{0} `{2}`"),
                    (i == 0 ? " ORDER BY" : ","),
                    orderbyColumns[i].Table.Name,
                    orderbyColumns[i].Column.Name);
            }

            return queryBuilder.ToString();
        }

        private static void VerifyOrderByColumn(TableColumn tableColumn)
        {
            if (tableColumn.Column.Type != typeof(int) &&
                tableColumn.Column.Type != typeof(short))
            {
                throw new NotSupportedException(
                    "Cannot orderby column: " + tableColumn.Column.Name +
                    "; orderby is only supported on integer fields");
            }
        }

        private IEnumerator<T> InvokeQuery(string query)
        {
            TextWriter log = db.Log;
            if (log != null)
            {
                log.WriteLine();
                log.WriteLine(query);
            }

            using (View queryView = db.OpenView(query))
            {
                if (whereParameters != null && whereParameters.Count > 0)
                {
                    using (Record paramsRec = db.CreateRecord(whereParameters.Count))
                    {
                        for (int i = 0; i < whereParameters.Count; i++)
                        {
                            paramsRec[i + 1] = whereParameters[i];

                            if (log != null)
                            {
                                log.WriteLine("    ? = " + whereParameters[i]);
                            }
                        }

                        queryView.Execute(paramsRec);
                    }
                }
                else
                {
                    queryView.Execute();
                }

                foreach (Record resultRec in queryView)
                {
                    using (resultRec)
                    {
                        yield return GetResult(resultRec);
                    }
                }
            }
        }

        private T GetResult(Record resultRec)
        {
            object[] results = new object[tables.Count];

            for (int i = 0; i < tables.Count; i++)
            {
                string[] values = new string[tables[i].Columns.Count];
                for (int j = 0; j < selectColumns.Count; j++)
                {
                    TableColumn col = selectColumns[j];
                    if (col.Table.Name == tables[i].Name)
                    {
                        int index = tables[i].Columns.IndexOf(
                            col.Column.Name);
                        if (index >= 0)
                        {
                            if (col.Column.Type == typeof(Stream))
                            {
                                values[index] = "[Binary Data]";
                            }
                            else
                            {
                                values[index] = resultRec.GetString(j + 1);
                            }
                        }
                    }
                }

                QRecord result = (QRecord)recordTypes[i]
                    .GetConstructor(Type.EmptyTypes).Invoke(null);
                result.Database = db;
                result.TableInfo = tables[i];
                result.Values = values;
                result.Exists = true;
                results[i] = result;
            }

            if (projectionDelegates.Count > 0)
            {
                object resultsProjection = results[0];
                for (int i = 1; i <= results.Length; i++)
                {
                    if (i < results.Length)
                    {
                        resultsProjection = projectionDelegates[i - 1]
                            .DynamicInvoke(new object[] { resultsProjection, results[i] });
                    }
                    else
                    {
                        resultsProjection = projectionDelegates[i - 1]
                            .DynamicInvoke(resultsProjection);
                    }
                }

                return (T)resultsProjection;
            }
            else
            {
                return (T)results[0];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this).GetEnumerator();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            Query<TElement> q = new Query<TElement>(db, expression);
            q.tables.AddRange(tables);
            q.recordTypes.AddRange(recordTypes);
            q.selectors.AddRange(selectors);
            q.where = where;
            q.whereParameters.AddRange(whereParameters);
            q.orderbyColumns.AddRange(orderbyColumns);
            q.selectColumns.AddRange(selectColumns);
            q.joinColumns.AddRange(joinColumns);
            q.projectionDelegates.AddRange(projectionDelegates);

            MethodCallExpression methodCallExpression = (MethodCallExpression)expression;
            string methodName = methodCallExpression.Method.Name;
            if (methodName == "Select")
            {
                LambdaExpression argumentExpression = (LambdaExpression)
                    ((UnaryExpression)methodCallExpression.Arguments[1]).Operand;
                q.BuildProjection(null, argumentExpression);
            }
            else if (methodName == "Where")
            {
                LambdaExpression argumentExpression = (LambdaExpression)
                    ((UnaryExpression)methodCallExpression.Arguments[1]).Operand;
                q.BuildQuery(null, argumentExpression);
            }
            else if (methodName == "ThenBy")
            {
                LambdaExpression argumentExpression = (LambdaExpression)
                    ((UnaryExpression)methodCallExpression.Arguments[1]).Operand;
                q.BuildSequence(null, argumentExpression);
            }
            else if (methodName == "Join")
            {
                ConstantExpression constantExpression = (ConstantExpression)
                    methodCallExpression.Arguments[1];
                IQueryable inner = (IQueryable)constantExpression.Value;
                q.PerformJoin(
                    null,
                    null,
                    inner,
                    GetJoinLambda(methodCallExpression.Arguments[2]),
                    GetJoinLambda(methodCallExpression.Arguments[3]),
                    GetJoinLambda(methodCallExpression.Arguments[4]));
            }
            else
            {
                throw new NotSupportedException(
                    "Query operation not supported: " + methodName);
            }

            return q;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return CreateQuery<T>(expression);
        }

        private static LambdaExpression GetJoinLambda(Expression expression)
        {
            UnaryExpression unaryExpression = (UnaryExpression)expression;
            return (LambdaExpression)unaryExpression.Operand;
        }

        public TResult Execute<TResult>(Expression expression)
        {
            throw new NotSupportedException(
                "Direct method calls not supported -- use AsEnumerable() instead.");
        }

        object IQueryProvider.Execute(Expression expression)
        {
            throw new NotSupportedException(
                "Direct method calls not supported -- use AsEnumerable() instead.");
        }

        public IQueryProvider Provider => this;

        public Type ElementType => typeof(T);

        public Expression Expression => queryableExpression;

        internal void BuildQuery(TableInfo tableInfo, LambdaExpression expression)
        {
            if (tableInfo != null)
            {
                tables.Add(tableInfo);
                recordTypes.Add(typeof(T));
                selectors.Add(expression.Parameters[0].Name);
            }

            StringBuilder queryBuilder = new StringBuilder();

            ParseQuery(expression.Body, queryBuilder);

            where = queryBuilder.ToString();
        }

        internal void BuildNullQuery(TableInfo tableInfo, Type recordType, LambdaExpression expression)
        {
            tables.Add(tableInfo);
            recordTypes.Add(recordType);
            selectors.Add(expression.Parameters[0].Name);
        }

        private void ParseQuery(Expression expression, StringBuilder queryBuilder)
        {
            queryBuilder.Append("(");

            UnaryExpression unaryExpression;
            MethodCallExpression methodCallExpression;

            if (expression is BinaryExpression binaryExpression)
            {
                switch (binaryExpression.NodeType)
                {
                    case ExpressionType.AndAlso:
                        ParseQuery(binaryExpression.Left, queryBuilder);
                        queryBuilder.Append(" AND ");
                        ParseQuery(binaryExpression.Right, queryBuilder);
                        break;

                    case ExpressionType.OrElse:
                        ParseQuery(binaryExpression.Left, queryBuilder);
                        queryBuilder.Append(" OR ");
                        ParseQuery(binaryExpression.Right, queryBuilder);
                        break;

                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.LessThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThanOrEqual:
                        ParseQueryCondition(binaryExpression, queryBuilder);
                        break;

                    default:
                        throw new NotSupportedException(
                                  "Expression type not supported: " + binaryExpression.NodeType);
                }
            }
            else if ((unaryExpression = expression as UnaryExpression) != null)
            {
                throw new NotSupportedException(
                    "Expression type not supported: " + unaryExpression.NodeType);
            }
            else if ((methodCallExpression = expression as MethodCallExpression) != null)
            {
                throw new NotSupportedException(
                    "Method call not supported: " + methodCallExpression.Method.Name + "()");
            }
            else
            {
                throw new NotSupportedException(
                    "Query filter expression not supported: " + expression);
            }

            queryBuilder.Append(")");
        }

        private static ExpressionType OppositeExpression(ExpressionType e)
        {
            switch (e)
            {
                case ExpressionType.LessThan:
                    return ExpressionType.GreaterThan;

                case ExpressionType.LessThanOrEqual:
                    return ExpressionType.GreaterThanOrEqual;

                case ExpressionType.GreaterThan:
                    return ExpressionType.LessThan;

                case ExpressionType.GreaterThanOrEqual:
                    return ExpressionType.LessThanOrEqual;

                default:
                    return e;
            }
        }

        private static bool IsIntegerType(Type t)
        {
            return
                t == typeof(sbyte) ||
                t == typeof(byte) ||
                t == typeof(short) ||
                t == typeof(ushort) ||
                t == typeof(int) ||
                t == typeof(uint) ||
                t == typeof(long) ||
                t == typeof(ulong);
        }

        private void ParseQueryCondition(
            BinaryExpression binaryExpression, StringBuilder queryBuilder)
        {
            string column = GetConditionColumn(binaryExpression, out bool swap);
            queryBuilder.Append(column);

            ExpressionType expressionType = binaryExpression.NodeType;
            if (swap)
            {
                expressionType = OppositeExpression(expressionType);
            }

            LambdaExpression valueExpression = Expression.Lambda(
                swap ? binaryExpression.Left : binaryExpression.Right);
            object value = valueExpression.Compile().DynamicInvoke();

            bool valueIsInt = false;
            if (value != null)
            {
                if (IsIntegerType(value.GetType()))
                {
                    valueIsInt = true;
                }
                else
                {
                    value = value.ToString();
                }
            }

            switch (expressionType)
            {
                case ExpressionType.Equal:
                    if (value == null)
                    {
                        queryBuilder.Append(" IS NULL");
                    }
                    else if (valueIsInt)
                    {
                        queryBuilder.Append(" = ");
                        queryBuilder.Append(value);
                    }
                    else
                    {
                        queryBuilder.Append(" = ?");
                        whereParameters.Add(value);
                    }
                    return;

                case ExpressionType.NotEqual:
                    if (value == null)
                    {
                        queryBuilder.Append(" IS NOT NULL");
                    }
                    else if (valueIsInt)
                    {
                        queryBuilder.Append(" <> ");
                        queryBuilder.Append(value);
                    }
                    else
                    {
                        queryBuilder.Append(" <> ?");
                        whereParameters.Add(value);
                    }
                    return;
            }

            if (value == null)
            {
                throw new InvalidOperationException(
                    "A null value was used in a greater-than/less-than operation.");
            }

            if (!valueIsInt)
            {
                throw new NotSupportedException(
                    "Greater-than/less-than operators not supported on strings.");
            }

            switch (expressionType)
            {
                case ExpressionType.LessThan:
                    queryBuilder.Append(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    queryBuilder.Append(" <= ");
                    break;

                case ExpressionType.GreaterThan:
                    queryBuilder.Append(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    queryBuilder.Append(" >= ");
                    break;

                default:
                    throw new NotSupportedException(
                        "Unsupported query expression type: " + expressionType);
            }

            queryBuilder.Append(value);
        }

        private string GetConditionColumn(
            BinaryExpression binaryExpression, out bool swap)
        {
            MethodCallExpression methodCallExpression;

            if ((binaryExpression.Left is MemberExpression memberExpression) ||
                ((binaryExpression.Left.NodeType == ExpressionType.Convert ||
                  binaryExpression.Left.NodeType == ExpressionType.ConvertChecked) &&
                 (memberExpression = ((UnaryExpression)binaryExpression.Left).Operand
                  as MemberExpression) != null))
            {
                string column = GetConditionColumn(memberExpression);
                if (column != null)
                {
                    swap = false;
                    return column;
                }
            }
            else if (((memberExpression = binaryExpression.Right as MemberExpression) != null) ||
                     ((binaryExpression.Right.NodeType == ExpressionType.Convert ||
                       binaryExpression.Right.NodeType == ExpressionType.ConvertChecked) &&
                      (memberExpression = ((UnaryExpression)binaryExpression.Right).Operand
                       as MemberExpression) != null))
            {
                string column = GetConditionColumn(memberExpression);
                if (column != null)
                {
                    swap = true;
                    return column;
                }
            }
            else if ((methodCallExpression = binaryExpression.Left as MethodCallExpression) != null)
            {
                string column = GetConditionColumn(methodCallExpression);
                if (column != null)
                {
                    swap = false;
                    return column;
                }
            }
            else if ((methodCallExpression = binaryExpression.Right as MethodCallExpression) != null)
            {
                string column = GetConditionColumn(methodCallExpression);
                if (column != null)
                {
                    swap = true;
                    return column;
                }
            }

            throw new NotSupportedException(
                "Unsupported binary expression: " + binaryExpression);
        }

        private string GetConditionColumn(MemberExpression memberExpression)
        {
            string columnName = GetColumnName(memberExpression.Member);
            string selectorName = GetConditionSelectorName(memberExpression.Expression);
            string tableName = GetConditionTable(selectorName, columnName);
            return FormatColumn(tableName, columnName);
        }

        private string GetConditionColumn(MethodCallExpression methodCallExpression)
        {
            LambdaExpression argumentExpression =
                Expression.Lambda(methodCallExpression.Arguments[0]);
            string columnName = (string)argumentExpression.Compile().DynamicInvoke();
            string selectorName = GetConditionSelectorName(methodCallExpression.Object);
            string tableName = GetConditionTable(selectorName, columnName);
            return FormatColumn(tableName, columnName);
        }

        private static string GetConditionSelectorName(Expression expression)
        {
            MemberExpression memberExpression;
            if (expression is ParameterExpression parameterExpression)
            {
                return parameterExpression.Name;
            }
            else if ((memberExpression = expression as MemberExpression) != null)
            {
                return memberExpression.Member.Name;
            }
            else
            {
                throw new NotSupportedException(
                    "Unsupported conditional selector expression: " + expression);
            }
        }

        private string GetConditionTable(string selectorName, string columnName)
        {
            string tableName = null;

            for (int i = 0; i < tables.Count; i++)
            {
                if (selectors[i] == selectorName)
                {
                    tableName = tables[i].Name;
                    break;
                }
            }

            if (tableName == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "Conditional expression contains column {0}.{1} " +
                    "from a table that is not in the query.",
                    selectorName,
                    columnName));
            }

            return tableName;
        }

        private string FormatColumn(string tableName, string columnName)
        {
            if (tableName != null && tables.Count > 1)
            {
                return string.Format(CultureInfo.InvariantCulture, "`{0}`.`{1}`", tableName, columnName);
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "`{0}`", columnName);
            }
        }

        private static string GetColumnName(MemberInfo memberInfo)
        {
            foreach (object attr in memberInfo.GetCustomAttributes(
                typeof(DatabaseColumnAttribute), false))
            {
                return ((DatabaseColumnAttribute)attr).Column;
            }

            return memberInfo.Name;
        }

        internal void BuildProjection(TableInfo tableInfo, LambdaExpression expression)
        {
            if (tableInfo != null)
            {
                tables.Add(tableInfo);
                recordTypes.Add(typeof(T));
                selectors.Add(expression.Parameters[0].Name);
            }

            FindColumns(expression, selectColumns);
            projectionDelegates.Add(expression.Compile());
        }

        internal void BuildSequence(TableInfo tableInfo, LambdaExpression expression)
        {
            if (tableInfo != null)
            {
                tables.Add(tableInfo);
                recordTypes.Add(typeof(T));
                selectors.Add(expression.Parameters[0].Name);
            }

            FindColumns(expression.Body, orderbyColumns);
        }

        private static void AddAllColumns(TableInfo tableInfo, IList<TableColumn> columnList)
        {
            foreach (ColumnInfo column in tableInfo.Columns)
            {
                columnList.Add(new TableColumn(tableInfo, column));
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void FindColumns(Expression expression, IList<TableColumn> columnList)
        {
            if (expression is ParameterExpression)
            {
                ParameterExpression e = expression as ParameterExpression;
                string selector = e.Name;
                for (int i = 0; i < tables.Count; i++)
                {
                    if (selectors[i] == selector)
                    {
                        AddAllColumns(tables[i], columnList);
                        break;
                    }
                }
            }
            else if (expression.NodeType == ExpressionType.MemberAccess)
            {
                FindColumns(expression as MemberExpression, columnList);
            }
            else if (expression is MethodCallExpression)
            {
                FindColumns(expression as MethodCallExpression, columnList);
            }
            else if (expression is BinaryExpression)
            {
                BinaryExpression e = expression as BinaryExpression;
                FindColumns(e.Left, columnList);
                FindColumns(e.Right, columnList);
            }
            else if (expression is UnaryExpression)
            {
                UnaryExpression e = expression as UnaryExpression;
                FindColumns(e.Operand, columnList);
            }
            else if (expression is ConditionalExpression)
            {
                ConditionalExpression e = expression as ConditionalExpression;
                FindColumns(e.Test, columnList);
                FindColumns(e.IfTrue, columnList);
                FindColumns(e.IfFalse, columnList);
            }
            else if (expression is InvocationExpression)
            {
                InvocationExpression e = expression as InvocationExpression;
                FindColumns(e.Expression, columnList);
                FindColumns(e.Arguments, columnList);
            }
            else if (expression is LambdaExpression)
            {
                LambdaExpression e = expression as LambdaExpression;
                FindColumns(e.Body, columnList);
            }
            else if (expression is ListInitExpression)
            {
                ListInitExpression e = expression as ListInitExpression;
                FindColumns(e.NewExpression, columnList);
                foreach (ElementInit ei in e.Initializers)
                {
                    FindColumns(ei.Arguments, columnList);
                }
            }
            else if (expression is MemberInitExpression)
            {
                MemberInitExpression e = expression as MemberInitExpression;
                FindColumns(e.NewExpression, columnList);
                foreach (MemberAssignment b in e.Bindings)
                {
                    FindColumns(b.Expression, columnList);
                }
            }
            else if (expression is NewExpression)
            {
                NewExpression e = expression as NewExpression;
                FindColumns(e.Arguments, columnList);
            }
            else if (expression is NewArrayExpression)
            {
                NewArrayExpression e = expression as NewArrayExpression;
                FindColumns(e.Expressions, columnList);
            }
            else if (expression is TypeBinaryExpression)
            {
                TypeBinaryExpression e = expression as TypeBinaryExpression;
                FindColumns(e.Expression, columnList);
            }
        }

        private void FindColumns(IEnumerable<Expression> expressions, IList<TableColumn> columnList)
        {
            foreach (Expression expression in expressions)
            {
                FindColumns(expression, columnList);
            }
        }

        private void FindColumns(MemberExpression memberExpression, IList<TableColumn> columnList)
        {
            string selector = null;
            MemberExpression objectMemberExpression;
            if (memberExpression.Expression is ParameterExpression objectParameterExpression)
            {
                selector = objectParameterExpression.Name;
            }
            else if ((objectMemberExpression = memberExpression.Expression as
                MemberExpression) != null)
            {
                selector = objectMemberExpression.Member.Name;
            }

            if (selector != null)
            {
                for (int i = 0; i < tables.Count; i++)
                {
                    if (selectors[i] == selector)
                    {
                        string columnName = GetColumnName(memberExpression.Member);
                        ColumnInfo column = tables[i].Columns[columnName];
                        columnList.Add(new TableColumn(tables[i], column));
                        break;
                    }
                }
            }

            selector = memberExpression.Member.Name;
            for (int i = 0; i < tables.Count; i++)
            {
                if (selectors[i] == selector)
                {
                    AddAllColumns(tables[i], columnList);
                    break;
                }
            }
        }

        private void FindColumns(MethodCallExpression methodCallExpression, IList<TableColumn> columnList)
        {
            if (methodCallExpression.Method.Name == "get_Item" &&
                methodCallExpression.Arguments.Count == 1 &&
                methodCallExpression.Arguments[0].Type == typeof(string))
            {
                string selector = null;
                MemberExpression objectMemberExpression;
                if (methodCallExpression.Object is ParameterExpression objectParameterExpression)
                {
                    selector = objectParameterExpression.Name;
                }
                else if ((objectMemberExpression = methodCallExpression.Object as MemberExpression) != null)
                {
                    selector = objectMemberExpression.Member.Name;
                }

                if (selector != null)
                {
                    for (int i = 0; i < tables.Count; i++)
                    {
                        if (selectors[i] == selector)
                        {
                            LambdaExpression argumentExpression =
                                Expression.Lambda(methodCallExpression.Arguments[0]);
                            string columnName = (string)
                                argumentExpression.Compile().DynamicInvoke();
                            ColumnInfo column = tables[i].Columns[columnName];
                            columnList.Add(new TableColumn(tables[i], column));
                            break;
                        }
                    }
                }
            }

            if (methodCallExpression.Object != null && methodCallExpression.Object.NodeType != ExpressionType.Parameter)
            {
                FindColumns(methodCallExpression.Object, columnList);
            }
        }

        internal void PerformJoin(
            TableInfo tableInfo,
            Type recordType,
            IQueryable joinTable,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            LambdaExpression resultSelector)
        {
            if (joinTable == null)
            {
                throw new ArgumentNullException("joinTable");
            }

            if (tableInfo != null)
            {
                tables.Add(tableInfo);
                recordTypes.Add(recordType);
                selectors.Add(outerKeySelector.Parameters[0].Name);
            }

            PropertyInfo tableInfoProp = joinTable.GetType().GetProperty("TableInfo");
            if (tableInfoProp == null)
            {
                throw new NotSupportedException(
                    "Cannot join with object: " + joinTable.GetType().Name +
                    "; join is only supported on another QTable.");
            }

            TableInfo joinTableInfo = (TableInfo)tableInfoProp.GetValue(joinTable, null);
            if (joinTableInfo == null)
            {
                throw new InvalidOperationException("Missing join table info.");
            }

            tables.Add(joinTableInfo);
            recordTypes.Add(joinTable.ElementType);
            selectors.Add(innerKeySelector.Parameters[0].Name);
            projectionDelegates.Add(resultSelector.Compile());

            int joinColumnCount = joinColumns.Count;
            FindColumns(outerKeySelector.Body, joinColumns);
            if (joinColumns.Count > joinColumnCount + 1)
            {
                throw new NotSupportedException("Join operations involving " +
                  "multiple columns are not supported.");
            }
            else if (joinColumns.Count != joinColumnCount + 1)
            {
                throw new InvalidOperationException("Bad outer key selector for join.");
            }

            FindColumns(innerKeySelector.Body, joinColumns);
            if (joinColumns.Count > joinColumnCount + 2)
            {
                throw new NotSupportedException("Join operations involving " +
                  "multiple columns not are supported.");
            }
            if (joinColumns.Count != joinColumnCount + 2)
            {
                throw new InvalidOperationException("Bad inner key selector for join.");
            }
        }
    }

    internal class TableColumn
    {
        public TableColumn(TableInfo table, ColumnInfo column)
        {
            Table = table;
            Column = column;
        }

        public TableInfo Table { get; set; }
        public ColumnInfo Column { get; set; }
    }
}