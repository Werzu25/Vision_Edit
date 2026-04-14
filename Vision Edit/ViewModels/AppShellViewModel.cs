using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tools;

namespace Vision_Edit.ViewModels;

public partial class AppShellViewModel : ObservableObject
{
    private readonly UserManager _userManager;

    public UserManager UserManager => _userManager;

    public AppShellViewModel(UserManager userManager)
    {
        _userManager = userManager;
    }

    // FIXED: Was only clearing the username but never navigating away, so the user
    // remained on whatever screen they were on after logging out.
    [RelayCommand]
    private async Task Logout()
    {
        _userManager.Username = string.Empty;
        await Shell.Current.GoToAsync("//Login");
    }
}
