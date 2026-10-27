namespace TextProcessor.Logic.AbstractSyntaxTree;

/// <summary>
/// Один шаг трассировки вычисления слабейшего предусловия (wp).
/// </summary>
public sealed record WpTraceStep(
    int StepNumber,
    string StatementDescription,
    AstPredication RawCondition,
    AstPredication SimplifiedCondition,
    string RawNotation,
    string SimplifiedNotation,
    string HumanDescription);

/// <summary>
/// Итог работы wp-движка: финальное предусловие и список шагов.
/// </summary>
public sealed record WpEngineResult(
    AstPredication FinalWp,
    IReadOnlyList<WpTraceStep> Steps);

/// <summary>
/// Движок вычисления слабейшего предусловия wp(S, R).
/// </summary>
public static class WpEngine
{
    public static WpEngineResult Calculate(AstStatement program, AstPredication postCondition)
    {
        var steps = new List<WpTraceStep>();
        var simplifiedPost = AstSimplifier.Simplify(postCondition);
        var finalWp = ComputeWp(program, simplifiedPost, steps);
        return new WpEngineResult(finalWp, steps);
    }

    private static AstPredication ComputeWp(AstStatement statement, AstPredication post, List<WpTraceStep> steps)
    {
        switch (statement)
        {
            case Assign assign:
                {
                    // wp(x := e, R) = Def(e) && R[x -> e]
                    var defCond = AstSubstitutionAndDef.GetDefinedness(assign.Expression);
                    var substituted = AstSubstitutionAndDef.Substitute(post, assign.Name, assign.Expression);

                    AstPredication rawWp = defCond is BoolLiteral { Value: true }
                        ? substituted
                        : new And(defCond, substituted);

                    AstPredication simplifiedWp = AstSimplifier.Simplify(rawWp);

                    AddStep(steps, $"Через присваивание «{AstPrinter.PrintShort(assign)}»", rawWp, simplifiedWp);
                    return simplifiedWp;
                }

            case Block block:
                {
                    // wp(S1; S2, R) = wp(S1, wp(S2, R)) — идём с конца к началу
                    AstPredication current = post;
                    for (int i = block.Statements.Count - 1; i >= 0; i--)
                    {
                        current = ComputeWp(block.Statements[i], current, steps);
                    }
                    return current;
                }

            case Condition cond:
                {
                    // wp(if B then S1 else S2, R) = Def(B) && ((B && wp(S1, R)) || (!B && wp(S2, R)))
                    var thenSteps = new List<WpTraceStep>();
                    var elseSteps = new List<WpTraceStep>();

                    var wpThen = ComputeWp(cond.Then, post, thenSteps);
                    var wpElse = ComputeWp(cond.Else, post, elseSteps);

                    foreach (var s in thenSteps)
                        AddStep(steps, $"[Ветка then] {s.StatementDescription}", s.RawCondition, s.SimplifiedCondition);

                    foreach (var s in elseSteps)
                        AddStep(steps, $"[Ветка else] {s.StatementDescription}", s.RawCondition, s.SimplifiedCondition);

                    var defB = AstSubstitutionAndDef.GetDefinedness(cond.Predication);
                    var branchesOr = new Or(
                        new And(cond.Predication, wpThen),
                        new And(new Not(cond.Predication), wpElse));

                    AstPredication rawWp = defB is BoolLiteral { Value: true }
                        ? branchesOr
                        : new And(defB, branchesOr);

                    AstPredication simplifiedWp = AstSimplifier.Simplify(rawWp);

                    AddStep(steps, $"Через ветвление «{AstPrinter.PrintShort(cond)}»", rawWp, simplifiedWp);
                    return simplifiedWp;
                }

            default:
                throw new ArgumentException($"Неизвестный оператор: {statement.GetType().Name}");
        }
    }

    private static void AddStep(List<WpTraceStep> steps, string description, AstPredication raw, AstPredication simplified)
    {
        int stepNum = steps.Count + 1;
        steps.Add(new WpTraceStep(
            stepNum,
            description,
            raw,
            simplified,
            AstPrinter.Print(raw),
            AstPrinter.Print(simplified),
            AstHumanText.Describe(simplified)));
    }
}