using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Storage;
using Tools;
using Vision_Edit.ViewModels;
using Vision_Edit.Views;

namespace Vision_Edit;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        _services = services;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var appShell = _services.GetRequiredService<AppShell>();
        return new Window(appShell);
    }

    protected override async void OnStart()
    {
        base.OnStart();

        // Check if API key is stored; if not, show popup
        var apiKey = await SecureStorage.Default.GetAsync("openai_api_key");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            var popup = _services.GetRequiredService<ApiKeyPopupPage>();
            await Application.Current?.MainPage?.Navigation.PushModalAsync(popup);
        }
    }
}
