using System.Collections.Specialized;
using Microsoft.Extensions.DependencyInjection;
using Vision_Edit.ViewModels;

namespace Vision_Edit.Views;

public partial class ChatView : ContentView
{
    private ChatViewModel? _vm;

    public ChatView() : this(App.Current!.Handler!.MauiContext!.Services.GetRequiredService<ChatViewModel>())
    {
    }

    public ChatView(ChatViewModel chatViewModel)
    {
        InitializeComponent();
        _vm = chatViewModel;
        BindingContext = chatViewModel;
        chatViewModel.Messages.CollectionChanged += OnMessagesChanged;
    }

    private void OnMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_vm is null || _vm.Messages.Count == 0) return;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                MessagesView.ScrollTo(_vm.Messages.Count - 1,
                    position: ScrollToPosition.End, animate: true);
            }
            catch { /* CollectionView may not be ready */ }
        });
    }
}
