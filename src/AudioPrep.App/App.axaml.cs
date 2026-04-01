using AudioPrep.App.Localization;
using AudioPrep.App.Services;
using AudioPrep.App.ViewModels;
using AudioPrep.App.Views;
using AudioPrep.Core.Commands;
using AudioPrep.Core.Parsing;
using AudioPrep.Core.Services;
using AudioPrep.Infrastructure.Persistence;
using AudioPrep.Infrastructure.Processes;
using AudioPrep.Infrastructure.Services;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace AudioPrep.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var processRunner = new ProcessRunner();
            var toolResolver = new ExternalToolResolver();
            var ffprobeParser = new FfprobeJsonParser();
            var argumentBuilder = new FfmpegArgumentBuilder();
            ISettingsService settingsService = new JsonSettingsService();
            var currentSettings = settingsService.Load();
            var localizationService = new LocalizationService();

            var mediaProbeService = new MediaProbeService(processRunner, toolResolver, ffprobeParser);
            var audioProcessingService = new AudioProcessingService(processRunner, toolResolver, argumentBuilder);

            var mainWindow = new MainWindow();
            if (currentSettings.WindowWidth is > 500)
            {
                mainWindow.Width = currentSettings.WindowWidth.Value;
            }

            if (currentSettings.WindowHeight is > 400)
            {
                mainWindow.Height = currentSettings.WindowHeight.Value;
            }

            var fileDialogService = new FileDialogService(mainWindow);
            var mainWindowViewModel = new MainWindowViewModel(
                mediaProbeService,
                audioProcessingService,
                settingsService,
                localizationService,
                fileDialogService,
                currentSettings);

            mainWindow.Closing += (_, _) =>
            {
                mainWindowViewModel.SaveWindowSize(mainWindow.Width, mainWindow.Height);
            };

            mainWindow.DataContext = mainWindowViewModel;
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
