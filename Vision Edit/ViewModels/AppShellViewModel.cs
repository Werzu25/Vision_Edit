using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tools;

namespace Vision_Edit.ViewModels;

public partial class AppShellViewModel : ObservableObject
{
    [ObservableProperty]
    private UserManager _userManager;

    public AppShellViewModel(UserManager userManager)
    {
        _userManager = userManager;
    }

    [RelayCommand]
    private void Logout()
    {
        UserManager.Username = string.Empty;
    }
}