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

            var restrictions = RestrictionHelper.GetTypeRestrictions(target, args);
            var memberInfo = GetMemberInfo(target, args);
            var callArguments = SignatureHelper.TransformArguments(args, memberInfo);

            Sandboxer.ThrowIfReflectionMember(memberInfo);

            var callExpression = ResultHelper.EnsureObjectResult(
                Expression.Call(Expression.Convert(target.Expression, target.LimitType), memberInfo,
                    callArguments));

            var temp = Expression.Variable(typeof (object));
            var assign = Expression.Assign(temp, callExpression);
            var symbolDocumentInfo = Expression.SymbolDocument("somename");
            var debugInfoExpression = Expression.DebugInfo(symbolDocumentInfo, 1, 1, 10, 10);
            var clearDebugInfo = Expression.ClearDebugInfo(symbolDocumentInfo);
            var blockExpression = Expression.Block(new ParameterExpression[] { temp }, new Expression[] {debugInfoExpression, assign, clearDebugInfo, temp});
            return
                new DynamicMetaObject(
                blockExpression,
                restrictions); ;
        }

        private MethodInfo GetMemberInfo(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            MethodInfo[] methods = target.LimitType.GetMember(Name, MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public).Cast<MethodInfo>().ToArray();
            MethodInfo methodInfo = SignatureHelper.OrderSignatureMatches(args, methods).FirstOrDefault();

            if (methodInfo == null ||
                !SignatureHelper.AreArgumentTypesAssignable(args.Select(x => x.LimitType).ToArray(), methodInfo))
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