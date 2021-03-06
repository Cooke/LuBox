//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.3
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:\Development\LuBox\LuBox.Parser\Nu.g4 by ANTLR 4.3

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591

namespace LuBox.Parser {
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="NuParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.3")]
[System.CLSCompliant(false)]
public interface INuVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.operatorUnary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperatorUnary([NotNull] NuParser.OperatorUnaryContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.funcname"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFuncname([NotNull] NuParser.FuncnameContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.lambdaArgs"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLambdaArgs([NotNull] NuParser.LambdaArgsContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.operatorAnd"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperatorAnd([NotNull] NuParser.OperatorAndContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.fieldsep"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFieldsep([NotNull] NuParser.FieldsepContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.string"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitString([NotNull] NuParser.StringContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.functioncall"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctioncall([NotNull] NuParser.FunctioncallContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.parlist"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParlist([NotNull] NuParser.ParlistContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.chunk"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitChunk([NotNull] NuParser.ChunkContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.explist"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExplist([NotNull] NuParser.ExplistContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.retstat"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRetstat([NotNull] NuParser.RetstatContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.dotOrQuestionMarkDot"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDotOrQuestionMarkDot([NotNull] NuParser.DotOrQuestionMarkDotContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.varOrExp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVarOrExp([NotNull] NuParser.VarOrExpContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.number"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNumber([NotNull] NuParser.NumberContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.prefixexp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrefixexp([NotNull] NuParser.PrefixexpContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.lambda"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLambda([NotNull] NuParser.LambdaContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.nameAndArgs"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNameAndArgs([NotNull] NuParser.NameAndArgsContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.namelist"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNamelist([NotNull] NuParser.NamelistContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.functiondef"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctiondef([NotNull] NuParser.FunctiondefContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBlock([NotNull] NuParser.BlockContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.operatorComparison"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperatorComparison([NotNull] NuParser.OperatorComparisonContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.varlist"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVarlist([NotNull] NuParser.VarlistContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.exp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExp([NotNull] NuParser.ExpContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.funcbody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFuncbody([NotNull] NuParser.FuncbodyContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.stat"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStat([NotNull] NuParser.StatContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.operatorPower"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperatorPower([NotNull] NuParser.OperatorPowerContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.operatorOr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperatorOr([NotNull] NuParser.OperatorOrContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.var"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVar([NotNull] NuParser.VarContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.operatorAddSub"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperatorAddSub([NotNull] NuParser.OperatorAddSubContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.operatorMulDivMod"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperatorMulDivMod([NotNull] NuParser.OperatorMulDivModContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.label"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLabel([NotNull] NuParser.LabelContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.fieldlist"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFieldlist([NotNull] NuParser.FieldlistContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.colonOrQuestionMarkColon"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitColonOrQuestionMarkColon([NotNull] NuParser.ColonOrQuestionMarkColonContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.operatorStrcat"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOperatorStrcat([NotNull] NuParser.OperatorStrcatContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.args"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArgs([NotNull] NuParser.ArgsContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.field"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitField([NotNull] NuParser.FieldContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.varSuffix"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVarSuffix([NotNull] NuParser.VarSuffixContext context);

	/// <summary>
	/// Visit a parse tree produced by <see cref="NuParser.tableconstructor"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTableconstructor([NotNull] NuParser.TableconstructorContext context);
}
} // namespace LuBox.Parser
