using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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

            var instanceExpression = Expression.Convert(target.Expression, target.LimitType);

            var indexerInfo = target.LimitType.GetProperty("Item");
            if (indexerInfo != null)
            {
                var indexAccessExpression = Expression.MakeIndex(instanceExpression, indexerInfo,
                    new[]
                    {
                        Expression.Dynamic(new LuConvertBinder(indexes[0].LimitType), indexes[0].LimitType,
                            indexes[0].Expression)
                    });
                var assignExpression = Expression.Assign(indexAccessExpression, Expression.Dynamic(new LuConvertBinder(indexerInfo.PropertyType), indexerInfo.PropertyType, value.Expression));
                var restrictions = RestrictionHelper.GetTypeRestrictions(target, new[] {indexes[0], value});
                return new DynamicMetaObject(assignExpression, restrictions);
            }
            else if (target.LimitType == typeof (int[]))
            {
                var setMethodIfo = target.LimitType.GetMethod("SetValue", new[] {typeof (int)});

                var indexExpression = Expression.Call(instanceExpression,
                    setMethodIfo, indexes[0].Expression, value.Expression);
                return new DynamicMetaObject(indexExpression,
                    BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType));
            }
            else
            {
                throw new LuRuntimeException("Only custom indexer and arrays of int[] are allowed");
            }
        }
    }
}