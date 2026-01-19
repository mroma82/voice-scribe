using System.Text;
using Whisper.net;

namespace digital_recorder.Services;

public sealed class AudioTranscriptionService : IDisposable
{
    private readonly WhisperFactory _whisperFactory;
    private bool _disposed;

    public AudioTranscriptionService(string modelPath)
    {
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"Whisper model not found at: {modelPath}", modelPath);
        }

        _whisperFactory = WhisperFactory.FromPath(modelPath);
    }

    public async Task<string> TranscribeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Audio file not found: {filePath}", filePath);
        }

        using var processor = _whisperFactory.CreateBuilder()
            .WithLanguage("auto")
            .Build();

        await using var fileStream = File.OpenRead(filePath);

        var transcription = new StringBuilder();

        await foreach (var segment in processor.ProcessAsync(fileStream, cancellationToken))
        {
            transcription.Append(segment.Text);
        }

        return transcription.ToString().Trim();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _whisperFactory.Dispose();
        _disposed = true;
    }
}
