using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LuBox.Runtime
{
    internal interface IScope
    {
        Expression Set(string key, Expression exp);
        Expression Get(string key);
        IScope Parent { get; }
        IEnumerable<ParameterExpression> Locals { get; }
        ParameterExpression CreateLocal(string name);
        ParameterExpression CreateLocal(string name, Type type);
    }

    internal class Scope : IScope
    {
        private readonly IScope _scope;
        private readonly Dictionary<string, ParameterExpression> _localsByName = new Dictionary<string, ParameterExpression>();
        private readonly List<ParameterExpression> _locals = new List<ParameterExpression>();

        public Scope(IScope scope)
        {
            _scope = scope;
        }

        public IScope Parent
        {
            get { return _scope; }
        }

        public IEnumerable<ParameterExpression> Locals
        {
            get { return _locals; }
        }

        public Expression Set(string key, Expression exp)
        {
            if (_localsByName.ContainsKey(key))
            {
                return Expression.Assign(_localsByName[key], exp);
            }

            return _scope.Set(key, exp);
        }

        public Expression Get(string key)
        {
            if (_localsByName.ContainsKey(key))
            {
                return _localsByName[key];
            }

            return _scope.Get(key);
        }

        public ParameterExpression CreateLocal(string name)
        {
            return CreateLocal(name, typeof (object));
        }

        public ParameterExpression CreateLocal(string name, Type type)
        {
            var parameterExpression = Expression.Variable(type, name);
            _locals.Add(parameterExpression);
            _localsByName[name] = parameterExpression;
            return parameterExpression;
        }
    }
}