namespace LuBox.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class LuInvokeHelper
    {
        public static BindingRestrictions GetRestrictions(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            var restrictions = target.Restrictions.Merge(BindingRestrictions.Combine(args))
                .Merge(BindingRestrictions.GetTypeRestriction(
                    target.Expression, target.LimitType));
            foreach (var result in args.Select(x => BindingRestrictions.GetTypeRestriction(
                x.Expression, x.LimitType)))
            {
                restrictions = restrictions.Merge(result);
            }

            return restrictions;
        }

        public static IEnumerable<Expression> GetCallArguments(DynamicMetaObject[] args, MemberInfo memberInfo)
        {
            IEnumerable<Expression> callArguments = args.Select(x => Expression.Convert(x.Expression, x.LimitType));
            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo != null)
            {
                ParameterInfo paramsParameterInfo =
                    methodInfo.GetParameters().FirstOrDefault(x => x.GetCustomAttribute<ParamArrayAttribute>() != null);
                if (paramsParameterInfo != null)
                {
                    var paramsIndex = Array.IndexOf(methodInfo.GetParameters(), paramsParameterInfo);
                    Type elementType = paramsParameterInfo.ParameterType.GetElementType();
                    callArguments =
                        args.Select(x => x.Expression)
                            .Take(paramsIndex)
                            .Concat(new Expression[]
                                        {
                                            Expression.NewArrayInit(elementType,
                                                args.Select(x => Expression.Convert(x.Expression, elementType)).Skip(paramsIndex))
                                        });
                }
            }

            return callArguments;
        }
    }
}