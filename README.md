# AudioPrep

AudioPrep is a lightweight Avalonia desktop utility that prepares clean audio from video or audio files for dubbing, editing, transcription, and AI workflows.

## Why this tool exists

Many media workflows start with the same repetitive prep step: pick a source file, isolate the right audio track, normalize or clean it, and export into a reliable format for downstream tools. AudioPrep makes that process deterministic and fast.

## Features (V1)

- Single-window desktop app (Avalonia)
- Simple mode by default for one-click WAV export
- Advanced mode for full preset and processing control
- Drag-and-drop file import
- File picker fallback
- FFprobe media inspection (container, duration, audio streams)
- Audio stream selection for multi-track media
- Localization-ready UI with English and Portuguese (Brazil)
- Export presets:
  - WAV (48 kHz, 16-bit PCM, mono)
  - WAV (48 kHz, 16-bit PCM, stereo)
  - MP3 (high quality)
  - AAC/M4A
- Optional processing flags:
  - Normalize loudness (`loudnorm`)
  - Trim silence (`silenceremove`)
  - Downmix to mono
  - Resample to target sample rate
- Progress and status updates during processing
- Expandable process log panel
- Open output folder shortcut
- Local settings persistence (preset/options/output folder + window size)

## Screenshots

Place screenshots in:

- `docs/screenshots/main-window.png`
- `docs/screenshots/processing.png`

## Solution structure

```text
AudioPrep.sln
src/
  AudioPrep.App/
    App.axaml
    Program.cs
    Services/
    ViewModels/
    Views/
  AudioPrep.Core/
    Commands/
    Exceptions/
    Models/
    Parsing/
    Presets/
    Services/
    Utilities/
    Validation/
  AudioPrep.Infrastructure/
    Persistence/
    Processes/
    Services/
tests/
  AudioPrep.Tests/
```

## Setup

1. Install .NET 9 SDK.
2. Clone this repository.
3. Build once. The app project auto-downloads bundled `ffmpeg.exe` and `ffprobe.exe` on Windows if they are missing.

```powershell
dotnet build AudioPrep.sln
```

The bootstrap script used by the build is:

- `src/AudioPrep.App/scripts/Ensure-BundledFfmpeg.ps1`

Default download source:

- `https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip`

Bundled binaries are stored at:

- `src/AudioPrep.App/tools/ffmpeg/win-x64/`

To skip auto-bootstrap (for offline or custom CI):

```powershell
dotnet build AudioPrep.sln -p:SkipFfmpegBootstrap=true
```

### FFmpeg/FFprobe discovery order

AudioPrep resolves tools from bundled folders only (no system `PATH` fallback):

1. `tools/ffmpeg/<runtime-id>/<tool>`
2. `tools/ffmpeg/<tool>`
3. `tools/<tool>`

For Windows x64, the expected runtime-id folder is `win-x64`.

Bundled files are copied to build and publish output automatically.

## UX Modes

- Simple mode (default): minimal UI focused on direct WAV extraction.
- Advanced mode: exposes stream/preset/processing controls and log tools.

## Build and run

```powershell
dotnet build AudioPrep.sln
dotnet run --project src/AudioPrep.App/AudioPrep.App.csproj
```

## Tests

```powershell
dotnet test AudioPrep.sln
```

Current tests cover:

- output preset mapping
- FFmpeg argument generation
- filter chain generation
- settings roundtrip
- output filename generation
- FFprobe JSON parsing
- validation for invalid inputs

## Native AOT publish (Windows)

```powershell
dotnet publish src/AudioPrep.App/AudioPrep.App.csproj -c Release -r win-x64 -p:PublishAot=true -p:StripSymbols=true
```

Published output is generated under:

- `src/AudioPrep.App/bin/Release/net9.0/win-x64/publish/`

## Supported formats

Input support is driven by your bundled FFmpeg build. Typical formats include MP4, MOV, MKV, AVI, MP3, WAV, M4A, AAC, FLAC, and OGG.

Output formats in V1:

- WAV (`pcm_s16le`)
- MP3 (`libmp3lame`, `-q:a 2`)
- AAC/M4A (`aac`, `-b:a 192k`)

## Roadmap

- sequential batch processing
- drag multiple files
- saved preset profiles
- direct upload handoff integrations
- waveform preview
- transcript sidecar generation
- automatic chunk splitting
- watch folder mode

## License

MIT License (see `LICENSE`).

Note: FFmpeg binaries are distributed under their own licenses. Review FFmpeg and included codec licenses before redistribution.
