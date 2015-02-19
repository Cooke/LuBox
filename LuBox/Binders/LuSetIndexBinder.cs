using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace LuBox.Runtime
{
    internal class LuSetIndexBinder : SetIndexBinder
    {
        public LuSetIndexBinder(CallInfo callInfo) : base(callInfo)
        {
        }

        public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value,
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

            // TODO add support for more types of arrays
            if (target.LimitType != typeof (int[]))
            {
                throw new LuRuntimeException("Only arrays of type int[] are allowed");
            }
            
            var setMethodIfo = target.LimitType.GetMethod("SetValue", new[] { typeof(int) });

            var indexExpression = Expression.Call(Expression.Convert(target.Expression, target.LimitType), setMethodIfo, indexes[0].Expression, value.Expression);
            return new DynamicMetaObject(indexExpression, BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
        }
    }
}