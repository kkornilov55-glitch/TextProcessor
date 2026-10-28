using TextProcessor.Logic.AbstractSyntaxTree;
using static TextProcessingTests.AbstractSyntaxTree.T;

namespace TextProcessingTests.AbstractSyntaxTree;

public class AstSimplifierTests
{
    private static AstExpression ParseExpr(string expr) =>
        ((Cmp)ParsePred(expr + " == 0")).First;

    [Theory]
    [InlineData("2 + 3 * 4", "14")]
    [InlineData("10 / 2", "5")]
    [InlineData("abs(-7)", "7")]
    [InlineData("-(-x)", "x")]
    [InlineData("x + 0", "x")]
    [InlineData("0 + x", "x")]
    [InlineData("x * 1", "x")]
    [InlineData("1 * x", "x")]
    [InlineData("x * 0", "0")]
    [InlineData("0 / x", "0")]
    [InlineData("x / 1", "x")]
    [InlineData("x / x", "1")]
    [InlineData("x - x", "0")]
    [InlineData("(x + 10) - 9", "x + 1")]
    public void Simplify_Expression_ReducesAlgebraicForms(string input, string expected)
    {
        var expr = ParseExpr(input);

        var simplified = AstSimplifier.Simplify(expr);

        Assert.Equal(expected, AstPrinter.Print(simplified));
    }

    [Theory]
    [InlineData("5 > 3", "true")]
    [InlineData("5 < 3", "false")]
    [InlineData("x + 1 == x + 1", "true")]
    [InlineData("x + 1 != x + 1", "false")]
    [InlineData("x + 1 < x + 1", "false")]
    [InlineData("x + 1 <= x + 1", "true")]
    [InlineData("x + 10 > 15", "x > 5")]
    [InlineData("2 * x + 5 > 15", "x > 5")]
    [InlineData("2 * a != 0", "a != 0")]
    [InlineData("!(x1 >= x2)", "x1 < x2")]
    [InlineData("!!(x > 0)", "x > 0")]
    [InlineData("true && x > 5", "x > 5")]
    [InlineData("x > 5 && true", "x > 5")]
    [InlineData("false && x > 5", "false")]
    [InlineData("false || x > 5", "x > 5")]
    [InlineData("true || x > 5", "true")]
    [InlineData("x > 5 && x > 5", "x > 5")]
    [InlineData("x > 5 || x > 5", "x > 5")]
    public void Simplify_Predication_ReducesComparisonsAndLogic(string input, string expected)
    {
        var pred = ParsePred(input);

        var simplified = AstSimplifier.Simplify(pred);

        Assert.Equal(expected, AstPrinter.Print(simplified));
    }
}