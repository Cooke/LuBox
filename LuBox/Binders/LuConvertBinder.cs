namespace LuBox.Runtime
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class LuConvertBinder : ConvertBinder
    {
        public LuConvertBinder(Type type)
            : base(type, false)
        {
        }

        public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue)
            {
                return Defer(target);
            }

            var rest = RestrictionHelper.GetTypeOrNullRestriction(target);

            return new DynamicMetaObject(Expression.Convert(target.Expression, Type), rest);
        }
    }
}