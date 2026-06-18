using SkyFocus.Utils;
namespace SkyFocus.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public AppsListViewModel AppsList { get; }

    public MainWindowViewModel()
    {
        AppsList = new AppsListViewModel();
    }
}