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
        private readonly Dictionary<string, ParameterExpression> _locals = new Dictionary<string, ParameterExpression>();
        private readonly List<ParameterExpression> _allLocals = new List<ParameterExpression>();

        public Scope(ParameterExpression globalParameter)
        {
            _globalParameter = globalParameter;
        }

        public IEnumerable<ParameterExpression> LocalParameterExpression
        {
            get { return _allLocals; }
        }

        public ScopeVariable GetOrCreate(string key)
        {
            if (_locals.ContainsKey(key))
            {
                return new ScopeVariable(_locals[key], x => Expression.Assign(_locals[key], x));
            }

            return new ScopeVariable(
                Expression.Call(_globalParameter, typeof(InternalEnvironment).GetMethod("Get"), Expression.Constant(key)),
                x => Expression.Call(_globalParameter, typeof(InternalEnvironment).GetMethod("Set"), Expression.Constant(key), x));
        }

        public ParameterExpression CreateLocal(string name)
        {
            var parameterExpression = Expression.Variable(typeof (object), name);
            _allLocals.Add(parameterExpression);
            _locals[name] = parameterExpression;
            return parameterExpression;
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