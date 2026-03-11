using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using System.Net.Http.Json;
using System.Text;
using Tools;

namespace Vision_Edit.ViewModels;

public partial class EditorViewModel : ObservableObject
{
    private readonly ApiHandler _apiHandler;
    private readonly HttpClient _httpClient;
    private readonly UserManager _userManager;
    private CancellationTokenSource? _completionCts;
    private bool _isApplyingCompletion;
    private bool _suppressUnsaved;

    private const int CompletionDebounceMs = 500;
    private const int MinCompletionContextLength = 8;

    // ── Document ──────────────────────────────────────────────────────────
    [ObservableProperty] private string _documentTitle = "Untitled";
    [ObservableProperty] private string _currentFilePath = string.Empty;
    [ObservableProperty] private bool   _hasUnsavedChanges;

    // ── Editor state ──────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CursorInfo))]
    private int _cursorPosition;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CursorInfo))]
    private int _selectionLength;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CursorInfo))]
    private string _text = string.Empty;

    [ObservableProperty] private string _selectedText = string.Empty;

    // ── Formatting ────────────────────────────────────────────────────────
    [ObservableProperty] private double _fontSize  = 14;
    [ObservableProperty] private Color  _fontColor = Colors.White;

    // ── Completion ────────────────────────────────────────────────────────
    [ObservableProperty] private bool   _isCompletionEnabled = true;
    [ObservableProperty] private bool   _isCompletionVisible;
    [ObservableProperty] private bool   _isLoadingCompletion;
    [ObservableProperty] private string _completionSuggestion = string.Empty;

    // ── Cursor display ────────────────────────────────────────────────────
    public string CursorInfo
    {
        get
        {
            if (string.IsNullOrEmpty(Text)) return "Ln 1  Col 1";
            int pos    = Math.Clamp(CursorPosition, 0, Text.Length);
            string pre = Text[..pos];
            int ln     = pre.Count(c => c == '\n') + 1;
            int lastNl = pre.LastIndexOf('\n');
            int col    = lastNl < 0 ? pos + 1 : pos - lastNl;
            return $"Ln {ln}  Col {col}";
        }
    }

    public EditorViewModel(ApiHandler apiHandler, IHttpClientFactory httpClientFactory, UserManager userManager)
    {
        _apiHandler = apiHandler;
        _httpClient = httpClientFactory.CreateClient("Base");
        _userManager = userManager;
    }

    // ── Selection tracking ─────────────────────────────────────────────────
    partial void OnSelectionLengthChanged(int value) => SetSelectedText();

    partial void OnCursorPositionChanged(int value)
    {
        SetSelectedText();

        if (IsCompletionVisible && !_isApplyingCompletion)
            DismissCompletion();

        if (IsCompletionEnabled && !_suppressUnsaved)
            QueueInlineCompletion();
    }

    partial void OnTextChanged(string value)
    {
        if (!_suppressUnsaved)
            HasUnsavedChanges = true;

        if (_suppressUnsaved || _isApplyingCompletion)
            return;

        if (!IsCompletionEnabled || string.IsNullOrWhiteSpace(value))
        {
            CancelPendingCompletion();
            DismissCompletion();
            return;
        }

        QueueInlineCompletion();
    }

    public string GetSelectedText()
    {
        if (string.IsNullOrEmpty(Text) || SelectionLength <= 0 || CursorPosition < 0
            || CursorPosition + SelectionLength > Text.Length)
            return string.Empty;
        return Text.Substring(CursorPosition, SelectionLength);
    }

    [RelayCommand]
    public void SetSelectedText() => SelectedText = GetSelectedText();

    // ── Font controls ──────────────────────────────────────────────────────
    [RelayCommand] public void IncreaseFontSize() { if (FontSize < 32) FontSize++; }
    [RelayCommand] public void DecreaseFontSize() { if (FontSize > 8)  FontSize--; }
    [RelayCommand] public void SetFontColor(string hex) => FontColor = Color.FromArgb(hex);

    // ── Completion toggle ──────────────────────────────────────────────────
    [RelayCommand]
    public void ToggleCompletion()
    {
        IsCompletionEnabled = !IsCompletionEnabled;
        if (!IsCompletionEnabled)
        {
            CancelPendingCompletion();
            DismissCompletion();
        }
        else
        {
            QueueInlineCompletion();
        }
    }

    [RelayCommand]
    public async Task RequestCompletion()
    {
        CancelPendingCompletion();
        await RequestCompletionInternal(CancellationToken.None, force: true);
    }

    private async Task RequestCompletionInternal(CancellationToken cancellationToken, bool force = false)
    {
        if (!IsCompletionEnabled || string.IsNullOrWhiteSpace(Text)) return;

        string currentText = Text ?? string.Empty;
        int pos = Math.Clamp(CursorPosition, 0, currentText.Length);
        string context = currentText[..pos];

        if (!force && (pos < MinCompletionContextLength || string.IsNullOrWhiteSpace(context)))
        {
            DismissCompletion();
            return;
        }

        IsLoadingCompletion = true;
        IsCompletionVisible = false;
        CompletionSuggestion = string.Empty;

        try
        {
            string result = await _apiHandler.GetCompletionResponse(context);
            cancellationToken.ThrowIfCancellationRequested();

            string latestText = Text ?? string.Empty;
            int latestPos = Math.Clamp(CursorPosition, 0, latestText.Length);
            string latestContext = latestText[..latestPos];

            if (!string.Equals(context, latestContext, StringComparison.Ordinal))
                return;

            if (!string.IsNullOrWhiteSpace(result))
            {
                CompletionSuggestion = result;
                IsCompletionVisible = true;
            }
            else
            {
                DismissCompletion();
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            IsLoadingCompletion = false;
        }
    }

    private void QueueInlineCompletion()
    {
        CancelPendingCompletion();
        _completionCts = new CancellationTokenSource();
        _ = QueueInlineCompletionAsync(_completionCts.Token, _completionCts);
    }

    private async Task QueueInlineCompletionAsync(CancellationToken cancellationToken, CancellationTokenSource owner)
    {
        try
        {
            await Task.Delay(CompletionDebounceMs, cancellationToken);
            await RequestCompletionInternal(cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            owner.Dispose();
            if (ReferenceEquals(_completionCts, owner))
                _completionCts = null;
        }
    }

    private void CancelPendingCompletion()
    {
        if (_completionCts is null) return;
        _completionCts.Cancel();
    }

    [RelayCommand]
    public void AcceptCompletion()
    {
        if (string.IsNullOrEmpty(CompletionSuggestion)) return;

        int pos = Math.Clamp(CursorPosition, 0, Text?.Length ?? 0);
        _isApplyingCompletion = true;
        try
        {
            Text = (Text ?? string.Empty).Insert(pos, CompletionSuggestion);
            CursorPosition = pos + CompletionSuggestion.Length;
            SelectionLength = 0;
        }
        finally
        {
            _isApplyingCompletion = false;
        }

        DismissCompletion();
    }

    // ── Document save / open ───────────────────────────────────────────────
    [RelayCommand]
    public async Task SaveDocument()
    {
        if (_userManager.IsLoggedIn && await SaveDocumentToDatabase())
        {
            HasUnsavedChanges = false;
            return;
        }

        if (!string.IsNullOrEmpty(CurrentFilePath))
        {
            await File.WriteAllTextAsync(CurrentFilePath, Text ?? string.Empty, Encoding.UTF8);
            HasUnsavedChanges = false;
            return;
        }

        await SaveDocumentAs();
    }

    private async Task<bool> SaveDocumentToDatabase()
    {
        try
        {
            SaveDocumentModel document = new()
            {
                Name = string.IsNullOrWhiteSpace(DocumentTitle) ? "Untitled" : DocumentTitle.Trim(),
                Content = Text ?? string.Empty,
                Username = _userManager.Username
            };

            var result = await _httpClient.PostAsJsonAsync("Document/save", document);
            return result.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    [RelayCommand]
    public async Task SaveDocumentAs()
    {
        try
        {
            byte[] bytes  = Encoding.UTF8.GetBytes(Text ?? string.Empty);
            using var ms  = new MemoryStream(bytes);
            string name   = (DocumentTitle ?? "Untitled").TrimEnd() + ".txt";
            var result    = await FileSaver.Default.SaveAsync(name, ms, CancellationToken.None);
            if (result.IsSuccessful)
            {
                CurrentFilePath   = result.FilePath ?? string.Empty;
                DocumentTitle     = Path.GetFileNameWithoutExtension(CurrentFilePath);
                HasUnsavedChanges = false;
            }
        }
        catch { /* user cancelled */ }
    }

    [RelayCommand]
    public async Task OpenDocument()
    {
        try
        {
            var pick = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Open document",
                FileTypes   = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".txt", ".md", ".cs", ".py", ".js", ".ts", ".json", ".xml", ".html", ".css" } },
                    { DevicePlatform.macOS, new[] { "txt", "md", "cs", "py", "js" } }
                })
            });

            if (pick is null) return;

            CancelPendingCompletion();
            DismissCompletion();
            _suppressUnsaved = true;
            try
            {
                Text              = await File.ReadAllTextAsync(pick.FullPath, Encoding.UTF8);
                DocumentTitle     = Path.GetFileNameWithoutExtension(pick.FileName);
                CurrentFilePath   = pick.FullPath;
                HasUnsavedChanges = false;
                CursorPosition    = 0;
            }
            finally { _suppressUnsaved = false; }
        }
        catch { /* user cancelled */ }
    }

    [RelayCommand]
    public void NewDocument()
    {
        CancelPendingCompletion();
        DismissCompletion();
        _suppressUnsaved  = true;
        Text              = string.Empty;
        DocumentTitle     = "Untitled";
        CurrentFilePath   = string.Empty;
        HasUnsavedChanges = false;
        CursorPosition    = 0;
        _suppressUnsaved  = false;
    }

    [RelayCommand]
    public void DismissCompletion()
    {
        IsCompletionVisible  = false;
        CompletionSuggestion = string.Empty;
    }
}
