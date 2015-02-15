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

            return new DynamicMetaObject(Expression.Convert(target.Expression, Type), BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
        }
    }
}