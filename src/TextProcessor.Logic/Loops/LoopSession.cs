using TextProcessor.Logic.AbstractSyntaxTree;

namespace TextProcessor.Logic.Loops;

/// <summary>
/// Выполнение одного цикла над снимком входного массива. Вызовы одной сессии выполняются последовательно
/// </summary>
public sealed class LoopSession
{
    private readonly List<LoopStep> steps = [];
    private readonly List<int> variantHistory = [];

    public LoopPreset Preset { get; }
    public IReadOnlyList<int> Values { get; }
    public int Threshold { get; }
    public LoopState InitialState { get; }
    public LoopState State { get; private set; }
    public IReadOnlyList<LoopStep> Steps { get; }
    public IReadOnlyList<int> VariantHistory { get; }
    public bool IsCompleted => State.Index == Values.Count;
    public int? CurrentIndex => IsCompleted ? null : State.Index;

    /// <summary>Результат проверки Post. До выхода из цикла значение равно null</summary>
    public bool? PostconditionHolds { get; private set; }

    public LoopSession(LoopMode mode, IEnumerable<int> values, int threshold = 0)
    {
        ArgumentNullException.ThrowIfNull(values);
        Preset = LoopPreset.Get(mode);
        Values = Array.AsReadOnly(values.ToArray());
        Threshold = threshold;

        int startIndex = mode == LoopMode.PrefixMax ? 1 : 0;
        if (Values.Count < startIndex) throw new ArgumentException("Для поиска максимума нужен хотя бы один элемент.", nameof(values));

        long startResult = startIndex == 1 ? Values[0] : 0;
        InitialState = new LoopState(startIndex, startResult, Values.Count - startIndex);
        State = InitialState;
        variantHistory.Add(InitialState.Variant);
        VariantHistory = variantHistory.AsReadOnly();
        Steps = steps.AsReadOnly();

        if (IsCompleted) PostconditionHolds = CheckPostcondition(State.Result);
    }

    /// <summary>Выполняет одну итерацию. После завершения возвращает null</summary>
    public LoopStep? Step()
    {
        if (IsCompleted)
            return null;

        LoopState before = State;
        int currentElement = Values[before.Index];
        var variables = new Dictionary<string, decimal>
        {
            ["j"] = before.Index,
            ["res"] = before.Result,
            ["T"] = Threshold,
            [LoopPreset.CurrentElementVariable] = currentElement
        };
        var updated = AstInterpreter.Execute(Preset.RuntimeBody, variables);
        int nextIndex = before.Index + 1;
        if (updated["j"] != nextIndex) throw new InvalidOperationException("Тело цикла должно увеличить j ровно на единицу.");

        var after = new LoopState(nextIndex, checked((long)updated["res"]), Values.Count - nextIndex);
        var step = new LoopStep(before, after, currentElement);
        if (!step.VariantDecreased) throw new InvalidOperationException("Вариант-функция должна строго убывать и оставаться неотрицательной.");

        steps.Add(step);
        variantHistory.Add(after.Variant);
        State = after;

        if (IsCompleted)
            PostconditionHolds = CheckPostcondition(after.Result);

        return step;
    }

    /// <summary>Выполняет оставшиеся итерации и возвращает итоговое состояние</summary>
    public LoopState RunToEnd()
    {
        while (!IsCompleted)
            Step();

        return State;
    }

    private bool CheckPostcondition(long result) => Preset.Mode switch
    {
        LoopMode.PrefixSum => result == Values.Sum(value => (long)value),
        LoopMode.CountAboveThreshold => result == Values.Count(value => value > Threshold),
        LoopMode.PrefixMax => result == Values.Max(),
        _ => throw new InvalidOperationException("Неизвестный режим цикла.")
    };
}
