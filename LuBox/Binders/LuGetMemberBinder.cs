using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using LuBox.Sandboxing;
using Microsoft.CSharp.RuntimeBinder;

namespace LuBox.Runtime
{
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

            if (target.Value == null)
            {
                throw new LuRuntimeException("Target null reference error while getting member with name " + Name);
            }

            const BindingFlags flags = BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public;
            var members = target.LimitType.GetMember(Name, flags);
            if (members.Length != 1)
            {
                throw new LuRuntimeException("Failed to find exactly one member with name " + Name);
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