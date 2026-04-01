using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AudioPrep.App.Services;

public sealed class FileDialogService : IFileDialogService
{
    private readonly Window _owner;

    public FileDialogService(Window owner)
    {
        _owner = owner;
    }

    public async Task<string?> PickInputFileAsync()
    {
        var topLevel = TopLevel.GetTopLevel(_owner);
        if (topLevel?.StorageProvider is null)
        {
            return null;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select an audio or video file",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Media files")
                {
                    Patterns =
                    [
                        "*.mp4", "*.mov", "*.mkv", "*.avi",
                        "*.mp3", "*.wav", "*.m4a", "*.aac", "*.flac", "*.ogg"
                    ]
                },
                FilePickerFileTypes.All
            ]
        });

        return ToLocalPath(files.FirstOrDefault());
    }

    public async Task<string?> PickOutputFolderAsync()
    {
        var topLevel = TopLevel.GetTopLevel(_owner);
        if (topLevel?.StorageProvider is null)
        {
            return null;
        }

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select output folder",
            AllowMultiple = false
        });

        return ToLocalPath(folders.FirstOrDefault());
    }

    private static string? ToLocalPath(IStorageItem? item)
    {
        if (item?.Path is null)
        {
            return null;
        }

        var uri = item.Path;
        if (uri.IsAbsoluteUri && uri.IsFile)
        {
            return uri.LocalPath;
        }

        return uri.OriginalString;
    }
}
