using digital_recorder.Services;
using Microsoft.Extensions.Logging;

const string InputFolder = "input";
const string CompletedFolder = "completed";
const string FailedFolder = "failed";

var logseqPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "Notes");

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddConsole();
});

var logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("Audio WAV File Processor starting");

var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    logger.LogCritical("OPENAI_API_KEY environment variable is not set");
    return 1;
}

if (!Directory.Exists(logseqPath))
{
    logger.LogCritical("Logseq graph not found at {Path}", logseqPath);
    return 1;
}

logger.LogInformation("Input folder: {InputFolder}", InputFolder);
logger.LogInformation("Logseq graph: {LogseqPath}", logseqPath);

var transcriptionService = new AudioTranscriptionService(apiKey);
var outputService = new TranscriptionOutputService(logseqPath);
var fileProcessorLogger = loggerFactory.CreateLogger<FileProcessorService>();
var fileProcessor = new FileProcessorService(
    InputFolder,
    CompletedFolder,
    FailedFolder,
    transcriptionService,
    outputService,
    fileProcessorLogger);

var (processed, failed) = await fileProcessor.ProcessAllFilesAsync();

logger.LogInformation("Processing complete. Processed: {Processed}, Failed: {Failed}", processed, failed);

return failed > 0 ? 1 : 0;
