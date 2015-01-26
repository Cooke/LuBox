using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LuBox.Runtime
{
    internal class InternalEnvironment
    {
        private readonly IDictionary<object, object> _env;

        public InternalEnvironment(IDictionary<object, object> env)
        {
            _env = env;
        }

        public object Get(string key)
        {
            return _env[key];
        }

        public object Set(string key, object value)
        {
            return _env[key] = value;
        }
    }

    internal class Scope
    {
        private readonly ParameterExpression _globalParameter;

        public Scope(ParameterExpression globalParameter)
        {
            _globalParameter = globalParameter;
        }

        public ScopeVariable GetOrCreate(string key)
        {
            return new ScopeVariable(
                Expression.Call(_globalParameter, typeof(InternalEnvironment).GetMethod("Get"), Expression.Constant(key)),
                x => Expression.Call(_globalParameter, typeof(InternalEnvironment).GetMethod("Set"), Expression.Constant(key), x));
        }
    }

    internal class ScopeVariable
    {
        private readonly MethodCallExpression _getExpression;
        private readonly Func<Expression, Expression> _setExpressionGenerator;

        public ScopeVariable(MethodCallExpression getExpression, Func<Expression, Expression> setExpressionGenerator)
        {
            _getExpression = getExpression;
            _setExpressionGenerator = setExpressionGenerator;
        }

        public MethodCallExpression GetExpression
        {
            get { return _getExpression; }
        }

        public Func<Expression, Expression> SetExpressionGenerator
        {
            get { return _setExpressionGenerator; }
        }
    }
}