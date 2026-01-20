using digital_recorder.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace digital_recorder.Services;

public static class ConfigurationService
{
    private static readonly string ConfigDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".config",
        "digital-recorder");

    private static readonly string ConfigPath = Path.Combine(ConfigDirectory, "config.yaml");

    public static AppConfig Load()
    {
        if (!File.Exists(ConfigPath))
        {
            throw new FileNotFoundException(
                $"Configuration file not found at {ConfigPath}. Please create it with the required settings.");
        }

        var yaml = File.ReadAllText(ConfigPath);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        var config = deserializer.Deserialize<AppConfig>(yaml)
            ?? throw new InvalidOperationException("Failed to parse configuration file.");

        config.LogseqPath = ExpandPath(config.LogseqPath);
        config.InputFolder = ExpandPath(config.InputFolder);
        config.CompletedFolder = ExpandPath(config.CompletedFolder);
        config.FailedFolder = ExpandPath(config.FailedFolder);

        return config;
    }

    public static void CreateDefaultConfig()
    {
        Directory.CreateDirectory(ConfigDirectory);

        var defaultConfig = new AppConfig
        {
            InputFolder = "input",
            CompletedFolder = "completed",
            FailedFolder = "failed",
            OpenAiKey = "your-openai-api-key-here",
            LogseqPath = "~/Notes"
        };

        var serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        var yaml = serializer.Serialize(defaultConfig);
        File.WriteAllText(ConfigPath, yaml);
    }

    public static string GetConfigPath() => ConfigPath;

    private static string ExpandPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        if (path.StartsWith("~/"))
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, path[2..]);
        }

        return path;
    }
}
