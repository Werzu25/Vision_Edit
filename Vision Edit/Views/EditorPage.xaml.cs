using Microsoft.Extensions.DependencyInjection;
using Vision_Edit.ViewModels;

namespace Vision_Edit.Views;

public partial class EditorPage : ContentPage
{
    public EditorPage()
    {
        InitializeComponent();
        BindingContext = App.Current!.Handler!.MauiContext!.Services.GetRequiredService<EditorViewModel>();
    }
}

