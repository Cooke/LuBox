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
        private readonly Type[] _extensionMethodTypes;

        public LuInvokeMemberBinder(Type[] extensionMethodTypes, string name, bool ignoreCase, CallInfo callInfo) : base(name, ignoreCase, callInfo)
        {
            _extensionMethodTypes = extensionMethodTypes;
        }

        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args,
            DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue || !args.All(x => x.HasValue))
            {
                return Defer(target, args);
            }

            Sandboxer.ThrowIfReflectionType(target.LimitType);

            var memberInfo = GetMemberInfo(target, args);

            Sandboxer.ThrowIfReflectionMember(memberInfo);

            Expression callExpression;
            if (!memberInfo.IsStatic)
            {
                var callArguments = SignatureHelper.TransformArguments(args, memberInfo);

                callExpression = ResultHelper.EnsureObjectResult(
                    Expression.Call(Expression.Convert(target.Expression, target.LimitType), memberInfo,
                        callArguments));
            }
            else
            {
                // Extension method
                var callArguments = SignatureHelper.TransformArguments(new[] {target}.Concat(args).ToArray(), memberInfo);
                callExpression = ResultHelper.EnsureObjectResult(Expression.Call(memberInfo, callArguments));
            }


            var restrictions = RestrictionHelper.GetTypeRestrictions(target, args);
            return
                new DynamicMetaObject(
                    callExpression,
                    restrictions);
        }

        private MethodInfo GetMemberInfo(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            var methods = target.LimitType.GetMember(Name, MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public).Cast<MethodBase>();

            MethodInfo methodInfo = (MethodInfo)SignatureHelper.OrderSignatureMatches(args, methods.ToArray()).FirstOrDefault();

            if (methodInfo == null)
            {
                var linqMethods = _extensionMethodTypes.SelectMany(x => x.GetMember(Name, MemberTypes.Method, BindingFlags.Static | BindingFlags.Public)).Cast<MethodBase>();

                args = new[] { target }.Concat(args).ToArray();
                methodInfo = (MethodInfo)SignatureHelper.OrderSignatureMatches(args, linqMethods.ToArray()).FirstOrDefault();
            }

            if (methodInfo == null ||
                !SignatureHelper.AreArgumentTypesAssignable(args.Select(x => x.LimitType).ToArray(), methodInfo))
            {
                throw new LuRuntimeException("Could not find a matching member signature with name " + Name);
            }

            if (methodInfo.IsGenericMethodDefinition)
            {
                var genericTypeArguments = SignatureHelper.GetGenericTypeArguments(methodInfo, args.Select(x => x.LimitType).ToArray());

                return methodInfo.MakeGenericMethod(genericTypeArguments);
            }

            return methodInfo;
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}