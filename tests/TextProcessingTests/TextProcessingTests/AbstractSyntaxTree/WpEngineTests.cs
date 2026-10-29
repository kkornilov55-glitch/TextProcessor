using TextProcessor.Logic.AbstractSyntaxTree;
using static TextProcessingTests.AbstractSyntaxTree.T;

namespace TextProcessingTests.AbstractSyntaxTree;

public class WpEngineTests
{
    [Fact]
    public void Calculate_SingleAssignment_LectureExample_ReturnsXGreaterThanFive()
    {
        var prog = ParseProg("x := 2 * x + 5;");
        var post = ParsePred("x > 15");

        var result = WpEngine.Calculate(prog, post);

        Assert.Equal("x > 5", AstPrinter.Print(result.FinalWp));
        Assert.Single(result.Steps);
        Assert.Equal("2 * x + 5 > 15", result.Steps[0].RawNotation);
        Assert.Equal("x > 5", result.Steps[0].SimplifiedNotation);
    }

    [Fact]
    public void Calculate_AssignmentWithDivision_IncludesDefinednessCondition()
    {
        var prog = ParseProg("y := 10 / x;");
        var post = ParsePred("y > 2");

        var result = WpEngine.Calculate(prog, post);

        Assert.Equal("x != 0 && 10 / x > 2", AstPrinter.Print(result.FinalWp));
    }

    [Fact]
    public void Calculate_SequenceOfAssignments_PullsPostBackwardAndRecordsTrace()
    {
        var prog = ParseProg("y := x + 1; x := x + 10;");
        var post = ParsePred("y == x - 9 && x > 15");

        var result = WpEngine.Calculate(prog, post);

        Assert.Equal("x > 5", AstPrinter.Print(result.FinalWp));
        Assert.Equal(2, result.Steps.Count);
        // 1-й шаг: через последний оператор (x := x + 10)
        Assert.Equal("y == x + 1 && x > 5", result.Steps[0].SimplifiedNotation);
        // 2-й шаг: через предыдущий оператор (y := x + 1)
        Assert.Equal("x > 5", result.Steps[1].SimplifiedNotation);
    }

    [Fact]
    public void Calculate_IfElseStatement_BuildsDisjunctionOfBranches()
    {
        var prog = ParseProg("if (x1 >= x2) { max := x1; } else { max := x2; }");
        var post = ParsePred("max > 100");

        var result = WpEngine.Calculate(prog, post);

        Assert.Equal("x1 >= x2 && x1 > 100 || x1 < x2 && x2 > 100", AstPrinter.Print(result.FinalWp));
        Assert.Equal(3, result.Steps.Count); // ветка then, ветка else, общее ветвление
    }

    [Fact]
    public void Calculate_WpGuaranteesPost_OnSampledDataWithDivisionAndBranching()
    {
        var prog = ParseProg("if (a != 0) { y := 12 / a; } else { y := -1; }");
        var post = ParsePred("y >= 3");

        var wp = WpEngine.Calculate(prog, post).FinalWp;

        for (int a = -10; a <= 10; a++)
        {
            var initialState = State(("a", a), ("y", 0));

            // Защищаем вычисление wp через try-catch из-за отсутствия short-circuit в интерпретаторе
            bool wpHolds = false;
            try { wpHolds = AstInterpreter.Evalute(wp, initialState); }
            catch (EvalException) { wpHolds = false; }

            bool executedOk = AstInterpreter.TryExecute(prog, initialState, out var finalState);
            bool postHolds = executedOk && AstInterpreter.Evalute(post, finalState!);

            Assert.Equal(wpHolds, postHolds);
        }
    }
}