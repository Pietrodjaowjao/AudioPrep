using AudioPrep.Core.Commands;
using AudioPrep.Core.Models;

namespace AudioPrep.Tests.Commands;

public sealed class FfmpegArgumentBuilderTests
{
    private readonly FfmpegArgumentBuilder _builder = new();

    [Fact]
    public void BuildArguments_ForWavMono_ShouldMatchExpectedShape()
    {
        var options = CreateOptions(
            codecKind: AudioCodecKind.WavPcm16,
            channels: 1,
            sampleRate: 48000,
            normalize: false,
            trimSilence: false);

        var arguments = _builder.BuildArguments(options);

        Assert.Contains("-map", arguments);
        Assert.Contains("0:1", arguments);
        Assert.Contains("-vn", arguments);
        Assert.Contains("pcm_s16le", arguments);
        Assert.DoesNotContain("-af", arguments);
        Assert.Equal("output.wav", arguments[^1]);
    }

    [Fact]
    public void BuildArguments_ForMp3_ShouldIncludeLameAndQuality()
    {
        var options = CreateOptions(
            codecKind: AudioCodecKind.Mp3,
            channels: 2,
            sampleRate: 48000,
            normalize: true,
            trimSilence: true);

        var arguments = _builder.BuildArguments(options);

        Assert.Contains("libmp3lame", arguments);
        Assert.Contains("-q:a", arguments);
        Assert.Contains("2", arguments);
        Assert.Contains("-af", arguments);
    }

    [Fact]
    public void BuildFilterChain_WhenNormalizeAndTrimEnabled_ShouldUseDeterministicOrder()
    {
        var options = CreateOptions(
            codecKind: AudioCodecKind.Aac,
            channels: 2,
            sampleRate: 48000,
            normalize: true,
            trimSilence: true);

        var filterChain = _builder.BuildFilterChain(options);

        Assert.Equal($"{FfmpegArgumentBuilder.TrimSilenceFilter},{FfmpegArgumentBuilder.LoudnessFilter}", filterChain);
    }

    [Fact]
    public void BuildFilterChain_WhenNoFlagsEnabled_ShouldBeEmpty()
    {
        var options = CreateOptions(
            codecKind: AudioCodecKind.Aac,
            channels: 2,
            sampleRate: 48000,
            normalize: false,
            trimSilence: false);

        var filterChain = _builder.BuildFilterChain(options);

        Assert.Equal(string.Empty, filterChain);
    }

    private static ProcessingOptions CreateOptions(
        AudioCodecKind codecKind,
        int channels,
        int sampleRate,
        bool normalize,
        bool trimSilence)
    {
        return new ProcessingOptions
        {
            AudioStreamIndex = 1,
            InputPath = "input.mp4",
            OutputPath = codecKind switch
            {
                AudioCodecKind.WavPcm16 => "output.wav",
                AudioCodecKind.Mp3 => "output.mp3",
                AudioCodecKind.Aac => "output.m4a",
                _ => "output.wav"
            },
            CodecKind = codecKind,
            SampleRate = sampleRate,
            Channels = channels,
            NormalizeLoudness = normalize,
            TrimSilence = trimSilence,
            InputDuration = TimeSpan.FromSeconds(120)
        };
    }
}
