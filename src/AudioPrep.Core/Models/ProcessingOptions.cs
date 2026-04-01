namespace AudioPrep.Core.Models;

public sealed class ProcessingOptions
{
    public required int AudioStreamIndex { get; init; }

    public required string InputPath { get; init; }

    public required string OutputPath { get; init; }

    public required AudioCodecKind CodecKind { get; init; }

    public required int SampleRate { get; init; }

    public required int Channels { get; init; }

    public required bool NormalizeLoudness { get; init; }

    public required bool TrimSilence { get; init; }

    public TimeSpan? InputDuration { get; init; }
}
