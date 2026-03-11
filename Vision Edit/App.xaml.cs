using Microsoft.Extensions.DependencyInjection;
using Tools;
using Vision_Edit.ViewModels;

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
}