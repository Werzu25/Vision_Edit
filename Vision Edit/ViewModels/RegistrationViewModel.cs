using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vision_Edit.ViewModels;

public partial class RegistrationViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;
    
    public RegistrationViewModel (HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    [RelayCommand]
    public async Task SwitchToMainPage()
    {
        await Shell.Current.GoToAsync("//MainPage");
    }

    [RelayCommand]
    public async Task Register()
    {
        _httpClient.PostAsync("", new StringContent(""));
    }
}