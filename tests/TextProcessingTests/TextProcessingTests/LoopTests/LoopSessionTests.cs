using System;
using System.Linq;
using TextProcessor.Logic.Loops;
using Xunit;

namespace TextProcessingTests.Loops;

public class LoopSessionTests
{
    [Fact]
    public void LoopPreset_All_ShouldContainThreeModes()
    {
        Assert.Equal(3, LoopPreset.All.Count);
        Assert.Contains(LoopPreset.All, p => p.Mode == LoopMode.PrefixSum);
        Assert.Contains(LoopPreset.All, p => p.Mode == LoopMode.CountAboveThreshold);
        Assert.Contains(LoopPreset.All, p => p.Mode == LoopMode.PrefixMax);
    }

    [Theory]
    [InlineData(LoopMode.PrefixSum)]
    [InlineData(LoopMode.CountAboveThreshold)]
    [InlineData(LoopMode.PrefixMax)]
    public void LoopPreset_Get_ShouldReturnPresetWithParsedBody(LoopMode mode)
    {
        var preset = LoopPreset.Get(mode);
        Assert.NotNull(preset);
        Assert.Equal(mode, preset.Mode);
        Assert.False(string.IsNullOrWhiteSpace(preset.BodyCode));
    }

    [Fact]
    public void LoopSession_PrefixSum_InitialState_ShouldBeZeroed()
    {
        var session = new LoopSession(LoopMode.PrefixSum, new[] { 10, 20, 30 });
        Assert.Equal(0, session.InitialState.Index);
        Assert.Equal(0, session.InitialState.Result);
        Assert.Equal(3, session.InitialState.Variant);
        Assert.False(session.IsCompleted);
        Assert.Null(session.PostconditionHolds);
    }

    [Fact]
    public void LoopSession_PrefixMax_InitialState_ShouldStartFromFirstElement()
    {
        var session = new LoopSession(LoopMode.PrefixMax, new[] { 42, 10, 99 });
        Assert.Equal(1, session.InitialState.Index);
        Assert.Equal(42, session.InitialState.Result);
        Assert.Equal(2, session.InitialState.Variant);
    }

    [Fact]
    public void LoopSession_PrefixMax_EmptyArray_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new LoopSession(LoopMode.PrefixMax, Array.Empty<int>()));
    }

    [Fact]
    public void LoopSession_PrefixMax_SingleElement_ShouldBeCompletedImmediately()
    {
        var session = new LoopSession(LoopMode.PrefixMax, new[] { 7 });
        Assert.True(session.IsCompleted);
        Assert.True(session.PostconditionHolds);
        Assert.Null(session.Step());
    }

    [Fact]
    public void LoopSession_PrefixSum_StepByStep_ShouldAccumulate()
    {
        var session = new LoopSession(LoopMode.PrefixSum, new[] { 5, 10, 15 });

        var s1 = session.Step();
        Assert.NotNull(s1);
        Assert.Equal(5, s1!.Value.ProcessedValue);
        Assert.Equal(5, session.State.Result);
        Assert.True(s1.Value.VariantDecreased);

        session.RunToEnd();
        Assert.Equal(30, session.State.Result);
        Assert.True(session.IsCompleted);
        Assert.True(session.PostconditionHolds);
    }

    [Fact]
    public void LoopSession_CountAboveThreshold_ShouldCountOnlyGreaterThanT()
    {
        var values = new[] { 5, 15, 25, 3 };
        var session = new LoopSession(LoopMode.CountAboveThreshold, values, threshold: 10);
        session.RunToEnd();
        Assert.Equal(2, session.State.Result); 
        Assert.True(session.PostconditionHolds);
    }

    [Fact]
    public void LoopSession_VariantHistory_ShouldStrictlyDecreaseToZero()
    {
        var session = new LoopSession(LoopMode.PrefixSum, new[] { 1, 2, 3 });
        session.RunToEnd();
        Assert.Equal(new[] { 3, 2, 1, 0 }, session.VariantHistory);
    }

    [Fact]
    public void LoopStep_VariantDecreased_FalseWhenNotDecreased()
    {
        var before = new LoopState(2, 0, 5);
        var after = new LoopState(2, 0, 5);
        var step = new LoopStep(before, after, 0);
        Assert.False(step.VariantDecreased);
    }
}