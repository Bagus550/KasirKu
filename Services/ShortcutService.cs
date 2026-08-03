using System;
using System.IO;
using System.Text.Json;
using KasirKu.Models;

namespace KasirKu.Services
{
    public interface IShortcutService
    {
        ShortcutSetting LoadShortcuts();
        void SaveShortcuts(ShortcutSetting settings);
    }

    public class ShortcutService : IShortcutService
    {
        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shortcut_config.json");

        public ShortcutSetting LoadShortcuts()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    return JsonSerializer.Deserialize<ShortcutSetting>(json) ?? new ShortcutSetting();
                }
            }
            catch { }

            return new ShortcutSetting(); // Default
        }

        public void SaveShortcuts(ShortcutSetting settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.ReadAllText(_filePath);
                File.WriteAllText(_filePath, json);
            }
            catch { }
        }
    }
}