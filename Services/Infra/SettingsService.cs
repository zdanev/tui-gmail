namespace TuiGmail.Services.Infra;

using System.IO;
using System.Text.Json;

public class SettingsService
{
    private readonly string _settingsFilePath = Path.Combine(AppContext.BaseDirectory, "settings.json");

    public Settings LoadSettings()
    {
        if (!File.Exists(_settingsFilePath))
        {
            return new Settings { Theme = "Default" };
        }

        var json = File.ReadAllText(_settingsFilePath);
        return JsonSerializer.Deserialize<Settings>(json) ?? new Settings { Theme = "Default" };
    }

    public void SaveSettings(Settings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsFilePath, json);
    }
}