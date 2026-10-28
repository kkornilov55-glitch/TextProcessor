namespace TextProcessor.Logic.AbstractSyntaxTree;

/// <summary>
/// Подстановка выражений вместо переменных и извлечение условий определённости (Def).
/// </summary>
public static class AstSubstitutionAndDef
{
    /// <summary>
    /// Заменяет все вхождения переменной <paramref name="variableName"/> на <paramref name="replacement"/> в арифметическом выражении.
    /// </summary>
    public static AstExpression Substitute(AstExpression expr, string variableName, AstExpression replacement) => expr switch
    {
        Num num => num,
        Var v => v.Name == variableName ? replacement : v,
        Neg neg => new Neg(Substitute(neg.AstExpression, variableName, replacement)),
        Abs abs => new Abs(Substitute(abs.AstExpression, variableName, replacement)),
        Bin bin => new Bin(
            bin.Operator,
            Substitute(bin.First, variableName, replacement),
            Substitute(bin.Second, variableName, replacement)),
        _ => throw new ArgumentException($"Неизвестное выражение: {expr.GetType().Name}")
    };

    /// <summary>
    /// Заменяет все вхождения переменной <paramref name="variableName"/> на <paramref name="replacement"/> в предикате.
    /// </summary>
    public static AstPredication Substitute(AstPredication pred, string variableName, AstExpression replacement) => pred switch
    {
        BoolLiteral b => b,
        Cmp cmp => new Cmp(
            cmp.Operator,
            Substitute(cmp.First, variableName, replacement),
            Substitute(cmp.Second, variableName, replacement)),
        And and => new And(
            Substitute(and.First, variableName, replacement),
            Substitute(and.Second, variableName, replacement)),
        Or or => new Or(
            Substitute(or.First, variableName, replacement),
            Substitute(or.Second, variableName, replacement)),
        Not not => new Not(Substitute(not.Predication, variableName, replacement)),
        _ => throw new ArgumentException($"Неизвестный предикат: {pred.GetType().Name}")
    };

    /// <summary>
    /// Возвращает условие определённости (Def) для арифметического выражения:
    /// для каждого деления A / B добавляет требование B != 0.
    /// </summary>
    public static AstPredication GetDefinedness(AstExpression expr)
    {
        var conditions = new List<AstPredication>();
        CollectExprDef(expr, conditions);
        return CombineWithAnd(conditions);
    }

    /// <summary>
    /// Возвращает условие определённости (Def) для предиката.
    /// </summary>
    public static AstPredication GetDefinedness(AstPredication pred)
    {
        var conditions = new List<AstPredication>();
        CollectPredDef(pred, conditions);
        return CombineWithAnd(conditions);
    }

    private static void CollectExprDef(AstExpression expr, List<AstPredication> sink)
    {
        switch (expr)
        {
            case Num or Var:
                break;
            case Neg neg:
                CollectExprDef(neg.AstExpression, sink);
                break;
            case Abs abs:
                CollectExprDef(abs.AstExpression, sink);
                break;
            case Bin bin:
                CollectExprDef(bin.First, sink);
                CollectExprDef(bin.Second, sink);
                if (bin.Operator == ExpressionOperatorKind.Divide)
                {
                    var notZero = new Cmp(PredicationOperatorKind.NotEqual, bin.Second, new Num(0));
                    if (!sink.Contains(notZero))
                    {
                        sink.Add(notZero);
                    }
                }
                break;
        }
    }

    private static void CollectPredDef(AstPredication pred, List<AstPredication> sink)
    {
        switch (pred)
        {
            case BoolLiteral:
                break;
            case Cmp cmp:
                CollectExprDef(cmp.First, sink);
                CollectExprDef(cmp.Second, sink);
                break;
            case And and:
                CollectPredDef(and.First, sink);
                CollectPredDef(and.Second, sink);
                break;
            case Or or:
                CollectPredDef(or.First, sink);
                CollectPredDef(or.Second, sink);
                break;
            case Not not:
                CollectPredDef(not.Predication, sink);
                break;
        }
    }

    private static AstPredication CombineWithAnd(List<AstPredication> conditions)
    {
        if (conditions.Count == 0)
            return new BoolLiteral(true);

        AstPredication result = conditions[0];
        for (int i = 1; i < conditions.Count; i++)
        {
            result = new And(result, conditions[i]);
        }
        return result;
    }
}