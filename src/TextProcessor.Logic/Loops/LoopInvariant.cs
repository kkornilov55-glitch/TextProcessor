using TextProcessor.Logic.AbstractSyntaxTree;

namespace TextProcessor.Logic.Loops;

public static class LoopInvariant
{
    /// <summary>
    /// Генерирует инвариант цикла для заданного режима.
    /// Инвариант выражается через текущие переменные: res, j, n, T, currentElement
    /// </summary>
    public static AstPredication Get(LoopMode mode)
    {
        var j = new Var("j");
        var n = new Var("n");
        var res = new Var("res");
        var T = new Var("T");
        var currentElement = new Var("currentElement");

        return mode switch
        {
            // Инвариант: res содержит сумму обработанных элементов (упрощенно: res >= 0 и j <= n)
            // Для полноценной суммы нужен PrefixExpr, но для WP шага достаточно связать res_new и res_old
            LoopMode.PrefixSum => new And(
                new Cmp(PredicationOperatorKind.GreaterOrEqual, res, new Num(0)),
                new Cmp(PredicationOperatorKind.LessOrEqual, j, n)
            ),

            // Инвариант: количество элементов > T не может быть отрицательным и не превышает j
            LoopMode.CountAboveThreshold => new And(
                new Cmp(PredicationOperatorKind.GreaterOrEqual, res, new Num(0)),
                new Cmp(PredicationOperatorKind.LessOrEqual, res, j)
            ),

            // Инвариант: res является максимумом среди обработанных, поэтому res >= currentElement (если он был)
            LoopMode.PrefixMax => new Cmp(PredicationOperatorKind.GreaterOrEqual, res, currentElement),

            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
    }

    /// <summary>
    /// Условие продолжения цикла (Guard): j < n
    /// </summary>
    public static AstPredication Guard => new Cmp(PredicationOperatorKind.Less, new Var("j"), new Var("n"));

    /// <summary>
    /// Вариант-функция должна быть >= 0: n - j >= 0
    /// </summary>
    public static AstPredication VariantNonNegative =>
        new Cmp(PredicationOperatorKind.GreaterOrEqual, new Bin(ExpressionOperatorKind.Sum, new Var("n"), new Neg(new Var("j"))), new Num(0));
}