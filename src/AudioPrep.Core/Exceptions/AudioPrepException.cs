namespace AudioPrep.Core.Exceptions;

public class AudioPrepException : Exception
{
    public AudioPrepException(string message, string? details = null, Exception? innerException = null)
        : base(message, innerException)
    {
        Details = details;
    }

    public string? Details { get; }
}

public sealed class ExternalToolNotFoundException : AudioPrepException
{
    public ExternalToolNotFoundException(string toolName)
        : base($"{toolName} was not found in bundled tools. Rebuild the app to auto-fetch binaries or run scripts/Ensure-BundledFfmpeg.ps1.")
    {
    }
}

public sealed class MediaProbeException : AudioPrepException
{
    public MediaProbeException(string message, string? details = null, Exception? innerException = null)
        : base(message, details, innerException)
    {
    }
}

public sealed class AudioProcessingException : AudioPrepException
{
    public AudioProcessingException(string message, string? details = null, Exception? innerException = null)
        : base(message, details, innerException)
    {
    }
}

public sealed class NoAudioStreamsException : AudioPrepException
{
    public NoAudioStreamsException()
        : base("No audio streams were found in this file.")
    {
    }
}
