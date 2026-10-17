namespace TextProcessor.Logic.AbstractSyntaxTree;

/// <summary>
/// Оператор сравнения
/// </summary>
public enum PredicationOperatorKind
{
    Equal,
    NotEqual,
    Less,
    LessOrEqual,
    Greater,
    GreaterOrEqual,
}

/// <summary>
/// Выражения для сравнения
/// </summary>
public abstract record AstPredication();

/// <summary>
/// Выражение, что отождествлено с типом bool
/// </summary>
/// <param name="Value">Значение литерала</param>
public record BoolLiteral(bool Value) : AstPredication;

/// <summary>
/// Операция <paramref name="Operator"/> сравнения меж <paramref name="First"/> и <paramref name="Second"/>
/// </summary>
/// <param name="Operator">Операнд</param>
/// <param name="First">Первое арифметическое выражение</param>
/// <param name="Second">Второе арифметическое выражение</param>
public record Cmp(PredicationOperatorKind Operator, AstExpression First, AstExpression Second) : AstPredication;

/// <summary>
/// Операция конъюнкции меж выражениями сравнения <paramref name="First"/> и <paramref name="Second"/>
/// </summary>
/// <param name="First">Выражение сравнения</param>
/// <param name="Second">Выражение сравнения</param>
public record And(AstPredication First, AstPredication Second) : AstPredication;

/// <summary>
/// Операция дизъюнкции меж выражениями сравнения <paramref name="First"/> и <paramref name="Second"/>
/// </summary>
/// <param name="First">Выражение сравнения</param>
/// <param name="Second">Выражение сравнения</param>
public record Or(AstPredication First, AstPredication Second) : AstPredication;

/// <summary>
/// Операция отрицания выражения <paramref name="Predication"/>
/// </summary>
/// <param name="Predication">Выражение сравнения</param>
public record Not(AstPredication Predication) : AstPredication;