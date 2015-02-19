using System.Dynamic;
using System.Linq.Expressions;

namespace LuBox.Runtime
{
    internal class LuBoolConvertBinder : ConvertBinder
    {
        public LuBoolConvertBinder()
            : base(typeof(bool), false)
        {
        }

        public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue)
            {
                return Defer(target);
            }

            if (target.LimitType == typeof (bool))
            {
                return new DynamicMetaObject(
                    Expression.Convert(target.Expression, typeof(bool)),
                    BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
            }
            
            BindingRestrictions restriction = BindingRestrictions.GetExpressionRestriction(
                Expression.Not(Expression.TypeIs(target.Expression, typeof (bool))));
            return
                new DynamicMetaObject(
                    Expression.ReferenceNotEqual(Expression.Convert(target.Expression, typeof(object)), Expression.Constant(null, typeof (object))),
                    restriction);
        }
    }
}