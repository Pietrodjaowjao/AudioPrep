using AudioPrep.Core.Commands;
using AudioPrep.Core.Exceptions;
using AudioPrep.Core.Models;
using AudioPrep.Core.Services;
using AudioPrep.Core.Validation;
using System.Globalization;

namespace AudioPrep.Infrastructure.Services;

public sealed class AudioProcessingService : IAudioProcessingService
{
    private readonly IProcessRunner _processRunner;
    private readonly IExternalToolResolver _toolResolver;
    private readonly FfmpegArgumentBuilder _argumentBuilder;

    public AudioProcessingService(
        IProcessRunner processRunner,
        IExternalToolResolver toolResolver,
        FfmpegArgumentBuilder argumentBuilder)
    {
        _processRunner = processRunner;
        _toolResolver = toolResolver;
        _argumentBuilder = argumentBuilder;
    }

    public async Task<ProcessingResult> ProcessAsync(
        ProcessingOptions options,
        Action<ProcessingProgress>? onProgress = null,
        Action<string>? onLog = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var validationErrors = ProcessingOptionsValidator.Validate(options);
        if (validationErrors.Count > 0)
        {
            throw new AudioProcessingException(string.Join(" ", validationErrors));
        }

        var ffmpegPath = _toolResolver.ResolveToolPath("ffmpeg")
            ?? throw new ExternalToolNotFoundException("FFmpeg");

        var outputDirectory = Path.GetDirectoryName(options.OutputPath);
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new AudioProcessingException("Output directory is invalid.");
        }

        Directory.CreateDirectory(outputDirectory);
        var arguments = _argumentBuilder.BuildArguments(options);
        var logBuffer = new List<string>();

        onProgress?.Invoke(new ProcessingProgress(0d, "Starting FFmpeg..."));

        void HandleProcessLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            lock (logBuffer)
            {
                logBuffer.Add(line);
            }

            onLog?.Invoke(line);
            TryReportProgress(line, options.InputDuration, onProgress);
        }

        ProcessResult processResult;
        try
        {
            processResult = await _processRunner
                .RunAsync(
                    new ProcessRequest(ffmpegPath, arguments, outputDirectory),
                    onStdOut: HandleProcessLine,
                    onStdErr: HandleProcessLine,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            onLog?.Invoke("Processing cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            throw new AudioProcessingException("Failed to run FFmpeg.", ex.Message, ex);
        }

        var combinedLogs = string.Join(Environment.NewLine, logBuffer);
        if (processResult.ExitCode != 0)
        {
            throw new AudioProcessingException("FFmpeg exited with an error.", combinedLogs);
        }

        onProgress?.Invoke(new ProcessingProgress(1d, "Processing complete."));
        return new ProcessingResult(
            Success: true,
            OutputPath: options.OutputPath,
            Logs: combinedLogs,
            ErrorMessage: null);
    }

    private static void TryReportProgress(
        string line,
        TimeSpan? totalDuration,
        Action<ProcessingProgress>? onProgress)
    {
        if (onProgress is null || !TryParseProcessedTime(line, out var processed))
        {
            return;
        }

        if (totalDuration is null || totalDuration <= TimeSpan.Zero)
        {
            onProgress(new ProcessingProgress(0d, $"Processing... ({processed:hh\\:mm\\:ss})"));
            return;
        }

        var ratio = processed.TotalSeconds / totalDuration.Value.TotalSeconds;
        ratio = Math.Clamp(ratio, 0d, 1d);

        onProgress(new ProcessingProgress(
            Value: ratio,
            Message: $"Processing... ({processed:hh\\:mm\\:ss} / {totalDuration:hh\\:mm\\:ss})"));
    }

    private static bool TryParseProcessedTime(string line, out TimeSpan processed)
    {
        const string marker = "time=";
        var markerIndex = line.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            processed = TimeSpan.Zero;
            return false;
        }

        var valueStart = markerIndex + marker.Length;
        var valueEnd = valueStart;
        while (valueEnd < line.Length && !char.IsWhiteSpace(line[valueEnd]))
        {
            valueEnd++;
        }

        if (valueEnd <= valueStart)
        {
            processed = TimeSpan.Zero;
            return false;
        }

        var rawValue = line[valueStart..valueEnd];
        return TimeSpan.TryParse(rawValue, CultureInfo.InvariantCulture, out processed);
    }
}
