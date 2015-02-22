namespace LuBox.Runtime
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class LuConvertBinder : ConvertBinder
    {
        public LuConvertBinder(Type type, bool @explicit)
            : base(type, @explicit)
        {
        }

        public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue)
            {
                return Defer(target);
            }

            BindingRestrictions rest = target.Value == null
                ? BindingRestrictions.GetInstanceRestriction(target.Expression, null)
                : BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType);

            return new DynamicMetaObject(Expression.Convert(target.Expression, Type), rest);
        }
    }
}