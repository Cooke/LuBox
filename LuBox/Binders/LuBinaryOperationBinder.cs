using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

namespace LuBox.Runtime
{
    internal class LuBinaryOperationBinder : BinaryOperationBinder
    {
        public LuBinaryOperationBinder(ExpressionType operation)
            : base(operation)
        {
        }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject left, DynamicMetaObject right,
            DynamicMetaObject errorSuggestion)
        {
            if (!left.HasValue || !right.HasValue)
            {
                return Defer(left, right);
            }

            var leftRestriction = RestrictionHelper.GetTypeOrNullRestriction(left);
            var rigRestriction = RestrictionHelper.GetTypeOrNullRestriction(right);
            var restrictions = left.Restrictions.Merge(right.Restrictions)
                .Merge(leftRestriction)
                .Merge(rigRestriction);

            if (Operation == ExpressionType.Equal || Operation == ExpressionType.NotEqual)
            {
                return EqualOperation(left, right, restrictions);
            }
            else if (left.LimitType != right.LimitType || Operation == ExpressionType.Divide)
            {
                var leftExpression = UnBoxIfNeeded(left);
                var rightExpression = UnBoxIfNeeded(right);

                return
                    new DynamicMetaObject(
                        Expression.Convert(
                            Expression.MakeBinary(Operation, Expression.Convert(leftExpression, typeof(double)),
                                Expression.Convert(rightExpression, typeof(double))), typeof(object)), restrictions);
            }
            else
            {
                return
                    new DynamicMetaObject(
                        Expression.Convert(
                            Expression.MakeBinary(Operation, Expression.Convert(left.Expression, left.LimitType),
                                Expression.Convert(right.Expression, right.LimitType)), typeof(object)), restrictions);
            }
        }

        private DynamicMetaObject EqualOperation(DynamicMetaObject left, DynamicMetaObject right,
            BindingRestrictions restrictions)
        {
            var bothAreNumberTypes = (SignatureHelper.IsNumberType(left.LimitType) &&
                                      SignatureHelper.IsNumberType(right.LimitType));
            if (left.LimitType != right.LimitType && !bothAreNumberTypes)
            {
                return new DynamicMetaObject(Expression.Constant(Operation != ExpressionType.Equal, typeof(object)), restrictions);
            }

            if (left.LimitType != right.LimitType)
            {
                return new DynamicMetaObject(
                    Expression.Convert(
                        Expression.MakeBinary(Operation,
                            Expression.Convert(left.Expression, typeof (double)),
                            Expression.Convert(right.Expression, typeof (double))), typeof (object)), restrictions);
            }

            return new DynamicMetaObject(
                    Expression.Convert(
                        Expression.MakeBinary(Operation,
                            Expression.Convert(left.Expression, left.LimitType),
                            Expression.Convert(right.Expression, right.LimitType)), typeof(object)), restrictions);
        }

        private static Expression UnBoxIfNeeded(DynamicMetaObject left)
        {
            Expression leftExpression = left.Expression;
            if (left.Expression.Type == typeof(object) && left.LimitType != typeof(object))
            {
                leftExpression = Expression.Unbox(left.Expression, left.LimitType);
            }

            return leftExpression;
        }
    }
}