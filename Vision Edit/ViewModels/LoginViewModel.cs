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

    public LoginViewModel(HttpClient httpClient, UserManager userManager) 
    {
        _userManager = userManager;
        _httpClient = httpClient;
    }
    
    [RelayCommand]
    public async Task Login()
    {
        var result = await _httpClient.PostAsJsonAsync($"/User/login", new
        {
            Username = Username,
            Password = Password
        });
        
    }

    [RelayCommand]
    public async Task RedirectToRegisterPage()
    {
        await Shell.Current.GoToAsync("//RegisterPage");
    }
}