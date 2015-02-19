using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LuBox.Runtime
{
    internal class EnvironmentScope : IScope
    {
        private readonly ParameterExpression _environmentParameter;

        public EnvironmentScope(ParameterExpression environmentParameter)
        {
            _environmentParameter = environmentParameter;
        }

        public Expression Set(string key, Expression exp)
        {
            // TODO add support for _ENV
            return Expression.Call(_environmentParameter, LuTable.SetMethodInfo, Expression.Constant(key), Expression.Convert(exp, typeof(object)));
        }

        public Expression Get(string key)
        {
            // TODO add support for _ENV
            return Expression.Call(_environmentParameter, LuTable.GetMethodInfo, Expression.Constant(key));
        }

        public IScope Parent { get { return null; } }

        public IEnumerable<ParameterExpression> Locals { get { return null; } }

        public ParameterExpression CreateLocal(string name)
        {
            throw new System.NotImplementedException();
        }

        public ParameterExpression CreateLocal(string name, Type type)
        {
            throw new NotImplementedException();
        }
    }
}