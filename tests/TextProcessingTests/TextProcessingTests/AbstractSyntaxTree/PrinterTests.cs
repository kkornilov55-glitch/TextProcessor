using TextProcessor.Logic.AbstractSyntaxTree;
using static TextProcessingTests.AbstractSyntaxTree.T;

namespace TextProcessingTests.AbstractSyntaxTree;

public class PrinterTests
{
        [Theory]
        [InlineData("y==x-9", "y == x - 9")]
        [InlineData("a + b + c > 0", "a + b + c > 0")]
        [InlineData("a + (b + c) > 0", "a + (b + c) > 0")]
        [InlineData("a - b - c > 0", "a - b - c > 0")]
        [InlineData("a - (b + c) > 0", "a - (b + c) > 0")]
        [InlineData("a - (b - c) > 0", "a - (b - c) > 0")]
        [InlineData("a + (b - c) > 0", "a + (b - c) > 0")]
        [InlineData("a - b * c > 0", "a - b * c > 0")]
        [InlineData("-(a + b) > 0", "-(a + b) > 0")]
        [InlineData("-a * b > 0", "-a * b > 0")]
        [InlineData("-(a * b) > 0", "-(a * b) > 0")]
        [InlineData("- -x > 0", "-(-x) > 0")]
        [InlineData("a - -b > 0", "a - -b > 0")]
        [InlineData("a * -b > 0", "a * -b > 0")]
        [InlineData("a * (b + c) > 0", "a * (b + c) > 0")]
        [InlineData("a * (b * c) > 0", "a * (b * c) > 0")]
        [InlineData("a / (b / c) > 0", "a / (b / c) > 0")]
        [InlineData("a / b / c > 0", "a / b / c > 0")]
        [InlineData("abs(x - 1) >= 2.50", "abs(x - 1) >= 2.5")]
        public void Print_Expression_MinimalParentheses(string source, string expected)
        {
            Assert.Equal(expected, AstPrinter.Print(ParsePred(source)));
        }
 
        [Theory]
        [InlineData("a > b && c < d || e == f", "a > b && c < d || e == f")]
        [InlineData("a > b && (c < d || e == f)", "a > b && (c < d || e == f)")]
        [InlineData("a > b || (c < d || e == f)", "a > b || (c < d || e == f)")]
        [InlineData("!(a > b) && c < d", "!(a > b) && c < d")]
        [InlineData("!(a > b && c < d)", "!(a > b && c < d)")]
        [InlineData("!!(a > b)", "!!(a > b)")]
        [InlineData("true && !false", "true && !false")]
        [InlineData("x ≥ 1 ∧ ¬(y ≠ 2)", "x >= 1 && !(y != 2)")]
        public void Print_Predicate_MinimalParentheses(string source, string expected)
        {
            Assert.Equal(expected, AstPrinter.Print(ParsePred(source)));
        }
 
        [Fact]
        public void Print_HandBuiltNegativeNumber_PrintedAsSubtraction()
        {
            // так выглядят деревья после упрощения: Num(-9) вместо Neg(Num(9))
            Assert.Equal("x - 9", AstPrinter.Print(Add(V("x"), N(-9))));
            Assert.Equal("x * -2", AstPrinter.Print(Mul(V("x"), N(-2))));
            Assert.Equal("-(-5)", AstPrinter.Print(new Neg(N(-5))));
        }
 
        [Fact]
        public void Print_NestedIf_UsesIndentation()
        {
            var program = ParseProg("if (x1 >= x2) { max := x1; } else { if (x2>0) {max := x2;} else {max := 0;} }");
 
            var expected = string.Join("\n",
                "if (x1 >= x2) {",
                "    max := x1;",
                "} else {",
                "    if (x2 > 0) {",
                "        max := x2;",
                "    } else {",
                "        max := 0;",
                "    }",
                "}");
            Assert.Equal(expected, AstPrinter.Print(program));
        }
 
        [Fact]
        public void PrintShort_Assign_IsSingleLineWithoutSemicolon()
        {
            Assert.Equal("x := x + 10", AstPrinter.PrintShort(new Assign("x", Add(V("x"), N(10)))));
        }
 
        [Theory]
        [InlineData("y := x+1; x := x+10;")]
        [InlineData("if (a>0) { if (b>0) { r:=1; } else { r:=2; } } else { r:=3; }")]
        [InlineData("x := -(a+b) * 2 / (c - d);")]
        public void Print_ParsedProgram_ParsesBackToSameTree(string source)
        {
            var first = ParseProg(source);
 
            var second = ParseProg(AstPrinter.Print(first));
 
            Assert.Equal(first, second);
        }
 
        [Fact]
        public void Print_RandomExpressions_ParseBackToSameTree()
        {
            var random = new Random(12345);
            for (int i = 0; i < 3000; i++)
            {
                var expression = RandomExpression(random, 4);
                var text = AstPrinter.Print(expression) + " == 0";
 
                var parsed = AstParser.ParsePredication(AstLexer.Tokenize(text));
 
                Assert.True(parsed.Ok, text);
                Assert.True(new Cmp(PredicationOperatorKind.Equal, expression, N(0)).Equals(parsed.Value), text);
            }
        }
 
        [Fact]
        public void Print_RandomPredicates_ParseBackToSameTree()
        {
            var random = new Random(54321);
            for (int i = 0; i < 3000; i++)
            {
                var predicate = RandomPredicate(random, 4);
                var text = AstPrinter.Print(predicate);
 
                var parsed = AstParser.ParsePredication(AstLexer.Tokenize(text));
 
                Assert.True(parsed.Ok, text);
                Assert.True(predicate.Equals(parsed.Value), text);
            }
        }
 
        internal static AstExpression RandomExpression(Random random, int depth)
        {
            int kind = depth <= 0 ? random.Next(2) : random.Next(7);
            return kind switch
            {
                0 => N(random.Next(0, 20) + (random.Next(4) == 0 ? 0.5m : 0m)),
                1 => V("xyz"[random.Next(3)].ToString()),
                2 => new Neg(RandomExpression(random, depth - 1)),
                3 => new Abs(RandomExpression(random, depth - 1)),
                4 => Add(RandomExpression(random, depth - 1), RandomExpression(random, depth - 1)),
                5 => Mul(RandomExpression(random, depth - 1), RandomExpression(random, depth - 1)),
                _ => Div(RandomExpression(random, depth - 1), RandomExpression(random, depth - 1)),
            };
        }
 
        internal static AstPredication RandomPredicate(Random random, int depth)
        {
            var operators = Enum.GetValues<PredicationOperatorKind>();
            int kind = depth <= 0 ? random.Next(2) : random.Next(6);
            return kind switch
            {
                0 => new BoolLiteral(random.Next(2) == 0),
                1 => Rel(operators[random.Next(operators.Length)], RandomExpression(random, 2), RandomExpression(random, 2)),
                2 => new And(RandomPredicate(random, depth - 1), RandomPredicate(random, depth - 1)),
                3 => new Or(RandomPredicate(random, depth - 1), RandomPredicate(random, depth - 1)),
                4 => new Not(RandomPredicate(random, depth - 1)),
                _ => Rel(operators[random.Next(operators.Length)], RandomExpression(random, 3), RandomExpression(random, 3)),
            };
        }
    }