namespace VoiceScribe.Models;

public class AppConfig
{
    public string InputFolder { get; set; } = "input";
    public string CompletedFolder { get; set; } = "completed";
    public string FailedFolder { get; set; } = "failed";
    public string OpenAiKey { get; set; } = string.Empty;
    public string LogseqPath { get; set; } = string.Empty;
}
