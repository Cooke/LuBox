using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace LuBox.Runtime
{
    internal class NuGetMemberBinder : GetMemberBinder
    {
        public NuGetMemberBinder(string name) : base(name, false)
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

            return new DynamicMetaObject(
                RuntimeHelpers.EnsureObjectResult(
                    Expression.MakeMemberAccess(Expression.Convert(target.Expression, member.DeclaringType), member)),
                BindingRestrictions.GetTypeRestriction(target.Expression,
                    target.LimitType));
        }
    }
}