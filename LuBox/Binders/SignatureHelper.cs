using System.Collections;

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
        private static readonly Type[] NumberTypes = { typeof(int), typeof(float), typeof(long), typeof(double) };

        public static IEnumerable<MethodBase> OrderSignatureMatches(DynamicMetaObject[] args, MethodBase[] signatures)
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

        public static bool AreArgumentTypesAssignable(Type[] argTypes, MethodBase methodInfo)
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

            if ((pType.IsGenericType && pType.GetGenericTypeDefinition() == typeof(IEnumerable<>) || pType == typeof(IEnumerable)) && argType == typeof(LuTable))
            {
                return true;
            }

            // Need to add constrains checks
            if (pType.IsGenericParameter)
            {
                return true;
            }

            if (pType.IsGenericType)
            {
                if (
                    argType.GetInterfaces()
                        .Where(x => x.IsGenericType)
                        .Select(x => x.GetGenericTypeDefinition())
                        .Contains(pType.GetGenericTypeDefinition()))
                {
                    return true;
                }
            }

            if (pType.IsClass && pType.GetConstructor(new Type[0]) != null)
            {
                return argType == typeof(LuTable);
            }

            if (typeof(Delegate).IsAssignableFrom(pType) && argType == typeof(LuFunction))
            {
                return true;
            }
            
            if (IsNumberType(pType))
            {
                return IsNumberType(argType);
            }

            return false;
        }

        public static bool IsNumberType(Type type)
        {
            return NumberTypes.Contains(type);
        }

        public static IEnumerable<Expression> TransformArguments(DynamicMetaObject[] args, MethodBase methodInfo)
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
                    newArguments.Add(Expression.Dynamic(new LuConvertBinder(parameterInfo.ParameterType), parameterInfo.ParameterType, arg));
                }
                else
                {
                    newArguments.Add(Expression.Default(parameterInfo.ParameterType));
                }
            }

            return newArguments;
        }

        public static Type[] GetGenericTypeArguments(MethodInfo methodInfo, Type[] args)
        {
            var genericArguments = methodInfo.GetGenericArguments();
            var parameters = methodInfo.GetParameters();

            return genericArguments.Select(x => GetGenericArgumentType(args, parameters, x)).ToArray();
        }

        private static Type GetGenericArgumentType(Type[] args, ParameterInfo[] parameters, Type genericArgument)
        {
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameterInfo = parameters[index];
                var arg = args[index];

                var parameterType = parameterInfo.ParameterType;
                if (parameterType.IsGenericType)
                {
                    Type genericArgumentType;
                    if (FindGenericTypeArgument(genericArgument, parameterType, arg, out genericArgumentType))
                    {
                        return genericArgumentType;
                    }
                }
                else if (parameterType.IsGenericParameter && parameterType == genericArgument)
                {
                    return arg;
                }
            }

            throw new Exception("BAD");
        }

        private static bool FindGenericTypeArgument(Type genericArgument, Type parameterType, Type argumentType, out Type genericArgumentType)
        {
            // Simplyficatino
            var argGenericMatch =
                argumentType.GetInterfaces()
                    .First(
                        x =>
                            x.IsGenericType &&
                            x.GetGenericTypeDefinition() == parameterType.GetGenericTypeDefinition());

            var genericTypeArgumentIndex = Array.IndexOf(parameterType.GenericTypeArguments, genericArgument);
            if (genericTypeArgumentIndex != -1)
            {
                
                    genericArgumentType = argGenericMatch.GenericTypeArguments[genericTypeArgumentIndex];
                    return true;
            }
            else
            {
                for (int index = 0; index < parameterType.GenericTypeArguments.Length; index++)
                {
                    var paramGenericTypeArgument = parameterType.GenericTypeArguments[index];
                    var argGenericTypeArgument = argGenericMatch.GenericTypeArguments[index];

                    if (FindGenericTypeArgument(genericArgument, paramGenericTypeArgument, argGenericTypeArgument,
                        out genericArgumentType))
                    {
                        return true;
                    }
                }
            }

            genericArgumentType = null;
            return false;
        }
    }
}