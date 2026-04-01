using AudioPrep.Core.Models;
using AudioPrep.Infrastructure.Persistence;

namespace AudioPrep.Tests.Persistence;

public sealed class JsonSettingsServiceTests
{
    [Fact]
    public void Load_WhenFileDoesNotExist_ShouldReturnDefaults()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "audioprep-tests-" + Guid.NewGuid());
        var settingsPath = Path.Combine(tempDirectory, "settings.json");
        var service = new JsonSettingsService(settingsPath);

        var settings = service.Load();

        Assert.NotNull(settings);
        Assert.Null(settings.LastOutputDirectory);
        Assert.Equal(48000, settings.LastSampleRate);
    }

    [Fact]
    public void SaveAndLoad_ShouldRoundTripSettings()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "audioprep-tests-" + Guid.NewGuid());
        var settingsPath = Path.Combine(tempDirectory, "settings.json");
        var service = new JsonSettingsService(settingsPath);

        var expected = new AppSettings
        {
            LastOutputDirectory = @"C:\exports",
            LastPresetId = "mp3-hq",
            LastSampleRate = 44100,
            NormalizeLoudness = true,
            TrimSilence = true,
            DownmixToMono = false,
            UseAdvancedMode = true,
            LastLanguage = "PortugueseBrazil",
            WindowWidth = 1000,
            WindowHeight = 720
        };

        service.Save(expected);
        var actual = service.Load();

        Assert.Equal(expected.LastOutputDirectory, actual.LastOutputDirectory);
        Assert.Equal(expected.LastPresetId, actual.LastPresetId);
        Assert.Equal(expected.LastSampleRate, actual.LastSampleRate);
        Assert.Equal(expected.NormalizeLoudness, actual.NormalizeLoudness);
        Assert.Equal(expected.TrimSilence, actual.TrimSilence);
        Assert.Equal(expected.DownmixToMono, actual.DownmixToMono);
        Assert.Equal(expected.UseAdvancedMode, actual.UseAdvancedMode);
        Assert.Equal(expected.LastLanguage, actual.LastLanguage);
        Assert.Equal(expected.WindowWidth, actual.WindowWidth);
        Assert.Equal(expected.WindowHeight, actual.WindowHeight);

        Directory.Delete(tempDirectory, recursive: true);
    }
}
