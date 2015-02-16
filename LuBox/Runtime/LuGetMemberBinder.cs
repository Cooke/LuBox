using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using LuBox.Sandboxing;

namespace LuBox.Runtime
{
    internal class LuGetIndexBinder : GetIndexBinder
    {
        public LuGetIndexBinder(CallInfo callInfo) : base(callInfo)
        {
        }

        public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes,
            DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue || !indexes.All(x => x.HasValue))
            {
                return Defer(target, indexes);
            }

            if (indexes.Length != 1)
            {
                throw new LuRuntimeException("Only one indexer is allowed");
            }

            var getMethodIfo = target.LimitType.GetMethod("GetValue", new[] {typeof (int)});

            var indexExpression = Expression.Call(Expression.Convert(target.Expression, target.LimitType), getMethodIfo, indexes.First().Expression);
            return new DynamicMetaObject(indexExpression, BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
        }
    }

    internal class LuGetMemberBinder : GetMemberBinder
    {
        public LuGetMemberBinder(string name) : base(name, false)
        {
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {            
            if (!target.HasValue)
            {
                return Defer(target);
            }

            const BindingFlags flags = BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public;
            var members = target.LimitType.GetMember(Name, flags);
            if (members.Length != 1)
            {
                throw new NotSupportedException();
            }
            
            var member = members[0];
            Sandboxer.ThrowIfReflectionMember(member);

            if (member.MemberType == MemberTypes.Event)
            {
                return new DynamicMetaObject(
                    Expression.New(typeof(LuEventWrapper).GetConstructor(new[] { typeof(EventInfo), typeof(object) }), Expression.Constant(member), target.Expression),
                    BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
            }

            return new DynamicMetaObject(
                RuntimeHelpers.EnsureObjectResult(Expression.MakeMemberAccess(Expression.Convert(target.Expression, member.DeclaringType), member)),
                BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
        }
    }
}