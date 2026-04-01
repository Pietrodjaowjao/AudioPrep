using AudioPrep.Core.Models;

namespace AudioPrep.Core.Presets;

public static class OutputPresets
{
    public static readonly OutputPreset WavMono48k = new(
        Id: "wav-mono-48k",
        Name: "WAV (48 kHz, 16-bit PCM, mono)",
        Extension: "wav",
        CodecKind: AudioCodecKind.WavPcm16,
        DefaultSampleRate: 48000,
        DefaultChannels: 1,
        SupportsMono: true,
        SupportsStereo: false,
        Lossless: true);

    public static readonly OutputPreset WavStereo48k = new(
        Id: "wav-stereo-48k",
        Name: "WAV (48 kHz, 16-bit PCM, stereo)",
        Extension: "wav",
        CodecKind: AudioCodecKind.WavPcm16,
        DefaultSampleRate: 48000,
        DefaultChannels: 2,
        SupportsMono: true,
        SupportsStereo: true,
        Lossless: true);

    public static readonly OutputPreset Mp3HighQuality = new(
        Id: "mp3-hq",
        Name: "MP3 (high quality)",
        Extension: "mp3",
        CodecKind: AudioCodecKind.Mp3,
        DefaultSampleRate: 48000,
        DefaultChannels: 2,
        SupportsMono: true,
        SupportsStereo: true,
        Lossless: false);

    public static readonly OutputPreset AacM4a = new(
        Id: "aac-m4a",
        Name: "AAC/M4A",
        Extension: "m4a",
        CodecKind: AudioCodecKind.Aac,
        DefaultSampleRate: 48000,
        DefaultChannels: 2,
        SupportsMono: true,
        SupportsStereo: true,
        Lossless: false);

    public static IReadOnlyList<OutputPreset> All { get; } =
        new[] { WavMono48k, WavStereo48k, Mp3HighQuality, AacM4a };

    public static OutputPreset? FindById(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return All.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));
    }
}
