using System.Linq.Expressions;

namespace NuBox.Compiler
{
    internal class RuntimeHelpers
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