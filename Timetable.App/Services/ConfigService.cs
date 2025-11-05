using System;
using System.IO;
using System.Threading;
using Timetable.App.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Timetable.App.Services;

public class ConfigService : IConfigService
{
    private readonly IDeserializer _deserializer;
    private FileSystemWatcher? _watcher;
    private string? _configPath;

    public AppConfig Config { get; private set; } = new();
    public event EventHandler? ConfigReloaded;

    public ConfigService()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
    }

    public void Load(string path)
    {
        _configPath = path;
        Reload();
        SetupWatcher();
    }

    private void Reload()
    {
        if (string.IsNullOrEmpty(_configPath) || !File.Exists(_configPath))
        {
            // In a real app, you might want to create a default config or show an error.
            Config = new AppConfig();
        }
        else
        {
            try
            {
                var yamlContent = File.ReadAllText(_configPath);
                Config = _deserializer.Deserialize<AppConfig>(yamlContent);
            }
            catch (Exception ex)
            {
                // Handle parsing errors, e.g., log them.
                Console.WriteLine($"Error loading config: {ex.Message}");
                Config = new AppConfig(); // Fallback to a default config
            }
        }
        
        ConfigReloaded?.Invoke(this, EventArgs.Empty);
    }

    private void SetupWatcher()
    {
        if (string.IsNullOrEmpty(_configPath)) return;

        var directory = Path.GetDirectoryName(_configPath);
        var fileName = Path.GetFileName(_configPath);

        if (directory == null || fileName == null) return;

        _watcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };

        _watcher.Changed += OnConfigFileChanged;
        _watcher.EnableRaisingEvents = true;
    }

    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce to avoid multiple triggers on a single save
        Thread.Sleep(200); 
        Reload();
    }
}