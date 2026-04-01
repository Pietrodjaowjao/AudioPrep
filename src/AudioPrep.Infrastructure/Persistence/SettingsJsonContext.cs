using System.Text.Json.Serialization;
using AudioPrep.Core.Models;

namespace AudioPrep.Infrastructure.Persistence;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
internal sealed partial class SettingsJsonContext : JsonSerializerContext
{
}
