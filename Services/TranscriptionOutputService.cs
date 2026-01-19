using digital_recorder.Models;
using digital_recorder.Utilities;

namespace digital_recorder.Services;

public class TranscriptionOutputService
{
    private readonly string _outputFilePath;
    private readonly object _fileLock = new();

    public TranscriptionOutputService(string outputFilePath)
    {
        _outputFilePath = outputFilePath;
        EnsureOutputDirectoryExists();
    }

    private void EnsureOutputDirectoryExists()
    {
        var directory = Path.GetDirectoryName(_outputFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public void AppendTranscription(TranscriptionResult result)
    {
        var formattedEntry = FormatTranscriptionEntry(result);

        lock (_fileLock)
        {
            File.AppendAllText(_outputFilePath, formattedEntry);
        }
    }

    private static string FormatTranscriptionEntry(TranscriptionResult result)
    {
        var separator = new string('=', 60);
        var lineSeparator = new string('-', 60);
        var formattedTimestamp = FileNameParser.FormatTimestampForDisplay(result.RecordingTimestamp);
        var processedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        return $"""
                {separator}
                Recording: {result.SourceFileName}
                Timestamp: {formattedTimestamp}
                Processed: {processedTime}
                {lineSeparator}

                {result.TranscribedText}


                """;
    }
}
