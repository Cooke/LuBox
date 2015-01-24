using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Antlr4.Runtime;
using LuBox.Compiler;
using LuBox.Parser;
using LuBox.Runtime;

namespace LuBox
{
    public class NuEngine
    {
        private readonly IDictionary<object, object> _globals = new Dictionary<object, object>();
        private readonly ParameterExpression _globalParameter = Expression.Variable(typeof(Scope));
        private readonly DynamicDictionaryWrapper _globalsDynamic;

        public NuEngine()
        {
            _globalsDynamic = new DynamicDictionaryWrapper(_globals);
        }

        public IDictionary<object, object> GlobalDictionary
        {
            get { return _globals; }
        }

        public dynamic Globals
        {
            get { return _globalsDynamic; }
        }

        public T Evaluate<T>(string expression)
        {
            var lexer = new NuLexer(new AntlrInputStream(expression));
            var parser = new NuParser(new CommonTokenStream(lexer));
            var visitor = new Visitor(_globalParameter);

            Expression visit = visitor.Visit(parser.exp());
            object foo = Expression.Lambda(visit, _globalParameter).Compile().DynamicInvoke(new Scope(_globals));

            return (T)Convert.ChangeType(foo, typeof(T));
        }

        public object Evaluate(string expression)
        {
            return Evaluate<object>(expression);
        }

        public void Execute(string code)
        {
            var lexer = new NuLexer(new AntlrInputStream(code));
            var parser = new NuParser(new CommonTokenStream(lexer));
            var visitor = new Visitor(_globalParameter);

            Expression visit = visitor.Visit(parser.chunk());
            Expression.Lambda<Action<Scope>>(visit, _globalParameter).Compile()(new Scope(_globals));
        }

        public void SetGlobal(object key, object value)
        {
            _globals[key] = value;
        }

        public object GetGlobal(object key)
        {
            return _globals[key];
        }

        public void ClearGlobals()
        {
            _globals.Clear();
        }
    }
}
