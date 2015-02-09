namespace LuBox.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class LuFunctionMetaObject : DynamicMetaObject
    {
        private readonly Type _delegateType;

        public LuFunctionMetaObject(Expression expression, BindingRestrictions restrictions, object instance, Type delegateType) : base(expression, restrictions, instance)
        {
            this._delegateType = delegateType;
        }

        public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
        {
            Expression delegateExpression = Expression.Convert(Expression.Property(Expression.Convert(Expression, typeof(LuFunction)), "Delegate"), _delegateType);
            Expression invokeExpression = System.Linq.Expressions.Expression.Invoke(delegateExpression, args.Select(x => RuntimeHelpers.EnsureObjectResult(x.Expression)));

            // TODO improve restriction to allow different functions with same delegate type
            BindingRestrictions instanceRestriction = BindingRestrictions.GetInstanceRestriction(Expression, Value);
            return new DynamicMetaObject(invokeExpression, instanceRestriction);
        }

        public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
        {
            bool b = binder.Operation == ExpressionType.Convert;
            return base.BindUnaryOperation(binder);
        }

        public override DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            Type targetType = binder.Type;
            MethodInfo targetMethodInfo = binder.Type.GetMethod("Invoke");
            MethodInfo sourceMethodInfo = _delegateType.GetMethod("Invoke");

            Expression delegateExpression = Expression.Property(Expression.Convert(Expression, typeof(LuFunction)), "Delegate");

            // TODO improve restriction
            BindingRestrictions restriction = BindingRestrictions.GetInstanceRestriction(Expression, Value);

            if (!targetType.IsAssignableFrom(_delegateType))
            {
                ParameterInfo[] targetParameters = targetMethodInfo.GetParameters();
                ParameterInfo[] sourceParameters = sourceMethodInfo.GetParameters();

                ParameterExpression[] targetParameterExpressions = targetParameters.Select(x => Expression.Parameter(x.ParameterType)).ToArray();
                IEnumerable<Expression> sourceParameterExpressions;
                if (targetParameters.Length > sourceParameters.Length)
                {
                    sourceParameterExpressions = targetParameterExpressions.Take(sourceParameters.Length);
                }
                else
                {
                    var extraParaExpressions = Enumerable.Repeat(Expression.Constant(null, typeof(object)), sourceParameters.Length - targetParameters.Length);
                    sourceParameterExpressions = targetParameterExpressions.Concat<Expression>(extraParaExpressions);
                }

                LambdaExpression lambdaExpression = Expression.Lambda(targetType, Expression.Invoke(Expression.Convert(delegateExpression, _delegateType), sourceParameterExpressions.Select(x => Expression.Convert(x, typeof(object)))), targetParameterExpressions);
                return new DynamicMetaObject(lambdaExpression, restriction);
            }
            else
            {
                return new DynamicMetaObject(Expression.Convert(delegateExpression, targetType), restriction);
            }
        }
    }
}