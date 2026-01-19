using digital_recorder.Services;

const string InputFolder = "input";
const string CompletedFolder = "completed";
const string FailedFolder = "failed";
const string OutputFile = "output/transcriptions.txt";

Console.WriteLine("Audio WAV File Processor");
Console.WriteLine("========================\n");

var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.WriteLine("Error: OPENAI_API_KEY environment variable is not set.");
    Console.WriteLine("Please set your OpenAI API key before running.");
    return 1;
}

Console.WriteLine($"Input folder: {InputFolder}");
Console.WriteLine($"Output file: {OutputFile}\n");

var transcriptionService = new AudioTranscriptionService(apiKey);
var outputService = new TranscriptionOutputService(OutputFile);
var fileProcessor = new FileProcessorService(
    InputFolder,
    CompletedFolder,
    FailedFolder,
    transcriptionService,
    outputService);

var (processed, failed) = await fileProcessor.ProcessAllFilesAsync();

Console.WriteLine($"\n========================");
Console.WriteLine($"Processing complete!");
Console.WriteLine($"  Processed: {processed}");
Console.WriteLine($"  Failed: {failed}");

return failed > 0 ? 1 : 0;
