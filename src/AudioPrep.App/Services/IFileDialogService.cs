namespace AudioPrep.App.Services;

public interface IFileDialogService
{
    Task<string?> PickInputFileAsync();

    Task<string?> PickOutputFolderAsync();
}
