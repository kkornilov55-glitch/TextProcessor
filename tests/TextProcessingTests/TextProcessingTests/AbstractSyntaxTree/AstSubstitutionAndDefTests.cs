using TextProcessor.Logic.AbstractSyntaxTree;
using static TextProcessingTests.AbstractSyntaxTree.T;

namespace TextProcessingTests.AbstractSyntaxTree;

public class AstSubstitutionAndDefTests
{
    private static AstExpression ParseExpr(string expr) =>
        ((Cmp)ParsePred(expr + " == 0")).First;

    [Theory]
    [InlineData("x + 5", "x", "2 * y", "2 * y + 5")]
    [InlineData("a + b", "x", "10", "a + b")]
    [InlineData("-(x) + abs(x)", "x", "y - 1", "-(y - 1) + abs(y - 1)")]
    [InlineData("x * x", "x", "x + 1", "(x + 1) * (x + 1)")]
    public void Substitute_Expression_ReplacesTargetVariableOnly(
        string original, string varName, string replacement, string expected)
    {
        var expr = ParseExpr(original);
        var repl = ParseExpr(replacement);

        var result = AstSubstitutionAndDef.Substitute(expr, varName, repl);

        Assert.Equal(expected, AstPrinter.Print(result));
    }

    [Theory]
    [InlineData("x > 15", "x", "2 * x + 5", "2 * x + 5 > 15")]
    [InlineData("y == x - 9 && x > 15", "x", "x + 10", "y == x + 10 - 9 && x + 10 > 15")]
    [InlineData("!(x == 0) || y < x", "x", "a", "!(a == 0) || y < a")]
    [InlineData("true && false", "x", "100", "true && false")]
    public void Substitute_Predication_ReplacesTargetVariableInAllBranches(
        string originalPred, string varName, string replacementExpr, string expectedPred)
    {
        var pred = ParsePred(originalPred);
        var repl = ParseExpr(replacementExpr);

        var result = AstSubstitutionAndDef.Substitute(pred, varName, repl);

        Assert.Equal(expectedPred, AstPrinter.Print(result));
    }

    [Fact]
    public void GetDefinedness_ExpressionWithoutDivision_ReturnsTrue()
    {
        var expr = ParseExpr("2 * x + 5 - abs(y)");

        var def = AstSubstitutionAndDef.GetDefinedness(expr);

        Assert.Equal(new BoolLiteral(true), def);
    }

    [Theory]
    [InlineData("10 / a", "a != 0")]
    [InlineData("(-b + s) / (2 * a)", "2 * a != 0")]
    [InlineData("1 / x + 2 / (y - 1)", "x != 0 && y - 1 != 0")]
    [InlineData("1 / x + 5 / x", "x != 0")] // без дубликатов
    public void GetDefinedness_ExpressionWithDivision_ReturnsDenominatorNotZero(
        string expression, string expectedDef)
    {
        var expr = ParseExpr(expression);

        var def = AstSubstitutionAndDef.GetDefinedness(expr);

        Assert.Equal(expectedDef, AstPrinter.Print(def));
    }

    [Theory]
    [InlineData("x > 5 && y < 10", "true")]
    [InlineData("10 / x > 2", "x != 0")]
    [InlineData("1 / a == 1 && !(1 / b == 0)", "a != 0 && b != 0")]
    public void GetDefinedness_Predication_CollectsAllDivisions(
        string predication, string expectedDef)
    {
        var pred = ParsePred(predication);

        var def = AstSubstitutionAndDef.GetDefinedness(pred);

        Assert.Equal(expectedDef, AstPrinter.Print(def));
    }
}