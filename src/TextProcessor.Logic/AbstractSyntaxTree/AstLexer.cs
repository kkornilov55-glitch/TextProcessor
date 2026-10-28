using System.Text.RegularExpressions;

namespace TextProcessor.Logic.AbstractSyntaxTree;

public enum TokenKind
{
    Number, Ident,
    // ключевые слова
    If, Else, Abs, True, False, Loop,
    // операторы
    Assign,                       // :=
    Plus, Minus, Star, Slash,
    Lt, Le, Gt, Ge, Eq, Ne,       // < <= > >= == !=
    And, Or, Not,                 // && || !
    LParen, RParen, LBrace, RBrace, Semicolon,
    Eof
}

public readonly record struct SourcePos(uint Index, uint Line, uint Column);
public readonly record struct Token(TokenKind Kind, string Text, SourcePos Pos);
public readonly record struct LexError(string Message, SourcePos Pos);

public sealed record LexResult(IReadOnlyList<Token> Tokens, IReadOnlyList<LexError> Errors)
{
    public bool Ok => Errors.Count == 0;
}

public static class AstLexer
{
    // Упрощённый regex без IgnorePatternWhitespace
    private static readonly Regex TokenRe = new(
        @"\G(?:\s+|\d+(?:\.\d+)?|[\p{L}_][\p{L}\p{Nd}_]*|:=|<=|>=|==|!=|&&|\|\||[-+*/<>!(){};≤≥≠∧∨¬])",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Dictionary<string, TokenKind> Operators = new()
    {
        [":="] = TokenKind.Assign,
        ["≔"] = TokenKind.Assign,
        ["+"] = TokenKind.Plus,
        ["-"] = TokenKind.Minus,
        ["*"] = TokenKind.Star,
        ["/"] = TokenKind.Slash,
        ["<"] = TokenKind.Lt,
        ["<="] = TokenKind.Le,
        ["≤"] = TokenKind.Le,
        [">"] = TokenKind.Gt,
        [">="] = TokenKind.Ge,
        ["≥"] = TokenKind.Ge,
        ["=="] = TokenKind.Eq,
        ["!="] = TokenKind.Ne,
        ["≠"] = TokenKind.Ne,
        ["&&"] = TokenKind.And,
        [""] = TokenKind.And,
        ["||"] = TokenKind.Or,
        ["∨"] = TokenKind.Or,
        ["!"] = TokenKind.Not,
        ["¬"] = TokenKind.Not,
        ["("] = TokenKind.LParen,
        [")"] = TokenKind.RParen,
        ["{"] = TokenKind.LBrace,
        ["}"] = TokenKind.RBrace,
        [";"] = TokenKind.Semicolon,
    };

    private static readonly Dictionary<string, TokenKind> Keywords = new()
    {
        ["if"] = TokenKind.If,
        ["else"] = TokenKind.Else,
        ["abs"] = TokenKind.Abs,
        ["true"] = TokenKind.True,
        ["false"] = TokenKind.False,
    };

    private static string DescribeUnexpected(char c) => c switch
    {
        '=' => "Одиночный '=': для присваивания используйте ':=', для сравнения '=='",
        ':' => "':' без '=': присваивание записывается ':='",
        '&' => "Одиночный '&': используйте '&&'",
        '|' => "Одиночный '|': используйте '||'",
        _ => $"Неожиданный символ '{c}'",
    };

    public static LexResult Tokenize(string? source)
    {
        source ??= string.Empty;

        var tokens = new List<Token>();
        var errors = new List<LexError>();

        uint pos = 0, line = 1, lineStart = 0;
        while (pos < source.Length)
        {
            SourcePos here = new(pos, line, pos - lineStart + 1);
            var tokenMatch = TokenRe.Match(source, (int)pos);

            if (!tokenMatch.Success)
            {
                errors.Add(new LexError(DescribeUnexpected(source[(int)pos]), here));
                pos++;
                continue;
            }

            string matched = tokenMatch.Value;

            // Пропускаем пробелы
            if (string.IsNullOrWhiteSpace(matched))
            {
                for (uint i = 0; i < matched.Length; i++)
                    if (matched[(int)i] == '\n') { line++; lineStart = pos + i + 1; }
            }
            // Число
            else if (char.IsDigit(matched[0]))
            {
                tokens.Add(new(TokenKind.Number, matched, here));
            }
            // Идентификатор или ключевое слово
            else if (char.IsLetter(matched[0]) || matched[0] == '_')
            {
                tokens.Add(new Token(Keywords.GetValueOrDefault(matched, TokenKind.Ident), matched, here));
            }
            // Оператор
            else if (Operators.ContainsKey(matched))
            {
                tokens.Add(new Token(Operators[matched], matched, here));
            }
            else
            {
                errors.Add(new LexError($"Неизвестный оператор: '{matched}'", here));
            }

            pos += (uint)matched.Length;
        }

        tokens.Add(new Token(TokenKind.Eof, string.Empty, new SourcePos(pos, line, pos - lineStart + 1)));
        return new LexResult(tokens, errors);
    }
}