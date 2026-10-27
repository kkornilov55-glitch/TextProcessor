using TextProcessor.Logic.AbstractSyntaxTree;

namespace TextProcessingTests.AbstractSyntaxTree;

public class LexerTests
{
    private static string Kinds(string source) =>
        string.Join(" ", AstLexer.Tokenize(source).Tokens.Select(t => t.Kind));
    
    [Theory]
    [InlineData("x := 2*x+5;", "Ident Assign Number Star Ident Plus Number Semicolon Eof")]
    [InlineData("a<=b a<b a==b a!=b", "Ident Le Ident Ident Lt Ident Ident Eq Ident Ident Ne Ident Eof")]
    [InlineData("a>=b a>b", "Ident Ge Ident Ident Gt Ident Eof")]
    [InlineData("a&&b || !c", "Ident And Ident Or Not Ident Eof")]
    [InlineData("abs(x) true false", "Abs LParen Ident RParen True False Eof")]
    [InlineData("if (x1 >= x2) { max := x1; } else { max := x2; }",
        "If LParen Ident Ge Ident RParen LBrace Ident Assign Ident Semicolon RBrace Else LBrace Ident Assign Ident Semicolon RBrace Eof")]
    public void Tokenize_ValidSource_ProducesExpectedKinds(string source, string expected)
    {
        Assert.Equal(expected, Kinds(source));
    }
    
    [Theory]
    [InlineData("iff ifx elsewhere")]
    [InlineData("absolute trueish")]
    public void Tokenize_IdentifierStartingWithKeyword_IsIdentifier(string source)
    {
        var kinds = Kinds(source).Split(' ');
        Assert.Equal("Ident", kinds[0]);
        Assert.Equal("Ident", kinds[1]);
    }
    
    [Fact]
        public void Tokenize_UnicodeOperators_AreAliasesOfAscii()
        {
            Assert.Equal(
                "Ident Assign Number Semicolon Ident Ge Ident And Ident Ne Ident Or Not Ident Eof",
                Kinds("x ≔ 1; a ≥ b ∧ c ≠ d ∨ ¬e"));
        }
 
        [Theory]
        [InlineData("while (x > 0) { }")]
        [InlineData("for (;;) { }")]
        [InlineData("do { } ")]
        public void Tokenize_LoopKeyword_IsMarkedAsLoop(string source)
        {
            Assert.Equal("Loop", Kinds(source).Split(' ')[0]);
        }
 
        [Fact]
        public void Tokenize_DecimalNumber_KeepsFractionalPart()
        {
            var tokens = AstLexer.Tokenize("1.5 + 10").Tokens;
            Assert.Equal("1.5", tokens[0].Text);
            Assert.Equal("10", tokens[2].Text);
        }
 
        [Fact]
        public void Tokenize_NumberGluedToLetters_IsNumberThenIdentifier()
        {
            Assert.Equal("Number Ident Eof", Kinds("2x"));
        }
 
        [Fact]
        public void Tokenize_NullOrEmpty_ProducesOnlyEof()
        {
            Assert.Equal("Eof", Kinds(""));
            Assert.Equal("Eof", string.Join(" ", AstLexer.Tokenize(null).Tokens.Select(t => t.Kind)));
            Assert.True(AstLexer.Tokenize("").Ok);
        }
 
        [Fact]
        public void Tokenize_TokensAfterNewline_HaveLineAndColumn()
        {
            var tokens = AstLexer.Tokenize("a\n  b").Tokens;
 
            Assert.Equal(1u, tokens[0].Pos.Line);
            Assert.Equal(1u, tokens[0].Pos.Column);
            Assert.Equal(2u, tokens[1].Pos.Line);
            Assert.Equal(3u, tokens[1].Pos.Column);
            Assert.Equal(4u, tokens[1].Pos.Index);
        }
 
        [Fact]
        public void Tokenize_SeveralBadCharacters_CollectsAllErrorsWithPositions()
        {
            var result = AstLexer.Tokenize("x = 1;\ny := 2 & 3 @");
 
            Assert.False(result.Ok);
            Assert.Equal(3, result.Errors.Count);
            Assert.Contains("Одиночный '='", result.Errors[0].Message);
            Assert.Equal(1u, result.Errors[0].Pos.Line);
            Assert.Equal(3u, result.Errors[0].Pos.Column);
            Assert.Contains("Одиночный '&'", result.Errors[1].Message);
            Assert.Equal(2u, result.Errors[1].Pos.Line);
            Assert.Equal(8u, result.Errors[1].Pos.Column);
            Assert.Contains("'@'", result.Errors[2].Message);
            Assert.Equal(12u, result.Errors[2].Pos.Column);
        }
 
        [Theory]
        [InlineData("x : 1", "':' без '='")]
        [InlineData("a | b", "Одиночный '|'")]
        [InlineData("a $ b", "Неожиданный символ '$'")]
        public void Tokenize_BadCharacter_GivesHelpfulMessage(string source, string expectedPart)
        {
            var result = AstLexer.Tokenize(source);
 
            Assert.False(result.Ok);
            Assert.Contains(expectedPart, result.Errors[0].Message);
        }
}