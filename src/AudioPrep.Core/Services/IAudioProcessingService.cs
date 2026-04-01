using AudioPrep.Core.Models;

namespace AudioPrep.Core.Services;

public interface IAudioProcessingService
{
    Task<ProcessingResult> ProcessAsync(
        ProcessingOptions options,
        Action<ProcessingProgress>? onProgress = null,
        Action<string>? onLog = null,
        CancellationToken cancellationToken = default);
}
