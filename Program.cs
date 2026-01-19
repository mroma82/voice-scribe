using digital_recorder.Services;

const string ModelPath = "ggml-base.bin";
const string InputFolder = "input";
const string CompletedFolder = "completed";
const string FailedFolder = "failed";
const string OutputFile = "output/transcriptions.txt";

Console.WriteLine("Audio WAV File Processor");
Console.WriteLine("========================\n");

if (!File.Exists(ModelPath))
{
    Console.WriteLine($"Error: Whisper model not found at '{ModelPath}'");
    Console.WriteLine("Please ensure the model file exists before running.");
    return 1;
}

Console.WriteLine($"Model: {ModelPath}");
Console.WriteLine($"Input folder: {InputFolder}");
Console.WriteLine($"Output file: {OutputFile}\n");

using var transcriptionService = new AudioTranscriptionService(ModelPath);
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
