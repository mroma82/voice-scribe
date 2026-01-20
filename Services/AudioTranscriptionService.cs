using OpenAI.Audio;

namespace VoiceScribe.Services;

public sealed class AudioTranscriptionService
{
    // services
    private readonly AudioClient _audioClient;

    // new
    public AudioTranscriptionService(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("OpenAI API key is required", nameof(apiKey));
        }

        _audioClient = new AudioClient("whisper-1", apiKey);
    }

    // transcribe
    public async Task<string> TranscribeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // check if the path exists
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Audio file not found: {filePath}", filePath);
        }

        // open the file
        await using var fileStream = File.OpenRead(filePath);

        // transcribe
        var result = await _audioClient.TranscribeAudioAsync(
            fileStream,
            Path.GetFileName(filePath),
            cancellationToken: cancellationToken);

        // return the text
        return result.Value.Text.Trim();
    }
}
