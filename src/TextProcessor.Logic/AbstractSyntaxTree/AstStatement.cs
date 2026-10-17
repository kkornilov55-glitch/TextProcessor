namespace TextProcessor.Logic.AbstractSyntaxTree;

/// <summary>
/// Императивная логика
/// </summary>
public abstract record AstStatement();

/// <summary>
/// Присвоение литералу значения выражения
/// </summary>
/// <param name="Name">Имя литерала</param>
/// <param name="Expression">Выражение</param>
public record Assign(string Name, AstExpression Expression) : AstStatement;

/// <summary>
/// Блок императивной логики
/// </summary>
/// <param name="Statements"></param>
public record Block(IReadOnlyList<AstStatement> Statements) : AstStatement;

/// <summary>
/// Ветвление в зависимости от условия, указанного в <paramref name="Predication"/>
/// </summary>
/// <param name="Predication">Логическое выражение</param>
/// <param name="Then">Если <paramref name="Predication"/> верно</param>
/// <param name="Else">Если <paramref name="Predication"/> неверно</param>
public record Condition(AstPredication Predication, Block Then, Block Else) : AstStatement;