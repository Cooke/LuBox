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
        private readonly ParameterExpression _globalParameter = Expression.Variable(typeof(InternalEnvironment));

        public T Evaluate<T>(string expression)
        {
            return Evaluate<T>(expression, new LuEnvironment());
        }

        public T Evaluate<T>(string expression, LuEnvironment environment)
        {
            var lexer = new NuLexer(new AntlrInputStream(expression));
            var parser = new NuParser(new CommonTokenStream(lexer));
            var globalScope = new GlobalScope(_globalParameter);
            var visitor = new Visitor(globalScope);

            Expression content = visitor.VisitExp(parser.exp());

            object foo = Expression.Lambda(content, _globalParameter).Compile().DynamicInvoke(new InternalEnvironment(environment));

            return (T)Convert.ChangeType(foo, typeof(T));
        }

        public object Evaluate(string expression)
        {
            return Evaluate(expression, new LuEnvironment());
        }

        public object Evaluate(string expression, LuEnvironment environment)
        {
            return Evaluate<object>(expression, environment);
        }

        public void Execute(string code, LuEnvironment environment)
        {
            Compile(code)(environment);
        }

        public Action<LuEnvironment> Compile(string code)
        {
            var lexer = new NuLexer(new AntlrInputStream(code));
            var parser = new NuParser(new CommonTokenStream(lexer));
            var globalScope = new GlobalScope(_globalParameter);
            var visitor = new Visitor(globalScope);

            Expression content = visitor.Visit(parser.chunk());

            var compiled = Expression.Lambda<Action<InternalEnvironment>>(content, _globalParameter).Compile();
            return env => compiled(new InternalEnvironment(env));
        }
    }
}
