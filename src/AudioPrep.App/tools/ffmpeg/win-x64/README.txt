This folder contains bundled FFmpeg binaries for Windows x64.

Expected files:
- ffmpeg.exe
- ffprobe.exe

By default, `dotnet build` runs `scripts/Ensure-BundledFfmpeg.ps1` and auto-downloads
these binaries if they are missing.
