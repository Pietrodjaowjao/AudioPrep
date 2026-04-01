using AudioPrep.Core.Parsing;

namespace AudioPrep.Tests.Parsing;

public sealed class FfprobeJsonParserTests
{
    private const string SampleJson = """
        {
          "streams": [
            {
              "index": 0,
              "codec_name": "h264",
              "codec_type": "video"
            },
            {
              "index": 1,
              "codec_name": "aac",
              "codec_type": "audio",
              "channels": 2,
              "sample_rate": "48000",
              "tags": {
                "language": "eng",
                "title": "Main Mix"
              }
            },
            {
              "index": 2,
              "codec_name": "ac3",
              "codec_type": "audio",
              "channels": 6,
              "sample_rate": "48000",
              "tags": {
                "language": "spa"
              }
            }
          ],
          "format": {
            "format_name": "mov,mp4,m4a,3gp,3g2,mj2",
            "duration": "12.345"
          }
        }
        """;

    [Fact]
    public void Parse_ShouldReturnMediaInfoWithAudioStreams()
    {
        var parser = new FfprobeJsonParser();

        var result = parser.Parse(@"C:\media\movie.mp4", SampleJson);

        Assert.Equal("movie.mp4", result.FileName);
        Assert.Equal("mov", result.ContainerFormat);
        Assert.Equal(TimeSpan.FromSeconds(12.345), result.Duration);
        Assert.Equal(2, result.AudioStreams.Count);
        Assert.Equal(1, result.AudioStreams[0].Index);
        Assert.Equal("aac", result.AudioStreams[0].Codec);
        Assert.Equal("eng", result.AudioStreams[0].Language);
        Assert.Equal(2, result.AudioStreams[0].Channels);
        Assert.Equal(48000, result.AudioStreams[0].SampleRate);
    }
}
