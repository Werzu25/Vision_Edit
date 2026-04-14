using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Vision_Edit.ViewModels;

namespace Vision_Edit.Views;

public partial class ChatView : ContentView
{
    private ChatViewModel? _vm;

    // Parameterless ctor used when XAML instantiates the view (WorkspacePage, ChatPage)
    public ChatView() : this(App.Current!.Handler!.MauiContext!.Services.GetRequiredService<ChatViewModel>())
    {
    }

    public ChatView(ChatViewModel vm)
    {
        InitializeComponent();
        Attach(vm);
    }

    // WorkspacePage overrides BindingContext after InitializeComponent —
    // re-attach subscriptions to the correct instance.
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is ChatViewModel vm)
            Attach(vm);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private void Attach(ChatViewModel vm)
    {
        if (ReferenceEquals(_vm, vm)) return;

        // Detach old subscriptions
        if (_vm is not null)
        {
            _vm.Messages.CollectionChanged -= OnMessagesChanged;
            _vm.PropertyChanged           -= OnVmPropertyChanged;
        }

        _vm            = vm;
        BindingContext = vm;

        vm.Messages.CollectionChanged += OnMessagesChanged;
        vm.PropertyChanged           += OnVmPropertyChanged;
    }

    private void OnMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => ScrollToBottom();

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ChatViewModel.StreamingContent))
            ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                // ScrollView.ScrollToAsync with MaxValue reliably scrolls to
                // the bottom regardless of content height, unlike CollectionView.ScrollTo.
                await Scroll.ScrollToAsync(0, double.MaxValue, animated: false);
            }
            catch { /* view may not be laid out yet */ }
        });
    }
}
