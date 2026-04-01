namespace AudioPrep.Core.Models;

public sealed record AudioStreamInfo(
    int Index,
    string Codec,
    string? Language,
    int Channels,
    int SampleRate,
    string? Title)
{
    public string DisplayName
    {
        get
        {
            var parts = new List<string>
            {
                $"Stream {Index}",
                string.IsNullOrWhiteSpace(Codec) ? "unknown" : Codec
            };

            if (!string.IsNullOrWhiteSpace(Language))
            {
                parts.Add(Language);
            }

            if (Channels > 0)
            {
                parts.Add($"{Channels}ch");
            }

            if (SampleRate > 0)
            {
                parts.Add($"{SampleRate} Hz");
            }

            if (!string.IsNullOrWhiteSpace(Title))
            {
                parts.Add(Title);
            }

            return string.Join(" | ", parts);
        }
    }
}
