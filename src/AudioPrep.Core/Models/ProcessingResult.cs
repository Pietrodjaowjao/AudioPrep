namespace AudioPrep.Core.Models;

public sealed record ProcessingResult(
    bool Success,
    string OutputPath,
    string Logs,
    string? ErrorMessage);
