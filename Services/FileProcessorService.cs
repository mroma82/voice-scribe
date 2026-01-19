using System.Diagnostics;
using digital_recorder.Models;
using digital_recorder.Utilities;
using Microsoft.Extensions.Logging;

namespace digital_recorder.Services;

public class FileProcessorService
{
    private readonly string _inputFolder;
    private readonly string _completedFolder;
    private readonly string _failedFolder;
    private readonly AudioTranscriptionService _transcriptionService;
    private readonly TranscriptionOutputService _outputService;
    private readonly ILogger<FileProcessorService> _logger;

    public FileProcessorService(
        string inputFolder,
        string completedFolder,
        string failedFolder,
        AudioTranscriptionService transcriptionService,
        TranscriptionOutputService outputService,
        ILogger<FileProcessorService> logger)
    {
        _inputFolder = inputFolder;
        _completedFolder = completedFolder;
        _failedFolder = failedFolder;
        _transcriptionService = transcriptionService;
        _outputService = outputService;
        _logger = logger;

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
            _logger.LogInformation("No WAV files found in input folder");
            return (0, 0);
        }

        _logger.LogInformation("Found {Count} WAV file(s) to process", wavFiles.Count);

        int processed = 0;
        int failed = 0;

        foreach (var filePath in wavFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = Path.GetFileName(filePath);
            _logger.LogInformation("Processing: {FileName}", fileName);

            var success = await ProcessFileAsync(filePath, cancellationToken);

            if (success)
            {
                processed++;
                _logger.LogInformation("Completed successfully: {FileName}", fileName);
            }
            else
            {
                failed++;
                _logger.LogWarning("Failed: {FileName}", fileName);
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
                _logger.LogError("Invalid filename format for {FileName} - expected R followed by yyyyMMddHHmmss", fileName);
                MoveToFailed(filePath, fileName);
                return false;
            }

            var stopwatch = Stopwatch.StartNew();
            var transcribedText = await _transcriptionService.TranscribeAsync(filePath, cancellationToken);
            stopwatch.Stop();

            if (string.IsNullOrWhiteSpace(transcribedText))
            {
                _logger.LogError("Empty transcription result for {FileName}", fileName);
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
            _logger.LogDebug("Transcription saved for {FileName} in {Duration:F1}s", fileName, stopwatch.Elapsed.TotalSeconds);

            MoveToCompleted(filePath, fileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing {FileName}", fileName);
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
