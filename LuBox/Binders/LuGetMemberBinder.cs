using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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

            if (members.Length == 0)
            {
                throw new LuRuntimeException("There is no member with name " + Name);
            }

            var firstMember = members.First();
            var memberType = firstMember.MemberType;

            Sandboxer.ThrowIfReflectionMember(firstMember);

            switch (memberType)
            {
                case MemberTypes.Method:
                    // TODO optimize calling CLR objects with '.' notation by look-a-head and see that an 
                    // invocation will be performed and skip returning an object as intermidiate value
                    return new DynamicMetaObject(
                        Expression.New(
                            typeof (MethodWrapper).GetConstructor(new[] {typeof (MethodInfo[]), typeof (object)}),
                            Expression.Constant(members.Cast<MethodInfo>().ToArray()), target.Expression),
                        BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
                    break;
                case MemberTypes.Event:
                    return new DynamicMetaObject(
                        Expression.New(typeof(EventWrapper).GetConstructor(new[] { typeof(EventInfo), typeof(object) }), Expression.Constant(firstMember), target.Expression),
                        BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
                case MemberTypes.Field:
                case MemberTypes.Property:
                    return new DynamicMetaObject(
                        ResultHelper.EnsureObjectResult(
                            Expression.MakeMemberAccess(Expression.Convert(target.Expression, firstMember.DeclaringType), firstMember)),
                        BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
                default:
                    throw new LuRuntimeException("Unsupported member type " + memberType);
            }
        }
    }
}