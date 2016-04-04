using System;
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

            var instanceExpression = Expression.Convert(target.Expression, target.LimitType);

            if (target.Value == null)
            {
                return new DynamicMetaObject(Expression.Constant(null), BindingRestrictions.GetInstanceRestriction(target.Expression, null));
            }

            var indexerInfo = target.LimitType.GetProperty("Item");
            if (indexerInfo != null)
            {
                var indexAccessExpression = Expression.MakeIndex(instanceExpression, indexerInfo, new[] { indexes[0].Expression });
                return new DynamicMetaObject(indexAccessExpression, BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
            }
            else if (target.LimitType == typeof(int[]))
            {
                var getMethodIfo = target.LimitType.GetMethod("GetValue", new[] { typeof(int) });
                var indexExpression = Expression.Call(instanceExpression, getMethodIfo, indexes.First().Expression);
                return new DynamicMetaObject(indexExpression, BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
            }
            else
            {
                throw new LuRuntimeException("Only custom indexer and arrays of int[] are allowed");
            }
        }
    }
}