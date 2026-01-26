using VoiceScribe.Models;

namespace VoiceScribe.Services;

public class TranscriptionOutputService
{
    // constants
    private const string AudioRecordingsHeading = "## Audio Recordings";

    // fields
    private readonly string _notesFolder;
    private readonly string _notesSystem;
    private readonly object _fileLock = new();

    // new
    public TranscriptionOutputService(string notesPath, string notesSystem = "logseq", string dailyNotesFolder = "")
    {
        _notesSystem = notesSystem.ToLower();
        
        // validate notes system
        if (_notesSystem != "logseq" && _notesSystem != "obsidian")
        {
            throw new ArgumentException($"Invalid notes system: {_notesSystem}. Must be 'logseq' or 'obsidian'.", nameof(notesSystem));
        }
        
        // find the notes path based on the system
        if (_notesSystem == "obsidian")
        {
            // for obsidian, use the daily notes folder if specified, otherwise use root
            _notesFolder = string.IsNullOrEmpty(dailyNotesFolder) 
                ? notesPath 
                : Path.Combine(notesPath, dailyNotesFolder);
        }
        else
        {
            // for logseq, use journals folder
            _notesFolder = Path.Combine(notesPath, "journals");
        }

        // ensure it exists
        EnsureNotesFolderExists();
    }

    // function that ensures notes folder exists
    private void EnsureNotesFolderExists()
    {
        if (!Directory.Exists(_notesFolder))
        {
            Directory.CreateDirectory(_notesFolder);
        }
    }

    // function that appends transcription to the 
    public void AppendTranscription(TranscriptionResult result)
    {
        // get the note path
        var notePath = GetNotePath(result.RecordingTimestamp);

        // setup the heading
        var timeHeading = result.RecordingTimestamp.ToString("h:mm:ss tt");

        // create the entry
        var entry = FormatTranscriptionEntry(timeHeading, result.TranscribedText);

        // append to the note
        lock (_fileLock)
        {
            AppendToNote(notePath, entry);
        }
    }

    // function that gets the path of the note based on the date
    private string GetNotePath(DateTime date)
    {
        // use different date formats for different systems
        var fileName = _notesSystem == "obsidian" 
            ? date.ToString("yyyy-MM-dd") + ".md"
            : date.ToString("yyyy_MM_dd") + ".md";
        return Path.Combine(_notesFolder, fileName);
    }

    // function that appends the entry to the note file
    private void AppendToNote(string notePath, string entry)
    {
        // if no note file, create it
        if (!File.Exists(notePath))
        {
            var prefix = _notesSystem == "obsidian" ? "" : "- ";
            File.WriteAllText(notePath, $"{prefix}{AudioRecordingsHeading}\n{entry}");
            return;
        }

        // read the content
        var content = File.ReadAllText(notePath);

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
            var prefix = _notesSystem == "obsidian" ? "" : "- ";
            content = content.TrimEnd() + $"\n{prefix}{AudioRecordingsHeading}\n{entry}";
        }

        // write the file
        File.WriteAllText(notePath, content);
    }

    // function that finds the insertion point after the audio recordings heading
    private int FindInsertionPoint(string content)
    {
        // find the index of the audio recordings heading
        var headingIndex = content.IndexOf(AudioRecordingsHeading, StringComparison.Ordinal);
        var afterHeading = headingIndex + AudioRecordingsHeading.Length;

        // for obsidian, find the next heading or end of file
        if (_notesSystem == "obsidian")
        {
            var nextHeading = content.IndexOf("\n## ", afterHeading, StringComparison.Ordinal);
            return nextHeading == -1 ? content.Length : nextHeading;
        }

        // for logseq, find the next top-level bullet point
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
    private string FormatTranscriptionEntry(string timeHeading, string transcribedText)
    {
        // split the transcribed text into lines and format each line
        var lines = transcribedText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        if (_notesSystem == "obsidian")
        {
            // for obsidian, use standard markdown
            var formattedLines = string.Join("\n", lines.Select(line => $"  - {line.Trim()}"));
            return $"### {timeHeading}\n{formattedLines}\n";
        }
        else
        {
            // for logseq, use outliner format
            var formattedLines = string.Join("\n", lines.Select(line => $"\t\t- {line.Trim()}"));
            return $"\t- ### {timeHeading}\n{formattedLines}\n";
        }
    }
}
