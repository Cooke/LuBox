using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace LuBox.Runtime
{
    internal static class RestrictionHelper
    {
        public static BindingRestrictions GetTypeOrNullRestriction(DynamicMetaObject meta)
        {
            return meta.Value == null
                ? BindingRestrictions.GetInstanceRestriction(meta.Expression, null)
                : BindingRestrictions.GetTypeRestriction(meta.Expression, meta.LimitType);
        }

        public static BindingRestrictions CombineRestrictions(params BindingRestrictions[] restrictions)
        {
            var resultRestriction = BindingRestrictions.Empty;
            foreach (var restriction in restrictions)
            {
                resultRestriction = resultRestriction.Merge(restriction);
            }

            return resultRestriction;
        }

        public static BindingRestrictions GetTypeRestrictions(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            var targetRestriction = BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType);
            var argumentRestrictions = args.Select(x => x.Value == null ? BindingRestrictions.GetInstanceRestriction(x.Expression, null) : BindingRestrictions.GetTypeRestriction(
                x.Expression, x.LimitType));

            var resultDescription = targetRestriction;
            foreach (var result in argumentRestrictions)
            {
                resultDescription = resultDescription.Merge(result);
            }

            return resultDescription;
        }
    }
}