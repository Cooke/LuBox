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

        public LuFunctionMetaObject(Expression expression, BindingRestrictions restrictions, object instance, Type delegateType)
            : base(expression, restrictions, instance)
        {
            _delegateType = delegateType;
        }

        public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
        {
            var delegateExpression = Expression.Convert(Expression.Property(Expression.Convert(Expression, typeof(LuFunction)), "Delegate"), _delegateType);
            var callExpression = Expression.Dynamic(new LuInvokeBinder(new CallInfo(args.Length)), typeof(object),
                new[] { delegateExpression }.Concat(args.Select(x => x.Expression)));

            var restrictions = BindingRestrictions.GetTypeRestriction(delegateExpression, _delegateType);
            return new DynamicMetaObject(callExpression, restrictions);

            //var funcType = Expression.GetFuncType(Enumerable.Repeat(typeof (object), args.Length + 1).ToArray());
            //Expression delegateExpression = Expression.Convert(Expression.Property(Expression.Convert(Expression, typeof(LuFunction)), "Delegate"), _delegateType);

            //var signature = GetOrCreateSignature(funcType, delegateExpression);
            //Expression invokeExpression = Expression.Invoke(signature, args.Select(x => RuntimeHelpers.EnsureObjectResult(x.Expression)));
            //var bindingRestrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);

            //return new DynamicMetaObject(invokeExpression, bindingRestrictions);
        }

        public override DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            Type targetType = binder.Type;
            Expression delegateExpression = Expression.Property(Expression.Convert(Expression, typeof(LuFunction)), "Delegate");

            BindingRestrictions restriction = BindingRestrictions.GetTypeRestriction(Expression, typeof(LuFunction));

            var signature = GetOrCreateSignature(targetType, delegateExpression);
            return new DynamicMetaObject(signature, restriction);
        }

        private Expression GetOrCreateSignature(Type targetType, Expression delegateExpression)
        {
            if (!targetType.IsAssignableFrom(_delegateType))
            {
                var tryGetCompatibleSignatureMethodInfo = typeof(LuFunction).GetMethod("TryGetCompatibleSignature");
                var resultSignature = Expression.Parameter(typeof(Delegate));
                var tryGetSignature = Expression.Call(Expression.Convert(Expression, typeof(LuFunction)),
                    tryGetCompatibleSignatureMethodInfo, Expression.Constant(targetType), resultSignature);
                var lambdaExpression = CreateSignature(targetType, delegateExpression);
                var addSignatureMethodInfo = typeof(LuFunction).GetMethod("AddSignature");
                var addSignatureExpression =
                    Expression.Convert(
                        Expression.Call(Expression.Convert(Expression, typeof(LuFunction)), addSignatureMethodInfo,
                            lambdaExpression), targetType);
                var getOrAddExpression = Expression.Condition(tryGetSignature, Expression.Convert(resultSignature, targetType),
                    addSignatureExpression);
                var blockExpression = Expression.Block(new[] { resultSignature }, getOrAddExpression);
                return blockExpression;
            }
            else
            {
                return Expression.Convert(delegateExpression, targetType);
            }
        }

        private LambdaExpression CreateSignature(Type targetType, Expression delegateExpression)
        {
            MethodInfo targetMethodInfo = targetType.GetMethod("Invoke");
            MethodInfo sourceMethodInfo = _delegateType.GetMethod("Invoke");
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

            LambdaExpression lambdaExpression = Expression.Lambda(
                targetType,
                Expression.Invoke(Expression.Convert(delegateExpression, _delegateType), sourceParameterExpressions.Select(x => Expression.Convert(x, typeof(object)))),
                targetParameterExpressions);
            return lambdaExpression;
        }
    }
}