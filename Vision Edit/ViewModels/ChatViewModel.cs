using System.Collections.ObjectModel;
using System.Text;
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

    // Proper multi-turn history sent to the API each call
    private readonly List<(string role, string content)> _history = new();
    private CancellationTokenSource? _streamCts;

    [ObservableProperty]
    private string userInput = string.Empty;

    [ObservableProperty]
    private bool isStreaming;

    [ObservableProperty]
    private string streamingContent = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public ChatViewModel(ApiHandler apiHandler, EditorViewModel editorViewModel)
    {
        _apiHandler = apiHandler;
        _editorViewModel = editorViewModel;
    }

    [RelayCommand]
    public async Task SendMessage()
    {
        string input = UserInput.Trim();
        if (string.IsNullOrWhiteSpace(input) || IsStreaming) return;

        // Cancel any ongoing stream and clear state
        CancelStream();
        ErrorMessage = string.Empty;
        UserInput = string.Empty;

        // Capture editor context before adding to history
        string? editorCtx = _editorViewModel.SelectedText is { Length: > 0 } s ? s : null;

        // Add user turn to history and visible messages
        _history.Add(("user", input));
        Messages.Add(new ChatMessage { Content = input, IsUser = true });

        IsStreaming = true;
        StreamingContent = string.Empty;

        _streamCts = new CancellationTokenSource();
        var ct = _streamCts.Token;
        var response = new StringBuilder();

        try
        {
            await foreach (var token in _apiHandler.StreamChatAsync(_history, editorCtx, ct))
            {
                response.Append(token);
                StreamingContent = response.ToString();
            }

            string result = response.ToString();
            if (!string.IsNullOrWhiteSpace(result))
            {
                _history.Add(("assistant", result));
                Messages.Add(new ChatMessage { Content = result, IsUser = false });
            }
        }
        catch (OperationCanceledException)
        {
            // User cancelled — roll back the user turn so history stays consistent
            RollbackLastUserTurn();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
            RollbackLastUserTurn();
        }
        finally
        {
            StreamingContent = string.Empty;
            IsStreaming = false;
            _streamCts?.Dispose();
            _streamCts = null;
        }
    }

    [RelayCommand]
    public void ClearChat()
    {
        CancelStream();
        Messages.Clear();
        _history.Clear();
        ErrorMessage = string.Empty;
        StreamingContent = string.Empty;
    }

    [RelayCommand]
    public async Task OpenEditor()
    {
        await Shell.Current.GoToAsync("//Workspace");
    }

    private void CancelStream()
    {
        try { _streamCts?.Cancel(); }
        catch (ObjectDisposedException) { }
    }

    private void RollbackLastUserTurn()
    {
        if (_history.Count > 0 && _history[^1].role == "user")
            _history.RemoveAt(_history.Count - 1);
        if (Messages.Count > 0 && Messages[^1].IsUser)
            Messages.RemoveAt(Messages.Count - 1);
    }
}
