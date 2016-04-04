using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using Antlr4.Runtime;
using LuBox.Binders;
using LuBox.Compilation;
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
        private readonly BinderProvider _binderProvider;

        public LuScriptEngine() : this(new Type[0])
        {
        }

        public LuScriptEngine(params Type[] ext) : this((IEnumerable<Type>)ext)
        {
        }

        public LuScriptEngine(IEnumerable<Type> extensionMethodTyps)
        {
            _defaultEnvironment = CreateStandardEnvironment();
            _binderProvider = new BinderProvider(extensionMethodTyps.ToArray());
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

        public object Evaluate(string expression)
        {
            return CompileExpressionInternal<object>(expression)(_defaultEnvironment);
        }

        public T Evaluate<T>(string expression)
        {
            return CompileExpression<T>(expression)(_defaultEnvironment);
        }

        public T Evaluate<T>(string expression, LuTable environment)
        {
            return CompileExpression<T>(expression)(environment);
        }

        public void Execute(string code)
        {
            Compile(code)(_defaultEnvironment);
        }

        public void Execute(string code, LuTable environment)
        {
            Compile(code)(environment);
        }

        public Func<T> CompileExpressionBind<T>(string code)
        {
            return CompileExpressionBind<T>(code, _defaultEnvironment);
        }

        public Func<T> CompileExpressionBind<T>(string code, LuTable environment)
        {
            var action = CompileExpression<T>(code);
            return () => action(environment);
        }

        public Func<LuTable, T> CompileExpression<T>(string expression)
        {
            return CompileExpressionInternal<T>(expression);
        }

        private Func<LuTable, T> CompileExpressionInternal<T>(string expression)
        {
            var lexer = new NuLexer(new AntlrInputStream(expression));
            var parser = new NuParser(new CommonTokenStream(lexer));
            var globalScope = new EnvironmentScope(_environmentParameter);
            var visitor = new Visitor(globalScope, _binderProvider);

            var parserErrorListener = new MemoryParserErrorListener();
            parser.RemoveErrorListeners();
            lexer.RemoveErrorListeners();
            parser.RemoveParseListeners();

            parser.AddErrorListener(parserErrorListener);

            try
            {
                Expression body = Expression.Convert(visitor.VisitExp(parser.exp()), typeof(object));
                var innerLambda = (Func<LuTable, object>) Expression.Lambda(body, _environmentParameter).Compile();
                return x =>
                {
                    var result = innerLambda(x);
                    if (result is IConvertible)
                    {
                        return (T) Convert.ChangeType(result, typeof (T));
                    }
                    else if (result is IDynamicMetaObjectProvider)
                    {
                        dynamic dynResult = result;
                        return (T) dynResult;
                    }
                    else
                    {
                        return (T) result;
                    }
                };
            }
            catch (Exception)
            {
                throw new LuCompileException(parserErrorListener.Messages);
            }
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

            var parserErrorListener = new MemoryParserErrorListener();
            parser.AddErrorListener(parserErrorListener);

            try
            {
                Expression content = visitor.Visit(parser.chunk());
                return Expression.Lambda<Action<LuTable>>(content, _environmentParameter).Compile();
            }
            catch (Exception)
            {
                throw new LuCompileException(parserErrorListener.Messages);
            }
        }

        private class MemoryParserErrorListener : IAntlrErrorListener<IToken>
        {
            private readonly List<LuCompileMessage> messages = new List<LuCompileMessage>();

            public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg,
                RecognitionException e)
            {
                messages.Add(new LuCompileMessage(line, charPositionInLine, msg));
            }

            public IEnumerable<LuCompileMessage> Messages
            {
                get { return messages; }
            }
        }
    }
}
