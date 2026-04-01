using AudioPrep.Infrastructure.Processes;
using System.Runtime.InteropServices;

namespace AudioPrep.Tests.Processes;

public sealed class ExternalToolResolverTests
{
    [Fact]
    public void ResolveToolPath_ShouldFindBundledRuntimeSpecificBinary()
    {
        var root = CreateTempDirectory();
        try
        {
            var runtimeFolder = GetRuntimeFolder();
            var toolsDirectory = runtimeFolder is null
                ? Path.Combine(root, "tools", "ffmpeg")
                : Path.Combine(root, "tools", "ffmpeg", runtimeFolder);

            Directory.CreateDirectory(toolsDirectory);
            var expectedPath = Path.Combine(toolsDirectory, GetExecutableName("ffmpeg"));
            File.WriteAllText(expectedPath, "stub");

            var resolver = new ExternalToolResolver(root);
            var actualPath = resolver.ResolveToolPath("ffmpeg");

            Assert.Equal(expectedPath, actualPath);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ResolveToolPath_ShouldNotFallbackToSystemPath()
    {
        var appRoot = CreateTempDirectory();
        var pathRoot = CreateTempDirectory();
        var originalPath = Environment.GetEnvironmentVariable("PATH");

        try
        {
            var pathTool = Path.Combine(pathRoot, GetExecutableName("ffprobe"));
            File.WriteAllText(pathTool, "stub");
            Environment.SetEnvironmentVariable("PATH", pathRoot);

            var resolver = new ExternalToolResolver(appRoot);
            var actualPath = resolver.ResolveToolPath("ffprobe");

            Assert.Null(actualPath);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
            Directory.Delete(appRoot, recursive: true);
            Directory.Delete(pathRoot, recursive: true);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "audioprep-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string GetExecutableName(string toolName)
    {
        return OperatingSystem.IsWindows()
            ? $"{toolName}.exe"
            : toolName;
    }

    private static string? GetRuntimeFolder()
    {
        var osPart = OperatingSystem.IsWindows() ? "win"
            : OperatingSystem.IsLinux() ? "linux"
            : OperatingSystem.IsMacOS() ? "osx"
            : null;

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
}
