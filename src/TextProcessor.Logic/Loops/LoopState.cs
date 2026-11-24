namespace TextProcessor.Logic.Loops;

/// <summary>Текущее значение j, накопленный res и целочисленная вариант-функция t</summary>
/// <param name="Index">Индекс следующего элемента, начиная с нуля. На выходе равен длине массива</param>
/// <param name="Result">Сумма, счётчик или максимум обработанного префикса</param>
/// <param name="Variant">Значение n − j.</param>
public readonly record struct LoopState(int Index, long Result, int Variant);

/// <summary>Результат одной итерации цикла</summary>
public readonly record struct LoopStep(LoopState Before, LoopState After, int ProcessedValue)
{
    public int ProcessedIndex => Before.Index;
    public bool VariantDecreased => After.Variant >= 0 && After.Variant < Before.Variant;
}
