using System.Globalization;

namespace TextProcessor.Logic.AbstractSyntaxTree;

public readonly record struct ParseError(string Message, SourcePos Pos);

public sealed record ParseResult<T>(T? Value, IReadOnlyList<ParseError> Errors) where T : class
{
    public bool Ok => Errors.Count == 0;
}

public sealed class SyntaxError(string message, SourcePos pos, uint index) : Exception(message)
{
    public SourcePos Pos { get; } = pos;
    public uint Index { get; } = index;
}

/// <summary>
/// Парсер Post-условий (предикатов) и фрагментов кода. Исключения наружу не выбрасывает:
/// лексические ошибки возвращаются списком, синтаксическая — первая найденная.
/// </summary>
/// <remarks>
/// Грамматика:
///   program = { stmt }
///   stmt    = IDENT ":=" expr ";" | "if" "(" pred ")" block "else" block
///   block   = "{" { stmt } "}"
///   pred    = and { "||" and };   and = not { "&amp;&amp;" not };   not = "!" not | atom
///   atom    = "true" | "false" | cmp | "(" pred ")"
///   cmp     = expr ("&lt;"|"&lt;="|"&gt;"|"&gt;="|"=="|"!=") expr
///   expr    = term { ("+"|"-") term };   term = unary { ("*"|"/") unary }
///   unary   = "-" unary | primary
///   primary = NUMBER | IDENT | "abs" "(" expr ")" | "(" expr ")"
/// </remarks>
public static class AstParser
{
    public static ParseResult<AstPredication> ParsePredication(LexResult lexResult) =>
        Run(lexResult, s => s.ParsePredication());
    
    public static ParseResult<Block> ParseProgram(LexResult lexResult) =>
        Run(lexResult, s => s.ParseStatements(until: TokenKind.Eof));
    
    private static ParseResult<T> Run<T>(LexResult lexResult, Func<State, T> parse) where T : class
    {
        if (!lexResult.Ok)
            return new ParseResult<T>(null, lexResult.Errors.Select(e => new ParseError(e.Message, e.Pos)).ToList());
 
        var state = new State(lexResult.Tokens);
        try
        {
            T value = parse(state);
            state.ExpectEnd();
            return new ParseResult<T>(value, Array.Empty<ParseError>());
        }
        catch (SyntaxError e)
        {
            return new ParseResult<T>(null, new[] { new ParseError(e.Message, e.Pos) });
        }
    }

    private sealed class State(IReadOnlyList<Token> tokens)
    {
        private uint pos;

        private Token Peek => tokens[(int)pos];
        private bool Check(TokenKind tokenKind) => Peek.Kind == tokenKind;
        private Token Next() => tokens[pos < tokens.Count  ? (int)pos++ : (int)pos];
        
        private SyntaxError Error(string message) => new(message, Peek.Pos, pos);
        
        private static string Describe(Token t) => t.Kind == TokenKind.Eof ? "конец ввода" : $"'{t.Text}'";
        
        public Token Expect(TokenKind kind, string what)
        {
            if (!Check(kind)) throw Error($"Ожидалось {what}, найдено {Describe(Peek)}");
            return Next();
        }
        
        public void ExpectEnd()
        {
            if (!Check(TokenKind.Eof)) throw Error($"Лишний текст после конца выражения: найдено {Describe(Peek)}");
        }
        
        public Block ParseStatements(TokenKind until)
        {
            var list = new List<AstStatement>();
            while (!Check(until) && !Check(TokenKind.Eof)) list.Add(ParseStatement());
            return new Block(list);
        }
 
        private Block ParseBlock()
        {
            Expect(TokenKind.LBrace, "'{'");
            var block = ParseStatements(until: TokenKind.RBrace);
            Expect(TokenKind.RBrace, "'}'");
            return block;
        }
 
        private AstStatement ParseStatement()
        {
            var t = Peek;
            switch (t.Kind)
            {
                case TokenKind.Ident:
                {
                    Next();
                    Expect(TokenKind.Assign, "':='");
                    var e = ParseExpression();
                    Expect(TokenKind.Semicolon, "';'");
                    return new Assign(t.Text, e);
                }
                case TokenKind.If:
                    return ParseCondition();
                case TokenKind.Loop:
                    throw Error("Циклы не поддерживаются");
                default:
                    throw Error($"Ожидался оператор (присваивание или if), найдено {Describe(t)}");
            }
        }
        
        private Condition ParseCondition()
        {
            Next();
            Expect(TokenKind.LParen, "'('");
            var cond = ParsePredication();
            Expect(TokenKind.RParen, "')'");
            var then = ParseBlock();
            if (!Check(TokenKind.Else))
                throw Error($"Ветвление должно быть в полной форме: после блока ожидалось 'else', найдено {Describe(Peek)}");
            Next();
            var @else = ParseBlock();
            return new Condition(cond, then, @else);
        }
        
        public AstPredication ParsePredication()
        {
            var left = ParseAnd();
            while (Check(TokenKind.Or)) { Next(); left = new Or(left, ParseAnd()); }
            return left;
        }
 
        private AstPredication ParseAnd()
        {
            var left = ParseNot();
            while (Check(TokenKind.And)) { Next(); left = new And(left, ParseNot()); }
            return left;
        }
 
        private AstPredication ParseNot()
        {
            if (!Check(TokenKind.Not)) return ParseAtom();
            Next();
            return new Not(ParseNot());
        }
 
        private AstPredication ParseAtom()
        {
            switch (Peek.Kind)
            {
                case TokenKind.True: Next(); return new BoolLiteral(true);
                case TokenKind.False: Next(); return new BoolLiteral(false);
                case TokenKind.LParen: return ParseParenthesized();
                default: return ParseComparison();
            }
        }
        
        private AstPredication ParseParenthesized()
        {
            uint start = pos;
            SyntaxError first;
            try { return ParseComparison(); }
            catch (SyntaxError e) { first = e; pos = start; }
 
            try
            {
                Next();   // (
                var inner = ParsePredication();
                Expect(TokenKind.RParen, "')'");
                return inner;
            }
            catch (SyntaxError second)
            {
                // сообщаем ошибку той попытки, что продвинулась дальше по тексту
                throw second.Index >= first.Index ? second : first;
            }
        }
 
        private Cmp ParseComparison()
        {
            var left = ParseExpression();
            var op = Peek.Kind switch
            {
                TokenKind.Eq => PredicationOperatorKind.Equal,
                TokenKind.Ne => PredicationOperatorKind.NotEqual,
                TokenKind.Lt => PredicationOperatorKind.Less,
                TokenKind.Le => PredicationOperatorKind.LessOrEqual,
                TokenKind.Gt => PredicationOperatorKind.Greater,
                TokenKind.Ge => PredicationOperatorKind.GreaterOrEqual,
                _ => throw Error($"Ожидался оператор сравнения (<, <=, >, >=, ==, !=), найдено {Describe(Peek)}"),
            };
            Next();
            return new Cmp(op, left, ParseExpression());
        }
        
        private AstExpression ParseExpression()
        {
            var left = ParseTerm();
            while (Check(TokenKind.Plus) || Check(TokenKind.Minus))
            {
                bool minus = Next().Kind == TokenKind.Minus;
                var right = ParseTerm();
                // вычитание = сумма с отрицанием (так задумано в ExpressionOperatorKind)
                left = new Bin(ExpressionOperatorKind.Sum, left, minus ? new Neg(right) : right);
            }
            return left;
        }
 
        private AstExpression ParseTerm()
        {
            var left = ParseUnary();
            while (Check(TokenKind.Star) || Check(TokenKind.Slash))
            {
                var op = Next().Kind == TokenKind.Star ? ExpressionOperatorKind.Multiply : ExpressionOperatorKind.Divide;
                left = new Bin(op, left, ParseUnary());
            }
            return left;
        }
 
        private AstExpression ParseUnary()
        {
            if (!Check(TokenKind.Minus)) return ParsePrimary();
            Next();
            return new Neg(ParseUnary());
        }
 
        private AstExpression ParsePrimary()
        {
            var t = Peek;
            switch (t.Kind)
            {
                case TokenKind.Number:
                    Next();
                    if (!decimal.TryParse(t.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
                        throw new SyntaxError($"Число '{t.Text}' слишком велико", t.Pos, pos - 1);
                    return new Num(value);
                case TokenKind.Ident:
                    Next();
                    return new Var(t.Text);
                case TokenKind.Abs:
                {
                    Next();
                    Expect(TokenKind.LParen, "'(' после abs");
                    var e = ParseExpression();
                    Expect(TokenKind.RParen, "')'");
                    return new Abs(e);
                }
                case TokenKind.LParen:
                {
                    Next();
                    var e = ParseExpression();
                    Expect(TokenKind.RParen, "')'");
                    return e;
                }
                default:
                    throw Error($"Ожидалось выражение, найдено {Describe(t)}");
            }
        }
    }
}