using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Antlr4.Runtime.Tree;

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
            return _env.ContainsKey(key) ? _env[key] : null;
        }

        public object Set(string key, object value)
        {
            return _env[key] = value;
        }
    }

    internal class ScopeVariable
    {
        private readonly Expression _getExpression;
        private readonly Func<Expression, Expression> _setExpressionGenerator;

        public ScopeVariable(Expression getExpression, Func<Expression, Expression> setExpressionGenerator)
        {
            _getExpression = getExpression;
            _setExpressionGenerator = setExpressionGenerator;
        }

        public Expression GetExpression
        {
            get { return _getExpression; }
        }

        public Func<Expression, Expression> SetExpressionGenerator
        {
            get { return _setExpressionGenerator; }
        }
    }
}