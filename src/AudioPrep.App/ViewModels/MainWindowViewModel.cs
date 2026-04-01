using System.Diagnostics;
using System.Globalization;
using System.Text;
using AudioPrep.App.Localization;
using AudioPrep.App.Services;
using AudioPrep.Core.Exceptions;
using AudioPrep.Core.Models;
using AudioPrep.Core.Presets;
using AudioPrep.Core.Services;
using AudioPrep.Core.Utilities;
using AudioPrep.Core.Validation;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;

namespace AudioPrep.App.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly IMediaProbeService _mediaProbeService;
    private readonly IAudioProcessingService _audioProcessingService;
    private readonly ISettingsService _settingsService;
    private readonly ILocalizationService _localizationService;
    private readonly IFileDialogService _fileDialogService;
    private readonly StringBuilder _logBuilder = new();

    private CancellationTokenSource? _processingCancellationTokenSource;
    private bool _suppressSettingsPersistence;
    private double? _windowWidth;
    private double? _windowHeight;

    private MainWindowState _currentState;
    private string? _inputPath;
    private string? _fileName;
    private string? _containerFormat;
    private TimeSpan? _duration;
    private IReadOnlyList<AudioStreamInfo> _audioStreams = Array.Empty<AudioStreamInfo>();
    private AudioStreamInfo? _selectedAudioStream;
    private OutputPreset _selectedPreset = OutputPresets.WavStereo48k;
    private string _outputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private string _outputFileName = "output_prepared.wav";
    private bool _normalizeLoudness;
    private bool _trimSilence;
    private bool _downmixToMono;
    private int _targetSampleRate = 48000;
    private double _progress;
    private string _statusMessage = string.Empty;
    private bool _isBusy;
    private bool _showLogs;
    private string _logText = string.Empty;
    private bool _isAdvancedMode;
    private LanguageOption _selectedLanguage = new(AppLanguage.English, "English");

    public MainWindowViewModel(
        IMediaProbeService mediaProbeService,
        IAudioProcessingService audioProcessingService,
        ISettingsService settingsService,
        ILocalizationService localizationService,
        IFileDialogService fileDialogService,
        AppSettings? initialSettings = null)
    {
        _mediaProbeService = mediaProbeService;
        _audioProcessingService = audioProcessingService;
        _settingsService = settingsService;
        _localizationService = localizationService;
        _fileDialogService = fileDialogService;

        Presets = OutputPresets.All;
        SampleRates = new[] { 16000, 22050, 32000, 44100, 48000 };
        LanguageOptions = _localizationService.SupportedLanguages;
        if (LanguageOptions.Count > 0)
        {
            _selectedLanguage = LanguageOptions[0];
        }

        BrowseFileCommand = new AsyncRelayCommand(BrowseFileAsync, () => !IsBusy);
        BrowseOutputFolderCommand = new AsyncRelayCommand(BrowseOutputFolderAsync, () => !IsBusy);
        ProcessCommand = new AsyncRelayCommand(ProcessAsync, () => CanProcess);
        CancelCommand = new RelayCommand(CancelProcessing, () => CanCancel);
        OpenOutputFolderCommand = new RelayCommand(OpenOutputFolder, () => Directory.Exists(OutputDirectory));
        ToggleLogsCommand = new RelayCommand(() => ShowLogs = !ShowLogs);
        ToggleModeCommand = new RelayCommand(ToggleMode, () => !IsBusy);

        var settings = initialSettings ?? _settingsService.Load();
        ApplySettings(settings);
        _windowWidth = settings.WindowWidth;
        _windowHeight = settings.WindowHeight;

        if (string.IsNullOrWhiteSpace(StatusMessage))
        {
            StatusMessage = L.StartStatus;
        }
    }

    public IAsyncRelayCommand BrowseFileCommand { get; }

    public IAsyncRelayCommand BrowseOutputFolderCommand { get; }

    public IAsyncRelayCommand ProcessCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public IRelayCommand OpenOutputFolderCommand { get; }

    public IRelayCommand ToggleLogsCommand { get; }

    public IRelayCommand ToggleModeCommand { get; }

    public UiText L => _localizationService.Text;

    public IReadOnlyList<LanguageOption> LanguageOptions { get; }

    public LanguageOption SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (value is null)
            {
                return;
            }

            if (SetProperty(ref _selectedLanguage, value))
            {
                _localizationService.CurrentLanguage = value.Language;
                RefreshLocalizedText();
                SaveSettings();
            }
        }
    }

    public MainWindowState CurrentState
    {
        get => _currentState;
        private set
        {
            if (SetProperty(ref _currentState, value))
            {
                RefreshCommandStates();
            }
        }
    }

    public bool IsAdvancedMode
    {
        get => _isAdvancedMode;
        set
        {
            if (SetProperty(ref _isAdvancedMode, value))
            {
                OnPropertyChanged(nameof(IsSimpleMode));
                OnPropertyChanged(nameof(ModeToggleText));
                OnPropertyChanged(nameof(ProcessButtonText));
                OnPropertyChanged(nameof(ShouldShowStreamSelector));

                if (!value)
                {
                    ShowLogs = false;
                }

                ApplyOutputFileNameForCurrentMode();
                SaveSettings();
            }
        }
    }

    public bool IsSimpleMode => !IsAdvancedMode;

    public string ModeToggleText => IsAdvancedMode ? L.SwitchToSimpleModeButton : L.SwitchToAdvancedModeButton;

    public string ProcessButtonText => IsAdvancedMode ? L.ProcessButton : L.ExtractWavButton;

    public string? InputPath
    {
        get => _inputPath;
        private set
        {
            if (SetProperty(ref _inputPath, value))
            {
                OnPropertyChanged(nameof(HasMediaLoaded));
                OnPropertyChanged(nameof(ShouldShowStreamSelector));
                RefreshCommandStates();
            }
        }
    }

    public string? FileName
    {
        get => _fileName;
        private set => SetProperty(ref _fileName, value);
    }

    public string? ContainerFormat
    {
        get => _containerFormat;
        private set => SetProperty(ref _containerFormat, value);
    }

    public TimeSpan? Duration
    {
        get => _duration;
        private set
        {
            if (SetProperty(ref _duration, value))
            {
                OnPropertyChanged(nameof(DurationText));
            }
        }
    }

    public string DurationText => Duration is null ? "-" : Duration.Value.ToString(@"hh\:mm\:ss");

    public IReadOnlyList<AudioStreamInfo> AudioStreams
    {
        get => _audioStreams;
        private set
        {
            if (SetProperty(ref _audioStreams, value))
            {
                OnPropertyChanged(nameof(AudioStreamCountText));
                OnPropertyChanged(nameof(ShouldShowStreamSelector));
            }
        }
    }

    public string AudioStreamCountText => AudioStreams.Count.ToString(CultureInfo.InvariantCulture);

    public bool ShouldShowStreamSelector => HasMediaLoaded && (IsAdvancedMode || AudioStreams.Count > 1);

    public AudioStreamInfo? SelectedAudioStream
    {
        get => _selectedAudioStream;
        set
        {
            if (SetProperty(ref _selectedAudioStream, value))
            {
                RefreshCommandStates();
            }
        }
    }

    public IReadOnlyList<OutputPreset> Presets { get; }

    public OutputPreset SelectedPreset
    {
        get => _selectedPreset;
        set
        {
            if (SetProperty(ref _selectedPreset, value))
            {
                TargetSampleRate = value.DefaultSampleRate;

                if (IsAdvancedMode)
                {
                    OutputFileName = OutputFileNameGenerator.EnsureExtension(OutputFileName, value.Extension);
                }

                SaveSettings();
                RefreshCommandStates();
            }
        }
    }

    public string OutputDirectory
    {
        get => _outputDirectory;
        set
        {
            if (SetProperty(ref _outputDirectory, value))
            {
                SaveSettings();
                RefreshCommandStates();
            }
        }
    }

    public string OutputFileName
    {
        get => _outputFileName;
        set
        {
            if (SetProperty(ref _outputFileName, value))
            {
                RefreshCommandStates();
            }
        }
    }

    public bool NormalizeLoudness
    {
        get => _normalizeLoudness;
        set
        {
            if (SetProperty(ref _normalizeLoudness, value))
            {
                SaveSettings();
            }
        }
    }

    public bool TrimSilence
    {
        get => _trimSilence;
        set
        {
            if (SetProperty(ref _trimSilence, value))
            {
                SaveSettings();
            }
        }
    }

    public bool DownmixToMono
    {
        get => _downmixToMono;
        set
        {
            if (SetProperty(ref _downmixToMono, value))
            {
                SaveSettings();
            }
        }
    }

    public IReadOnlyList<int> SampleRates { get; }

    public int TargetSampleRate
    {
        get => _targetSampleRate;
        set
        {
            if (SetProperty(ref _targetSampleRate, value))
            {
                SaveSettings();
            }
        }
    }

    public double Progress
    {
        get => _progress;
        private set => SetProperty(ref _progress, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommandStates();
            }
        }
    }

    public bool CanProcess =>
        !IsBusy &&
        !string.IsNullOrWhiteSpace(InputPath) &&
        SelectedAudioStream is not null &&
        !string.IsNullOrWhiteSpace(OutputDirectory) &&
        !string.IsNullOrWhiteSpace(OutputFileName);

    public bool CanCancel => IsBusy && CurrentState == MainWindowState.Processing;

    public bool ShowLogs
    {
        get => _showLogs;
        set
        {
            if (SetProperty(ref _showLogs, value))
            {
                OnPropertyChanged(nameof(LogToggleText));
            }
        }
    }

    public string LogText
    {
        get => _logText;
        private set => SetProperty(ref _logText, value);
    }

    public string LogToggleText => ShowLogs ? L.HideLogsButton : L.ShowLogsButton;

    public bool HasMediaLoaded => !string.IsNullOrWhiteSpace(InputPath);

    public async Task LoadInputFileAsync(string inputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            return;
        }

        try
        {
            IsBusy = true;
            CurrentState = MainWindowState.FileLoading;
            StatusMessage = L.InspectingMediaStatus;
            Progress = 0d;
            ClearLogs();
            AppendLog($"Probing file: {inputPath}");

            var mediaInfo = await _mediaProbeService
                .ProbeAsync(inputPath)
                .ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                InputPath = mediaInfo.Path;
                FileName = mediaInfo.FileName;
                ContainerFormat = mediaInfo.ContainerFormat;
                Duration = mediaInfo.Duration;
                AudioStreams = mediaInfo.AudioStreams;
                SelectedAudioStream = AudioStreams.FirstOrDefault();

                if (string.IsNullOrWhiteSpace(OutputDirectory))
                {
                    OutputDirectory = Path.GetDirectoryName(mediaInfo.Path) ?? OutputDirectory;
                }

                var extension = IsAdvancedMode
                    ? SelectedPreset.Extension
                    : OutputPresets.WavStereo48k.Extension;
                OutputFileName = OutputFileNameGenerator.Suggest(mediaInfo.FileName, extension);

                CurrentState = MainWindowState.Ready;
                StatusMessage = string.Format(
                    CultureInfo.CurrentCulture,
                    L.ReadyStatusTemplate,
                    mediaInfo.AudioStreams.Count);
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                CurrentState = MainWindowState.Failed;
                StatusMessage = ToUserMessage(ex);
                AppendExceptionDetails(ex);
            });
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() => IsBusy = false);
        }
    }

    public void SaveWindowSize(double width, double height)
    {
        _windowWidth = width > 0 ? width : _windowWidth;
        _windowHeight = height > 0 ? height : _windowHeight;

        var settings = CreateSettingsSnapshot();
        settings.WindowWidth = _windowWidth;
        settings.WindowHeight = _windowHeight;
        _settingsService.Save(settings);
    }

    private async Task BrowseFileAsync()
    {
        var filePath = await _fileDialogService.PickInputFileAsync();
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            await LoadInputFileAsync(filePath);
        }
    }

    private async Task BrowseOutputFolderAsync()
    {
        var folderPath = await _fileDialogService.PickOutputFolderAsync();
        if (!string.IsNullOrWhiteSpace(folderPath))
        {
            OutputDirectory = folderPath;
        }
    }

    private async Task ProcessAsync()
    {
        if (!CanProcess || SelectedAudioStream is null || string.IsNullOrWhiteSpace(InputPath))
        {
            return;
        }

        var effectivePreset = IsAdvancedMode ? SelectedPreset : OutputPresets.WavStereo48k;
        var outputFileName = OutputFileNameGenerator.EnsureExtension(OutputFileName, effectivePreset.Extension);
        OutputFileName = outputFileName;
        var outputPath = Path.Combine(OutputDirectory, outputFileName);

        if (File.Exists(outputPath))
        {
            CurrentState = MainWindowState.Failed;
            StatusMessage = L.OutputExistsStatus;
            return;
        }

        var processingOptions = new ProcessingOptions
        {
            AudioStreamIndex = SelectedAudioStream.Index,
            InputPath = InputPath,
            OutputPath = outputPath,
            CodecKind = effectivePreset.CodecKind,
            SampleRate = IsAdvancedMode ? TargetSampleRate : effectivePreset.DefaultSampleRate,
            Channels = IsAdvancedMode
                ? (DownmixToMono ? 1 : effectivePreset.DefaultChannels)
                : 2,
            NormalizeLoudness = IsAdvancedMode && NormalizeLoudness,
            TrimSilence = IsAdvancedMode && TrimSilence,
            InputDuration = Duration
        };

        var validationErrors = ProcessingOptionsValidator.Validate(processingOptions);
        if (validationErrors.Count > 0)
        {
            CurrentState = MainWindowState.Failed;
            StatusMessage = validationErrors[0];
            return;
        }

        _processingCancellationTokenSource?.Dispose();
        _processingCancellationTokenSource = new CancellationTokenSource();

        IsBusy = true;
        CurrentState = MainWindowState.Processing;
        StatusMessage = L.ProcessingStatus;
        Progress = 0d;
        ClearLogs();

        try
        {
            var processingResult = await _audioProcessingService.ProcessAsync(
                processingOptions,
                onProgress: ReportProgress,
                onLog: AppendLog,
                cancellationToken: _processingCancellationTokenSource.Token);

            CurrentState = MainWindowState.Completed;
            StatusMessage = string.Format(
                CultureInfo.CurrentCulture,
                L.CompletedStatusTemplate,
                Path.GetFileName(processingResult.OutputPath));
            Progress = 1d;
            SaveSettings();
        }
        catch (OperationCanceledException)
        {
            CurrentState = MainWindowState.Cancelled;
            StatusMessage = L.CancelledStatus;
        }
        catch (Exception ex)
        {
            CurrentState = MainWindowState.Failed;
            StatusMessage = ToUserMessage(ex);
            AppendExceptionDetails(ex);
        }
        finally
        {
            IsBusy = false;
            _processingCancellationTokenSource?.Dispose();
            _processingCancellationTokenSource = null;
            SaveSettings();
        }
    }

    private void CancelProcessing()
    {
        _processingCancellationTokenSource?.Cancel();
    }

    private void ToggleMode()
    {
        IsAdvancedMode = !IsAdvancedMode;
    }

    private void OpenOutputFolder()
    {
        if (string.IsNullOrWhiteSpace(OutputDirectory) || !Directory.Exists(OutputDirectory))
        {
            StatusMessage = L.OutputFolderMissingStatus;
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = OutputDirectory,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusMessage = string.Format(
                CultureInfo.CurrentCulture,
                L.OpenOutputFolderFailedTemplate,
                ex.Message);
        }
    }

    private void ApplySettings(AppSettings settings)
    {
        _suppressSettingsPersistence = true;

        try
        {
            var defaultLanguage = _localizationService.CurrentLanguage;
            if (LocalizationService.TryParseLanguage(settings.LastLanguage, out var parsedLanguage))
            {
                defaultLanguage = parsedLanguage;
            }

            var preferredLanguage = LanguageOptions.FirstOrDefault(l => l.Language == defaultLanguage)
                ?? LanguageOptions.First();
            SelectedLanguage = preferredLanguage;

            var selectedPreset = OutputPresets.FindById(settings.LastPresetId) ?? OutputPresets.WavStereo48k;
            SelectedPreset = selectedPreset;
            TargetSampleRate = settings.LastSampleRate > 0
                ? settings.LastSampleRate
                : selectedPreset.DefaultSampleRate;

            NormalizeLoudness = settings.NormalizeLoudness;
            TrimSilence = settings.TrimSilence;
            DownmixToMono = settings.DownmixToMono;
            IsAdvancedMode = settings.UseAdvancedMode;

            OutputDirectory = string.IsNullOrWhiteSpace(settings.LastOutputDirectory)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : settings.LastOutputDirectory;

            ApplyOutputFileNameForCurrentMode();
        }
        finally
        {
            _suppressSettingsPersistence = false;
        }
    }

    private void SaveSettings()
    {
        if (_suppressSettingsPersistence)
        {
            return;
        }

        _settingsService.Save(CreateSettingsSnapshot());
    }

    private AppSettings CreateSettingsSnapshot()
    {
        return new AppSettings
        {
            LastOutputDirectory = OutputDirectory,
            LastPresetId = SelectedPreset.Id,
            LastSampleRate = TargetSampleRate,
            NormalizeLoudness = NormalizeLoudness,
            TrimSilence = TrimSilence,
            DownmixToMono = DownmixToMono,
            UseAdvancedMode = IsAdvancedMode,
            LastLanguage = SelectedLanguage.Language.ToString(),
            WindowWidth = _windowWidth,
            WindowHeight = _windowHeight
        };
    }

    private void ReportProgress(ProcessingProgress progress)
    {
        _ = Dispatcher.UIThread.InvokeAsync(() =>
        {
            Progress = Math.Clamp(progress.Value, 0d, 1d);
            StatusMessage = IsAdvancedMode && !string.IsNullOrWhiteSpace(progress.Message)
                ? progress.Message
                : L.ProcessingStatus;
        });
    }

    private void ApplyOutputFileNameForCurrentMode()
    {
        var extension = IsAdvancedMode
            ? SelectedPreset.Extension
            : OutputPresets.WavStereo48k.Extension;

        OutputFileName = OutputFileNameGenerator.EnsureExtension(OutputFileName, extension);
    }

    private void RefreshLocalizedText()
    {
        OnPropertyChanged(nameof(L));
        OnPropertyChanged(nameof(LogToggleText));
        OnPropertyChanged(nameof(ModeToggleText));
        OnPropertyChanged(nameof(ProcessButtonText));

        if (CurrentState == MainWindowState.Idle)
        {
            StatusMessage = L.StartStatus;
        }
    }

    private void AppendLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _ = Dispatcher.UIThread.InvokeAsync(() =>
        {
            _logBuilder.AppendLine(message);
            LogText = _logBuilder.ToString();
        });
    }

    private void ClearLogs()
    {
        _logBuilder.Clear();
        LogText = string.Empty;
    }

    private void RefreshCommandStates()
    {
        BrowseFileCommand.NotifyCanExecuteChanged();
        BrowseOutputFolderCommand.NotifyCanExecuteChanged();
        ProcessCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
        OpenOutputFolderCommand.NotifyCanExecuteChanged();
        ToggleModeCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanProcess));
        OnPropertyChanged(nameof(CanCancel));
    }

    private string ToUserMessage(Exception exception)
    {
        return exception switch
        {
            AudioPrepException audioPrepException => audioPrepException.Message,
            _ => L.UnexpectedErrorStatus
        };
    }

    private void AppendExceptionDetails(Exception exception)
    {
        AppendLog(exception.Message);

        if (exception is AudioPrepException { Details: not null } audioPrepException)
        {
            AppendLog(audioPrepException.Details);
        }
    }
}
