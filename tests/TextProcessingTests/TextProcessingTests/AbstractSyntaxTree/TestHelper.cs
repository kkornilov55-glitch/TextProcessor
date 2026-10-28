using TextProcessor.Logic.AbstractSyntaxTree;

namespace TextProcessingTests.AbstractSyntaxTree;

internal static class T
{
    public static AstExpression V(string name) => new Var(name);
    public static AstExpression N(decimal value) => new Num(value);
    public static AstExpression Add(AstExpression a, AstExpression b) => new Bin(ExpressionOperatorKind.Sum, a, b);
    public static AstExpression Sub(AstExpression a, AstExpression b) => Add(a, new Neg(b));
    public static AstExpression Mul(AstExpression a, AstExpression b) => new Bin(ExpressionOperatorKind.Multiply, a, b);
    public static AstExpression Div(AstExpression a, AstExpression b) => new Bin(ExpressionOperatorKind.Divide, a, b);
    public static Cmp Rel(PredicationOperatorKind op, AstExpression a, AstExpression b) => new(op, a, b);
    public static Block Blk(params AstStatement[] statements) => new(statements);
 
    public static AstPredication ParsePred(string source)
    {
        var result = AstParser.ParsePredication(AstLexer.Tokenize(source));
        Assert.True(result.Ok, $"'{source}': {(result.Ok ? "" : result.Errors[0].Message)}");
        return result.Value!;
    }
 
    public static Block ParseProg(string source)
    {
        var result = AstParser.ParseProgram(AstLexer.Tokenize(source));
        Assert.True(result.Ok, $"'{source}': {(result.Ok ? "" : result.Errors[0].Message)}");
        return result.Value!;
    }
 
    public static Dictionary<string, decimal> State(params (string Name, decimal Value)[] values) =>
        values.ToDictionary(v => v.Name, v => v.Value);
}