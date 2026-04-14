using Vision_Edit.ViewModels;

namespace Vision_Edit.Views;

public partial class ChatPage : ContentPage
{
    public ChatPage(ChatViewModel chatViewModel)
    {
        InitializeComponent();
        // Page and embedded ChatView share the same instance.
        // ChatView.OnBindingContextChanged will re-attach its subscriptions.
        BindingContext = chatViewModel;
        ChatArea.BindingContext = chatViewModel;
    }
}
