namespace AudioPrep.Core.Models;

public sealed record MediaInfo(
    string Path,
    string FileName,
    string ContainerFormat,
    TimeSpan Duration,
    IReadOnlyList<AudioStreamInfo> AudioStreams);
