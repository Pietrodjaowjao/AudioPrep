param(
    [string]$DestinationDir = (Join-Path $PSScriptRoot "..\tools\ffmpeg\win-x64"),
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$ffmpegExe = Join-Path $DestinationDir "ffmpeg.exe"
$ffprobeExe = Join-Path $DestinationDir "ffprobe.exe"

if (-not $Force -and (Test-Path $ffmpegExe) -and (Test-Path $ffprobeExe)) {
    Write-Host "Bundled FFmpeg already present at $DestinationDir"
    exit 0
}

$downloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip"
$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("audioprep-ffmpeg-" + [Guid]::NewGuid().ToString("N"))
$archivePath = Join-Path $tempRoot "ffmpeg.zip"
$extractPath = Join-Path $tempRoot "extract"

try {
    New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

    Write-Host "Downloading FFmpeg bundle from $downloadUrl"
    Invoke-WebRequest -Uri $downloadUrl -OutFile $archivePath

    Write-Host "Extracting FFmpeg bundle"
    Expand-Archive -LiteralPath $archivePath -DestinationPath $extractPath -Force

    $binDirectory = Get-ChildItem -Path $extractPath -Directory -Recurse |
        Where-Object { $_.Name -eq "bin" } |
        Select-Object -First 1

    if ($null -eq $binDirectory) {
        throw "Could not locate the FFmpeg 'bin' directory in the downloaded archive."
    }

    $sourceFfmpeg = Join-Path $binDirectory.FullName "ffmpeg.exe"
    $sourceFfprobe = Join-Path $binDirectory.FullName "ffprobe.exe"

    if (-not (Test-Path $sourceFfmpeg) -or -not (Test-Path $sourceFfprobe)) {
        throw "Downloaded archive did not contain ffmpeg.exe and ffprobe.exe."
    }

    New-Item -ItemType Directory -Path $DestinationDir -Force | Out-Null

    Copy-Item -LiteralPath $sourceFfmpeg -Destination $ffmpegExe -Force
    Copy-Item -LiteralPath $sourceFfprobe -Destination $ffprobeExe -Force

    Write-Host "Bundled FFmpeg binaries installed at $DestinationDir"
}
finally {
    if (Test-Path $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force
    }
}
