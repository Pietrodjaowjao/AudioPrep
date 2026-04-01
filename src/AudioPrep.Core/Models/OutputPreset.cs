namespace AudioPrep.Core.Models;

public sealed record OutputPreset(
    string Id,
    string Name,
    string Extension,
    AudioCodecKind CodecKind,
    int DefaultSampleRate,
    int DefaultChannels,
    bool SupportsMono,
    bool SupportsStereo,
    bool Lossless);
