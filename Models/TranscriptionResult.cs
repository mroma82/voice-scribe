namespace VoiceScribe.Models;

public record TranscriptionResult(
    string SourceFileName,
    DateTime RecordingTimestamp,
    string TranscribedText,
    TimeSpan ProcessingDuration
);
