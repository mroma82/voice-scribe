using System.Diagnostics;
using digital_recorder.Models;
using digital_recorder.Utilities;

namespace digital_recorder.Services;

public class FileProcessorService
{
    private readonly string _inputFolder;
    private readonly string _completedFolder;
    private readonly string _failedFolder;
    private readonly AudioTranscriptionService _transcriptionService;
    private readonly TranscriptionOutputService _outputService;

    public FileProcessorService(
        string inputFolder,
        string completedFolder,
        string failedFolder,
        AudioTranscriptionService transcriptionService,
        TranscriptionOutputService outputService)
    {
        _inputFolder = inputFolder;
        _completedFolder = completedFolder;
        _failedFolder = failedFolder;
        _transcriptionService = transcriptionService;
        _outputService = outputService;

        EnsureDirectoriesExist();
    }

    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(_inputFolder);
        Directory.CreateDirectory(_completedFolder);
        Directory.CreateDirectory(_failedFolder);
    }

    public async Task<(int Processed, int Failed)> ProcessAllFilesAsync(CancellationToken cancellationToken = default)
    {
        var wavFiles = Directory.GetFiles(_inputFolder, "*.WAV", SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(_inputFolder, "*.wav", SearchOption.TopDirectoryOnly))
            .Distinct()
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        if (wavFiles.Count == 0)
        {
            Console.WriteLine("No WAV files found in input folder.");
            return (0, 0);
        }

        Console.WriteLine($"Found {wavFiles.Count} WAV file(s) to process.");

        int processed = 0;
        int failed = 0;

        foreach (var filePath in wavFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = Path.GetFileName(filePath);
            Console.WriteLine($"\nProcessing: {fileName}");

            var success = await ProcessFileAsync(filePath, cancellationToken);

            if (success)
            {
                processed++;
                Console.WriteLine($"  ✓ Completed successfully");
            }
            else
            {
                failed++;
                Console.WriteLine($"  ✗ Failed");
            }
        }

        return (processed, failed);
    }

    private async Task<bool> ProcessFileAsync(string filePath, CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(filePath);

        try
        {
            var timestamp = FileNameParser.TryParseTimestamp(fileName);
            if (timestamp is null)
            {
                Console.WriteLine($"  Error: Invalid filename format - expected R followed by yyyyMMddHHmmss");
                MoveToFailed(filePath, fileName);
                return false;
            }

            var stopwatch = Stopwatch.StartNew();
            var transcribedText = await _transcriptionService.TranscribeAsync(filePath, cancellationToken);
            stopwatch.Stop();

            if (string.IsNullOrWhiteSpace(transcribedText))
            {
                Console.WriteLine($"  Error: Empty transcription result");
                MoveToFailed(filePath, fileName);
                return false;
            }

            var result = new TranscriptionResult(
                SourceFileName: fileName,
                RecordingTimestamp: timestamp.Value,
                TranscribedText: transcribedText,
                ProcessingDuration: stopwatch.Elapsed
            );

            _outputService.AppendTranscription(result);
            Console.WriteLine($"  Transcription saved ({stopwatch.Elapsed.TotalSeconds:F1}s)");

            MoveToCompleted(filePath, fileName);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error: {ex.Message}");
            MoveToFailed(filePath, fileName);
            return false;
        }
    }

    private void MoveToCompleted(string sourcePath, string fileName)
    {
        var destinationPath = Path.Combine(_completedFolder, fileName);
        MoveFile(sourcePath, destinationPath);
    }

    private void MoveToFailed(string sourcePath, string fileName)
    {
        var destinationPath = Path.Combine(_failedFolder, fileName);
        MoveFile(sourcePath, destinationPath);
    }

    private static void MoveFile(string sourcePath, string destinationPath)
    {
        if (File.Exists(destinationPath))
        {
            var baseName = Path.GetFileNameWithoutExtension(destinationPath);
            var extension = Path.GetExtension(destinationPath);
            var directory = Path.GetDirectoryName(destinationPath)!;
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            destinationPath = Path.Combine(directory, $"{baseName}_{timestamp}{extension}");
        }

        File.Move(sourcePath, destinationPath);
    }
}
