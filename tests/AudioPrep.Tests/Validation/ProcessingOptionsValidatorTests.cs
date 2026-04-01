using AudioPrep.Core.Models;
using AudioPrep.Core.Validation;

namespace AudioPrep.Tests.Validation;

public sealed class ProcessingOptionsValidatorTests
{
    [Fact]
    public void Validate_WhenInputFileMissing_ShouldReturnError()
    {
        var options = new ProcessingOptions
        {
            AudioStreamIndex = 0,
            InputPath = @"C:\missing-input-file.wav",
            OutputPath = @"C:\output\result.wav",
            CodecKind = AudioCodecKind.WavPcm16,
            SampleRate = 48000,
            Channels = 1,
            NormalizeLoudness = false,
            TrimSilence = false
        };

        var errors = ProcessingOptionsValidator.Validate(options);

        Assert.Contains(errors, e => e.Contains("Input file was not found.", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_WhenFieldsInvalid_ShouldReturnMultipleErrors()
    {
        var tempInput = Path.GetTempFileName();

        try
        {
            var options = new ProcessingOptions
            {
                AudioStreamIndex = -1,
                InputPath = tempInput,
                OutputPath = tempInput,
                CodecKind = AudioCodecKind.Aac,
                SampleRate = 0,
                Channels = 3,
                NormalizeLoudness = true,
                TrimSilence = true
            };

            var errors = ProcessingOptionsValidator.Validate(options);

            Assert.Contains(errors, e => e.Contains("Audio stream index", StringComparison.Ordinal));
            Assert.Contains(errors, e => e.Contains("Channels must", StringComparison.Ordinal));
            Assert.Contains(errors, e => e.Contains("Sample rate", StringComparison.Ordinal));
            Assert.Contains(errors, e => e.Contains("Output path must be different", StringComparison.Ordinal));
        }
        finally
        {
            File.Delete(tempInput);
        }
    }
}
