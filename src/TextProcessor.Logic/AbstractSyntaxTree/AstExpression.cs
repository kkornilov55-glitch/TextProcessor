namespace TextProcessor.Logic.AbstractSyntaxTree;

/// <summary>
/// Операторы для арифметических операций.
/// Для вычитания используйте суммирование с <see cref="Neg"/>
/// </summary>
public enum ExpressionOperatorKind
{
    Sum,
    Multiply,
    Divide,
}

/// <summary>
/// Здесь содержатся арифметические выражения, что будут использованы
/// для их вычисления, как бы это не звучало, но это
/// элементы абстрактного синтаксического дерева.
/// </summary>
public abstract record AstExpression;

/// <summary>
/// Число в выражении
/// </summary>
/// <param name="Value">значение числа</param>
public record Num(decimal Value) : AstExpression;

/// <summary>
/// Переменная в выражении
/// </summary>
/// <param name="Name">Именование переменной</param>
public record Var(string Name) : AstExpression;

/// <summary>
/// Делает результат выражение <paramref name="AstExpression"/> отрицательным
/// </summary>
/// <param name="AstExpression">Выражение</param>
public record Neg(AstExpression AstExpression) : AstExpression;

/// <summary>
/// Арифметическая операция над <paramref name="First"/> и <paramref name="Second"/>
/// </summary>
/// <param name="Operator">Оператор</param>
/// <param name="First">Первое выражение</param>
/// <param name="Second">Второе выражение</param>
public record Bin(ExpressionOperatorKind Operator, AstExpression First, AstExpression Second) : AstExpression;

/// <summary>
/// Извлечение модуля из <paramref name="AstExpression"/>
/// </summary>
/// <param name="AstExpression">Выражение, из которого нужно получить модуль</param>
public record Abs(AstExpression AstExpression) : AstExpression;

/// <summary>
/// Доступ к элементу массива: a[j]
/// </summary>
public record ArrAccess(string ArrayName, AstExpression Index) : AstExpression;

/// <summary>
/// Операции над префиксом массива (для инвариантов)
/// </summary>
public enum PrefixOpKind { Sum, CountAbove, Max }

/// <summary>
/// Префиксная операция, например: Sum(a, j) или CountAbove(a, j, T)
/// </summary>
public record PrefixExpr(PrefixOpKind Op, string ArrayName, AstExpression Bound, AstExpression? Threshold = null) : AstExpression;
