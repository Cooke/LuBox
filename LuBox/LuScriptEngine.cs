using System;
using System.Linq.Expressions;
using Antlr4.Runtime;
using LuBox.Compiler;
using LuBox.Parser;
using LuBox.Runtime;

namespace LuBox
{
    public class LuScriptEngine
    {
        private readonly ParameterExpression _environmentParameter = Expression.Variable(typeof(LuTable));

        public T Evaluate<T>(string expression)
        {
            return Evaluate<T>(expression, new LuTable());
        }

        public T Evaluate<T>(string expression, LuTable environment)
        {
            var lexer = new NuLexer(new AntlrInputStream(expression));
            var parser = new NuParser(new CommonTokenStream(lexer));
            var globalScope = new EnvironmentScope(_environmentParameter);
            var visitor = new Visitor(globalScope);

            Expression content = visitor.VisitExp(parser.exp());

            object foo = Expression.Lambda(content, _environmentParameter).Compile().DynamicInvoke(environment);

            return (T)Convert.ChangeType(foo, typeof(T));
        }

        public object Evaluate(string expression)
        {
            return Evaluate(expression, new LuTable());
        }

        public object Evaluate(string expression, LuTable environment)
        {
            return Evaluate<object>(expression, environment);
        }

        public void Execute(string code, LuTable environment)
        {
            Compile(code)(environment);
        }

        public Action<LuTable> Compile(string code)
        {
            var lexer = new NuLexer(new AntlrInputStream(code));
            var parser = new NuParser(new CommonTokenStream(lexer));
            var globalScope = new EnvironmentScope(_environmentParameter);
            var visitor = new Visitor(globalScope);

            Expression content = visitor.Visit(parser.chunk());

            return Expression.Lambda<Action<LuTable>>(content, _environmentParameter).Compile();
        }
    }
}
