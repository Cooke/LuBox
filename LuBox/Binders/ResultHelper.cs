using System.Linq.Expressions;

namespace LuBox.Runtime
{
    internal class ResultHelper
    {
        public static Expression EnsureObjectResult(Expression expr)
        {
            if (!expr.Type.IsValueType)
            {
                return expr;
            }

            if (expr.Type == typeof (void))
            {
                return Expression.Block(
                    expr, Expression.Default(typeof (object)));
            }
            
            return Expression.Convert(expr, typeof(object));
        }
    }
}