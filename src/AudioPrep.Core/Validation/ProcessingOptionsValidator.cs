using AudioPrep.Core.Models;

namespace AudioPrep.Core.Validation;

public static class ProcessingOptionsValidator
{
    public static IReadOnlyList<string> Validate(ProcessingOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.InputPath))
        {
            errors.Add("Input path is required.");
        }
        else if (!File.Exists(options.InputPath))
        {
            errors.Add("Input file was not found.");
        }

        if (string.IsNullOrWhiteSpace(options.OutputPath))
        {
            errors.Add("Output path is required.");
        }
        else
        {
            var outputDirectory = Path.GetDirectoryName(options.OutputPath);
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                errors.Add("Output directory is invalid.");
            }
        }

        if (options.AudioStreamIndex < 0)
        {
            errors.Add("Audio stream index must be zero or greater.");
        }

        if (options.Channels is < 1 or > 2)
        {
            errors.Add("Channels must be 1 (mono) or 2 (stereo).");
        }

        if (options.SampleRate <= 0)
        {
            errors.Add("Sample rate must be greater than zero.");
        }

        if (!string.IsNullOrWhiteSpace(options.InputPath) &&
            !string.IsNullOrWhiteSpace(options.OutputPath) &&
            string.Equals(
                Path.GetFullPath(options.InputPath),
                Path.GetFullPath(options.OutputPath),
                StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Output path must be different from the input path.");
        }

        return errors;
    }
}
