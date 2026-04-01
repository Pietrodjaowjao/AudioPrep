namespace AudioPrep.Core.Services;

public interface IExternalToolResolver
{
    string? ResolveToolPath(string toolName);
}
