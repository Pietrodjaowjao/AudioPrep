using AudioPrep.Core.Utilities;

namespace AudioPrep.Tests.Utilities;

public sealed class OutputFileNameGeneratorTests
{
    [Fact]
    public void Suggest_ShouldUseInputNameAndPresetExtension()
    {
        var fileName = OutputFileNameGenerator.Suggest(@"C:\media\clip.mp4", "wav");

        Assert.Equal("clip_prepared.wav", fileName);
    }

    [Fact]
    public void EnsureExtension_ShouldReplaceDifferentExtension()
    {
        var fileName = OutputFileNameGenerator.EnsureExtension("clip.mp3", "m4a");

        Assert.Equal("clip.m4a", fileName);
    }

    [Fact]
    public void EnsureExtension_ShouldFallbackWhenNameIsEmpty()
    {
        var fileName = OutputFileNameGenerator.EnsureExtension(string.Empty, "wav");

        Assert.Equal("output.wav", fileName);
    }
}
