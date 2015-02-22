using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace LuBox.Runtime
{
    internal class LuSetMemberBinder : SetMemberBinder
    {
        public LuSetMemberBinder(string name) : base(name, false)
        {
        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue || !value.HasValue)
            {
                return Defer(target, value);
            }

            const BindingFlags flags = BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public;
            var members = target.LimitType.GetMember(Name, flags);
            if (members.Length != 1)
            {
                throw new NotSupportedException();
            }

            var member = members[0];

            Expression val = null;
            if (member.MemberType == MemberTypes.Property)
            {
                val = Expression.Convert(value.Expression, ((PropertyInfo) member).PropertyType);
            }
            else if (member.MemberType == MemberTypes.Field)
            {
                val = Expression.Convert(value.Expression,
                    ((FieldInfo) member).FieldType);
            }

            return new DynamicMetaObject(                
                ResultHelper.EnsureObjectResult(
                    Expression.Assign(
                        Expression.MakeMemberAccess(Expression.Convert(target.Expression, member.DeclaringType), member), val)),
                BindingRestrictions.GetTypeRestriction(target.Expression,
                    target.LimitType));
        }
    }
}