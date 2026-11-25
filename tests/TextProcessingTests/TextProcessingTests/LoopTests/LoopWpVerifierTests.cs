using System;
using TextProcessor.Logic.AbstractSyntaxTree;
using TextProcessor.Logic.Loops;
using Xunit;

namespace TextProcessingTests.Loops;

public class LoopWpVerifierTests
{
    // ===== Проверка структуры условия верификации (VC) =====

    [Theory]
    [InlineData(LoopMode.PrefixSum)]
    [InlineData(LoopMode.CountAboveThreshold)]
    [InlineData(LoopMode.PrefixMax)]
    public void VerifyStep_ShouldGenerateValidVerificationCondition(LoopMode mode)
    {
        var preset = LoopPreset.Get(mode);
        var result = LoopWpVerifier.VerifyStep(preset);

        // Проверяем, что условие верификации сформировано
        Assert.False(string.IsNullOrWhiteSpace(result.VerificationCondition));

        // VC имеет структуру импликации (A => B), которая в AST представляется как (!A || B)
        Assert.Contains("||", result.VerificationCondition);
        Assert.Contains("!", result.VerificationCondition);
    }

    [Theory]
    [InlineData(LoopMode.PrefixSum)]
    [InlineData(LoopMode.CountAboveThreshold)]
    [InlineData(LoopMode.PrefixMax)]
    public void VerifyStep_ShouldProduceDetailedWpTrace(LoopMode mode)
    {
        var preset = LoopPreset.Get(mode);
        var result = LoopWpVerifier.VerifyStep(preset);

        Assert.False(string.IsNullOrWhiteSpace(result.WpTrace));
        Assert.Contains("Цель (Инвариант):", result.WpTrace);
        Assert.Contains("Шаг 1.", result.WpTrace);
        Assert.Contains("После подстановки:", result.WpTrace);
        Assert.Contains("После упрощения:", result.WpTrace);
    }

    [Fact]
    public void VerifyStep_PrefixMax_ShouldHandleBranchingInTrace()
    {
        var preset = LoopPreset.Get(LoopMode.PrefixMax);
        var result = LoopWpVerifier.VerifyStep(preset);

        // Для PrefixMax в трейсе должны быть отражены вычисления для веток if/else
        Assert.Contains("if", result.WpTrace, StringComparison.OrdinalIgnoreCase);
    }

    // ===== Проверка пояснений и стабильности =====

    [Theory]
    [InlineData(LoopMode.PrefixSum)]
    [InlineData(LoopMode.CountAboveThreshold)]
    [InlineData(LoopMode.PrefixMax)]
    public void VerifyStep_ShouldProvideHumanExplanation(LoopMode mode)
    {
        var preset = LoopPreset.Get(mode);
        var result = LoopWpVerifier.VerifyStep(preset);

        Assert.False(string.IsNullOrWhiteSpace(result.HumanExplanation));
        // Объяснение должно быть осмысленным и достаточно длинным
        Assert.True(result.HumanExplanation.Length > 20);
    }

    [Fact]
    public void VerifyStep_AllModes_ShouldBeDeterministic()
    {
        var preset = LoopPreset.Get(LoopMode.CountAboveThreshold);

        var first = LoopWpVerifier.VerifyStep(preset);
        var second = LoopWpVerifier.VerifyStep(preset);

        Assert.Equal(first.VerificationCondition, second.VerificationCondition);
        Assert.Equal(first.SimplifiedVC, second.SimplifiedVC);
        Assert.Equal(first.IsInvariantPreserved, second.IsInvariantPreserved);
    }

    // ===== Проверка инвариантов =====

    [Fact]
    public void LoopInvariant_PrefixSum_ShouldBeConjunction()
    {
        var inv = LoopInvariant.Get(LoopMode.PrefixSum);
        Assert.IsType<And>(inv);
    }

    [Fact]
    public void LoopInvariant_PrefixMax_ShouldBeComparison()
    {
        var inv = LoopInvariant.Get(LoopMode.PrefixMax);
        Assert.IsType<Cmp>(inv);
    }

    [Fact]
    public void LoopInvariant_Guard_ShouldBeLessComparison()
    {
        var guard = LoopInvariant.Guard;
        Assert.IsType<Cmp>(guard);
        var cmp = (Cmp)guard;
        Assert.Equal(PredicationOperatorKind.Less, cmp.Operator);
    }
}