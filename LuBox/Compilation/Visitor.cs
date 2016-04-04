using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using LuBox.Binders;
using LuBox.Compiler;
using LuBox.Parser;
using LuBox.Runtime;

namespace LuBox.Compilation
{
    internal class Visitor : NuBaseVisitor<Expression>
    {
        private IScope _scope;
        private readonly BinderProvider _binderProvider;
        private readonly Stack<LabelTarget> _returnTargets = new Stack<LabelTarget>(); 

        public Visitor(IScope scope, BinderProvider binderProvider)
        {
            _scope = scope;
            _binderProvider = binderProvider;
        }

        public override Expression VisitNumber(NuParser.NumberContext context)
        {
            if (context.INT() != null)
            {
                return Expression.Constant(int.Parse(context.INT().GetText()));
            }
            else if (context.FLOAT() != null)
            {
                return Expression.Constant(double.Parse(context.FLOAT().Symbol.Text, System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
            else 
            {
                throw new NotSupportedException("Number type is not supported");
            }
        }

        public override Expression VisitString(NuParser.StringContext context)
        {
            return Expression.Constant(context.GetText().TrimStart('"', '\'', '[').TrimEnd('"', '\'', ']'));
        }

        public override Expression VisitPrefixexp(NuParser.PrefixexpContext context)
        {
            Expression leftExp;
            if (context.varOrExp().exp() != null)
            {
                leftExp = VisitExp(context.varOrExp().exp());
                return ProcessNameAndArgs(context.nameAndArgs())(leftExp);
            }
            else 
            {
                var rightEval = ProcessNameAndArgs(context.nameAndArgs());
                return VisitVar(context.varOrExp().var(), rightEval);
            }
        }

        private Func<Expression, Expression> ProcessNameAndArgs(IEnumerable<NuParser.NameAndArgsContext> nameAndArgsList)
        {
            if (!nameAndArgsList.Any())
            {
                return x => x;
            }

            return leftExp =>
            {
                var rightEval = ProcessNameAndArgs(nameAndArgsList.Skip(1));
                var nameAndArgsContext = nameAndArgsList.First();
                var thisExp = DoProcessNameAndArgs(nameAndArgsContext, leftExp);
                return rightEval(thisExp);
            };
        }

        private Expression DoProcessNameAndArgs(NuParser.NameAndArgsContext nameAndArgsContext, Expression leftExp)
        {
            string memberName = nameAndArgsContext.NAME() != null ? nameAndArgsContext.NAME().GetText() : null;

            var leftTempExp = Expression.Variable(typeof (object), "___TEMP___" + Guid.NewGuid());

            IEnumerable<Expression> args = new[] { leftTempExp };
            if (nameAndArgsContext.args().explist() != null)
            {
                args = args.Concat(nameAndArgsContext.args().explist().exp().Select(VisitExp));
            }

            if (memberName != null)
            {
                var callInfo = new CallInfo(nameAndArgsContext.args().ChildCount);
                var binder = _binderProvider.GetInvokeMemberBinder(_scope.Get("__ExtensionMethodTypes__"), memberName, callInfo);
                var rightExp = Expression.Dynamic(binder, typeof(object), args);

                if (nameAndArgsContext.colonOrQuestionMarkColon().GetText() == "?:")
                {
                    var nullExp = Expression.Constant(null);
                    return Expression.Block(new[] {leftTempExp},
                        Expression.Assign(leftTempExp, leftExp),
                        Expression.Condition(Expression.ReferenceEqual(nullExp, leftTempExp), nullExp, rightExp));
                }
                else
                {
                    return Expression.Block(new[] { leftTempExp },
                        Expression.Assign(leftTempExp, leftExp),
                        rightExp);
                }
            }
            else
            {
                return Expression.Block(new[] { leftTempExp },
                        Expression.Assign(leftTempExp, leftExp),
                        Expression.Dynamic(new LuInvokeBinder(new CallInfo(args.Count())), typeof(object), args));
            }
        }

        public Func<Expression, Expression> ProcessVarSuffix(NuParser.VarSuffixContext suffix)
        {
            return leftExp =>
            {
                var rightEval = ProcessNameAndArgs(suffix.nameAndArgs());
                var thisExp = DoVisitVarSuffix(suffix, leftExp, x => x);
                return rightEval(thisExp);
            };

        }

        private Expression DoVisitVarSuffix(NuParser.VarSuffixContext suffix, Expression leftExp, Func<Expression, Expression> outerRightEval)
        {
            if (suffix.NAME() != null)
            {
                ParameterExpression leftTempExp = Expression.Variable(typeof(object), "___TEMP___" + Guid.NewGuid());
                var rightExp = outerRightEval(Expression.Dynamic(_binderProvider.GetGetMemberBinder(suffix.NAME().GetText()), typeof (object),
                    leftTempExp));

                if (suffix.dotOrQuestionMarkDot().GetText() == "?.")
                {
                    var nullExp = Expression.Constant(null);
                    
                    return Expression.Block(
                        new[] { leftTempExp },
                        Expression.Assign(leftTempExp, leftExp),
                        Expression.Condition(Expression.ReferenceEqual(nullExp, leftTempExp), nullExp, rightExp)
                        );
                }
                else
                {
                    return Expression.Block(
                        new[] { leftTempExp },
                        Expression.Assign(leftTempExp, leftExp),
                        rightExp);
                }
            }
            else if (suffix.exp() != null)
            {
                return outerRightEval(Expression.Dynamic(new LuGetIndexBinder(new CallInfo(1)),
                    typeof (object), leftExp, VisitExp(suffix.exp())));
            }
            else
            {
                throw new ParseCanceledException();
            }
        }

        public override Expression VisitVar(NuParser.VarContext context)
        {
            return VisitVar(context, null);
        }

        public Expression VisitVar(NuParser.VarContext context, Func<Expression, Expression> rightEval)
        {
            Expression left = _scope.Get(context.NAME().GetText());
            return ProcessVarSuffixes(context.varSuffix(), rightEval)(left);
        }

        private Func<Expression, Expression> ProcessVarSuffixes(IEnumerable<NuParser.VarSuffixContext> varSuffix, Func<Expression, Expression> outerRightEval)
        {
            if (!varSuffix.Any())
            {
                return outerRightEval ?? (x => x);
            }
            
            return leftExp =>
            {
                var rightEval = ProcessVarSuffixes(varSuffix.Skip(1), outerRightEval);
                var processNameAndArgs = ProcessNameAndArgs(varSuffix.First().nameAndArgs())(leftExp);
                var thisExp = DoVisitVarSuffix(varSuffix.First(), processNameAndArgs, rightEval);
                return thisExp;
            };
        }

        public override Expression VisitExp(NuParser.ExpContext context)
        {
            if (context.ChildCount == 3)
            {
                return VisitBinaryOperatorExp(context);
            }
            else if (context.ChildCount == 2)
            {
                return VisitUnaryOperatorExp(context);
            }
            else
            {
                return VisitSingleChildExp(context);
            }
        }

        private Expression VisitSingleChildExp(NuParser.ExpContext context)
        {
            var childTree = context.GetChild(0);

            var terminalNodeImpl = childTree as TerminalNodeImpl;
            if (terminalNodeImpl == null)
            {
                var ruleIndex = childTree.GetRuleIndex();
                switch (ruleIndex)
                {
                    case NuParser.RULE_prefixexp:
                        return VisitPrefixexp(context.prefixexp());
                    case NuParser.RULE_number:
                        return VisitNumber(context.number());
                    case NuParser.RULE_string:
                        return VisitString(context.@string());
                    case NuParser.RULE_tableconstructor:
                        return VisitTableconstructor(context.tableconstructor());
                    case NuParser.RULE_functiondef:
                        return VisitFunctiondef(context.functiondef());
                    case NuParser.RULE_lambda:
                        return VisitLambda(context.lambda());
                    default:
                        throw new ParseCanceledException("Invalid expression");
                }
            }
            else
            {
                return VisitTerminalExp(context);
            }
        }

        public override Expression VisitFunctiondef(NuParser.FunctiondefContext context)
        {
            return VisitFuncbody(context.funcbody());
        }

        public override Expression VisitTableconstructor(NuParser.TableconstructorContext context)
        {
            if (context.fieldlist() != null)
            {
                var fieldsListExpression = VisitFieldlist(context.fieldlist());
                return Expression.New(LuTable.ValuesConstructorInfo, fieldsListExpression);
            }
            else
            {
                return Expression.New(LuTable.EmptyConstructorInfo);
            }
        }

        public override Expression VisitFieldlist(NuParser.FieldlistContext context)
        {
            System.Reflection.MethodInfo addMethod = typeof(Dictionary<object, object>).GetMethod("Add");

            var elements = new List<ElementInit>();

            int count = 0;
            foreach (var fieldContext in context.field())
            {
                count++;
                if (fieldContext.exp().Count == 2)
                {
                    elements.Add(Expression.ElementInit(
                    addMethod,
                    Expression.Convert(VisitExp(fieldContext.exp(0)), typeof(object)),
                    Expression.Convert(VisitExp(fieldContext.exp(1)), typeof(object))));
                }
                else if (fieldContext.NAME() != null)
                {
                    elements.Add(Expression.ElementInit(
                    addMethod,
                    Expression.Constant(fieldContext.NAME().GetText(), typeof(object)),
                    Expression.Convert(VisitExp(fieldContext.exp(0)), typeof(object))));
                }
                else
                {
                    elements.Add(Expression.ElementInit(
                        addMethod,
                        Expression.Constant(count, typeof(object)),
                        Expression.Convert(VisitExp(fieldContext.exp(0)), typeof(object))));
                }
            }

            NewExpression newDictionaryExpression = Expression.New(typeof(Dictionary<object, object>));
            return Expression.ListInit(newDictionaryExpression, elements);
        }

        private static Expression VisitTerminalExp(NuParser.ExpContext context)
        {
            var text = context.GetText();
            if (bool.TrueString.Equals(text, StringComparison.InvariantCultureIgnoreCase))
            {
                return Expression.Constant(true);
            }
            else if (bool.FalseString.Equals(text, StringComparison.InvariantCultureIgnoreCase))
            {
                return Expression.Constant(false);
            }
            else if ("nil".Equals(text, StringComparison.InvariantCultureIgnoreCase))
            {
                return Expression.Constant(null, typeof (object));
            }

            throw new ParseCanceledException("Invalid expression");
        }

        private Expression VisitUnaryOperatorExp(NuParser.ExpContext context)
        {
            var unaryOperator = context.operatorUnary();
            var exp = context.exp(0);
            switch (unaryOperator.GetText())
            {
                case "-":
                    return Expression.Negate(VisitExp(exp));
                case "not":
                    return Expression.Not(Expression.Dynamic(new LuBoolConvertBinder(), typeof(bool), VisitExp(exp)));
                default:
                    throw new ParseCanceledException("Invalid expression");
            }
        }

        private Expression VisitBinaryOperatorExp(NuParser.ExpContext context)
        {
            Expression left = VisitExp(context.exp(0));
            IParseTree oper = context.GetChild(1);
            Expression right = VisitExp(context.exp(1));

            if (oper.GetText() == "and")
            {
                DynamicExpression test = Expression.Dynamic(new LuBoolConvertBinder(), typeof (bool), left);
                return Expression.Condition(test, Expression.Convert(right, typeof(object)), Expression.Convert(left, typeof(object)));
            }
            else if (oper.GetText() == "or")
            {
                DynamicExpression test = Expression.Dynamic(new LuBoolConvertBinder(), typeof(bool), left);
                return Expression.Condition(test, Expression.Convert(left, typeof(object)), Expression.Convert(right, typeof(object)));
            }
            else
            {
                ExpressionType operatorType = GetOperatorType(oper);
                return Expression.Dynamic(new LuBinaryOperationBinder(operatorType), typeof (object), left, right);
            }
        }

        private static ExpressionType GetOperatorType(IParseTree oper)
        {
            switch (oper.GetText())
            {
                case "+":
                    return ExpressionType.Add;
                case "-":
                    return ExpressionType.Subtract;
                case "/":
                    return ExpressionType.Divide;
                case "*":
                    return ExpressionType.Multiply;
                case ">":
                    return ExpressionType.GreaterThan;
                case ">=":
                    return ExpressionType.GreaterThanOrEqual;
                case "<":
                    return ExpressionType.LessThan;
                case "<=":
                    return ExpressionType.LessThanOrEqual;
                case "==":
                    return ExpressionType.Equal;
                case "~=":
                    return ExpressionType.NotEqual;
                default:
                    throw new ParseCanceledException("Invalid operator");
            }
        }

        public override Expression VisitLambda(NuParser.LambdaContext context)
        {
            _scope = new Scope(_scope);

            var parameters = new List<ParameterExpression>();
            if (context.lambdaArgs().namelist() != null)
            {
                foreach (var paraName in context.lambdaArgs().namelist().NAME().Select(x => x.GetText()))
                {
                    var parameterExpression = _scope.CreateLocal(paraName);
                    parameters.Add(parameterExpression);
                }
            }
            else
            {
                var parameterExpression = _scope.CreateLocal(context.lambdaArgs().NAME().GetText());
                parameters.Add(parameterExpression);
            }

            var visitExp = VisitExp(context.exp());

            _scope = _scope.Parent;

            return Expression.New(typeof(LuFunction).GetConstructor(new[] { typeof(Delegate) }),
                Expression.Lambda(visitExp, parameters));
            //return Expression.Lambda(block, parameters);
        }

        public override Expression VisitFuncbody(NuParser.FuncbodyContext context)
        {
            _scope = new Scope(_scope);

            var parameters = new List<ParameterExpression>();
            if (context.parlist() != null)
            {
                foreach (var paraName in context.parlist().namelist().NAME().Select(x => x.GetText()))
                {
                    var parameterExpression = _scope.CreateLocal(paraName);
                    parameters.Add(parameterExpression);
                }
            }

            var returnLabel = Expression.Label(typeof(object));
            _returnTargets.Push(returnLabel);
            var block = Expression.Block(new[] { VisitBlock(context.block()), Expression.Label(returnLabel, Expression.Constant(null, typeof(object))) });
            _scope = _scope.Parent;
            _returnTargets.Pop();

            return Expression.New(typeof (LuFunction).GetConstructor(new[] {typeof (Delegate)}),
                Expression.Lambda(block, parameters));
            //return Expression.Lambda(block, parameters);
        }

        public override Expression VisitStat(NuParser.StatContext context)
        {
            if (context.varlist() != null)
            {
                return VisitAssignments(context);
            }
            else if (context.ChildCount > 0 && context.GetChild(0).GetText() == "local")
            {
                return CreateLocalAssignmentExpression(context);
            }
            else if (context.funcname() != null)
            {
                var function = VisitFuncbody(context.funcbody());
                return _scope.Set(context.funcname().NAME(0).GetText(), function);
            }
            else if (context.ChildCount > 0 && context.GetChild(0).GetText() == "if")
            {
                return CreateIfExpression(context);
            }
            else if (context.ChildCount > 0 && context.GetChild(0).GetText() == "for")
            {
                if (context.NAME() != null)
                {
                    return VisitNumericFor(context);
                }
                else
                {
                    return VisitGenericFor(context);
                }
            }
            else if (context.functioncall() != null)
            {
                return VisitFunctioncall(context.functioncall());
            }
            else if (context.GetText() == ";")
            {
                return Expression.Empty();
            }
            else
            {
                throw new LuVisitorException();
            }
        }

        private Expression VisitGenericFor(NuParser.StatContext context)
        {
            _scope = new Scope(_scope);

            Expression fValueExp = VisitExp(context.explist().exp(0));
            Expression sValueExp = context.explist().ChildCount > 1 ? VisitExp(context.explist().exp(1)) : Expression.Constant(null, typeof(object));
            Expression vValueExp = context.explist().ChildCount > 2 ? VisitExp(context.explist().exp(2)) : Expression.Constant(null, typeof(object));

            ParameterExpression f = Expression.Variable(fValueExp.Type);
            ParameterExpression s = Expression.Variable(sValueExp.Type);
            ParameterExpression v = Expression.Variable(vValueExp.Type);

            ParameterExpression[] varList = context.namelist().NAME().Select(x => _scope.CreateLocal(x.GetText(), typeof(object))).ToArray();
            
            Expression innerBlock = VisitBlock(context.block(0));
            LabelTarget breakLabel = Expression.Label("break");
            BlockExpression forBlock = Expression.Block(
                new[] {f, s, v},
                Expression.Assign(f, fValueExp),
                Expression.Assign(s, sValueExp),
                Expression.Assign(v, vValueExp),
                Expression.Loop(
                    Expression.Block(
                        varList, 
                        AssignMultiRet(varList,
                            Expression.Dynamic(new LuInvokeBinder(new CallInfo(3)), typeof (object), f, s, v)),
                        Expression.Assign(v, varList.First()),
                        Expression.IfThenElse(
                            Expression.Dynamic(new LuBoolConvertBinder(), typeof (bool), varList.First()), 
                            innerBlock,
                            Expression.Break(breakLabel))),
                    breakLabel));

            _scope = _scope.Parent;
            return forBlock;
        }

        private Expression AssignMultiRet(IEnumerable<ParameterExpression> varList, Expression right)
        {
            var temp = Expression.Variable(typeof (object));
            return Expression.Block(
                new[] { temp },
                Expression.Assign(temp, right),
                Expression.IfThenElse(
                Expression.TypeIs(temp, typeof (LuMultiResult)),
                Expression.Block(GetVarListAssignments(varList, Expression.Convert(temp, typeof(LuMultiResult))).ToArray()),
                Expression.Assign(varList.First(), temp)));
        }

        private IEnumerable<Expression> GetVarListAssignments(IEnumerable<ParameterExpression> varList, Expression multiResultExpression)
        {
            int count = 0;
            foreach (var var in varList)
            {
                yield return
                    Expression.Assign(var, Expression.Call(multiResultExpression, LuMultiResult.GetMethodInfo, Expression.Constant(count)));
                count++;
            }
        }

        private Expression VisitNumericFor(NuParser.StatContext context)
        {
            _scope = new Scope(_scope);

            Expression varValueExp = VisitExp(context.exp(0));
            Expression limitValueExp = Expression.Dynamic(new LuConvertBinder(typeof(int)), typeof(int), VisitExp(context.exp(1)));
            Expression stepValueExp = context.exp(2) != null ? (Expression)Expression.Dynamic(new LuConvertBinder(typeof(int)), typeof(int), VisitExp(context.exp(2))) : Expression.Constant(1);

            ParameterExpression var = _scope.CreateLocal(context.NAME().GetText(), varValueExp.Type);
            ParameterExpression limit = Expression.Variable(limitValueExp.Type);
            ParameterExpression step = Expression.Variable(stepValueExp.Type);

            Expression innerBlock = VisitBlock(context.block(0));
            LabelTarget breakLabel = Expression.Label("break");

            BlockExpression forBlock = Expression.Block(
                new[] {limit, step}.Concat(_scope.Locals),
                Expression.Assign(var, varValueExp),
                Expression.Assign(limit, limitValueExp),
                Expression.Assign(step, stepValueExp),
                Expression.Loop(
                    Expression.Block(
                        Expression.IfThenElse(
                            Expression.Or(
                                Expression.And(
                                    Expression.GreaterThanOrEqual(step, Expression.Constant(0)),
                                    Expression.GreaterThan(var, limit)),
                                Expression.And(
                                    Expression.LessThan(step, Expression.Constant(0)),
                                    Expression.LessThan(var, limit))),
                            Expression.Break(breakLabel), innerBlock),
                        Expression.AddAssign(var, step)),
                    breakLabel));

            _scope = _scope.Parent;
            return forBlock;
        }

        private Expression CreateIfExpression(NuParser.StatContext context)
        {
            var expExpressions = context.exp().Select(x => VisitIfExp(x)).ToArray();
            var blockExpressions = context.block().Select(VisitBlock).ToArray();

            return VisitIfPart(expExpressions, blockExpressions);
        }

        private Expression VisitIfExp(NuParser.ExpContext expContext)
        {
            Expression visitExp = VisitExp(expContext);
            return Expression.Dynamic(new LuBoolConvertBinder(), typeof (bool), visitExp);
        }

        private Expression VisitIfPart(IEnumerable<Expression> expExpressions, IEnumerable<Expression> blockExpressions)
        {
            Expression firstExp = expExpressions.First();
            Expression firstBlock = blockExpressions.First();

            if (expExpressions.Count() > 1)
            {
                return Expression.IfThenElse(firstExp, firstBlock,
                    VisitIfPart(expExpressions.Skip(1), blockExpressions.Skip(1)));
            }
            else if (blockExpressions.Count() > 1)
            {
                // Block expressions must the be 2
                return Expression.IfThenElse(firstExp, firstBlock, blockExpressions.Last());
            }
            else
            {
                return Expression.IfThen(firstExp, firstBlock);
            }
        }

        private BlockExpression CreateLocalAssignmentExpression(NuParser.StatContext context)
        {
            var varExpressions = context.namelist().NAME().Select(x => _scope.CreateLocal(x.GetText())).ToArray();
            var expExpressions = context.explist().exp().Select(VisitExp).ToArray();

            var assignmentExpression = new List<Expression>();
            for (int index = 0; index < varExpressions.Length; index++)
            {
                var varExpression = varExpressions[index];
                var expExpression = expExpressions.Length <= index
                    ? Expression.Constant(null, typeof (object))
                    : expExpressions[index];
                assignmentExpression.Add(Expression.Assign(varExpression, Expression.Convert(expExpression, typeof(object))));
            }

            return Expression.Block(
                assignmentExpression);
        }

        private BlockExpression VisitAssignments(NuParser.StatContext context)
        {
            // global, member, index, local
            var expExpressions = context.explist().exp().Select(VisitExp).ToArray();

            var assignmentExpression = new List<Expression>();
            for (int index = 0; index < context.varlist().var().Count; index++)
            {
                var varContext = context.varlist().var(index);
                var expExpression = expExpressions.Length <= index ? Expression.Constant(null, typeof (object)) : expExpressions[index];
                assignmentExpression.Add(CreateAssignmentExpression(varContext, expExpression));
            }

            return Expression.Block(assignmentExpression);
        }

        private Expression CreateAssignmentExpression(NuParser.VarContext context, Expression rightExp)
        {
            if (context.varSuffix().Any())
            {
                Expression left = _scope.Get(context.NAME().GetText());
                left = ProcessVarSuffixes(context.varSuffix().Take(context.varSuffix().Count - 1), null)(left);

                var lastSuffix = context.varSuffix(context.varSuffix().Count - 1);
                if (lastSuffix.NAME() != null)
                {
                    return Expression.Dynamic(_binderProvider.GetSetMemberBinder(lastSuffix.NAME().GetText()), typeof (object),
                        left, rightExp);
                }
                else if (lastSuffix.exp() != null)
                {
                    return Expression.Dynamic(new LuSetIndexBinder(new CallInfo(1)), typeof(object),
                        left, VisitExp(lastSuffix.exp()), rightExp);
                }
                else
                {
                    throw new ParseCanceledException();
                }
            }
            else
            {
                return _scope.Set(context.NAME().GetText(), rightExp);
            }
        }

        public override Expression VisitFunctioncall(NuParser.FunctioncallContext context)
        {
            Expression leftExp;
            if (context.varOrExp().exp() != null)
            {
                leftExp = VisitExp(context.varOrExp().exp());
                return ProcessNameAndArgs(context.nameAndArgs())(leftExp);
            }
            else
            {
                var rightEval = ProcessNameAndArgs(context.nameAndArgs());
                return VisitVar(context.varOrExp().var(), rightEval);
            }
        }

        public override Expression VisitBlock(NuParser.BlockContext context)
        {
            if (context.ChildCount == 0)
            {
                return Expression.Empty();
            }

            _scope = new Scope(_scope);
            var stats = context.children.Select(Visit);
            var blockExpression = Expression.Block(_scope.Locals, stats);
            _scope = _scope.Parent;
            return blockExpression;
        }

        public override Expression VisitRetstat(NuParser.RetstatContext context)
        {
            var returnExp = VisitExp(context.explist().exp(0));
            return Expression.Return(_returnTargets.Peek(), Expression.Convert(returnExp, typeof(object)));
        }

        public override Expression VisitChunk(NuParser.ChunkContext context)
        {
            var expressions = new List<Expression>();
            foreach (var child in context.children)
            {
                var expression = Visit(child);
                if (expression != null)
                {
                    expressions.Add(expression);
                }
            }

            return Expression.Block(expressions);
        }
    }

    internal class LuMultiResult
    {
        public static MethodInfo GetMethodInfo = typeof (LuMultiResult).GetMethod("Get");

        private readonly object[] _values;

        public LuMultiResult(IEnumerable<object> values)
        {
            _values = values.ToArray();
        }

        public object Get(int index)
        {
            return index < _values.Length ? _values[index] : null;
        }
    }
}