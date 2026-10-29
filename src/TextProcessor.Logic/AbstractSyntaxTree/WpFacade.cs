using System.Text;

namespace TextProcessor.Logic.AbstractSyntaxTree;

/// <summary>
/// Описание готового сценария (пресета) для выпадающего списка в UI
/// </summary>
public sealed record WpPreset(
    string Title,
    string Code,
    string PostCondition,
    string Description);

/// <summary>
/// Готовый результат расчёта wp для привязки к элементам управления WPF
/// </summary>
public sealed class WpProcessResult
{
    public bool IsSuccess { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;

    public string PostShort { get; init; } = string.Empty;
    public string PostHuman { get; init; } = string.Empty;

    public string FinalWpShort { get; init; } = string.Empty;
    public string FinalWpHuman { get; init; } = string.Empty;

    public string TraceText { get; init; } = string.Empty;
    public IReadOnlyList<WpTraceStep> Steps { get; init; } = Array.Empty<WpTraceStep>();

    public string TriadNotation { get; init; } = string.Empty;
    public string TriadWords { get; init; } = string.Empty;
}

/// <summary>
/// Единая точка входа (Фасад) для расчёта слабейшего предусловия из UI и тестов.
/// </summary>
public static class WpFacade
{
    /// <summary>
    /// Три пресета
    /// </summary>
    public static IReadOnlyList<WpPreset> Presets { get; } = new List<WpPreset>
    {
        new(
            Title: "1. Max из двух",
            Code: "if (x1 >= x2) {\n    max := x1;\n} else {\n    max := x2;\n}",
            PostCondition: "max > 100",
            Description: "Вычисление максимума из двух чисел с целью max > 100."
        ),
        new(
            Title: "2. Квадратное уравнение (ветвление по D)",
            Code: "if (D >= 0) {\n    x := (-b + s) / (2 * a);\n    hasRoot := 1;\n} else {\n    x := -1;\n    hasRoot := 0;\n}",
            PostCondition: "(hasRoot == 1 && x == (-b + s) / (2 * a)) || (hasRoot == 0 && x == -1)",
            Description: "Если D >= 0, вычисляется корень (добавляется условие определённости a != 0), иначе кладётся спец. значение."
        ),
        new(
            Title: "3. Последовательность присваиваний",
            Code: "y := x + 1;\nx := x + 10;",
            PostCondition: "y == x - 9 && x > 15",
            Description: "Протягивание цели y == x - 9 && x > 15 назад через цепочку присваиваний до условия x > 5."
        )
    };

    /// <summary>
    /// Парсит входной код и постусловие, вычисляет wp, строит пошаговый трейс и триаду Хоара.
    /// </summary>
    public static WpProcessResult Calculate(string? codeText, string? postConditionText)
    {
        if (string.IsNullOrWhiteSpace(codeText))
            return Fail("Код фрагмента программы не может быть пустым.");

        if (string.IsNullOrWhiteSpace(postConditionText))
            return Fail("Постусловие (Post) не может быть пустым.");

        // Отладка: показываем что пришло
        System.Diagnostics.Debug.WriteLine($"[FACADE] Постусловие (длина {postConditionText.Length}): '{postConditionText}'");
        System.Diagnostics.Debug.WriteLine($"[FACADE] Постусловие байты: {string.Join(" ", System.Text.Encoding.UTF8.GetBytes(postConditionText).Select(b => b.ToString()))}");

        // Разбор постусловия
        var postLex = AstLexer.Tokenize(postConditionText);

        // Отладка лексера
        System.Diagnostics.Debug.WriteLine($"[FACADE] Лекс постусловия: токенов={postLex.Tokens.Count}, ошибок={postLex.Errors.Count}");
        foreach (var token in postLex.Tokens)
        {
            System.Diagnostics.Debug.WriteLine($"[FACADE]   Токен: Kind={token.Kind}, Text='{token.Text}', Pos={token.Pos.Index}");
        }
        foreach (var error in postLex.Errors)
        {
            System.Diagnostics.Debug.WriteLine($"[FACADE]   Ошибка лексера: {error.Message} на позиции {error.Pos.Index}");
        }

        if (!postLex.Ok || postLex.Errors.Count > 0)
        {
            var err = postLex.Errors.FirstOrDefault();
            return Fail($"Лексическая ошибка в постусловии: {err.Message} (позиция {err.Pos.Index})");
        }

        var postParse = AstParser.ParsePredication(postLex);
        if (!postParse.Ok || postParse.Value is null)
        {
            var err = postParse.Errors[0];
            return Fail($"Ошибка в постусловии (строка {err.Pos.Line}, символ {err.Pos.Column}): {err.Message}");
        }

        // Разбор кода программы
        var progLex = AstLexer.Tokenize(codeText);
        var progParse = AstParser.ParseProgram(progLex);
        if (!progParse.Ok || progParse.Value is null)
        {
            var err = progParse.Errors[0];
            return Fail($"Ошибка в коде программы (строка {err.Pos.Line}, символ {err.Pos.Column}): {err.Message}");
        }

        // Запуск wp-движка
        var postAst = postParse.Value;
        var progAst = progParse.Value;
        var engineResult = WpEngine.Calculate(progAst, postAst);

        // Форматирование результатов
        var triad = AstHumanText.Triad(engineResult.FinalWp, progAst, postAst);
        string traceFormatted = FormatTrace(engineResult.Steps, postAst);

        return new WpProcessResult
        {
            IsSuccess = true,
            PostShort = AstPrinter.Print(postAst),
            PostHuman = AstHumanText.Describe(postAst),
            FinalWpShort = AstPrinter.Print(engineResult.FinalWp),
            FinalWpHuman = AstHumanText.Describe(engineResult.FinalWp),
            TraceText = traceFormatted,
            Steps = engineResult.Steps,
            TriadNotation = triad.Notation,
            TriadWords = triad.Words
        };
    }

    private static string FormatTrace(IReadOnlyList<WpTraceStep> steps, AstPredication initialPost)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Цель (Post): {AstPrinter.Print(initialPost)}");
        sb.AppendLine(new string('-', 50));

        foreach (var step in steps)
        {
            sb.AppendLine($"Шаг {step.StepNumber}. {step.StatementDescription}:");
            if (step.RawNotation != step.SimplifiedNotation)
            {
                sb.AppendLine($"  • После подстановки: {step.RawNotation}");
                sb.AppendLine($"  • После упрощения:   {step.SimplifiedNotation}");
            }
            else
            {
                sb.AppendLine($"  • Промежуточное условие: {step.SimplifiedNotation}");
            }
            sb.AppendLine($"  • Человеческим текстом:  {step.HumanDescription}");
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private static WpProcessResult Fail(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message
    };
}