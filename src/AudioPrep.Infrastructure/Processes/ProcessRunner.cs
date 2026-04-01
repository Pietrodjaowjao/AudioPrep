using System.Diagnostics;
using System.Text;
using AudioPrep.Core.Models;
using AudioPrep.Core.Services;

namespace AudioPrep.Infrastructure.Processes;

public sealed class ProcessRunner : IProcessRunner
{
    public async Task<ProcessResult> RunAsync(
        ProcessRequest request,
        Action<string>? onStdOut = null,
        Action<string>? onStdErr = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.ExecutablePath))
        {
            throw new ArgumentException("Executable path is required.", nameof(request));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var startInfo = new ProcessStartInfo
        {
            FileName = request.ExecutablePath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (!string.IsNullOrWhiteSpace(request.WorkingDirectory))
        {
            startInfo.WorkingDirectory = request.WorkingDirectory;
        }

        foreach (var argument in request.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        var standardOutput = new StringBuilder();
        var standardError = new StringBuilder();

        if (!process.Start())
        {
            throw new InvalidOperationException($"Could not start process '{request.ExecutablePath}'.");
        }

        using var cancellationRegistration = cancellationToken.Register(() => TryKill(process));

        var stdoutTask = ReadLinesAsync(process.StandardOutput, standardOutput, onStdOut);
        var stderrTask = ReadLinesAsync(process.StandardError, standardError, onStdErr);

        await process.WaitForExitAsync().ConfigureAwait(false);
        await Task.WhenAll(stdoutTask, stderrTask).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new ProcessResult(
            ExitCode: process.ExitCode,
            StandardOutput: standardOutput.ToString(),
            StandardError: standardError.ToString());
    }

    private static async Task ReadLinesAsync(
        StreamReader reader,
        StringBuilder sink,
        Action<string>? onLine)
    {
        while (true)
        {
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (line is null)
            {
                break;
            }

            sink.AppendLine(line);
            onLine?.Invoke(line);
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Best effort only.
        }
    }
}
