using SkyFocus.Services;
using SkyFocus.Utils;

namespace SkyFocus.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public AppsListViewModel AppsList { get; }
    public AppInfoViewModel AppInfo { get; }

    public MainWindowViewModel(AppsListViewModel appsList, AppInfoViewModel appInfo)
    {
        AppsList = appsList;
        AppInfo = appInfo;
    }
}