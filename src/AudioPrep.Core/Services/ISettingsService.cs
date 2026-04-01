using AudioPrep.Core.Models;

namespace AudioPrep.Core.Services;

public interface ISettingsService
{
    AppSettings Load();

    void Save(AppSettings settings);
}
