using System.Text.Json;
using AudioPrep.Core.Models;
using AudioPrep.Core.Services;

namespace AudioPrep.Infrastructure.Persistence;

public sealed class JsonSettingsService : ISettingsService
{
    private readonly string _settingsPath;

    public JsonSettingsService(string? settingsPath = null)
    {
        _settingsPath = settingsPath ?? BuildDefaultSettingsPath();
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(_settingsPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new AppSettings();
            }

            return JsonSerializer.Deserialize(json, SettingsJsonContext.Default.AppSettings)
                ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var directory = Path.GetDirectoryName(_settingsPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        Directory.CreateDirectory(directory);
        var json = JsonSerializer.Serialize(settings, SettingsJsonContext.Default.AppSettings);
        File.WriteAllText(_settingsPath, json);
    }

    private static string BuildDefaultSettingsPath()
    {
        var appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appDataDirectory, "AudioPrep", "settings.json");
    }
}
