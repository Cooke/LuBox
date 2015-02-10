using System;
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
                    leftExp = Expression.Dynamic(new NuInvokeBinder(new CallInfo(args.Count())), typeof (object), args);
                }
            }

            return leftExp;
        }

        public override Expression VisitVar(NuParser.VarContext context)
        {            
            Expression left = _scope.Get(context.NAME().GetText());
            foreach (var suffix in context.varSuffix())
            {
                left = ProcessNameAndArgs(suffix.nameAndArgs(), left);

                left =
                    Expression.Dynamic(new NuGetMemberBinder(suffix.NAME().GetText()), 
                        typeof(object), left);
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
            var parseTree = context.GetChild(0);
            var ruleIndex = parseTree.GetRuleIndex();
            switch (ruleIndex)
            {
                case NuParser.RULE_prefixexp:
                    return VisitPrefixexp(context.prefixexp());
                case NuParser.RULE_number:
                    return VisitNumber(context.number());
                case NuParser.RULE_string:
                    return VisitString(context.@string());
                default:
                    throw new ParseCanceledException("Invalid expression");
            }
        }

        private Expression VisitUnaryOperatorExp(NuParser.ExpContext context)
        {
            var unaryOperator = context.operatorUnary();
            var exp = context.exp(0);
            switch (unaryOperator.GetText())
            {
                case "-":
                    return Expression.Negate(VisitExp(exp));
                default:
                    throw new ParseCanceledException("Invalid expression");
            }
        }

        private Expression VisitBinaryOperatorExp(NuParser.ExpContext context)
        {
            Expression left = VisitExp(context.exp(0));
            IParseTree oper = context.GetChild(1);
            Expression right = VisitExp(context.exp(1));
            
            var ruleIndex = oper.GetRuleIndex();
            switch (ruleIndex)
            {
                case NuParser.RULE_operatorMulDivMod:
                    return
                        Expression.Dynamic(
                            new NuBinaryOperationBinder(oper.GetText() == "*"
                                ? ExpressionType.Multiply
                                : ExpressionType.Divide), typeof(object), left, right);
                case NuParser.RULE_operatorAddSub:
                    return
                        Expression.Dynamic(
                            new NuBinaryOperationBinder(oper.GetText() == "+"
                                ? ExpressionType.Add
                                : ExpressionType.Subtract), typeof (object),
                            left, right);
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

            var block = VisitBlock(context.block());
            _scope = _scope.Parent;

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
            else
            {
                return VisitFunctioncall(context.functioncall());
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
                    left = Expression.Dynamic(new NuGetMemberBinder(suffix.NAME().GetText()), typeof (object),
                        left);
                }

                var lastSuffix = context.varSuffix(context.varSuffix().Count - 1);
                return Expression.Dynamic(new NuSetMemberBinder(lastSuffix.NAME().GetText()), typeof (object),
                    left, expExpression);
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
            var blockExpression = Expression.Block(_scope.LocalParameterExpression, stats);
            _scope = _scope.Parent;
            return blockExpression;
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