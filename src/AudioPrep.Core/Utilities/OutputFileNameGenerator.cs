namespace AudioPrep.Core.Utilities;

public static class OutputFileNameGenerator
{
    public static string Suggest(string inputPath, string extension)
    {
        var baseName = string.IsNullOrWhiteSpace(inputPath)
            ? "output"
            : Path.GetFileNameWithoutExtension(inputPath);

        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "output";
        }

        baseName = Sanitize(baseName);
        return $"{baseName}_prepared.{NormalizeExtension(extension)}";
    }

    public static string EnsureExtension(string fileName, string extension)
    {
        var cleanName = string.IsNullOrWhiteSpace(fileName)
            ? "output"
            : fileName.Trim();

        var normalizedExtension = NormalizeExtension(extension);
        var currentExtension = Path.GetExtension(cleanName);

        if (string.Equals(currentExtension, $".{normalizedExtension}", StringComparison.OrdinalIgnoreCase))
        {
            return cleanName;
        }

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(cleanName);
        if (string.IsNullOrWhiteSpace(nameWithoutExtension))
        {
            nameWithoutExtension = "output";
        }

        nameWithoutExtension = Sanitize(nameWithoutExtension);
        return $"{nameWithoutExtension}.{normalizedExtension}";
    }

    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return "wav";
        }

        return extension.Trim().TrimStart('.').ToLowerInvariant();
    }

    private static string Sanitize(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var chars = value
            .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
            .ToArray();

        return new string(chars).Trim();
    }
}
