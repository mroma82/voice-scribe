using digital_recorder.Models;
using digital_recorder.Services;
using Microsoft.Extensions.Logging;

// setup the logger
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddConsole();
});

var logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("Audio WAV File Processor starting");

// load configuration
AppConfig config;
try
{
    config = ConfigurationService.Load();
}
catch (FileNotFoundException)
{
    logger.LogWarning("Configuration file not found, creating default at {Path}", ConfigurationService.GetConfigPath());
    ConfigurationService.CreateDefaultConfig();
    logger.LogInformation("Please edit the configuration file and run again");
    return 1;
}

// check open AI key
if (string.IsNullOrWhiteSpace(config.OpenAiKey) || config.OpenAiKey == "your-openai-api-key-here")
{
    logger.LogCritical("OpenAI API key not configured in {Path}", ConfigurationService.GetConfigPath());
    return 1;
}

// verify logseq path exists
if (!Directory.Exists(config.LogseqPath))
{
    logger.LogCritical("Logseq graph not found at {Path}", config.LogseqPath);
    return 1;
}

logger.LogInformation("Input folder: {InputFolder}", config.InputFolder);
logger.LogInformation("Logseq graph: {LogseqPath}", config.LogseqPath);

// setup service
var transcriptionService = new AudioTranscriptionService(config.OpenAiKey);
var outputService = new TranscriptionOutputService(config.LogseqPath);
var fileProcessorLogger = loggerFactory.CreateLogger<FileProcessorService>();
var fileProcessor = new FileProcessorService(
    config.InputFolder,
    config.CompletedFolder,
    config.FailedFolder,
    transcriptionService,
    outputService,
    fileProcessorLogger);

// run
var (processed, failed) = await fileProcessor.ProcessAllFilesAsync();

logger.LogInformation("Processing complete. Processed: {Processed}, Failed: {Failed}", processed, failed);

// return
return failed > 0 ? 1 : 0;
