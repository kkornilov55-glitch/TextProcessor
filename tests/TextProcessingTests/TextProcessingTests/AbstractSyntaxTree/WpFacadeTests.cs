using TextProcessor.Logic.AbstractSyntaxTree;
using static TextProcessingTests.AbstractSyntaxTree.T;

namespace TextProcessingTests.AbstractSyntaxTree;

public class WpFacadeTests
{
    [Fact]
    public void Presets_ContainsThreeMandatoryScenarios()
    {
        Assert.Equal(3, WpFacade.Presets.Count);
    }

    [Fact]
    public void Calculate_Preset1_MaxOfTwo_ReturnsExpectedWpAndTriad()
    {
        var preset = WpFacade.Presets[0];

        var res = WpFacade.Calculate(preset.Code, preset.PostCondition);

        Assert.True(res.IsSuccess);
        Assert.Equal("x1 >= x2 && x1 > 100 || x1 < x2 && x2 > 100", res.FinalWpShort);
        Assert.Contains("{ max > 100 }", res.TriadNotation);
        Assert.NotEmpty(res.TraceText);
    }

    [Fact]
    public void Calculate_Preset2_QuadraticEquation_IncludesDenominatorNotZero()
    {
        var preset = WpFacade.Presets[1];

        var res = WpFacade.Calculate(preset.Code, preset.PostCondition);

        Assert.True(res.IsSuccess);
        Assert.Equal("D >= 0 && a != 0 || D < 0", res.FinalWpShort);
        Assert.Contains("a != 0", res.FinalWpShort);
    }

    [Fact]
    public void Calculate_Preset3_AssignmentSequence_SimplifiesToXGreaterThanFive()
    {
        var preset = WpFacade.Presets[2];

        var res = WpFacade.Calculate(preset.Code, preset.PostCondition);

        Assert.True(res.IsSuccess);
        Assert.Equal("x > 5", res.FinalWpShort);
        Assert.Equal(2, res.Steps.Count);
    }

    [Theory]
    [InlineData("", "x > 0", "Код фрагмента программы не может быть пустым")]
    [InlineData("x := 1;", "", "Постусловие (Post) не может быть пустым")]
    [InlineData("x := 1;", "x === 0", "Ошибка в постусловии")]
    [InlineData("if (x > 0) { x := 1; }", "x > 0", "Ошибка в коде программы")] // нет ветки else
    public void Calculate_InvalidInput_ReturnsIsSuccessFalseWithMessage(
        string code, string post, string expectedErrorSubstring)
    {
        var res = WpFacade.Calculate(code, post);

        Assert.False(res.IsSuccess);
        Assert.Contains(expectedErrorSubstring, res.ErrorMessage);
    }
}