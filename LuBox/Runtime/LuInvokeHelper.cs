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
            var restrictions = BindingRestrictions.GetTypeRestriction(
                target.Expression, target.LimitType);
            foreach (var result in args.Select(x => x.Value == null ? BindingRestrictions.GetInstanceRestriction(x.Expression, null) : BindingRestrictions.GetTypeRestriction(
                x.Expression, x.LimitType)))
            {
                restrictions = restrictions.Merge(result);
            }

            return restrictions;
        }

        public static IEnumerable<Expression> GetCallArguments(DynamicMetaObject[] args, MemberInfo memberInfo)
        {
            IEnumerable<Expression> callArguments = args.Select(x => x.Expression);
            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo != null)
            {
                var parameterInfos = methodInfo.GetParameters();
                ParameterInfo paramsParameterInfo =
                    parameterInfos.FirstOrDefault(x => x.GetCustomAttribute<ParamArrayAttribute>() != null);
                if (paramsParameterInfo != null)
                {
                    var paramsIndex = Array.IndexOf(parameterInfos, paramsParameterInfo);
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

                if (parameterInfos.Length > args.Length)
                {
                    callArguments =
                        callArguments.Concat(Enumerable.Repeat(Expression.Constant(null, typeof (object)),
                            parameterInfos.Length - args.Length));
                }
                else
                {
                    callArguments = callArguments.Take(parameterInfos.Length);
                }


                callArguments = callArguments.Select((x, i) => Expression.Convert(x, parameterInfos[i].ParameterType));
            }

            return callArguments;
        }
    }
}