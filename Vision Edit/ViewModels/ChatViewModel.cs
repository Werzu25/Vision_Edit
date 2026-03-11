using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tools;

namespace Vision_Edit.ViewModels;

public class ChatMessage
{
    public string Content { get; init; } = string.Empty;
    public bool IsUser { get; init; }
}

public partial class ChatViewModel : ObservableObject
{
    private readonly ApiHandler _apiHandler;
    private readonly EditorViewModel _editorViewModel;

    [ObservableProperty] private string _userInput = string.Empty;
    [ObservableProperty] private bool _isLoading;

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public ChatViewModel(ApiHandler apiHandler, EditorViewModel editorViewModel)
    {
        _apiHandler = apiHandler;
        _editorViewModel = editorViewModel;
    }

    [RelayCommand]
    public async Task GetApiContent()
    {
        string input = UserInput;

        if (_editorViewModel.SelectedText is { Length: > 0 } selectedText)
            input += $"\n\nSelected text:\n{selectedText}";

        if (string.IsNullOrWhiteSpace(input)) return;

        Messages.Add(new ChatMessage { Content = input.Trim(), IsUser = true });
        UserInput = string.Empty;
        IsLoading = true;

        try
        {
            string history = string.Join("\n", Messages.Select(
                m => $"{(m.IsUser ? "User" : "Assistant")}: {m.Content}"));
            string result = await _apiHandler.getApiResponse($"{input}\n\nContext:\n{history}");
            Messages.Add(new ChatMessage { Content = result, IsUser = false });
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void ClearChat() => Messages.Clear();
}
