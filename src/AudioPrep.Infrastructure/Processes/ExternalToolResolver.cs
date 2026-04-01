using AudioPrep.Core.Services;
using System.Runtime.InteropServices;

namespace AudioPrep.Infrastructure.Processes;

public sealed class ExternalToolResolver : IExternalToolResolver
{
    private readonly string _baseDirectory;

    public ExternalToolResolver(string? baseDirectory = null)
    {
        _baseDirectory = baseDirectory ?? AppContext.BaseDirectory;
    }

    public string? ResolveToolPath(string toolName)
    {
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return null;
        }

        if (Path.IsPathRooted(toolName) && File.Exists(toolName))
        {
            return toolName;
        }

        var runtimeFolder = GetRuntimeFolder();
        foreach (var toolDirectory in EnumerateBundledToolDirectories(runtimeFolder))
        {
            var localPath = FindFirstExistingPath(toolDirectory, toolName);
            if (localPath is not null)
            {
                return localPath;
            }
        }

        return null;
    }

    private IEnumerable<string> EnumerateBundledToolDirectories(string? runtimeFolder)
    {
        if (!string.IsNullOrWhiteSpace(runtimeFolder))
        {
            yield return Path.Combine(_baseDirectory, "tools", "ffmpeg", runtimeFolder);
        }

        yield return Path.Combine(_baseDirectory, "tools", "ffmpeg");
        yield return Path.Combine(_baseDirectory, "tools");
    }

    private static string? FindFirstExistingPath(string directory, string toolName)
    {
        if (!Directory.Exists(directory))
        {
            return null;
        }

        foreach (var candidateName in BuildExecutableNames(toolName))
        {
            var fullPath = Path.Combine(directory, candidateName);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }

    private static string? GetRuntimeFolder()
    {
        var osPart = GetOsPart();
        var archPart = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => null
        };

        if (osPart is null || archPart is null)
        {
            return null;
        }

        return $"{osPart}-{archPart}";
    }

    private static string? GetOsPart()
    {
        if (OperatingSystem.IsWindows())
        {
            return "win";
        }

        if (OperatingSystem.IsLinux())
        {
            return "linux";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "osx";
        }

        return null;
    }

    private static IEnumerable<string> BuildExecutableNames(string toolName)
    {
        if (Path.HasExtension(toolName))
        {
            yield return toolName;
            yield break;
        }

        yield return toolName;

        if (OperatingSystem.IsWindows())
        {
            yield return toolName + ".exe";
        }
    }
}
