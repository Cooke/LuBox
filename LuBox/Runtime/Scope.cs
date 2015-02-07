using System.Collections.Generic;
using System.Linq.Expressions;

namespace LuBox.Runtime
{
    internal interface IScope
    {
        Expression Set(string key, Expression exp);
        Expression Get(string key);
        IScope Parent { get; }
        IEnumerable<ParameterExpression> LocalParameterExpression { get; }
        ParameterExpression CreateLocal(string name);
    }

    internal class GlobalScope : IScope
    {
        private readonly ParameterExpression _globalParameter;

        public GlobalScope(ParameterExpression globalParameter)
        {
            _globalParameter = globalParameter;
        }

        public Expression Set(string key, Expression exp)
        {
            return Expression.Call(_globalParameter, typeof(InternalEnvironment).GetMethod("Set"), Expression.Constant(key), Expression.Convert(exp, typeof(object)));
        }

        public Expression Get(string key)
        {
            return Expression.Call(_globalParameter, typeof(InternalEnvironment).GetMethod("Get"), Expression.Constant(key));
        }

        public IScope Parent { get { return null; } }

        public IEnumerable<ParameterExpression> LocalParameterExpression { get { return null; } }

        public ParameterExpression CreateLocal(string name)
        {
            throw new System.NotImplementedException();
        }
    }

    internal class Scope : IScope
    {
        private readonly IScope _scope;
        private readonly Dictionary<string, ParameterExpression> _locals = new Dictionary<string, ParameterExpression>();
        private readonly List<ParameterExpression> _allLocals = new List<ParameterExpression>();

        public Scope(IScope scope)
        {
            _scope = scope;
        }

        public IScope Parent
        {
            get { return _scope; }
        }

        public IEnumerable<ParameterExpression> LocalParameterExpression
        {
            get { return _allLocals; }
        }

        public Expression Set(string key, Expression exp)
        {
            if (_locals.ContainsKey(key))
            {
                return Expression.Assign(_locals[key], exp);
            }

            return _scope.Set(key, exp);
        }

        public Expression Get(string key)
        {
            if (_locals.ContainsKey(key))
            {
                return _locals[key];
            }

            return _scope.Get(key);
        }

        public ParameterExpression CreateLocal(string name)
        {
            var parameterExpression = Expression.Variable(typeof (object), name);
            _allLocals.Add(parameterExpression);
            _locals[name] = parameterExpression;
            return parameterExpression;
        }
    }
}