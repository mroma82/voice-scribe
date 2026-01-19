# Digital Recorder

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
- Logseq graph at `~/Notes`

## Installation

```bash
git clone <repository-url>
cd digital-recorder
dotnet restore
dotnet build
```

## Usage

1. Set your OpenAI API key:
   ```bash
   export OPENAI_API_KEY="your-api-key-here"
   ```

2. Place WAV files in the `input/` folder. Files must be named with the format `R<yyyyMMddHHmmss>.WAV` (e.g., `R20260103110314.WAV`).

3. Run the application:
   ```bash
   dotnet run
   ```

4. Processed files are moved to `completed/`. Failed files are moved to `failed/`.

## Folder Structure

```
digital-recorder/
├── input/          # Place WAV files here
├── completed/      # Successfully processed files
├── failed/         # Files that failed processing
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
