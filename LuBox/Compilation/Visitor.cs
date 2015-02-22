﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using LuBox.Parser;
using LuBox.Runtime;

namespace LuBox.Compiler
{
    internal class Visitor : NuBaseVisitor<Expression>
    {
        private IScope _scope;
        private System.Collections.Generic.Stack<LabelTarget> returnTargets = new Stack<LabelTarget>(); 

        public Visitor(IScope scope)
        {
            _scope = scope;
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
            }
            else 
            {
                leftExp = VisitVar(context.varOrExp().var());
            }
            
            return ProcessNameAndArgs(context.nameAndArgs(), leftExp);
        }

        private Expression ProcessNameAndArgs(IEnumerable<NuParser.NameAndArgsContext> nameAndArgsList, Expression leftExp)
        {
            foreach (var nameAndArgsContext in nameAndArgsList)
            {
                string memberName = nameAndArgsContext.NAME() != null ? nameAndArgsContext.NAME().GetText() : null;

                IEnumerable<Expression> args = new[] {leftExp};
                if (nameAndArgsContext.args().explist() != null)
                {
                    args = args.Concat(nameAndArgsContext.args().explist().exp().Select(VisitExp));
                }

                if (memberName != null)
                {
                    leftExp = Expression.Dynamic(
                        new LuInvokeMemberBinder(memberName, false, new CallInfo(nameAndArgsContext.args().ChildCount)),
                        typeof (object), args);
                }
                else
                {
                    leftExp = Expression.Dynamic(new LuInvokeBinder(new CallInfo(args.Count())), typeof (object), args);
                }
            }

            return leftExp;
        }

        public Expression VisitVarSuffix(NuParser.VarSuffixContext suffix, Expression left)
        {
            left = ProcessNameAndArgs(suffix.nameAndArgs(), left);

            if (suffix.NAME() != null)
            {
                left =
                    Expression.Dynamic(new LuGetMemberBinder(suffix.NAME().GetText()),
                        typeof(object), left);
            }
            else if (suffix.exp() != null)
            {
                left =
                    Expression.Dynamic(new LuGetIndexBinder(new CallInfo(1)),
                        typeof(object), left, VisitExp(suffix.exp()));
            }
            else
            {
                throw new ParseCanceledException();
            }

            return left;
        }

        public override Expression VisitVar(NuParser.VarContext context)
        {            
            Expression left = _scope.Get(context.NAME().GetText());
            foreach (var suffix in context.varSuffix())
            {
                left = ProcessNameAndArgs(suffix.nameAndArgs(), left);
                left = VisitVarSuffix(suffix, left);
            }

            return left;
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
                    default:
                        throw new ParseCanceledException("Invalid expression");
                }
            }
            else
            {
                return VisitTerminalExp(context);
            }
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
            returnTargets.Push(returnLabel);
            var block = Expression.Block(new[] { VisitBlock(context.block()), Expression.Label(returnLabel, Expression.Constant(null, typeof(object))) });
            _scope = _scope.Parent;
            returnTargets.Pop();

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
                throw new ParseCanceledException();
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

            IEnumerable<ParameterExpression> varList = context.namelist().NAME().Select(x => _scope.CreateLocal(x.GetText(), typeof(object))).ToArray();
            
            Expression innerBlock = VisitBlock(context.block(0));
            LabelTarget breakLabel = Expression.Label("break");

            BlockExpression forBlock = Expression.Block(
                new[] {f, s, v},
                Expression.Assign(f, fValueExp),
                Expression.Assign(s, sValueExp),
                Expression.Assign(v, vValueExp),
                Expression.Loop(
                    Expression.Block(
                        varList.Take(1), // only support 1 var now
                        Expression.Assign(varList.First(),
                            Expression.Dynamic(new LuInvokeBinder(new CallInfo(2)), typeof (object), f, s, v)),
                        Expression.Assign(v, varList.First()),
                        Expression.IfThenElse(
                            Expression.Dynamic(new LuBoolConvertBinder(), typeof (bool), varList.First()), 
                            innerBlock,
                            Expression.Break(breakLabel))),
                    breakLabel));

            _scope = _scope.Parent;
            return forBlock;
        }

        private Expression VisitNumericFor(NuParser.StatContext context)
        {
            _scope = new Scope(_scope);

            Expression varValueExp = VisitExp(context.exp(0));
            Expression limitValueExp = VisitExp(context.exp(1));
            Expression stepValueExp = context.exp(2) != null ? VisitExp(context.exp(2)) : Expression.Constant(1);

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
                            Expression.Convert(
                                Expression.Dynamic(new LuBinaryOperationBinder(ExpressionType.LessThanOrEqual), typeof (object),
                                    var, limit), typeof (bool)), innerBlock, Expression.Break(breakLabel)),
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

        private Expression CreateAssignmentExpression(NuParser.VarContext context, Expression expExpression)
        {
            if (context.varSuffix().Any())
            {
                Expression left = _scope.Get(context.NAME().GetText());
                for (int i = 0; i < context.varSuffix().Count - 1; i++)
                {
                    var suffix = context.varSuffix(i);
                    left = ProcessNameAndArgs(suffix.nameAndArgs(), left);
                    left = VisitVarSuffix(suffix, left);
                }

                var lastSuffix = context.varSuffix(context.varSuffix().Count - 1);
                if (lastSuffix.NAME() != null)
                {
                    return Expression.Dynamic(new LuSetMemberBinder(lastSuffix.NAME().GetText()), typeof (object),
                        left, expExpression);
                }
                else if (lastSuffix.exp() != null)
                {
                    return Expression.Dynamic(new LuSetIndexBinder(new CallInfo(1)), typeof(object),
                        left, VisitExp(lastSuffix.exp()), expExpression);
                }
                else
                {
                    throw new ParseCanceledException();
                }
            }
            else
            {
                return _scope.Set(context.NAME().GetText(), expExpression);
            }
        }

        public override Expression VisitFunctioncall(NuParser.FunctioncallContext context)
        {
            Expression leftExp;
            if (context.varOrExp().exp() != null)
            {
                leftExp = VisitExp(context.varOrExp().exp());
            }
            else
            {
                leftExp = VisitVar(context.varOrExp().var());
            }

            return ProcessNameAndArgs(context.nameAndArgs(), leftExp);
        }

        public override Expression VisitBlock(NuParser.BlockContext context)
        {
            _scope = new Scope(_scope);
            var stats = context.children.Select(Visit);
            var blockExpression = Expression.Block(_scope.Locals, stats);
            _scope = _scope.Parent;
            return blockExpression;
        }

        public override Expression VisitRetstat(NuParser.RetstatContext context)
        {
            var returnExp = VisitExp(context.explist().exp(0));
            return Expression.Return(returnTargets.Peek(), returnExp);
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
}