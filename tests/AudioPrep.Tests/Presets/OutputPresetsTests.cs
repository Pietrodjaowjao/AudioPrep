using AudioPrep.Core.Presets;

namespace AudioPrep.Tests.Presets;

public sealed class OutputPresetsTests
{
    [Fact]
    public void All_ShouldContainExpectedPresets()
    {
        Assert.Equal(4, OutputPresets.All.Count);

        Assert.Contains(OutputPresets.All, p => p.Id == "wav-mono-48k" && p.Extension == "wav");
        Assert.Contains(OutputPresets.All, p => p.Id == "wav-stereo-48k" && p.Extension == "wav");
        Assert.Contains(OutputPresets.All, p => p.Id == "mp3-hq" && p.Extension == "mp3");
        Assert.Contains(OutputPresets.All, p => p.Id == "aac-m4a" && p.Extension == "m4a");
    }

    [Fact]
    public void FindById_ShouldMatchCaseInsensitive()
    {
        var preset = OutputPresets.FindById("MP3-HQ");

        Assert.NotNull(preset);
        Assert.Equal("mp3-hq", preset!.Id);
    }
}
