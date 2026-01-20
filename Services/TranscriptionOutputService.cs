using VoiceScribe.Models;

namespace VoiceScribe.Services;

public class TranscriptionOutputService
{
    // constants
    private const string AudioRecordingsHeading = "## Audio Recordings";

    // fields
    private readonly string _journalsFolder;
    private readonly object _fileLock = new();

    // new
    public TranscriptionOutputService(string logseqGraphPath)
    {
        // find the journals path
        _journalsFolder = Path.Combine(logseqGraphPath, "journals");

        // ensure it exists
        EnsureJournalsFolderExists();
    }

    // function that ensures journals folder exists
    private void EnsureJournalsFolderExists()
    {
        if (!Directory.Exists(_journalsFolder))
        {
            Directory.CreateDirectory(_journalsFolder);
        }
    }

    // function that appends transcription to the 
    public void AppendTranscription(TranscriptionResult result)
    {
        // get the journal path
        var journalPath = GetJournalPath(result.RecordingTimestamp);

        // setup the heading
        var timeHeading = result.RecordingTimestamp.ToString("h:mm:ss tt");

        // create the entry
        var entry = FormatTranscriptionEntry(timeHeading, result.TranscribedText);

        // append to the journal
        lock (_fileLock)
        {
            AppendToJournal(journalPath, entry);
        }
    }

    // function that gets the path of the journal based on the date
    private string GetJournalPath(DateTime date)
    {
        var fileName = date.ToString("yyyy_MM_dd") + ".md";
        return Path.Combine(_journalsFolder, fileName);
    }

    // function that appens the entry to the journal file
    private void AppendToJournal(string journalPath, string entry)
    {
        // if no journal file, create it
        if (!File.Exists(journalPath))
        {
            File.WriteAllText(journalPath, $"- {AudioRecordingsHeading}\n{entry}");
            return;
        }

        // read the content
        var content = File.ReadAllText(journalPath);

        // check if the content contains the audio recordings heading
        if (content.Contains(AudioRecordingsHeading))
        {
            // add the entry at the heading
            var insertIndex = FindInsertionPoint(content);
            content = content.Insert(insertIndex, entry);
        }
        else
        {
            // else, append the heading and entry at the end
            content = content.TrimEnd() + $"\n- {AudioRecordingsHeading}\n{entry}";
        }

        // write the file
        File.WriteAllText(journalPath, content);
    }

    // function that finds the insertion point after the audio recordings heading
    private int FindInsertionPoint(string content)
    {
        // find the index of the audio recordings heading
        var headingIndex = content.IndexOf(AudioRecordingsHeading, StringComparison.Ordinal);
        var afterHeading = headingIndex + AudioRecordingsHeading.Length;

        // find the next top-level bullet point
        var nextTopLevelBullet = content.IndexOf("\n- ", afterHeading, StringComparison.Ordinal);

        // if no next top-level bullet point, return the end of the content
        if (nextTopLevelBullet == -1)
        {
            return content.Length;
        }

        // return the next top-level bullet point index
        return nextTopLevelBullet;
    }


    // function that formats the transcription entry
    private static string FormatTranscriptionEntry(string timeHeading, string transcribedText)
    {
        // split the transcribed text into lines and format each line as a sub-bullet point
        var lines = transcribedText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var formattedLines = string.Join("\n", lines.Select(line => $"\t\t- {line.Trim()}"));

        // return 
        return $"\t- ### {timeHeading}\n{formattedLines}\n";
    }
}
