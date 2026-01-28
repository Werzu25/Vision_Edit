using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tools;

namespace Vision_Edit.ViewModels;

public partial class AppShellViewModel : ObservableObject
{
    private readonly UserManager _userManager;

    [ObservableProperty] private bool _isLoggedIn; 

    public AppShellViewModel(UserManager userManager)
    {
        _userManager = userManager;
        _userManager.PropertyChanged += OnUserManagerPropertyChanged;
        IsLoggedIn = _userManager.IsLoggedIn;
    }

    private void OnUserManagerPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(UserManager.IsLoggedIn))
        {
            IsLoggedIn = _userManager.IsLoggedIn;
        }
    }

    [RelayCommand]
    private void Logout()
    {
        _userManager.Username = string.Empty;
    }
}