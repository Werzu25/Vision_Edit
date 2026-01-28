using Microsoft.Extensions.DependencyInjection;
using Tools;
using Vision_Edit.ViewModels;

namespace Vision_Edit;

public partial class App : Application
{
    public App(AppShell appShell)
    {
        InitializeComponent();
    }
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var appShell = Handler?.MauiContext?.Services.GetService<AppShell>();
        return new Window(appShell ?? new AppShell(new AppShellViewModel(new UserManager())));
    }
}