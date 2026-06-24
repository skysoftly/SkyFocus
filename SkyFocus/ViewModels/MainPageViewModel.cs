namespace SkyFocus.ViewModels;

public partial class MainPageViewModel(
    AppsListViewModel appsList,
    AppInfoViewModel appInfo,
    GeneralStatisticsViewModel generalStatistics) : ViewModelBase
{
    public AppsListViewModel AppsList { get; } = appsList;
    public AppInfoViewModel AppInfo { get; } = appInfo;
    public GeneralStatisticsViewModel GeneralStatistics { get; } = generalStatistics;
}