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

        ApplyEnvironmentOverrides(config);

        config.LogseqPath = ExpandPath(config.LogseqPath);
        config.InputFolder = ExpandPath(config.InputFolder);
        config.CompletedFolder = ExpandPath(config.CompletedFolder);
        config.FailedFolder = ExpandPath(config.FailedFolder);

        return config;
    }

    private static void ApplyEnvironmentOverrides(AppConfig config)
    {
        var inputFolder = Environment.GetEnvironmentVariable("DIGITAL_RECORDER_INPUT_FOLDER");
        if (!string.IsNullOrEmpty(inputFolder))
            config.InputFolder = inputFolder;

        var completedFolder = Environment.GetEnvironmentVariable("DIGITAL_RECORDER_COMPLETED_FOLDER");
        if (!string.IsNullOrEmpty(completedFolder))
            config.CompletedFolder = completedFolder;

        var failedFolder = Environment.GetEnvironmentVariable("DIGITAL_RECORDER_FAILED_FOLDER");
        if (!string.IsNullOrEmpty(failedFolder))
            config.FailedFolder = failedFolder;

        var openAiKey = Environment.GetEnvironmentVariable("DIGITAL_RECORDER_OPENAI_KEY")
                        ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrEmpty(openAiKey))
            config.OpenAiKey = openAiKey;

        var logseqPath = Environment.GetEnvironmentVariable("DIGITAL_RECORDER_LOGSEQ_PATH");
        if (!string.IsNullOrEmpty(logseqPath))
            config.LogseqPath = logseqPath;
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
