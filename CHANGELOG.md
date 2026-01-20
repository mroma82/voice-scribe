# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.0.2] - 2026-01-19

### Changed

- Renamed project from digital-recorder to VoiceScribe
- Config directory changed to `~/.config/voicescribe/`
- Environment variables now use `VOICESCRIBE_` prefix

### Added

- YAML configuration file support (`~/.config/voicescribe/config.yaml`)
- Environment variable overrides for all config options
- Makefile for build, test, and install

## [0.0.1] - 2026-01-18

### Added

- Initial release
- WAV file batch processing from input folder
- Audio transcription using OpenAI Whisper API
- Logseq journal integration with "Audio Recordings" heading
- Timestamp parsing from filenames (R + yyyyMMddHHmmss format)
- Automatic file organization (completed/failed folders)
- Structured logging with Microsoft.Extensions.Logging
- Console logging output
