using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LuBox.Compiler;

namespace LuBox.Runtime
{
    internal class NuInvokeHelper
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
            IEnumerable<Expression> callArguments = args.Select(x => x.Expression);
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

    internal class NuInvokeBinder : InvokeBinder
    {
        public NuInvokeBinder(CallInfo callInfo) : base(callInfo)
        {
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue || !args.All(x => x.HasValue))
            {
                return Defer(target, args);
            }

            var restrictions = NuInvokeHelper.GetRestrictions(target, args);
            
            return
                new DynamicMetaObject(
                    RuntimeHelpers.EnsureObjectResult(
                        Expression.Invoke(Expression.Convert(target.Expression, target.LimitType), args.Select(x => Expression.Convert(x.Expression, typeof(object))))),
                    restrictions);
        }
    }

    internal class NuInvokeMemberBinder : InvokeMemberBinder
    {
        public NuInvokeMemberBinder(string name, bool ignoreCase, CallInfo callInfo) : base(name, ignoreCase, callInfo)
        {
        }

        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args,
            DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue || !args.All(x => x.HasValue))
            {
                return Defer(target, args);
            }

            var restrictions = GetRestrictions(target, args);
            var memberInfo = GetMemberInfo(target, args);
            var callArguments = NuInvokeHelper.GetCallArguments(args, memberInfo);

            return
                new DynamicMetaObject(
                    RuntimeHelpers.EnsureObjectResult(
                        Expression.Call(Expression.Convert(target.Expression, target.LimitType), (MethodInfo)memberInfo, callArguments)),
                    restrictions);
        }

        private MemberInfo GetMemberInfo(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            var members = target.LimitType.GetMember(Name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            MemberInfo memberInfo = members.First(x => IsSignatureMatch(args, x));
            return memberInfo;
        }

        private static BindingRestrictions GetRestrictions(DynamicMetaObject target, DynamicMetaObject[] args)
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

        

        private static bool IsSignatureMatch(DynamicMetaObject[] args, MemberInfo memberInfo)
        {
            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo == null)
            {
                return false;
            }

            ParameterInfo paramsParameter = methodInfo.GetParameters().FirstOrDefault(x => x.GetCustomAttribute<ParamArrayAttribute>() != null);
            if (paramsParameter == null && methodInfo.GetParameters().Count() != args.Length)
            {
                return false;
            }

            ParameterInfo[] parameterInfos = methodInfo.GetParameters();

            for (int index = 0; index < args.Length; index++)
            {
                var arg = args[index];
                if (index < parameterInfos.Length - 1 || (index == parameterInfos.Length - 1 && paramsParameter == null))
                {
                    if (!parameterInfos[index].ParameterType.IsAssignableFrom(arg.LimitType))
                    {
                        return false;
                    }
                }
                else if (!paramsParameter.ParameterType.GetElementType().IsAssignableFrom(arg.LimitType))
                {
                    return false;
                }
            }

            return true;
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}