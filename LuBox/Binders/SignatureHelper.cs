namespace LuBox.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class SignatureHelper
    {
        public static IEnumerable<MethodInfo> OrderSignatureMatches(DynamicMetaObject[] args, MethodInfo[] signatures)
        {
            if (signatures.Length == 1)
            {
                return signatures;
            }

            Type[] argTypes = args.Select(x => x.LimitType).ToArray();

            var orderSignatureMatches = signatures
                .OrderByDescending(x => (AreArgumentTypesAssignable(argTypes, x)))
                .ThenBy(x => Math.Abs(x.GetParameters().Length - argTypes.Length))
                .ThenBy(x => x.GetParameters().Length).ToArray();

            return orderSignatureMatches;
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

            if (pType.IsClass && pType.GetConstructor(new Type[0]) != null)
            {
                return argType == typeof(LuTable);
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
            var callArguments = args.Select(x => x.Expression).ToArray();
            var parameterInfos = methodInfo.GetParameters();

            var newArguments = new List<Expression>();
            for (int index = 0; index < parameterInfos.Length; index++)
            {
                var parameterInfo = parameterInfos[index];
                bool isParamsParameter = parameterInfo.GetCustomAttribute<ParamArrayAttribute>() != null;

                if (isParamsParameter)
                {
                    var elementType = parameterInfo.ParameterType.GetElementType();

                    if (index < callArguments.Length)
                    {
                        var paramsArgumentExpression = Expression.NewArrayInit(elementType,
                            args.Select(x => Expression.Convert(x.Expression, elementType)).Skip(index));
                        newArguments.Add(paramsArgumentExpression);
                    }
                    else
                    {
                        newArguments.Add(Expression.NewArrayInit(elementType));
                    }
                }
                else if (index < callArguments.Length)
                {
                    var arg = callArguments[index];
                    newArguments.Add(Expression.Dynamic(new LuConvertBinder(parameterInfo.ParameterType, false), parameterInfo.ParameterType, arg));
                }
            }

            return newArguments;
        }
    }
}