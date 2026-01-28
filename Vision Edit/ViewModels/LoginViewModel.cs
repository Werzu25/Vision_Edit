using System.Net.Http.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using Tools;

namespace Vision_Edit.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;
    private readonly UserManager _userManager;

    [ObservableProperty] private string _username;
    [ObservableProperty] private string _password;

    public LoginViewModel(IHttpClientFactory httpClientFactory, UserManager userManager) 
    {
        _userManager = userManager;
        _httpClient = httpClientFactory.CreateClient("Base");
    }
    
    [RelayCommand]
    public async Task Login()
    {
        LoginModel login = new()
        {
            Username = Username,
            Password = Password
        };
        var result = await _httpClient.PostAsJsonAsync("User/login", login);
        if (result.IsSuccessStatusCode)
        {
            if (!_userManager.IsLoggedIn)
            {
                _userManager.Username = Username;
            }
            await Shell.Current.GoToAsync("//MainPage");
        }
    }

    [RelayCommand]
    public async Task RedirectToRegisterPage()
    {
        await Shell.Current.GoToAsync("//RegisterPage");
    }
}