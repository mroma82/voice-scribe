using digital_recorder.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace digital_recorder.Services;

public static class ConfigurationService
{
    // config directory and path
    private static readonly string ConfigDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".config",
        "digital-recorder");

    // config path
    private static readonly string ConfigPath = Path.Combine(ConfigDirectory, "config.yaml");

    // load
    public static AppConfig Load()
    {
        // verify the path exists
        if (!File.Exists(ConfigPath))
        {
            throw new FileNotFoundException(
                $"Configuration file not found at {ConfigPath}. Please create it with the required settings.");
        }

        // read the file
        var yaml = File.ReadAllText(ConfigPath);

        // setup parser
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        // parse
        var config = deserializer.Deserialize<AppConfig>(yaml)
            ?? throw new InvalidOperationException("Failed to parse configuration file.");

        // allow environment variable overrides
        ApplyEnvironmentOverrides(config);

        // expand paths
        config.LogseqPath = ExpandPath(config.LogseqPath);
        config.InputFolder = ExpandPath(config.InputFolder);
        config.CompletedFolder = ExpandPath(config.CompletedFolder);
        config.FailedFolder = ExpandPath(config.FailedFolder);

        // return
        return config;
    }

    // function that applies environment variable overrides
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

    // function that creates a default config
    public static void CreateDefaultConfig()
    {
        // create the directory if it doesn't exist
        Directory.CreateDirectory(ConfigDirectory);

        // define the default config
        var defaultConfig = new AppConfig
        {
            InputFolder = "input",
            CompletedFolder = "completed",
            FailedFolder = "failed",
            OpenAiKey = "your-openai-api-key-here",
            LogseqPath = "~/Notes"
        };

        // serialize to YAML
        var serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        // write
        var yaml = serializer.Serialize(defaultConfig);
        File.WriteAllText(ConfigPath, yaml);
    }

    // get config path
    public static string GetConfigPath() => ConfigPath;

    // function that expands paths
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
