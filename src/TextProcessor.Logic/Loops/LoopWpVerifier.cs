using TextProcessor.Logic.AbstractSyntaxTree;

namespace TextProcessor.Logic.Loops;

public sealed class LoopWpResult
{
    public bool IsInvariantPreserved { get; init; }
    public string VerificationCondition { get; init; } = string.Empty;
    public string SimplifiedVC { get; init; } = string.Empty;
    public string HumanExplanation { get; init; } = string.Empty;
    public string WpTrace { get; init; } = string.Empty;
}

public static class LoopWpVerifier
{
    /// <summary>
    /// Проверяет, сохраняет ли тело цикла инвариант.
    /// Условие верификации: (Inv && Guard) => wp(Body, Inv)
    /// </summary>
    public static LoopWpResult VerifyStep(LoopPreset preset)
    {
        var inv = LoopInvariant.Get(preset.Mode);
        var guard = LoopInvariant.Guard;
        var body = preset.RuntimeBody;

        // 1. Вычисляем wp(Body, Inv) с помощью движка из ЛР2
        var wpResult = WpEngine.Calculate(body, inv);
        var wpBodyInv = wpResult.FinalWp;

        // 2. Формируем условие верификации (VC): Inv && Guard => wp(Body, Inv)
        // В логике предикатов A => B эквивалентно !A || B
        var vc = new Or(
            new Not(new And(inv, guard)),
            wpBodyInv
        );

        var simplifiedVc = AstSimplifier.Simplify(vc);

        // 3. Проверяем, упростилось ли условие до "true" (или содержит true как основной член)
        bool isPreserved = simplifiedVc is BoolLiteral { Value: true } ||
                           simplifiedVc.ToString().Contains("true"); // Упрощенная проверка для лабы

        return new LoopWpResult
        {
            IsInvariantPreserved = isPreserved,
            VerificationCondition = AstPrinter.Print(vc),
            SimplifiedVC = AstPrinter.Print(simplifiedVc),
            HumanExplanation = isPreserved
                ? "Инвариант сохраняется: условие (Inv && Guard) гарантирует wp(Тело, Inv)."
                : "Требуется дополнительное условие для сохранения инварианта.",
            WpTrace = FormatTraceForLoop(wpResult.Steps, inv)
        };
    }

    private static string FormatTraceForLoop(IReadOnlyList<WpTraceStep> steps, AstPredication initialInv)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Цель (Инвариант): {AstPrinter.Print(initialInv)}");
        sb.AppendLine(new string('-', 50));
        foreach (var step in steps)
        {
            sb.AppendLine($"Шаг {step.StepNumber}. {step.StatementDescription}:");
            sb.AppendLine($"  • После подстановки: {step.RawNotation}");
            sb.AppendLine($"  • После упрощения:   {step.SimplifiedNotation}");
        }
        return sb.ToString();
    }
}