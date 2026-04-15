using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models;
using System.Net.Http.Json;
using System.Text;
using Tools;
using Vision_Edit.Views;

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

    // ── Document ───────────────────────────────────────────────────────────
    [ObservableProperty]
    private string documentTitle = "Untitled";

    [ObservableProperty]
    private string currentFilePath = string.Empty;

    [ObservableProperty]
    private bool hasUnsavedChanges;

    // ── Editor state ───────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CursorInfo))]
    private int cursorPosition;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CursorInfo))]
    private int selectionLength;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CursorInfo))]
    [NotifyPropertyChangedFor(nameof(WordCount))]
    [NotifyPropertyChangedFor(nameof(CharCount))]
    private string text = string.Empty;

    [ObservableProperty]
    private string selectedText = string.Empty;

    // ── Formatting ─────────────────────────────────────────────────────────
    [ObservableProperty]
    private double fontSize = 14;

    [ObservableProperty]
    private Color fontColor = Colors.White;

    // ── Completion ─────────────────────────────────────────────────────────
    [ObservableProperty]
    private bool isCompletionEnabled = true;

    [ObservableProperty]
    private bool isCompletionVisible;

    [ObservableProperty]
    private bool isLoadingCompletion;

    [ObservableProperty]
    private string completionSuggestion = string.Empty;

    // ── Computed status bar values ─────────────────────────────────────────
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

    public string WordCount
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Text)) return "0 words";
            int n = Text.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;
            return $"{n} {(n == 1 ? "word" : "words")}";
        }
    }

    public string CharCount => $"{Text?.Length ?? 0} chars";

    public EditorViewModel(ApiHandler apiHandler, IHttpClientFactory httpClientFactory, UserManager userManager)
    {
        _apiHandler   = apiHandler;
        _httpClient   = httpClientFactory.CreateClient("Base");
        _userManager  = userManager;
    }

    // ── Selection tracking ─────────────────────────────────────────────────
    partial void OnSelectionLengthChanged(int value) => SetSelectedText();

    partial void OnCursorPositionChanged(int value)
    {
        SetSelectedText();
        if (IsCompletionVisible && !_isApplyingCompletion)
            DismissCompletion();
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
    [RelayCommand]
    public void IncreaseFontSize() { if (FontSize < 32) FontSize++; }

    [RelayCommand]
    public void DecreaseFontSize() { if (FontSize > 8) FontSize--; }

    [RelayCommand]
    public void SetFontColor(string hex) => FontColor = Color.FromArgb(hex);

    // ── Completion toggle ──────────────────────────────────────────────────
    [RelayCommand]
    public void ToggleCompletion()
    {
        IsCompletionEnabled = !IsCompletionEnabled;
        if (!IsCompletionEnabled) { CancelPendingCompletion(); DismissCompletion(); }
        else QueueInlineCompletion();
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
        int pos    = Math.Clamp(CursorPosition, 0, currentText.Length);
        string ctx = currentText[..pos];

        if (!force && (pos < MinCompletionContextLength || string.IsNullOrWhiteSpace(ctx)))
        {
            DismissCompletion();
            return;
        }

        IsLoadingCompletion  = true;
        IsCompletionVisible  = false;
        CompletionSuggestion = string.Empty;

        try
        {
            string result = await _apiHandler.GetCompletionResponse(ctx);
            cancellationToken.ThrowIfCancellationRequested();

            string latestText = Text ?? string.Empty;
            int latestPos     = Math.Clamp(CursorPosition, 0, latestText.Length);
            string latestCtx  = latestText[..latestPos];

            if (!string.Equals(ctx, latestCtx, StringComparison.Ordinal)) return;

            if (!string.IsNullOrWhiteSpace(result))
            {
                CompletionSuggestion = result;
                IsCompletionVisible  = true;
            }
            else DismissCompletion();
        }
        catch (OperationCanceledException) { }
        finally { IsLoadingCompletion = false; }
    }

    private void QueueInlineCompletion()
    {
        CancelPendingCompletion();
        _completionCts = new CancellationTokenSource();
        _ = QueueInlineCompletionAsync(_completionCts.Token, _completionCts);
    }

    private async Task QueueInlineCompletionAsync(CancellationToken ct, CancellationTokenSource owner)
    {
        try
        {
            await Task.Delay(CompletionDebounceMs, ct);
            await RequestCompletionInternal(ct);
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (ReferenceEquals(_completionCts, owner)) _completionCts = null;
            owner.Dispose();
        }
    }

    private void CancelPendingCompletion()
    {
        if (_completionCts is null) return;
        try { _completionCts.Cancel(); }
        catch (ObjectDisposedException) { }
    }

    [RelayCommand]
    public void AcceptCompletion()
    {
        if (string.IsNullOrEmpty(CompletionSuggestion)) return;

        int pos = Math.Clamp(CursorPosition, 0, Text?.Length ?? 0);
        _isApplyingCompletion = true;
        try
        {
            Text           = (Text ?? string.Empty).Insert(pos, CompletionSuggestion);
            CursorPosition = pos + CompletionSuggestion.Length;
            SelectionLength = 0;
        }
        finally { _isApplyingCompletion = false; }

        DismissCompletion();
    }

    // ── Document save ──────────────────────────────────────────────────────
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
            var result = await _httpClient.PostAsJsonAsync("Document/save", new SaveDocumentModel
            {
                Name     = string.IsNullOrWhiteSpace(DocumentTitle) ? "Untitled" : DocumentTitle.Trim(),
                Content  = Text ?? string.Empty,
                Username = _userManager.Username
            });
            return result.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    [RelayCommand]
    public async Task SaveDocumentAs()
    {
        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(Text ?? string.Empty);
            using var ms = new MemoryStream(bytes);
            string name  = (DocumentTitle ?? "Untitled").TrimEnd() + ".txt";
            var result   = await FileSaver.Default.SaveAsync(name, ms, CancellationToken.None);
            if (result.IsSuccessful)
            {
                CurrentFilePath   = result.FilePath ?? string.Empty;
                DocumentTitle     = Path.GetFileNameWithoutExtension(CurrentFilePath);
                HasUnsavedChanges = false;
            }
        }
        catch { /* user cancelled */ }
    }

    // ── Document load ──────────────────────────────────────────────────────

    /// <summary>
    /// When logged in: show a picker of cloud-saved documents, then load the chosen one.
    /// When offline: fall back to the local file picker.
    /// </summary>
    [RelayCommand]
    public async Task LoadDocument()
    {
        if (!_userManager.IsLoggedIn)
        {
            await OpenLocalFile();
            return;
        }

        try
        {
            var listResp = await _httpClient.GetAsync($"Document/list/{_userManager.Username}");
            if (!listResp.IsSuccessStatusCode) { await OpenLocalFile(); return; }

            var docs = await listResp.Content.ReadFromJsonAsync<List<DocumentSummaryModel>>();
            if (docs is null || docs.Count == 0)
            {
                // Nothing saved in cloud yet — fall back to file picker
                await OpenLocalFile();
                return;
            }

            string[] names = [.. docs.Select(d => d.Name)];
            var tcs = new TaskCompletionSource<string?>();
            var picker = new OpenDocumentPage(names);
            picker.DocumentChosen += (_, name) => tcs.TrySetResult(name);
            await Shell.Current.Navigation.PushModalAsync(picker);
            string? chosen = await tcs.Task;

            if (string.IsNullOrEmpty(chosen)) return;

            var summary = docs.FirstOrDefault(d => d.Name == chosen);
            if (summary is null) return;

            var docResp = await _httpClient.GetAsync($"Document/{summary.Id}");
            if (!docResp.IsSuccessStatusCode) return;

            var doc = await docResp.Content.ReadFromJsonAsync<DocumentModel>();
            if (doc is null) return;

            LoadIntoEditor(doc.Name, doc.Content);
        }
        catch { /* network error, silently ignore */ }
    }

    private async Task OpenLocalFile()
    {
        try
        {
            var pick = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Open document",
                FileTypes   = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, [".txt", ".md", ".cs", ".py", ".js", ".ts", ".json", ".xml", ".html", ".css"] },
                    { DevicePlatform.macOS, ["txt", "md", "cs", "py", "js"] }
                })
            });
            if (pick is null) return;

            string content = await File.ReadAllTextAsync(pick.FullPath, Encoding.UTF8);
            LoadIntoEditor(Path.GetFileNameWithoutExtension(pick.FileName), content, pick.FullPath);
        }
        catch { /* user cancelled */ }
    }

    private void LoadIntoEditor(string title, string content, string filePath = "")
    {
        CancelPendingCompletion();
        DismissCompletion();
        _suppressUnsaved = true;
        try
        {
            DocumentTitle     = title;
            Text              = content;
            CurrentFilePath   = filePath;
            HasUnsavedChanges = false;
            CursorPosition    = 0;
        }
        finally { _suppressUnsaved = false; }
    }

    [RelayCommand]
    public void NewDocument()
    {
        CancelPendingCompletion();
        DismissCompletion();
        LoadIntoEditor("Untitled", string.Empty);
    }

    [RelayCommand]
    public void DismissCompletion()
    {
        IsCompletionVisible  = false;
        CompletionSuggestion = string.Empty;
    }

    [RelayCommand]
    public async Task OpenChat() => await Shell.Current.GoToAsync("//Workspace");
}
