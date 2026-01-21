using CommunityToolkit.Mvvm.ComponentModel;
using Tools;

namespace Vision_Edit.ViewModels;

public class AppViewModel
{
    private readonly UserManager _userManager;

    public AppViewModel(UserManager userManager)
    {
        _userManager = userManager;
    }

    public async Task<Boolean> IsLoggedIn()
    {
        if (_userManager.IsLoggedIn())
            return true;
        return false;
    }
}