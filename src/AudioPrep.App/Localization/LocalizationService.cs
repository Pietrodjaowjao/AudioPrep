using System.Globalization;

namespace AudioPrep.App.Localization;

public sealed class LocalizationService : ILocalizationService
{
    private static readonly UiText EnglishText = new()
    {
        AppTitle = "AudioPrep",
        Subtitle = "Prepare audio from media files for dubbing, editing, and AI workflows.",
        LanguageLabel = "Language",
        DropZoneText = "Drop audio or video files here",
        BrowseFilesButton = "Browse files",
        FileSummaryTitle = "File Summary",
        FileNameLabel = "File name",
        PathLabel = "Path",
        DurationLabel = "Duration",
        ContainerLabel = "Container",
        AudioStreamsLabel = "Audio streams",
        AudioStreamSectionTitle = "Audio Stream",
        OutputTitle = "Output",
        PresetLabel = "Preset",
        OutputFolderLabel = "Output folder",
        BrowseFolderButton = "Browse...",
        OutputFileNameLabel = "Output file name",
        ProcessingOptionsTitle = "Processing Options",
        NormalizeLoudnessLabel = "Normalize loudness",
        TrimSilenceLabel = "Trim silence",
        DownmixToMonoLabel = "Downmix to mono",
        SampleRateLabel = "Sample rate",
        ProcessButton = "Process",
        ExtractWavButton = "Extract WAV",
        CancelButton = "Cancel",
        OpenOutputFolderButton = "Open output folder",
        ShowLogsButton = "Show logs",
        HideLogsButton = "Hide logs",
        ProcessLogsHeader = "Process logs",
        SwitchToAdvancedModeButton = "Advanced mode",
        SwitchToSimpleModeButton = "Simple mode",
        SimpleModeTitle = "Quick Extract",
        SimpleModeDescription = "Simple mode exports a clean high-quality WAV with one click.",
        SimpleModeOutputLabel = "Output format",
        SimpleModeOutputValue = "WAV (48 kHz, 16-bit PCM, stereo)",
        StartStatus = "Drop a file to get started.",
        InspectingMediaStatus = "Inspecting media...",
        ReadyStatusTemplate = "Ready. {0} audio stream(s) found.",
        ProcessingStatus = "Processing...",
        CompletedStatusTemplate = "Completed: {0}",
        CancelledStatus = "Processing cancelled.",
        OutputExistsStatus = "Output file already exists. Choose a different output file name.",
        OutputFolderMissingStatus = "Output folder does not exist.",
        OpenOutputFolderFailedTemplate = "Could not open folder: {0}",
        UnexpectedErrorStatus = "An unexpected error occurred."
    };

    private static readonly UiText PortugueseBrazilText = new()
    {
        AppTitle = "AudioPrep",
        Subtitle = "Prepare áudio de arquivos de mídia para dublagem, edição e fluxos de IA.",
        LanguageLabel = "Idioma",
        DropZoneText = "Solte arquivos de áudio ou vídeo aqui",
        BrowseFilesButton = "Selecionar arquivos",
        FileSummaryTitle = "Resumo do arquivo",
        FileNameLabel = "Nome do arquivo",
        PathLabel = "Caminho",
        DurationLabel = "Duração",
        ContainerLabel = "Contêiner",
        AudioStreamsLabel = "Faixas de áudio",
        AudioStreamSectionTitle = "Faixa de áudio",
        OutputTitle = "Saída",
        PresetLabel = "Predefinição",
        OutputFolderLabel = "Pasta de saída",
        BrowseFolderButton = "Escolher...",
        OutputFileNameLabel = "Nome do arquivo de saída",
        ProcessingOptionsTitle = "Opções de processamento",
        NormalizeLoudnessLabel = "Normalizar loudness",
        TrimSilenceLabel = "Cortar silêncio",
        DownmixToMonoLabel = "Converter para mono",
        SampleRateLabel = "Taxa de amostragem",
        ProcessButton = "Processar",
        ExtractWavButton = "Extrair WAV",
        CancelButton = "Cancelar",
        OpenOutputFolderButton = "Abrir pasta de saída",
        ShowLogsButton = "Mostrar logs",
        HideLogsButton = "Ocultar logs",
        ProcessLogsHeader = "Logs do processo",
        SwitchToAdvancedModeButton = "Modo avançado",
        SwitchToSimpleModeButton = "Modo simples",
        SimpleModeTitle = "Extração rápida",
        SimpleModeDescription = "O modo simples exporta um WAV limpo e de alta qualidade com um clique.",
        SimpleModeOutputLabel = "Formato de saída",
        SimpleModeOutputValue = "WAV (48 kHz, PCM 16-bit, estéreo)",
        StartStatus = "Solte um arquivo para começar.",
        InspectingMediaStatus = "Inspecionando mídia...",
        ReadyStatusTemplate = "Pronto. {0} faixa(s) de áudio encontrada(s).",
        ProcessingStatus = "Processando...",
        CompletedStatusTemplate = "Concluído: {0}",
        CancelledStatus = "Processamento cancelado.",
        OutputExistsStatus = "O arquivo de saída já existe. Escolha outro nome.",
        OutputFolderMissingStatus = "A pasta de saída não existe.",
        OpenOutputFolderFailedTemplate = "Não foi possível abrir a pasta: {0}",
        UnexpectedErrorStatus = "Ocorreu um erro inesperado."
    };

    public LocalizationService(AppLanguage? preferredLanguage = null)
    {
        SupportedLanguages = new[]
        {
            new LanguageOption(AppLanguage.English, "English"),
            new LanguageOption(AppLanguage.PortugueseBrazil, "Português (Brasil)")
        };

        CurrentLanguage = preferredLanguage ?? DetectDefaultLanguage();
    }

    public IReadOnlyList<LanguageOption> SupportedLanguages { get; }

    public AppLanguage CurrentLanguage { get; set; }

    public UiText Text => CurrentLanguage == AppLanguage.PortugueseBrazil
        ? PortugueseBrazilText
        : EnglishText;

    public static AppLanguage DetectDefaultLanguage()
    {
        var name = CultureInfo.CurrentUICulture.Name;
        if (name.StartsWith("pt-BR", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("pt", StringComparison.OrdinalIgnoreCase))
        {
            return AppLanguage.PortugueseBrazil;
        }

        return AppLanguage.English;
    }

    public static bool TryParseLanguage(string? value, out AppLanguage language)
    {
        language = AppLanguage.English;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (Enum.TryParse<AppLanguage>(value, ignoreCase: true, out var parsed))
        {
            language = parsed;
            return true;
        }

        return false;
    }
}
