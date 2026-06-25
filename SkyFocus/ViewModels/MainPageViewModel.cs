namespace SkyFocus.ViewModels;

public partial class MainPageViewModel(
    AppsListViewModel appsList,
    AppInfoViewModel appInfo,
    GeneralStatisticsViewModel generalStatistics,
    TopAppsViewModel topAppsViewModel) : ViewModelBase
{
    public AppsListViewModel AppsList { get; } = appsList;
    public AppInfoViewModel AppInfo { get; } = appInfo;
    public GeneralStatisticsViewModel GeneralStatistics { get; } = generalStatistics;
    public TopAppsViewModel TopAppsViewModel { get; } = topAppsViewModel;
}