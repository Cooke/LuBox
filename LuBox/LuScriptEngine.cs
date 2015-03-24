using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Antlr4.Runtime;
using LuBox.Binders;
using LuBox.Compiler;
using LuBox.Library;
using LuBox.Parser;
using LuBox.Runtime;

namespace LuBox
{
    public class LuScriptEngine
    {
        private readonly ParameterExpression _environmentParameter = Expression.Variable(typeof(LuTable));
        private readonly LuTable _defaultEnvironment;
        private readonly BinderProvider _binderProvider = new BinderProvider();

        public LuScriptEngine()
        {
            _defaultEnvironment = CreateStandardEnvironment();
        }

        public LuTable DefaultEnvironment
        {
            get { return _defaultEnvironment; }
        }

        public LuTable CreateStandardEnvironment()
        {
            var standardEnvironment = LuBasicLibrary.Create();
            standardEnvironment.SetField("math", LuMathLibrary.Create());
            return standardEnvironment;
        }

        public LuTable CreateEmptyEnvironment()
        {
            return new LuTable();
        }

        public T Evaluate<T>(string expression)
        {
            return Evaluate<T>(expression, _defaultEnvironment);
        }

        public T Evaluate<T>(string expression, LuTable environment)
        {
            return (T)Convert.ChangeType(Evaluate(expression, environment), typeof (T));
        }

        public object Evaluate(string expression)
        {
            return Evaluate(expression, _defaultEnvironment);
        }

        public object Evaluate(string expression, LuTable environment)
        {
            var lexer = new NuLexer(new AntlrInputStream(expression));
            var parser = new NuParser(new CommonTokenStream(lexer));
            var globalScope = new EnvironmentScope(_environmentParameter);
            var visitor = new Visitor(globalScope, _binderProvider);

            Expression content = visitor.VisitExp(parser.exp());

            object foo = Expression.Lambda(content, _environmentParameter).Compile().DynamicInvoke(environment);
            return foo;
        }

        public void Execute(string code)
        {
            Compile(code)(_defaultEnvironment);
        }


        public void Execute(string code, LuTable environment)
        {
            Compile(code)(environment);
        }

        public Action CompileBind(string code)
        {
            return CompileBind(code, _defaultEnvironment);
        }

        public Action CompileBind(string code, LuTable environment)
        {
            var action = Compile(code);
            return () => action(environment);
        }

        public Action<LuTable> Compile(string code)
        {
            var lexer = new NuLexer(new AntlrInputStream(code));
            var parser = new NuParser(new CommonTokenStream(lexer));
            var globalScope = new EnvironmentScope(_environmentParameter);
            var visitor = new Visitor(globalScope, _binderProvider);

            Expression content = visitor.Visit(parser.chunk());

            return Expression.Lambda<Action<LuTable>>(content, _environmentParameter).Compile();
        }
    }
}
