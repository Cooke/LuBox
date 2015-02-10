using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LuBox.Runtime
{
    using LuBox.Utils;

    internal class LuInvokeMemberBinder : InvokeMemberBinder
    {
        public LuInvokeMemberBinder(string name, bool ignoreCase, CallInfo callInfo) : base(name, ignoreCase, callInfo)
        {
        }

        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args,
            DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue || !args.All(x => x.HasValue))
            {
                return Defer(target, args);
            }

            Sandboxer.ThrowIfReflectionType(target.LimitType);

            var restrictions = GetRestrictions(target, args);
            var memberInfo = GetMemberInfo(target, args);
            var callArguments = NuInvokeHelper.GetCallArguments(args, memberInfo);

            Sandboxer.ThrowIfReflectionMember(memberInfo);

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