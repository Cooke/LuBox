using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LuBox.Compiler;

namespace LuBox.Runtime
{
    internal class NuPropertyMemberBinder : InvokeMemberBinder
    {
        public NuPropertyMemberBinder(string name, bool ignoreCase, CallInfo callInfo)
            : base(name, ignoreCase, callInfo)
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
            var property = target.LimitType.GetProperty(Name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);

            return
                new DynamicMetaObject(
                    RuntimeHelpers.EnsureObjectResult(
                        Expression.Property(Expression.Convert(target.Expression, target.LimitType), property)),
                    restrictions);
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

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}