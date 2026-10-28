namespace TextProcessor.Logic.AbstractSyntaxTree;

/// <summary>Выражение или оператор не определены в данном состоянии (деление на ноль, неизвестная переменная, переполнение).</summary>
public sealed class EvalException(string message) : Exception(message);

/// <summary>
/// Класс интерпретатора
/// </summary>
public static class AstInterpreter
{
    /// <summary>
    /// Выполнить алгебраическое выражение
    /// </summary>
    /// <param name="expr">Выражение</param>
    /// <param name="state">Текущее состояние</param>
    /// <returns>Результат выражения</returns>
    /// <exception cref="EvalException">Рантайм исключения</exception>
    /// <exception cref="ArgumentException">Если выражение или оператор/операнд не верны</exception>
    public static decimal Evalute(AstExpression expr, IReadOnlyDictionary<string, decimal> state)
    {
        switch (expr)
        {
            case Num num:
                return num.Value;
            case Var var:
                return state.TryGetValue(var.Name, out var value)
                    ? value
                    : throw new EvalException($"'{var.Name}' не определена");
            case Neg neg:
                return -Evalute(neg.AstExpression, state);
            case Abs abs:
                return Math.Abs(Evalute(abs.AstExpression, state));
            case Bin bin:
                var first = Evalute(bin.First, state);
                var second = Evalute(bin.Second, state);
                var astOperator = bin.Operator;
                try
                {
                    return astOperator switch
                    {
                        ExpressionOperatorKind.Sum => first + second,
                        ExpressionOperatorKind.Multiply => first * second,
                        ExpressionOperatorKind.Divide => first / second,
                        _ => throw new ArgumentException($"Неизвестный оператор: {bin.Operator}"),
                    };
                }
                catch (DivideByZeroException)
                {
                    throw new EvalException("Деление на ноль");
                }
                catch (OverflowException)
                {
                    throw new EvalException("Переполнение при вычислении");
                }
            default:
                throw new ArgumentException($"Неизвестное выражение: {expr.GetType().Name}");
        }
    }

    /// <summary>
    /// Выполнить логическое выражение
    /// </summary>
    /// <param name="predication">Логическое выражение</param>
    /// <param name="state">Текущее состояние</param>
    /// <returns>Результат выражения</returns>
    /// <exception cref="EvalException">Рантайм исключения</exception>
    /// <exception cref="ArgumentException">Если выражение или оператор/операнд не верны</exception>
    public static bool Evalute(AstPredication predication, IReadOnlyDictionary<string, decimal> state)
    {
        switch (predication)
        {
            case BoolLiteral boolLiteral:
                return boolLiteral.Value;
            case Cmp compare:
            {
                var first = Evalute(compare.First, state);
                var second = Evalute(compare.Second, state);
                var astOperator = compare.Operator;

                return astOperator switch
                {
                    PredicationOperatorKind.Equal => first == second,
                    PredicationOperatorKind.NotEqual => first != second,
                    PredicationOperatorKind.Less => first < second,
                    PredicationOperatorKind.LessOrEqual => first <= second,
                    PredicationOperatorKind.Greater => first > second,
                    PredicationOperatorKind.GreaterOrEqual => first >= second,
                    _ => throw new ArgumentException($"Неизвестный оператор сравнения: {astOperator}"),
                };
            }
            case And and:
            {
                var first = Evalute(and.First, state);
                var second = Evalute(and.Second, state);
                return first && second;
            }
            case Or or:
            {
                var first = Evalute(or.First, state);
                var second = Evalute(or.Second, state);
                return first || second;
            }
            case Not not:
                return !Evalute(not.Predication, state);
            default:
                throw new ArgumentException($"Неизвестный предикат: {predication.GetType().Name}");
        }

    }
    
    /// <summary>
    /// Выполнить программу <paramref name="statement"/>
    /// </summary>
    /// <param name="statement">Программа</param>
    /// <param name="state">Состояние</param>
    /// <returns>Состояние выполненной программы</returns>
    public static Dictionary<string, decimal> Execute(AstStatement statement, IReadOnlyDictionary<string, decimal> state)
    {
        var next = new Dictionary<string, decimal>(state);
        Run(statement, next);
        return next;
    }
    
    /// <summary>
    /// Выполнить программу <paramref name="statement"/>
    /// </summary>
    /// <param name="statement">Программа</param>
    /// <param name="state">Входное состояние</param>
    /// <param name="result">Состояние выполненной программы</param>
    /// <returns>Выполнена ли программа без исключений?</returns>
    public static bool TryExecute(AstStatement statement, IReadOnlyDictionary<string, decimal> state,
        out Dictionary<string, decimal>? result)
    {
        try { result = Execute(statement, state); return true; }
        catch (EvalException) { result = null; return false; }
    }
    
    /// <summary>
    /// Выполнить программу <paramref name="statement"/>
    ///
    /// Состояние <paramref name="state"/> мутирует при выполнении.
    /// </summary>
    /// <param name="statement">Программа</param>
    /// <param name="state">Состояние</param>
    private static void Run(AstStatement statement, Dictionary<string, decimal> state)
    {
        switch (statement)
        {
            case Assign assign:
                state[assign.Name] = Evalute(assign.Expression, state);
                break;
            case Block block:
                foreach (var inner in block.Statements) Run(inner, state);
                break;
            case Condition condition:
                Run(Evalute(condition.Predication, state) ? condition.Then : condition.Else, state);
                break;
            default:
                throw new ArgumentException($"Неизвестный оператор: {statement.GetType().Name}");
        }
    }

}