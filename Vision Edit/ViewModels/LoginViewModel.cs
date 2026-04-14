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

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string errorMessage = string.Empty;

    // ADDED: Missing bindings used in LoginPage.xaml
    public bool IsNotLoading => !IsLoading;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public LoginViewModel(IHttpClientFactory httpClientFactory, UserManager userManager)
    {
        _userManager = userManager;
        _httpClient = httpClientFactory.CreateClient("Base");
    }

    // FIXED: Removed the unused string pageName parameter — nothing in the XAML passes one
    [RelayCommand]
    public async Task Login()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required";
            return;
        }

        IsLoading = true;
        try
        {
            LoginModel login = new()
            {
                Username = this.Username,
                Password = Password
            };

            var result = await _httpClient.PostAsJsonAsync("User/login", login);
            if (result.IsSuccessStatusCode)
            {
                if (!_userManager.IsLoggedIn)
                    _userManager.Username = Username;

                await Shell.Current.GoToAsync("//Workspace");
            }
            else
            {
                ErrorMessage = "Login failed. Please check your credentials.";
            }
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Network error. Please check your connection.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // FIXED: Renamed from RedirectToRegisterPage to NavigateToRegister
    // to match XAML binding {Binding NavigateToRegisterCommand}
    [RelayCommand]
    public async Task NavigateToRegister()
    {
        await Shell.Current.GoToAsync("//RegistrationPage");
    }
}
