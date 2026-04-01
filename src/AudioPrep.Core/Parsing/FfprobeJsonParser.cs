using System.Globalization;
using System.Text.Json;
using AudioPrep.Core.Exceptions;
using AudioPrep.Core.Models;

namespace AudioPrep.Core.Parsing;

public sealed class FfprobeJsonParser
{
    public MediaInfo Parse(string inputPath, string ffprobeJson)
    {
        if (string.IsNullOrWhiteSpace(ffprobeJson))
        {
            throw new MediaProbeException("FFprobe returned an empty response.");
        }

        try
        {
            using var document = JsonDocument.Parse(ffprobeJson);
            var root = document.RootElement;

            var containerFormat = ParseContainerFormat(root);
            var duration = ParseDuration(root);
            var streams = ParseAudioStreams(root);

            return new MediaInfo(
                Path: inputPath,
                FileName: Path.GetFileName(inputPath),
                ContainerFormat: containerFormat,
                Duration: duration,
                AudioStreams: streams);
        }
        catch (JsonException ex)
        {
            throw new MediaProbeException("FFprobe returned invalid JSON.", ex.Message, ex);
        }
    }

    private static string ParseContainerFormat(JsonElement root)
    {
        if (!root.TryGetProperty("format", out var formatElement))
        {
            return "unknown";
        }

        if (!formatElement.TryGetProperty("format_name", out var formatNameElement))
        {
            return "unknown";
        }

        var rawFormat = formatNameElement.GetString();
        if (string.IsNullOrWhiteSpace(rawFormat))
        {
            return "unknown";
        }

        return rawFormat.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault()
            ?? rawFormat;
    }

    private static TimeSpan ParseDuration(JsonElement root)
    {
        if (!root.TryGetProperty("format", out var formatElement))
        {
            return TimeSpan.Zero;
        }

        if (!formatElement.TryGetProperty("duration", out var durationElement))
        {
            return TimeSpan.Zero;
        }

        var rawDuration = durationElement.GetString();
        if (string.IsNullOrWhiteSpace(rawDuration))
        {
            return TimeSpan.Zero;
        }

        if (!double.TryParse(rawDuration, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
        {
            return TimeSpan.Zero;
        }

        if (seconds < 0)
        {
            return TimeSpan.Zero;
        }

        return TimeSpan.FromSeconds(seconds);
    }

    private static IReadOnlyList<AudioStreamInfo> ParseAudioStreams(JsonElement root)
    {
        if (!root.TryGetProperty("streams", out var streamsElement) || streamsElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<AudioStreamInfo>();
        }

        var streams = new List<AudioStreamInfo>();
        foreach (var streamElement in streamsElement.EnumerateArray())
        {
            var codecType = ReadString(streamElement, "codec_type");
            if (!string.Equals(codecType, "audio", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var tags = streamElement.TryGetProperty("tags", out var tagsElement) ? tagsElement : default;
            streams.Add(new AudioStreamInfo(
                Index: ReadInt(streamElement, "index"),
                Codec: ReadString(streamElement, "codec_name") ?? "unknown",
                Language: ReadString(tags, "language"),
                Channels: ReadInt(streamElement, "channels"),
                SampleRate: ReadInt(streamElement, "sample_rate"),
                Title: ReadString(tags, "title")));
        }

        return streams.OrderBy(s => s.Index).ToList();
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            _ => null
        };
    }

    private static int ReadInt(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Null)
        {
            return 0;
        }

        if (!element.TryGetProperty(propertyName, out var property))
        {
            return 0;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var intValue))
        {
            return intValue;
        }

        if (property.ValueKind == JsonValueKind.String &&
            int.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var stringValue))
        {
            return stringValue;
        }

        return 0;
    }
}
