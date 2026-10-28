namespace TextProcessor.Logic.AbstractSyntaxTree;

/// <summary>Триада Хоара в двух видах: краткая запись и словами.</summary>
/// <param name="Notation">«{ Pre }», код фрагмента, «{ Post }» — строки разделены '\n'</param>
/// <param name="Words">Одно-два предложения для отчёта</param>
public sealed record TriadText(string Notation, string Words);

public static class AstHumanText
{
    /// <summary>
    /// Описать условие
    /// </summary>
    /// <param name="predication">Условие</param>
    /// <returns>Описание условия</returns>
    /// <exception cref="ArgumentException">Если передан неизвестный предикат</exception>
    public static string Describe(AstPredication predication) => predication switch
    {
        BoolLiteral { Value: true } => "всегда истинно (без ограничений)",
        BoolLiteral => "никогда не выполняется",
        Cmp c => $"{AstPrinter.Print(c.First)} {Word(c.Operator)} {AstPrinter.Print(c.Second)}",
        And a => Chain(AndOperands(a), " и ", x => x is Or),
        Or o => Chain(OrOperands(o), " или ", x => x is And),
        Not n => DescribeNot(n.Predication),
        _ => throw new ArgumentException($"Неизвестный предикат: {predication.GetType().Name}"),
    };
    
    /// <summary>
    /// Составляет триаду Хоара
    /// </summary>
    /// <param name="pre">Предусловие программы</param>
    /// <param name="program">Программа</param>
    /// <param name="post">Постусловие программы</param>
    /// <returns></returns>
    public static TriadText Triad(AstPredication pre, AstStatement program, AstPredication post)
    {
        var code = program is Block { Statements.Count: 0 } ? "(пустой фрагмент)" : AstPrinter.Print(program);
        var notation = $"{{ {AstPrinter.Print(pre)} }}\n{code}\n{{ {AstPrinter.Print(post)} }}";
 
        var words = pre switch
        {
            BoolLiteral { Value: true } =>
                $"Каковы бы ни были начальные значения переменных (ограничений на вход нет), " +
                $"после выполнения фрагмента гарантированно верно: {Describe(post)}.",
            BoolLiteral { Value: false } =>
                $"Не существует начальных значений, при которых выполнение фрагмента гарантирует: {Describe(post)}. " +
                "Слабейшее предусловие — «ложь»: при таком фрагменте цель недостижима.",
            _ =>
                $"Если перед выполнением фрагмента верно: {Describe(pre)}, " +
                $"то после его выполнения гарантированно верно: {Describe(post)}.",
        };
 
        return new TriadText(notation, words);
    }
    
    /// <summary>
    /// Описывает инверсивные случаи
    /// </summary>
    /// <param name="inner">Тело инверсии</param>
    /// <returns>Описание</returns>
    private static string DescribeNot(AstPredication inner) => inner switch
    {
        BoolLiteral b => Describe(new BoolLiteral(!b.Value)),
        Cmp c => Describe(new Cmp(Invert(c.Operator), c.First, c.Second)),
        Not n => Describe(n.Predication),
        _ => $"неверно, что ({Describe(inner)})",
    };
    
    private static string Chain(IEnumerable<AstPredication> items, string separator, Func<AstPredication, bool> needsParens) =>
        string.Join(separator, items.Select(i => needsParens(i) ? $"({Describe(i)})" : Describe(i)));
 
    private static IEnumerable<AstPredication> AndOperands(AstPredication p) =>
        p is And a ? AndOperands(a.First).Concat(AndOperands(a.Second)) : new[] { p };
 
    private static IEnumerable<AstPredication> OrOperands(AstPredication p) =>
        p is Or o ? OrOperands(o.First).Concat(OrOperands(o.Second)) : new[] { p };
    
    private static string Word(PredicationOperatorKind k) => k switch
    {
        PredicationOperatorKind.Equal => "равно",
        PredicationOperatorKind.NotEqual => "не равно",
        PredicationOperatorKind.Less => "меньше",
        PredicationOperatorKind.LessOrEqual => "меньше или равно",
        PredicationOperatorKind.Greater => "больше",
        PredicationOperatorKind.GreaterOrEqual => "больше или равно",
        _ => throw new ArgumentException($"Неизвестный оператор сравнения: {k}"),
    };
 
    private static PredicationOperatorKind Invert(PredicationOperatorKind k) => k switch
    {
        PredicationOperatorKind.Equal => PredicationOperatorKind.NotEqual,
        PredicationOperatorKind.NotEqual => PredicationOperatorKind.Equal,
        PredicationOperatorKind.Less => PredicationOperatorKind.GreaterOrEqual,
        PredicationOperatorKind.LessOrEqual => PredicationOperatorKind.Greater,
        PredicationOperatorKind.Greater => PredicationOperatorKind.LessOrEqual,
        PredicationOperatorKind.GreaterOrEqual => PredicationOperatorKind.Less,
        _ => throw new ArgumentException($"Неизвестный оператор сравнения: {k}"),
    };
}