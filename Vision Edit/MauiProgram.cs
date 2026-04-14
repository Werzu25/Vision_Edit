using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Storage;
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
            c.BaseAddress = new Uri("https://ksw8zcnv-44311.euw.devtunnels.ms/api/"));

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf",  "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                // Drop SpaceMono.ttf into Resources/Fonts/ and register here
                // to restore monospace typography.
            });

        // Storage
        builder.Services.AddSingleton(FileSaver.Default);

        // Core singletons
        builder.Services.AddSingleton<UserManager>();
        builder.Services.AddSingleton(sp => CreateApiHandler());
        builder.Services.AddSingleton<EditorViewModel>();

        // Shell
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<AppShellViewModel>();

        // Auth pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegistrationPage>();
        builder.Services.AddTransient<RegistrationViewModel>();
        builder.Services.AddTransient<ApiKeyPopupPage>();

        // Editor page (standalone)
        builder.Services.AddTransient<EditorPage>();
        builder.Services.AddTransient<EditorView>();

        // Chat page (standalone)
        builder.Services.AddTransient<ChatPage>();
        builder.Services.AddTransient<ChatView>();
        builder.Services.AddTransient<ChatViewModel>();

        // Workspace (side-by-side)
        builder.Services.AddTransient<WorkspacePage>();

        // ── Platform handler customizations ──────────────────────────────
        // Remove all borders and focus rings from Editor and Entry controls.
        EditorHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
        {
#if WINDOWS
            var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            // Override WinUI theme resources so the focus state doesn't draw a border
            handler.PlatformView.Resources["TextControlBorderBrush"]             = transparent;
            handler.PlatformView.Resources["TextControlBorderBrushPointerOver"]  = transparent;
            handler.PlatformView.Resources["TextControlBorderBrushFocused"]      = transparent;
            handler.PlatformView.Resources["TextControlBorderBrushDisabled"]     = transparent;
            handler.PlatformView.Resources["TextControlBackground"]              = transparent;
            handler.PlatformView.Resources["TextControlBackgroundPointerOver"]   = transparent;
            handler.PlatformView.Resources["TextControlBackgroundFocused"]       = transparent;
            handler.PlatformView.Resources["TextControlBackgroundDisabled"]      = transparent;
            handler.PlatformView.Background = transparent;
#elif ANDROID
            handler.PlatformView.Background = null;
#elif IOS || MACCATALYST
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
            handler.PlatformView.Layer.BorderWidth = 0;
#endif
        });

        EntryHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
        {
#if WINDOWS
            var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Resources["TextControlBorderBrush"]             = transparent;
            handler.PlatformView.Resources["TextControlBorderBrushPointerOver"]  = transparent;
            handler.PlatformView.Resources["TextControlBorderBrushFocused"]      = transparent;
            handler.PlatformView.Resources["TextControlBorderBrushDisabled"]     = transparent;
            handler.PlatformView.Resources["TextControlBackground"]              = transparent;
            handler.PlatformView.Resources["TextControlBackgroundPointerOver"]   = transparent;
            handler.PlatformView.Resources["TextControlBackgroundFocused"]       = transparent;
            handler.PlatformView.Resources["TextControlBackgroundDisabled"]      = transparent;
            handler.PlatformView.Background = transparent;
#elif ANDROID
            handler.PlatformView.Background = null;
            handler.PlatformView.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
#elif IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
#endif
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }

    /// <summary>
    /// Factory method to create ApiHandler with API key from secure storage.
    /// If no key is stored, returns a placeholder that will be initialized later.
    /// </summary>
    private static ApiHandler CreateApiHandler()
    {
        var apiKey = SecureStorage.Default.GetAsync("openai_api_key")
            .GetAwaiter().GetResult() ?? string.Empty;

        return new ApiHandler(apiKey);
    }
}

