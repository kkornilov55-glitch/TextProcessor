using TextProcessor.Logic.AbstractSyntaxTree;
using static TextProcessingTests.AbstractSyntaxTree.T;

namespace TextProcessingTests.AbstractSyntaxTree;

public class ParserTests
{
        [Fact]
        public void ParsePredication_SimpleComparison_BuildsCmp()
        {
            Assert.Equal(Rel(PredicationOperatorKind.Greater, V("max"), N(100)), ParsePred("max>100"));
        }
 
        [Fact]
        public void ParsePredication_Subtraction_IsSumWithNeg()
        {
            Assert.Equal(Rel(PredicationOperatorKind.Equal, V("y"), Sub(V("x"), N(9))), ParsePred("y==x-9"));
        }
 
        [Fact]
        public void ParsePredication_MultiplicationBindsTighterThanSum()
        {
            Assert.Equal(
                Rel(PredicationOperatorKind.Greater, Add(V("a"), Mul(V("b"), V("c"))), N(0)),
                ParsePred("a + b * c > 0"));
        }
 
        [Fact]
        public void ParsePredication_SubtractionAndDivision_AreLeftAssociative()
        {
            Assert.Equal(
                Rel(PredicationOperatorKind.Equal, Sub(Sub(V("a"), V("b")), V("c")), N(0)),
                ParsePred("a - b - c == 0"));
            Assert.Equal(
                Rel(PredicationOperatorKind.Equal, Div(Div(V("a"), V("b")), V("c")), N(1)),
                ParsePred("a / b / c == 1"));
        }
 
        [Fact]
        public void ParsePredication_UnaryMinus_BindsTighterThanMultiplication()
        {
            Assert.Equal(
                Rel(PredicationOperatorKind.Less, Mul(new Neg(V("x")), V("y")), N(1)),
                ParsePred("-x * y < 1"));
        }
 
        [Fact]
        public void ParsePredication_AbsAndDecimalNumber_Parsed()
        {
            Assert.Equal(
                Rel(PredicationOperatorKind.GreaterOrEqual, new Abs(Sub(V("x"), N(1))), N(2.5m)),
                ParsePred("abs(x - 1) >= 2.5"));
        }
 
        [Fact]
        public void ParsePredication_ParenthesesAroundArithmetic_Parsed()
        {
            Assert.Equal(Rel(PredicationOperatorKind.Greater, Add(V("x"), N(1)), N(3)), ParsePred("(x+1) > 3"));
            Assert.Equal(Rel(PredicationOperatorKind.Greater, Add(V("x"), N(1)), N(3)), ParsePred("((x+1) > 3)"));
            Assert.Equal(
                Rel(PredicationOperatorKind.NotEqual, Mul(Add(V("x"), N(1)), N(2)), V("y")),
                ParsePred("(x+1)*2 != y"));
        }
 
        [Fact]
        public void ParsePredication_ParenthesesAroundPredicate_Parsed()
        {
            Assert.Equal(
                new And(Rel(PredicationOperatorKind.Greater, V("x"), N(3)), Rel(PredicationOperatorKind.Equal, V("y"), N(1))),
                ParsePred("(x > 3) && y == 1"));
            Assert.Equal(
                new Or(Rel(PredicationOperatorKind.Greater, V("a"), V("b")), Rel(PredicationOperatorKind.Less, V("c"), V("d"))),
                ParsePred("a > b || ((c < d))"));
        }
 
        [Fact]
        public void ParsePredication_AndBindsTighterThanOr()
        {
            Assert.Equal(
                new Or(new BoolLiteral(true), new And(new BoolLiteral(false), new BoolLiteral(true))),
                ParsePred("true || false && true"));
        }
 
        [Fact]
        public void ParsePredication_MaxExample_BuildsOrOfAnds()
        {
            var expected = new Or(
                new And(Rel(PredicationOperatorKind.GreaterOrEqual, V("x1"), V("x2")), Rel(PredicationOperatorKind.Greater, V("x1"), N(100))),
                new And(Rel(PredicationOperatorKind.Less, V("x1"), V("x2")), Rel(PredicationOperatorKind.Greater, V("x2"), N(100))));
 
            Assert.Equal(expected, ParsePred("x1>=x2 && x1>100 || x1<x2 && x2>100"));
        }
 
        [Fact]
        public void ParsePredication_Negation_AppliesToParenthesizedPredicate()
        {
            Assert.Equal(
                new And(new Not(Rel(PredicationOperatorKind.Greater, V("a"), V("b"))), Rel(PredicationOperatorKind.Less, V("c"), V("d"))),
                ParsePred("!(a > b) && c < d"));
        }
 
        [Fact]
        public void ParsePredication_UnicodeOperators_SameAsAscii()
        {
            Assert.Equal(ParsePred("x >= 1 && !(y != 2)"), ParsePred("x ≥ 1 ∧ ¬(y ≠ 2)"));
        }
 
        [Theory]
        [InlineData("", "найдено конец ввода", 1, 1)]
        [InlineData("x +", "Ожидалось выражение", 1, 4)]
        [InlineData("x", "Ожидался оператор сравнения", 1, 2)]
        [InlineData("x > 1 )", "Лишний текст", 1, 7)]
        [InlineData("(x > )", "Ожидалось выражение, найдено ')'", 1, 6)]
        [InlineData("x > 1 &&\n  y", "Ожидался оператор сравнения", 2, 4)]
        [InlineData("x = 1", "Одиночный '='", 1, 3)]
        [InlineData("abs x > 1", "'(' после abs", 1, 5)]
        public void ParsePredication_Invalid_ReportsMessageAndPosition(string source, string messagePart, uint line, uint column)
        {
            var result = AstParser.ParsePredication(AstLexer.Tokenize(source));
 
            Assert.False(result.Ok);
            Assert.Contains(messagePart, result.Errors[0].Message);
            Assert.Equal(line, result.Errors[0].Pos.Line);
            Assert.Equal(column, result.Errors[0].Pos.Column);
        }
 
        [Fact]
        public void ParseProgram_EmptySource_IsEmptyBlock()
        {
            Assert.Equal(Blk(), ParseProg(""));
        }
 
        [Fact]
        public void ParseProgram_Sequence_KeepsOrder()
        {
            Assert.Equal(
                Blk(new Assign("y", Add(V("x"), N(1))), new Assign("x", Add(V("x"), N(10)))),
                ParseProg("y := x+1; x := x+10;"));
        }
 
        [Fact]
        public void ParseProgram_IfElse_BuildsCondition()
        {
            var expected = Blk(new Condition(
                Rel(PredicationOperatorKind.GreaterOrEqual, V("x1"), V("x2")),
                Blk(new Assign("max", V("x1"))),
                Blk(new Assign("max", V("x2")))));
 
            Assert.Equal(expected, ParseProg("if (x1 >= x2) { max := x1; } else { max := x2; }"));
        }
 
        [Fact]
        public void ParseProgram_NestedIf_Parsed()
        {
            var expected = Blk(new Condition(
                Rel(PredicationOperatorKind.Greater, V("a"), N(0)),
                Blk(new Condition(
                    Rel(PredicationOperatorKind.Greater, V("b"), N(0)),
                    Blk(new Assign("r", N(1))),
                    Blk(new Assign("r", N(2))))),
                Blk(new Assign("r", N(3)))));
 
            Assert.Equal(expected, ParseProg("if (a > 0) { if (b > 0) { r := 1; } else { r := 2; } } else { r := 3; }"));
        }
 
        [Fact]
        public void ParseProgram_EmptyBranches_Allowed()
        {
            Assert.Equal(
                Blk(new Condition(Rel(PredicationOperatorKind.Greater, V("x"), N(0)), Blk(), Blk())),
                ParseProg("if (x > 0) { } else { }"));
        }
 
        [Theory]
        [InlineData("x := 1\ny := 2;", "Ожидалось ';', найдено 'y'", 2, 1)]
        [InlineData("x := ;", "Ожидалось выражение", 1, 6)]
        [InlineData("x = 1;", "Одиночный '='", 1, 3)]
        [InlineData("if (x > 0) { x := 1; }", "полной форме", 1, 23)]
        [InlineData("if (x > 0) { x := 1; } else if (y > 0) { }", "Ожидалось '{'", 1, 29)]
        [InlineData("while (x > 0) { x := x - 1; }", "Циклы не поддерживаются", 1, 1)]
        [InlineData("for (x := 0; x < 3; x := x + 1) { }", "Циклы не поддерживаются", 1, 1)]
        [InlineData("x := 1; }", "Ожидался оператор", 1, 9)]
        [InlineData("if (x > 0) { x := 1; ", "Ожидалось '}'", 1, 22)]
        [InlineData("5 := 1;", "Ожидался оператор", 1, 1)]
        [InlineData("x := 99999999999999999999999999999999;", "слишком велико", 1, 6)]
        public void ParseProgram_Invalid_ReportsMessageAndPosition(string source, string messagePart, uint line, uint column)
        {
            var result = AstParser.ParseProgram(AstLexer.Tokenize(source));
 
            Assert.False(result.Ok);
            Assert.Contains(messagePart, result.Errors[0].Message);
            Assert.Equal(line, result.Errors[0].Pos.Line);
            Assert.Equal(column, result.Errors[0].Pos.Column);
        }
 
        [Fact]
        public void ParseProgram_LexicalError_IsReturnedWithoutParsing()
        {
            var result = AstParser.ParseProgram(AstLexer.Tokenize("x := 1 @ 2;"));
 
            Assert.False(result.Ok);
            Assert.Contains("'@'", result.Errors[0].Message);
        }
    }