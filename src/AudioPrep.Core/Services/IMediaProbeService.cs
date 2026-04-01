using AudioPrep.Core.Models;

namespace AudioPrep.Core.Services;

public interface IMediaProbeService
{
    Task<MediaInfo> ProbeAsync(string inputPath, CancellationToken cancellationToken = default);
}
