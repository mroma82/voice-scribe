# Software Bill of Materials (SBOM)

## Project Information

- **Name:** digital-recorder
- **Version:** 1.0.0
- **Target Framework:** .NET 9.0
- **Generated:** 2026-01-18

## Direct Dependencies

| Package | Version | License |
|---------|---------|---------|
| Microsoft.Extensions.Logging | 9.0.0 | MIT |
| Microsoft.Extensions.Logging.Console | 9.0.0 | MIT |
| OpenAI | 2.1.0 | MIT |

## Transitive Dependencies

| Package | Version | License |
|---------|---------|---------|
| Microsoft.Extensions.Configuration | 9.0.0 | MIT |
| Microsoft.Extensions.Configuration.Abstractions | 9.0.0 | MIT |
| Microsoft.Extensions.Configuration.Binder | 9.0.0 | MIT |
| Microsoft.Extensions.DependencyInjection | 9.0.0 | MIT |
| Microsoft.Extensions.DependencyInjection.Abstractions | 9.0.0 | MIT |
| Microsoft.Extensions.Logging.Abstractions | 9.0.0 | MIT |
| Microsoft.Extensions.Logging.Configuration | 9.0.0 | MIT |
| Microsoft.Extensions.Options | 9.0.0 | MIT |
| Microsoft.Extensions.Options.ConfigurationExtensions | 9.0.0 | MIT |
| Microsoft.Extensions.Primitives | 9.0.0 | MIT |
| System.ClientModel | 1.2.1 | MIT |
| System.Diagnostics.DiagnosticSource | 6.0.1 | MIT |
| System.Memory.Data | 6.0.0 | MIT |
| System.Runtime.CompilerServices.Unsafe | 6.0.0 | MIT |
| System.Text.Encodings.Web | 6.0.0 | MIT |
| System.Text.Json | 6.0.10 | MIT |

## External Services

| Service | Purpose |
|---------|---------|
| OpenAI Whisper API | Audio transcription |

## Runtime Requirements

- .NET 9.0 Runtime
- Network access to OpenAI API (api.openai.com)
- File system access for input/output folders
- Logseq graph directory at ~/Notes
