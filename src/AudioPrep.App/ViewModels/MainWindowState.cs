namespace AudioPrep.App.ViewModels;

public enum MainWindowState
{
    Idle = 0,
    FileLoading = 1,
    Ready = 2,
    Processing = 3,
    Completed = 4,
    Failed = 5,
    Cancelled = 6
}
