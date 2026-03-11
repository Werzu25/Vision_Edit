using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using Tools;
using Vision_Edit.ViewModels;
using Vision_Edit.Views;

namespace Vision_Edit;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.Services.AddHttpClient("Base", c =>
            c.BaseAddress = new Uri("https://localhost:44311/api/"));

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf",   "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf",  "OpenSansSemibold");
            });

        // Storage
        builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);

        // Core singletons
        builder.Services.AddSingleton<UserManager>();
        builder.Services.AddSingleton<ApiHandler>();
        builder.Services.AddSingleton<EditorViewModel>();

        // Shell
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<AppShellViewModel>();

        // Auth pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegistrationPage>();
        builder.Services.AddTransient<RegistrationViewModel>();

        // Editor page
        builder.Services.AddTransient<EditorPage>();
        builder.Services.AddTransient<EditorView>();

        // Chat
        builder.Services.AddTransient<ChatView>();
        builder.Services.AddTransient<ChatViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
