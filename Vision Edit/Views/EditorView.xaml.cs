using Microsoft.Extensions.DependencyInjection;
using Vision_Edit.ViewModels;

#if WINDOWS
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
#endif

namespace Vision_Edit.Views;

public partial class EditorView : ContentView
{
#if WINDOWS
    private TextBox? _nativeEditor;
#endif

    public EditorView() : this(App.Current!.Handler!.MauiContext!.Services.GetRequiredService<EditorViewModel>())
    {
    }

    public EditorView(EditorViewModel editorViewModel)
    {
        InitializeComponent();
        BindingContext = editorViewModel;

#if WINDOWS
        MainEditor.HandlerChanged += OnEditorHandlerChanged;
        Unloaded += OnUnloaded;
#endif
    }

#if WINDOWS
    private void OnEditorHandlerChanged(object? sender, EventArgs e)
    {
        if (_nativeEditor is not null)
            _nativeEditor.KeyDown -= OnEditorKeyDown;

        _nativeEditor = MainEditor.Handler?.PlatformView as TextBox;
        if (_nativeEditor is not null)
            _nativeEditor.KeyDown += OnEditorKeyDown;
    }

    private void OnEditorKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Tab) return;
        if (BindingContext is not EditorViewModel vm) return;
        if (!vm.IsCompletionVisible || string.IsNullOrWhiteSpace(vm.CompletionSuggestion)) return;

        if (vm.AcceptCompletionCommand.CanExecute(null))
            vm.AcceptCompletionCommand.Execute(null);

        e.Handled = true;
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        if (_nativeEditor is null) return;
        _nativeEditor.KeyDown -= OnEditorKeyDown;
        _nativeEditor = null;
    }
#endif
}
