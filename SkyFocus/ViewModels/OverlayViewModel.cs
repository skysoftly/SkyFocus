using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkyFocus.Services;

namespace SkyFocus.ViewModels;

public partial class OverlayViewModel : ViewModelBase, IConfirmService
{
    [ObservableProperty] private bool _overlayVisible;
    [ObservableProperty] private string _overlayText = "";

    private TaskCompletionSource<bool>? _tcs;

    public async Task<bool> Show(string text)
    {
        OverlayText = text;
        OverlayVisible = true;

        _tcs = new TaskCompletionSource<bool>();

        return await _tcs.Task;
    }

    [RelayCommand]
    private void ConfirmOverlay()
    {
        OverlayVisible = false;
        _tcs?.TrySetResult(true);
    }

    [RelayCommand]
    private void CancelOverlay()
    {
        OverlayVisible = false;
        _tcs?.TrySetResult(false);
    }
}