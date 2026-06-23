using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SkyFocus.Services;

public class SettingsService
{
    private readonly string _path;
    private Dictionary<string, JsonElement> _settings = new();

    public SettingsService()
    {
        _path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SkyFocus",
            "settings.json"
        );
        
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        Load();
    }

    public void Set<T>(string key, T value)
    {
        var json = JsonSerializer.SerializeToElement(value);
        _settings[key] = json;
        Save();
    }

    public T Get<T>(string key, T defaultValue = default!)
    {
        if (_settings.TryGetValue(key, out var jsonElement))
        {
            try
            {
                var value = JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
                if (value != null)
                    return value;
            }
            catch
            {
                // Если ошибка - возвращаем defaultValue
            }
        }
        
        return defaultValue;
    }

    private void Save()
    {
        var dict = new Dictionary<string, object>();
        foreach (var kvp in _settings)
        {
            var value = JsonSerializer.Deserialize<object>(kvp.Value.GetRawText());
            dict[kvp.Key] = value!;
        }
        
        var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        File.WriteAllText(_path, json);
    }

    private void Load()
    {
        if (!File.Exists(_path)) return;
        
        try
        {
            var json = File.ReadAllText(_path);
            var temp = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new();
            
            _settings = new Dictionary<string, JsonElement>();
            foreach (var kvp in temp)
            {
                var element = JsonSerializer.SerializeToElement(kvp.Value);
                _settings[kvp.Key] = element;
            }
        }
        catch 
        { 
            _settings = new Dictionary<string, JsonElement>(); 
        }
    }
}