using AudioPrep.Core.Exceptions;
using AudioPrep.Core.Models;
using AudioPrep.Core.Parsing;
using AudioPrep.Core.Services;

namespace AudioPrep.Infrastructure.Services;

public sealed class MediaProbeService : IMediaProbeService
{
    private readonly IProcessRunner _processRunner;
    private readonly IExternalToolResolver _toolResolver;
    private readonly FfprobeJsonParser _ffprobeJsonParser;

    public MediaProbeService(
        IProcessRunner processRunner,
        IExternalToolResolver toolResolver,
        FfprobeJsonParser ffprobeJsonParser)
    {
        _processRunner = processRunner;
        _toolResolver = toolResolver;
        _ffprobeJsonParser = ffprobeJsonParser;
    }

    public async Task<MediaInfo> ProbeAsync(string inputPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
        {
            throw new MediaProbeException("Input file was not found.");
        }

        var ffprobePath = _toolResolver.ResolveToolPath("ffprobe")
            ?? throw new ExternalToolNotFoundException("FFprobe");

        var request = new ProcessRequest(
            ExecutablePath: ffprobePath,
            Arguments: new[]
            {
                "-v", "error",
                "-print_format", "json",
                "-show_format",
                "-show_streams",
                inputPath
            });

        ProcessResult processResult;
        try
        {
            processResult = await _processRunner
                .RunAsync(request, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MediaProbeException("Failed to run FFprobe.", ex.Message, ex);
        }

        if (processResult.ExitCode != 0)
        {
            throw new MediaProbeException("FFprobe could not read this file.", processResult.StandardError);
        }

        MediaInfo mediaInfo;
        try
        {
            mediaInfo = _ffprobeJsonParser.Parse(inputPath, processResult.StandardOutput);
        }
        catch (MediaProbeException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MediaProbeException("Could not parse FFprobe output.", ex.Message, ex);
        }

        if (mediaInfo.AudioStreams.Count == 0)
        {
            throw new NoAudioStreamsException();
        }

        return mediaInfo;
    }
}
