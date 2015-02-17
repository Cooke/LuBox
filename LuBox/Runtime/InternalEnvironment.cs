using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Antlr4.Runtime.Tree;

namespace LuBox.Runtime
{
    internal class InternalEnvironment
    {
        private readonly LuTable _env;

        public InternalEnvironment(LuTable env)
        {
            _env = env;
        }

        public object Get(string key)
        {
            return _env.GetField(key);
        }

        public object Set(string key, object value)
        {
            _env.SetField(key, value);
            return value;
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