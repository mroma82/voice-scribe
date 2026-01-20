using System.Diagnostics;
using VoiceScribe.Models;
using VoiceScribe.Utilities;
using Microsoft.Extensions.Logging;

namespace VoiceScribe.Services;

public class FileProcessorService
{
    // define inputs
    private readonly string _inputFolder;
    private readonly string _completedFolder;
    private readonly string _failedFolder;
    private readonly AudioTranscriptionService _transcriptionService;
    private readonly TranscriptionOutputService _outputService;
    private readonly ILogger<FileProcessorService> _logger;

    // new
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

        // make sure directories exist
        EnsureDirectoriesExist();
    }

    // create directories for outputs
    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(_completedFolder);
        Directory.CreateDirectory(_failedFolder);
    }

    // process all files
    public async Task<(int Processed, int Failed)> ProcessAllFilesAsync(CancellationToken cancellationToken = default)
    {
        // find the wave files
        var wavFiles = Directory.GetFiles(_inputFolder, "*.WAV", SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(_inputFolder, "*.wav", SearchOption.TopDirectoryOnly))
            .Distinct()
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        // check if none
        if (wavFiles.Count == 0)
        {
            _logger.LogInformation("No WAV files found in input folder");
            return (0, 0);
        }

        _logger.LogInformation("Found {Count} WAV file(s) to process", wavFiles.Count);

        // track progress
        int processed = 0;
        int failed = 0;

        // run through each
        foreach (var filePath in wavFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // get the filename
            var fileName = Path.GetFileName(filePath);
            _logger.LogInformation("Processing: {FileName}", fileName);

            // process a single file
            var success = await ProcessFileAsync(filePath, cancellationToken);

            // check if ok
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

        // return status
        return (processed, failed);
    }

    // function that processes a single task
    private async Task<bool> ProcessFileAsync(string filePath, CancellationToken cancellationToken)
    {
        // get hte filename
        var fileName = Path.GetFileName(filePath);

        try
        {
            // get the timestamp from the filename
            var timestamp = FileNameParser.TryParseTimestamp(fileName);
            if (timestamp is null)
            {
                _logger.LogError("Invalid filename format for {FileName} - expected R followed by yyyyMMddHHmmss", fileName);
                MoveToFailed(filePath, fileName);
                return false;
            }

            // transcribe and time
            var stopwatch = Stopwatch.StartNew();
            var transcribedText = await _transcriptionService.TranscribeAsync(filePath, cancellationToken);
            stopwatch.Stop();

            // check if no text
            if (string.IsNullOrWhiteSpace(transcribedText))
            {
                _logger.LogError("Empty transcription result for {FileName}", fileName);
                MoveToFailed(filePath, fileName);
                return false;
            }

            // return the result
            var result = new TranscriptionResult(
                SourceFileName: fileName,
                RecordingTimestamp: timestamp.Value,
                TranscribedText: transcribedText,
                ProcessingDuration: stopwatch.Elapsed
            );

            // handle the output
            _outputService.AppendTranscription(result);
            _logger.LogDebug("Transcription saved for {FileName} in {Duration:F1}s", fileName, stopwatch.Elapsed.TotalSeconds);

            // move the file to completed
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

    // funciton that moves the file to completed
    private void MoveToCompleted(string sourcePath, string fileName)
    {
        var destinationPath = Path.Combine(_completedFolder, fileName);
        MoveFile(sourcePath, destinationPath);
    }

    // function that moves the file to failed
    private void MoveToFailed(string sourcePath, string fileName)
    {
        var destinationPath = Path.Combine(_failedFolder, fileName);
        MoveFile(sourcePath, destinationPath);
    }

    // function that moves a file, handling name conflicts
    private static void MoveFile(string sourcePath, string destinationPath)
    {
        // handle name conflicts by appending a timestamp
        if (File.Exists(destinationPath))
        {
            var baseName = Path.GetFileNameWithoutExtension(destinationPath);
            var extension = Path.GetExtension(destinationPath);
            var directory = Path.GetDirectoryName(destinationPath)!;
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            destinationPath = Path.Combine(directory, $"{baseName}_{timestamp}{extension}");
        }

        // move
        File.Move(sourcePath, destinationPath);
    }
}
