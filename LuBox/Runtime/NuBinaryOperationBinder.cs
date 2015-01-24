using System.Dynamic;
using System.Linq.Expressions;

namespace LuBox.Runtime
{
    internal class NuBinaryOperationBinder : BinaryOperationBinder
    {
        public NuBinaryOperationBinder(ExpressionType operation) : base(operation)
        {
        }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg,
            DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue || !arg.HasValue)
            {
                return Defer(target, arg);
            }

            var restrictions = target.Restrictions.Merge(arg.Restrictions)
                .Merge(BindingRestrictions.GetTypeRestriction(
                    target.Expression, target.LimitType))
                .Merge(BindingRestrictions.GetTypeRestriction(
                    arg.Expression, arg.LimitType));

            if (target.LimitType != arg.LimitType || Operation == ExpressionType.Divide)
            {
                return
                    new DynamicMetaObject(
                        Expression.Convert(
                            Expression.MakeBinary(Operation, Expression.Convert(target.Expression, typeof (double)),
                                Expression.Convert(arg.Expression, typeof (double))), typeof(object)), restrictions);
            }
            else
            {
                return
                    new DynamicMetaObject(
                        Expression.Convert(
                            Expression.MakeBinary(Operation, Expression.Convert(target.Expression, target.LimitType),
                                Expression.Convert(arg.Expression, arg.LimitType)), typeof (object)), restrictions);
            }
        }
    }
}