using System.Net.Http.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;

namespace Vision_Edit.ViewModels;

public partial class RegistrationViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;

    [ObservableProperty] private string _username;
    [ObservableProperty] private string _firstName;
    [ObservableProperty] private string _lastName;
    [ObservableProperty] private string _email;
    [ObservableProperty] private string _password;
    
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
        UserModel user = new()
        {
            Username = _username,
            FirstName = _firstName,
            LastName = _lastName,
            Email = _email,
            Password = _password
        }; 
        var result =  await _httpClient.PostAsJsonAsync("/User", user);
        if (result.IsSuccessStatusCode)
        {
            SwitchToMainPage();
        }
    }
}