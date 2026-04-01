namespace AudioPrep.Core.Models;

public sealed class AppSettings
{
    public string? LastOutputDirectory { get; set; }

    public string? LastPresetId { get; set; }

    public int LastSampleRate { get; set; } = 48000;

    public bool NormalizeLoudness { get; set; }

    public bool TrimSilence { get; set; }

    public bool DownmixToMono { get; set; }

    public bool UseAdvancedMode { get; set; }

    public string? LastLanguage { get; set; }

    public double? WindowWidth { get; set; }

    public double? WindowHeight { get; set; }
}
