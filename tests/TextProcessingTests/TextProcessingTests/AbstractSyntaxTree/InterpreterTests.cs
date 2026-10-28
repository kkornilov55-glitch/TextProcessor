using TextProcessor.Logic.AbstractSyntaxTree;
using static TextProcessingTests.AbstractSyntaxTree.T;

namespace TextProcessingTests.AbstractSyntaxTree;

public class InterpreterTests
{
        private static decimal Eval(string expression, params (string, decimal)[] state)
        {
            var cmp = (Cmp)ParsePred(expression + " == 0");
            return AstInterpreter.Evalute(cmp.First, State(state));
        }
 
        private static bool Holds(string predicate, params (string, decimal)[] state) =>
            AstInterpreter.Evalute(ParsePred(predicate), State(state));
 
        [Theory]
        [InlineData("2*x+5", 3, 11)]
        [InlineData("-(x + 1) * 2", 3, -8)]
        [InlineData("abs(x - 10)", 3, 7)]
        [InlineData("x - 1 - 1", 10, 8)]
        public void EvalExpression_Arithmetic_ReturnsValue(string expression, int x, int expected)
        {
            Assert.Equal((decimal)expected, Eval(expression, ("x", x)));
        }
 
        [Fact]
        public void EvalExpression_Division_IsNotIntegerDivision()
        {
            Assert.Equal(3.5m, Eval("7 / 2"));
        }
 
        [Fact]
        public void EvalExpression_DivisionByZero_ThrowsEvalException()
        {
            var ex = Assert.Throws<EvalException>(() => Eval("1 / x", ("x", 0)));
            Assert.Contains("Деление на ноль", ex.Message);
        }
 
        [Fact]
        public void EvalExpression_UnknownVariable_ThrowsEvalException()
        {
            var ex = Assert.Throws<EvalException>(() => Eval("y + 1"));
            Assert.Contains("'y' не определена", ex.Message);
        }
 
        [Fact]
        public void EvalExpression_DecimalOverflow_ThrowsEvalException()
        {
            var ex = Assert.Throws<EvalException>(() => Eval("79228162514264337593543950335 * 2"));
            Assert.Contains("Переполнение", ex.Message);
        }
 
        [Theory]
        [InlineData("x > 1 && y <= 2", true)]
        [InlineData("x > 1 && y < 2", false)]
        [InlineData("x > 5 || y == 2", true)]
        [InlineData("x > 5 || y == 3", false)]
        [InlineData("!(x != 2)", true)]
        [InlineData("x != 3 && x >= 2 && x <= 2", true)]
        [InlineData("true && !false", true)]
        public void EvalPredicate_WithXAndYEqualTwo_ReturnsExpected(string predicate, bool expected)
        {
            Assert.Equal(expected, Holds(predicate, ("x", 2), ("y", 2)));
        }
 
        [Fact]
        public void EvalPredicate_ErrorInSecondOperand_IsNotHiddenByShortCircuit()
        {
            Assert.Throws<EvalException>(() => Holds("x != 0 && 1 / x > 1", ("x", 0)));
            Assert.Throws<EvalException>(() => Holds("x == 0 || 1 / x > 1", ("x", 0)));
        }
 
        [Fact]
        public void Execute_Sequence_RightHandSideSeesOldValues()
        {
            var end = AstInterpreter.Execute(ParseProg("y := x + 1; x := x + 10;"), State(("x", 5), ("y", 0)));
 
            Assert.Equal(15m, end["x"]);
            Assert.Equal(6m, end["y"]);
        }
 
        [Fact]
        public void Execute_DoesNotMutateInitialState()
        {
            var start = State(("x", 5), ("y", 0));
 
            AstInterpreter.Execute(ParseProg("y := x + 1; x := x + 10;"), start);
 
            Assert.Equal(5m, start["x"]);
            Assert.Equal(0m, start["y"]);
        }
 
        [Fact]
        public void Execute_AssignmentToNewVariable_AddsIt()
        {
            var end = AstInterpreter.Execute(ParseProg("z := 1;"), State());
 
            Assert.Equal(1m, end["z"]);
        }
 
        [Theory]
        [InlineData(3, 7, 7)]
        [InlineData(9, 7, 9)]
        [InlineData(5, 5, 5)]
        public void Execute_MaxProgram_ChoosesBranchByCondition(int x1, int x2, int expectedMax)
        {
            var program = ParseProg("if (x1 >= x2) { max := x1; } else { max := x2; }");
 
            var end = AstInterpreter.Execute(program, State(("x1", x1), ("x2", x2)));
 
            Assert.Equal((decimal)expectedMax, end["max"]);
        }
 
        [Theory]
        [InlineData(1, -1, 2)]
        [InlineData(1, 1, 1)]
        [InlineData(-1, 1, 3)]
        public void Execute_NestedIf_ChoosesInnerBranch(int a, int b, int expectedR)
        {
            var program = ParseProg("if (a > 0) { if (b > 0) { r := 1; } else { r := 2; } } else { r := 3; }");
 
            var end = AstInterpreter.Execute(program, State(("a", a), ("b", b)));
 
            Assert.Equal((decimal)expectedR, end["r"]);
        }
 
        [Fact]
        public void TryExecute_DivisionByZero_ReturnsFalse()
        {
            var ok = AstInterpreter.TryExecute(ParseProg("x := 1 / y;"), State(("y", 0)), out var result);
 
            Assert.False(ok);
            Assert.True(result is null);
        }
 
        [Fact]
        public void TryExecute_Valid_ReturnsFinalState()
        {
            var ok = AstInterpreter.TryExecute(ParseProg("y := x + 1;"), State(("x", 5)), out var result);
 
            Assert.True(ok);
            Assert.Equal(6m, result!["y"]);
        }
 
        [Fact]
        public void Execute_LabChain_WeakestPreconditionIsXGreaterThanFive()
        {
            var post = ParsePred("y == x - 9 && x > 15");
            var code = ParseProg("y := x + 1; x := x + 10;");
 
            for (int x = -50; x <= 50; x++)
            {
                var end = AstInterpreter.Execute(code, State(("x", x), ("y", 0)));
                Assert.Equal(x > 5, AstInterpreter.Evalute(post, end));
            }
        }
 
        [Fact]
        public void Execute_LabChainInOtherOrder_PostIsContradictory()
        {
            var post = ParsePred("y == x - 9 && x > 15");
            var code = ParseProg("x := x + 10; y := x + 1;");
 
            for (int x = -50; x <= 50; x++)
            {
                var end = AstInterpreter.Execute(code, State(("x", x), ("y", 0)));
                Assert.False(AstInterpreter.Evalute(post, end));
            }
        }
 
        [Fact]
        public void Execute_MaxExample_WpFormulaMatchesRealExecution()
        {
            var post = ParsePred("max > 100");
            var wp = ParsePred("x1 >= x2 && x1 > 100 || x1 < x2 && x2 > 100");
            var code = ParseProg("if (x1 >= x2) { max := x1; } else { max := x2; }");
            var values = new decimal[] { -5, 0, 50, 100, 101, 150, 200 };
 
            foreach (var x1 in values)
            foreach (var x2 in values)
            {
                var start = State(("x1", x1), ("x2", x2), ("max", 0));
                Assert.Equal(AstInterpreter.Evalute(wp, start), AstInterpreter.Evalute(post, AstInterpreter.Execute(code, start)));
            }
        }
    }