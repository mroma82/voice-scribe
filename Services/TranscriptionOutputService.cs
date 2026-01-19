using digital_recorder.Models;

namespace digital_recorder.Services;

public class TranscriptionOutputService
{
    private const string AudioRecordingsHeading = "## Audio Recordings";
    private readonly string _journalsFolder;
    private readonly object _fileLock = new();

    public TranscriptionOutputService(string logseqGraphPath)
    {
        _journalsFolder = Path.Combine(logseqGraphPath, "journals");
        EnsureJournalsFolderExists();
    }

    private void EnsureJournalsFolderExists()
    {
        if (!Directory.Exists(_journalsFolder))
        {
            Directory.CreateDirectory(_journalsFolder);
        }
    }

    public void AppendTranscription(TranscriptionResult result)
    {
        var journalPath = GetJournalPath(result.RecordingTimestamp);
        var timeHeading = result.RecordingTimestamp.ToString("h:mm:ss tt");
        var entry = FormatTranscriptionEntry(timeHeading, result.TranscribedText);

        lock (_fileLock)
        {
            AppendToJournal(journalPath, entry);
        }
    }

    private string GetJournalPath(DateTime date)
    {
        var fileName = date.ToString("yyyy_MM_dd") + ".md";
        return Path.Combine(_journalsFolder, fileName);
    }

    private void AppendToJournal(string journalPath, string entry)
    {
        if (!File.Exists(journalPath))
        {
            File.WriteAllText(journalPath, $"- {AudioRecordingsHeading}\n{entry}");
            return;
        }

        var content = File.ReadAllText(journalPath);

        if (content.Contains(AudioRecordingsHeading))
        {
            var insertIndex = FindInsertionPoint(content);
            content = content.Insert(insertIndex, entry);
        }
        else
        {
            content = content.TrimEnd() + $"\n- {AudioRecordingsHeading}\n{entry}";
        }

        File.WriteAllText(journalPath, content);
    }

    private int FindInsertionPoint(string content)
    {
        var headingIndex = content.IndexOf(AudioRecordingsHeading, StringComparison.Ordinal);
        var afterHeading = headingIndex + AudioRecordingsHeading.Length;

        var nextTopLevelBullet = content.IndexOf("\n- ", afterHeading, StringComparison.Ordinal);

        if (nextTopLevelBullet == -1)
        {
            return content.Length;
        }

        return nextTopLevelBullet;
    }

    private static string FormatTranscriptionEntry(string timeHeading, string transcribedText)
    {
        var lines = transcribedText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var formattedLines = string.Join("\n", lines.Select(line => $"\t\t- {line.Trim()}"));

        return $"\t- ### {timeHeading}\n{formattedLines}\n";
    }
}
