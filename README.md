# VoiceScribe

A .NET 9.0 console application that processes WAV audio files, transcribes them using OpenAI's Whisper API, and writes the transcriptions to Logseq journal files.

## Features

- Batch processes WAV files from an input folder
- Transcribes audio using OpenAI Whisper API with automatic language detection
- Writes transcriptions to Logseq journal files organized by recording date
- Parses recording timestamps from filenames (format: `R20260103110314.WAV`)
- Moves processed files to completed/failed folders

## Requirements

- .NET 9.0 SDK
- OpenAI API key
- Logseq graph

## Installation

```bash
git clone <repository-url>
cd voicescribe
dotnet restore
dotnet build
```

Or use the Makefile:

```bash
make build
make install  # Installs to /usr/local/bin (requires sudo)
```

## Configuration

Configuration is loaded from `~/.config/voicescribe/config.yaml`. On first run, a default configuration file is created.

### Config File

```yaml
input_folder: input
completed_folder: completed
failed_folder: failed
open_ai_key: your-openai-api-key-here
logseq_path: ~/Notes
```

| Option | Description | Default |
|--------|-------------|---------|
| `input_folder` | Folder to scan for WAV files | `input` |
| `completed_folder` | Destination for processed files | `completed` |
| `failed_folder` | Destination for failed files | `failed` |
| `open_ai_key` | OpenAI API key for Whisper | (required) |
| `logseq_path` | Path to Logseq graph | (required) |

Paths support `~/` expansion for home directory.

### Environment Variables

Environment variables override config file values:

| Variable | Overrides |
|----------|-----------|
| `VOICESCRIBE_INPUT_FOLDER` | `input_folder` |
| `VOICESCRIBE_COMPLETED_FOLDER` | `completed_folder` |
| `VOICESCRIBE_FAILED_FOLDER` | `failed_folder` |
| `VOICESCRIBE_OPENAI_KEY` | `open_ai_key` |
| `VOICESCRIBE_LOGSEQ_PATH` | `logseq_path` |
| `OPENAI_API_KEY` | `open_ai_key` (fallback) |

## Usage

1. Run once to generate the default config:
   ```bash
   voicescribe
   ```

2. Edit `~/.config/voicescribe/config.yaml` with your settings.

3. Place WAV files in the input folder. Files must be named with the format `R<yyyyMMddHHmmss>.WAV` (e.g., `R20260103110314.WAV`).

4. Run the application:
   ```bash
   voicescribe
   ```

5. Processed files are moved to `completed/`. Failed files are moved to `failed/`.

## Folder Structure

```
voicescribe/
├── Services/       # Application services
├── Models/         # Data models
├── Utilities/      # Helper utilities
└── Program.cs      # Entry point
```

## Output Format

Transcriptions are written to Logseq journal files at `~/Notes/journals/YYYY_MM_DD.md`:

```markdown
- ## Audio Recordings
	- ### 11:03:14 AM
		- [transcribed text...]
	- ### 11:04:52 AM
		- [another transcription...]
```

## License

MIT License - see [LICENSE](LICENSE) for details.
