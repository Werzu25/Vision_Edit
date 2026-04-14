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

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string errorMessage = string.Empty;

    // ADDED: Missing bindings used in RegistrationPage.xaml
    public bool IsNotLoading => !IsLoading;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public RegistrationViewModel(IHttpClientFactory httpClientFactory, UserManager userManager)
    {
        _userManager = userManager;
        _httpClient = httpClientFactory.CreateClient("Base");
    }

    // FIXED: Renamed from RedirectToMainPage to RedirectToMainPageCommand name is correct,
    // but the navigation target was //MainPage — after registration the user is logged in
    // so they should land on //Editor, not the marketing landing page.
    // The back-to-login tap in the XAML calls this to go back to login, so we keep
    // that as a separate command below.
    [RelayCommand]
    public async Task RedirectToMainPage()
    {
        await Shell.Current.GoToAsync("//Login");
    }

    [RelayCommand]
    public async Task Register()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password)
            || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(FirstName))
        {
            ErrorMessage = "All fields are required";
            return;
        }

        if (!IsValidEmail(Email))
        {
            ErrorMessage = "Invalid email format";
            return;
        }

        IsLoading = true;
        try
        {
            UserModel user = new()
            {
                Username = Username,
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Password = Password
            };

            var result = await _httpClient.PostAsJsonAsync("User", user);
            if (result.IsSuccessStatusCode)
            {
                _userManager.Username = user.Username;
                // FIXED: was navigating to //MainPage (the landing page) after registration.
                // User is now authenticated so they should go straight to the editor.
                await Shell.Current.GoToAsync("//Editor");
            }
            else
            {
                ErrorMessage = "Registration failed. Username or email may already exist.";
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

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
