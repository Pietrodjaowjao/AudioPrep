using AudioPrep.App.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;

namespace AudioPrep.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void DropZone_DragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.DataTransfer.Contains(DataFormat.File)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private async void DropZone_Drop(object? sender, DragEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var firstFile = e.DataTransfer.TryGetFiles()?.FirstOrDefault();
        var localPath = firstFile?.Path?.LocalPath;
        if (string.IsNullOrWhiteSpace(localPath))
        {
            return;
        }

        await viewModel.LoadInputFileAsync(localPath);
    }
}
