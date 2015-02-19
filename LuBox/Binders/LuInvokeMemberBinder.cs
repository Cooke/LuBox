using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LuBox.Sandboxing;

namespace LuBox.Runtime
{
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

            var restrictions = LuInvokeHelper.GetTypeRestrictions(target, args);
            var memberInfo = GetMemberInfo(target, args);
            var callArguments = LuInvokeHelper.TransformArguments(args, memberInfo);

            Sandboxer.ThrowIfReflectionMember(memberInfo);

            return
                new DynamicMetaObject(
                    RuntimeHelpers.EnsureObjectResult(
                        Expression.Call(Expression.Convert(target.Expression, target.LimitType), memberInfo,
                            callArguments)),
                    restrictions);
        }

        private MethodInfo GetMemberInfo(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            MethodInfo[] methods = target.LimitType.GetMember(Name, MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public).Cast<MethodInfo>().ToArray();
            MethodInfo methodInfo = LuInvokeHelper.OrderSignatureMatches(args, methods).FirstOrDefault();

            if (methodInfo == null ||
                !LuInvokeHelper.AreArgumentTypesAssignable(args.Select(x => x.LimitType).ToArray(), methodInfo))
            {
                throw new LuRuntimeException("Could not find a matching member signature with name " + Name);
            }

            return methodInfo;
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}