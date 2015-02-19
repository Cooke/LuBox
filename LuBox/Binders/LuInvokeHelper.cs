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
        public static BindingRestrictions GetTypeRestrictions(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            var targetRestriction = BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType);
            var argumentRestrictions = args.Select(x => x.Value == null ? BindingRestrictions.GetInstanceRestriction(x.Expression, null) : BindingRestrictions.GetTypeRestriction(
                x.Expression, x.LimitType));

            var resultDescription = targetRestriction;
            foreach (var result in argumentRestrictions)
            {
                resultDescription = resultDescription.Merge(result);
            }

            return resultDescription;
        }

        public static IEnumerable<MethodInfo> OrderSignatureMatches(DynamicMetaObject[] args, MethodInfo[] signatures)
        {
            if (signatures.Length == 1)
            {
                return signatures;
            }

            Type[] argTypes = args.Select(x => x.LimitType).ToArray();

            var orderSignatureMatches = signatures.OrderByDescending(x => AreArgumentTypesEqual(argTypes, x))
                .ThenByDescending(x => (AreArgumentTypesAssignable(argTypes, x)))
                .ThenBy(x => Math.Abs(x.GetParameters().Length - argTypes.Length))
                .ThenBy(x => x.GetParameters().Length).ToArray();

            return orderSignatureMatches;
        }

        private static bool AreArgumentTypesEqual(Type[] argTypes, MethodInfo methodInfo)
        {
            var paramTypes = methodInfo.GetParameters().Select(x => x.ParameterType).ToArray();
            return
                paramTypes.Where((pType, pIndex) => pIndex >= argTypes.Length || pType == argTypes[pIndex]).Count() ==
                paramTypes.Length;
        }

        public static bool AreArgumentTypesAssignable(Type[] argTypes, MethodInfo methodInfo)
        {
            var paramTypes = methodInfo.GetParameters().ToArray();
            return
                paramTypes.Where(
                    (para, pIndex) =>
                        pIndex >= argTypes.Length || IsAssignableFrom(para.ParameterType, argTypes[pIndex]) ||
                        IsParamsMatch(para, argTypes.Skip(pIndex))).Count() ==
                paramTypes.Length;
        }

        private static bool IsParamsMatch(ParameterInfo para, IEnumerable<Type> argTypes)
        {
            var paramsAttribute = para.GetCustomAttribute<ParamArrayAttribute>();
            if (paramsAttribute == null)
            {
                return false;
            }

            var paramsType = para.ParameterType.GetElementType();
            return argTypes.All(argType => IsAssignableFrom(paramsType, argType));
        }

        // TODO consider more implicit conversions
        private static bool IsAssignableFrom(Type pType, Type argType)
        {
            if (pType.IsAssignableFrom(argType))
            {
                return true;
            }

            if (pType == typeof (double))
            {
                return (argType == typeof (int) || argType == typeof (float) || argType == typeof (long));
            }

            if (pType == typeof (float))
            {
                return (argType == typeof(int) || argType == typeof(long));
            }

            if (pType == typeof(long))
            {
                return argType == typeof(int);
            }

            return false;
        }

        public static IEnumerable<Expression> TransformArguments(DynamicMetaObject[] args, MethodInfo methodInfo)
        {
            IEnumerable<Expression> callArguments = args.Select(x => x.Expression);

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

            return callArguments;
        }
    }
}