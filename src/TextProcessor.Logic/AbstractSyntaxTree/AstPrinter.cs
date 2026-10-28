using System.Globalization;

namespace TextProcessor.Logic.AbstractSyntaxTree;

public static class AstPrinter
{
    private const ushort PSum = 1, PMul = 2, PUnary = 3, PAtom = 4;
    private const ushort POr = 1, PAnd = 2, PNot = 3, PCmp = 4;
    
    private const string Indent = "    ";

    #region Арифметика
    private static (string, int) FmtSum(Bin b)
    {
        var left = Operand(b.First, PSum);
        return b.Second switch
        {
            Neg n => ($"{left} - {Operand(n.AstExpression, PMul)}", PSum),
            Num { Value: < 0 } num => ($"{left} - {FormatNumber(-num.Value)}", PSum),
            _ => ($"{left} + {Operand(b.Second, PMul)}", PSum),
        };
    }
 
    private static (string, int) FmtMulDiv(Bin b)
    {
        var op = b.Operator == ExpressionOperatorKind.Multiply ? "*" : "/";
        return ($"{Operand(b.First, PMul)} {op} {Operand(b.Second, PUnary)}", PMul);
    }
 
    private static string Operand(AstExpression e, int minPrec)
    {
        var (text, prec) = Fmt(e);
        return prec < minPrec ? $"({text})" : text;
    }
 
    private static string FormatNumber(decimal v) => v.ToString("G29", CultureInfo.InvariantCulture);
    
    private static (string Text, int Prec) Fmt(AstExpression e) => e switch
    {
        Num n => (FormatNumber(n.Value), n.Value < 0 ? PUnary : PAtom),
        Var v => (v.Name, PAtom),
        Abs a => ($"abs({Print(a.AstExpression)})", PAtom),
        Neg n => ("-" + Operand(n.AstExpression, PAtom), PUnary),
        Bin { Operator: ExpressionOperatorKind.Sum } b => FmtSum(b),
        Bin b => FmtMulDiv(b),
        _ => throw new ArgumentException($"Неизвестное выражение: {e.GetType().Name}"),
    };
    #endregion

    #region Логические операции
    private static (string Text, int Prec) Fmt(AstPredication p) => p switch
    {
        BoolLiteral b => (b.Value ? "true" : "false", PCmp),
        Cmp c => ($"{Print(c.First)} {CmpSymbol(c.Operator)} {Print(c.Second)}", PCmp),
        And a => ($"{Operand(a.First, PAnd)} && {Operand(a.Second, PNot)}", PAnd),
        Or o => ($"{Operand(o.First, POr)} || {Operand(o.Second, PAnd)}", POr),
        Not n => ("!" + (n.Predication is BoolLiteral or Not
            ? Fmt(n.Predication).Text
            : $"({Print(n.Predication)})"), PNot),
        _ => throw new ArgumentException($"Неизвестный предикат: {p.GetType().Name}"),
    };
 
    private static string Operand(AstPredication p, int minPrec)
    {
        var (text, prec) = Fmt(p);
        return prec < minPrec ? $"({text})" : text;
    }
 
    private static string CmpSymbol(PredicationOperatorKind k) => k switch
    {
        PredicationOperatorKind.Equal => "==",
        PredicationOperatorKind.NotEqual => "!=",
        PredicationOperatorKind.Less => "<",
        PredicationOperatorKind.LessOrEqual => "<=",
        PredicationOperatorKind.Greater => ">",
        PredicationOperatorKind.GreaterOrEqual => ">=",
        _ => throw new ArgumentException($"Неизвестный оператор сравнения: {k}"),
    };
    
    #endregion
    
    private static void Lines(AstStatement statement, uint level, List<string> sink)
    {
        var ind = string.Concat(Enumerable.Repeat(Indent, (int)level));
        switch (statement)
        {
            case Assign a:
                sink.Add($"{ind}{a.Name} := {Print(a.Expression)};");
                break;
            case Block b:
                foreach (var inner in b.Statements) Lines(inner, level, sink);
                break;
            case Condition c:
                sink.Add($"{ind}if ({Print(c.Predication)}) {{");
                Lines(c.Then, level + 1, sink);
                sink.Add($"{ind}}} else {{");
                Lines(c.Else, level + 1, sink);
                sink.Add($"{ind}}}");
                break;
            default:
                throw new ArgumentException($"Неизвестный оператор: {statement.GetType().Name}");
        }
    }
    
    public static string Print(AstExpression expression) => Fmt(expression).Text;
    public static string Print(AstPredication predication) => Fmt(predication).Text;
    public static string Print(AstStatement statement)
    {
        var lines = new List<string>();
        Lines(statement, 0, lines);
        return string.Join("\n", lines);
    }
    public static string PrintShort(AstStatement statement) => statement switch
    {
        Assign a => $"{a.Name} := {Print(a.Expression)}",
        Condition c => $"if ({Print(c.Predication)}) {{ … }} else {{ … }}",
        Block b => string.Join("; ", b.Statements.Select(PrintShort)),
        _ => throw new ArgumentException($"Неизвестный оператор: {statement.GetType().Name}"),
    };
}