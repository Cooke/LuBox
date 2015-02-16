using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace LuBox.Runtime
{
    internal class LuGetIndexBinder : GetIndexBinder
    {
        public LuGetIndexBinder(CallInfo callInfo) : base(callInfo)
        {
        }

        public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes,
            DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue || !indexes.All(x => x.HasValue))
            {
                return Defer(target, indexes);
            }

            if (indexes.Length != 1)
            {
                throw new LuRuntimeException("Only one indexer is allowed");
            }

            var getMethodIfo = target.LimitType.GetMethod("GetValue", new[] {typeof (int)});

            var indexExpression = Expression.Call(Expression.Convert(target.Expression, target.LimitType), getMethodIfo, indexes.First().Expression);
            return new DynamicMetaObject(indexExpression, BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
        }
    }
}