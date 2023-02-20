using Microsoft.UI.Xaml.Controls;
using WinUICommunity.Shared.DataModel;

namespace WinUICommunity.Shared.Internal;

public class NavigationArgs
{
    public NavigationView NavigationView;
    public object Parameter;
    public string JsonRelativeFilePath;
    public IncludedInBuildMode IncludedInBuildMode;
}