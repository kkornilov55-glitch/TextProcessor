using TextProcessor.Logic.AbstractSyntaxTree;

namespace TextProcessor.Logic.Loops;

public enum LoopMode
{
    PrefixSum,
    CountAboveThreshold,
    PrefixMax
}

public sealed class LoopPreset
{
    public const string GuardCode = "j < n";
    public const string VariantCode = "n - j";

    internal const string CurrentElementVariable = "currentElement";

    public static IReadOnlyList<LoopPreset> All { get; } = Array.AsReadOnly(new LoopPreset[]
    {
        new(LoopMode.PrefixSum, "Суммарное время обработки", """
            res := res + a[j];
            j := j + 1;
            """),
        new(LoopMode.CountAboveThreshold, "Заявки дольше порога", """
            if (a[j] > T) {
                res := res + 1;
            } else {
            }
            j := j + 1;
            """),
        new(LoopMode.PrefixMax, "Максимальное время обработки", """
            if (a[j] > res) {
                res := a[j];
            } else {
            }
            j := j + 1;
            """)
    });

    public LoopMode Mode { get; }
    public string Title { get; }

    /// <summary>Тело одного шага для экрана и вычисления wp</summary>
    public string BodyCode { get; }

    internal Block RuntimeBody { get; }

    private LoopPreset(LoopMode mode, string title, string bodyCode)
    {
        Mode = mode;
        Title = title;
        BodyCode = bodyCode;

        // Все чтения a[j] в пресетах происходят до изменения j
        // Скалярный интерпретатор получает значение этой ячейки перед шагом
        string scalarCode = bodyCode.Replace("a[j]", CurrentElementVariable, StringComparison.Ordinal);
        var parsed = AstParser.ParseProgram(AstLexer.Tokenize(scalarCode));
        RuntimeBody = parsed.Value ?? throw new InvalidOperationException($"Некорректное тело цикла «{title}»: {parsed.Errors[0].Message}");
    }

    public static LoopPreset Get(LoopMode mode) =>
        All.FirstOrDefault(preset => preset.Mode == mode) ?? throw new ArgumentOutOfRangeException(nameof(mode), mode, "Неизвестный режим цикла.");
}
