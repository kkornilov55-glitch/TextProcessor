using TextProcessor.Logic.AbstractSyntaxTree;
using static TextProcessingTests.AbstractSyntaxTree.T;

namespace TextProcessingTests.AbstractSyntaxTree;

public class AstHumanTextTests
{
    [Theory]
        [InlineData("max > 100", "max больше 100")]
        [InlineData("y == x - 9", "y равно x - 9")]
        [InlineData("x != 5", "x не равно 5")]
        [InlineData("x < 5", "x меньше 5")]
        [InlineData("x <= 5", "x меньше или равно 5")]
        [InlineData("x >= 5", "x больше или равно 5")]
        [InlineData("b != 0", "b не равно 0")]
        [InlineData("2 * x + 5 > 15", "2 * x + 5 больше 15")]
        public void Describe_Comparison_UsesWords(string source, string expected)
        {
            Assert.Equal(expected, AstHumanText.Describe(ParsePred(source)));
        }
 
        [Theory]
        [InlineData("x >= 1 && x <= 10", "x больше или равно 1 и x меньше или равно 10")]
        [InlineData("a > 0 || b > 0", "a больше 0 или b больше 0")]
        [InlineData("a > 0 && b > 0 && c > 0", "a больше 0 и b больше 0 и c больше 0")]
        [InlineData("a > 0 && (b > 0 && c > 0)", "a больше 0 и b больше 0 и c больше 0")]
        [InlineData("a > 0 || b > 0 || c > 0", "a больше 0 или b больше 0 или c больше 0")]
        public void Describe_SameConnective_IsFlatChain(string source, string expected)
        {
            Assert.Equal(expected, AstHumanText.Describe(ParsePred(source)));
        }
 
        [Theory]
        [InlineData("a > 0 && b > 0 || c > 0 && d > 0", "(a больше 0 и b больше 0) или (c больше 0 и d больше 0)")]
        [InlineData("a > 0 && (b > 0 || c > 0)", "a больше 0 и (b больше 0 или c больше 0)")]
        [InlineData("x1 >= x2 && x1 > 100 || x1 < x2 && x2 > 100",
                    "(x1 больше или равно x2 и x1 больше 100) или (x1 меньше x2 и x2 больше 100)")]
        public void Describe_MixedConnectives_GroupsWithParentheses(string source, string expected)
        {
            Assert.Equal(expected, AstHumanText.Describe(ParsePred(source)));
        }
 
        [Theory]
        [InlineData("!(x > 5)", "x меньше или равно 5")]
        [InlineData("!(x >= 5)", "x меньше 5")]
        [InlineData("!(x < 5)", "x больше или равно 5")]
        [InlineData("!(x <= 5)", "x больше 5")]
        [InlineData("!(x == 5)", "x не равно 5")]
        [InlineData("!(x != 5)", "x равно 5")]
        [InlineData("!!(x > 5)", "x больше 5")]
        [InlineData("!(x > 5 && y > 5)", "неверно, что (x больше 5 и y больше 5)")]
        [InlineData("!(x > 5 || y > 5)", "неверно, что (x больше 5 или y больше 5)")]
        public void Describe_Negation_InvertsComparisonOrSaysItIsWrong(string source, string expected)
        {
            Assert.Equal(expected, AstHumanText.Describe(ParsePred(source)));
        }
 
        [Theory]
        [InlineData("true", "всегда истинно (без ограничений)")]
        [InlineData("false", "никогда не выполняется")]
        [InlineData("!true", "никогда не выполняется")]
        [InlineData("!false", "всегда истинно (без ограничений)")]
        public void Describe_BooleanLiteral_SaysAlwaysOrNever(string source, string expected)
        {
            Assert.Equal(expected, AstHumanText.Describe(ParsePred(source)));
        }
 
        [Fact]
        public void Describe_NegationOfComparison_IsEquivalentToOriginalOnRealValues()
        {
            var original = ParsePred("!(x > y)");
            var inverted = ParsePred("x <= y");
 
            foreach (var x in new decimal[] { -1, 0, 1 })
            foreach (var y in new decimal[] { -1, 0, 1 })
            {
                var state = State(("x", x), ("y", y));
                Assert.Equal(AstInterpreter.Evalute(original, state), AstInterpreter.Evalute(inverted, state));
            }
        }
        
        [Fact]
        public void Triad_LabChain_BuildsNotationAndWords()
        {
            var pre = ParsePred("x > 5");
            var code = ParseProg("y := x + 1; x := x + 10;");
            var post = ParsePred("y == x - 9 && x > 15");
 
            var triad = AstHumanText.Triad(pre, code, post);
 
            Assert.Equal("{ x > 5 }\ny := x + 1;\nx := x + 10;\n{ y == x - 9 && x > 15 }", triad.Notation);
            Assert.Equal(
                "Если перед выполнением фрагмента верно: x больше 5, " +
                "то после его выполнения гарантированно верно: y равно x - 9 и x больше 15.",
                triad.Words);
        }
 
        [Fact]
        public void Triad_WithIf_PrintsIndentedCode()
        {
            var triad = AstHumanText.Triad(
                ParsePred("x1 >= x2 && x1 > 100 || x1 < x2 && x2 > 100"),
                ParseProg("if (x1 >= x2) { max := x1; } else { max := x2; }"),
                ParsePred("max > 100"));
 
            var expectedNotation = string.Join("\n",
                "{ x1 >= x2 && x1 > 100 || x1 < x2 && x2 > 100 }",
                "if (x1 >= x2) {",
                "    max := x1;",
                "} else {",
                "    max := x2;",
                "}",
                "{ max > 100 }");
            Assert.Equal(expectedNotation, triad.Notation);
            Assert.Contains("(x1 больше или равно x2 и x1 больше 100) или (x1 меньше x2 и x2 больше 100)", triad.Words);
            Assert.Contains("гарантированно верно: max больше 100.", triad.Words);
        }
 
        [Fact]
        public void Triad_PreconditionTrue_SaysNoRestrictionsOnInput()
        {
            var triad = AstHumanText.Triad(new BoolLiteral(true), ParseProg("x := 1;"), ParsePred("x == 1"));
 
            Assert.Equal("{ true }\nx := 1;\n{ x == 1 }", triad.Notation);
            Assert.Equal(
                "Каковы бы ни были начальные значения переменных (ограничений на вход нет), " +
                "после выполнения фрагмента гарантированно верно: x равно 1.",
                triad.Words);
        }
 
        [Fact]
        public void Triad_PreconditionFalse_SaysGoalIsUnreachable()
        {
            var triad = AstHumanText.Triad(
                new BoolLiteral(false),
                ParseProg("x := x + 10; y := x + 1;"),
                ParsePred("y == x - 9 && x > 15"));
 
            Assert.Equal("{ false }\nx := x + 10;\ny := x + 1;\n{ y == x - 9 && x > 15 }", triad.Notation);
            Assert.Contains("Не существует начальных значений", triad.Words);
            Assert.Contains("цель недостижима", triad.Words);
            Assert.Contains("y равно x - 9 и x больше 15", triad.Words);
        }
 
        [Fact]
        public void Triad_EmptyProgram_IsMarkedExplicitly()
        {
            var triad = AstHumanText.Triad(ParsePred("x > 0"), Blk(), ParsePred("x > 0"));
 
            Assert.Equal("{ x > 0 }\n(пустой фрагмент)\n{ x > 0 }", triad.Notation);
        }
}