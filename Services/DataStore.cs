using System.Text.Json;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Services;

/// <summary>Loads and saves application data to a JSON file.</summary>
public class DataStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string _filePath;

    public DataStore(string filePath)
    {
        _filePath = filePath;
    }

    public AppData Load()
    {
        if (!File.Exists(_filePath))
            return new AppData();

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            Console.WriteLine($"Could not read saved data ({ex.Message}). Starting with an empty database.");
            return new AppData();
        }
    }

    public void Save(AppData data)
    {
        var folder = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(folder))
            Directory.CreateDirectory(folder);

        File.WriteAllText(_filePath, JsonSerializer.Serialize(data, Options));
    }
}
