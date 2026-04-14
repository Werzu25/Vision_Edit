using Microsoft.Extensions.DependencyInjection;
using Vision_Edit.ViewModels;

namespace Vision_Edit.Views;

public partial class WorkspacePage : ContentPage
{
    public WorkspacePage(EditorViewModel editorViewModel, ChatViewModel chatViewModel)
    {
        InitializeComponent();

        // Each child ContentView owns its own BindingContext so compiled
        // bindings resolve against the correct ViewModel type.
        EditorArea.BindingContext = editorViewModel;
        ChatArea.BindingContext   = chatViewModel;
    }

    private async void OnEditorOnlyClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//Editor");

    private async void OnChatOnlyClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//Chat");
}
