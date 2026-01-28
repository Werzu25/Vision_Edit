using System.Net.Http.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using Tools;

namespace Vision_Edit.ViewModels;

public partial class RegistrationViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;
    private readonly UserManager _userManager;

    [ObservableProperty] private string _username;
    [ObservableProperty] private string _firstName;
    [ObservableProperty] private string _lastName;
    [ObservableProperty] private string _email;
    [ObservableProperty] private string _password;
    
    public RegistrationViewModel (IHttpClientFactory httpClientFactory, UserManager userManager)
    {
        _userManager = userManager;
        _httpClient = httpClientFactory.CreateClient("Base");
    }
    
    [RelayCommand]
    public async Task RedirectToMainPage()
    {
        await Shell.Current.GoToAsync("//MainPage");
    }

    [RelayCommand]
    public async Task Register()
    {
        UserModel user = new()
        {
            Username = Username,
            FirstName = FirstName,
            LastName = LastName,
            Email = Email,
            Password = Password
        }; 
        var result =  await _httpClient.PostAsJsonAsync("User", user);
        if (result.IsSuccessStatusCode)
        {
            _userManager.Username = user.Username;
            await RedirectToMainPage();
        }
    }
}
