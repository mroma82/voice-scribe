using OpenAI.Audio;

namespace digital_recorder.Services;

public sealed class AudioTranscriptionService
{
    private readonly AudioClient _audioClient;

    public AudioTranscriptionService(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("OpenAI API key is required", nameof(apiKey));
        }

        _audioClient = new AudioClient("whisper-1", apiKey);
    }

    public async Task<string> TranscribeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Audio file not found: {filePath}", filePath);
        }

        await using var fileStream = File.OpenRead(filePath);

        var result = await _audioClient.TranscribeAudioAsync(
            fileStream,
            Path.GetFileName(filePath),
            cancellationToken: cancellationToken);

        return result.Value.Text.Trim();
    }
}
