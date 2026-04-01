using System.Globalization;
using AudioPrep.Core.Models;

namespace AudioPrep.Core.Commands;

public sealed class FfmpegArgumentBuilder
{
    public const string LoudnessFilter = "loudnorm=I=-16:LRA=11:TP=-1.5";

    public const string TrimSilenceFilter =
        "silenceremove=start_periods=1:start_silence=0.2:start_threshold=-45dB:stop_periods=1:stop_silence=0.2:stop_threshold=-45dB";

    public IReadOnlyList<string> BuildArguments(ProcessingOptions options)
    {
        var arguments = new List<string>
        {
            "-y",
            "-hide_banner",
            "-i",
            options.InputPath,
            "-map",
            $"0:{options.AudioStreamIndex}",
            "-vn",
            "-ac",
            options.Channels.ToString(CultureInfo.InvariantCulture),
            "-ar",
            options.SampleRate.ToString(CultureInfo.InvariantCulture)
        };

        var filterChain = BuildFilterChain(options);
        if (!string.IsNullOrWhiteSpace(filterChain))
        {
            arguments.Add("-af");
            arguments.Add(filterChain);
        }

        switch (options.CodecKind)
        {
            case AudioCodecKind.WavPcm16:
                arguments.Add("-c:a");
                arguments.Add("pcm_s16le");
                break;
            case AudioCodecKind.Mp3:
                arguments.Add("-c:a");
                arguments.Add("libmp3lame");
                arguments.Add("-q:a");
                arguments.Add("2");
                break;
            case AudioCodecKind.Aac:
                arguments.Add("-c:a");
                arguments.Add("aac");
                arguments.Add("-b:a");
                arguments.Add("192k");
                break;
            default:
                throw new NotSupportedException($"Codec '{options.CodecKind}' is not supported.");
        }

        arguments.Add(options.OutputPath);
        return arguments;
    }

    public string BuildFilterChain(ProcessingOptions options)
    {
        var filters = new List<string>();

        if (options.TrimSilence)
        {
            filters.Add(TrimSilenceFilter);
        }

        if (options.NormalizeLoudness)
        {
            filters.Add(LoudnessFilter);
        }

        return string.Join(",", filters);
    }
}
