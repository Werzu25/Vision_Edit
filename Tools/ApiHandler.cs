using System.Runtime.CompilerServices;
using OpenAI.Chat;
using OpenAI.Responses;

#pragma warning disable OPENAI001

namespace Tools;

public class ApiHandler
{
    private readonly ChatClient _chatClient;
    private readonly ResponsesClient _completionClient;

    private const string ChatModel       = "gpt-4o";
    private const string CompletionModel = "gpt-4o-mini";

    private const string ChatSystemPrompt =
        "You are a helpful assistant embedded in a smart AI text editor. " +
        "Reply in plain text only — no markdown, no code fences, no bullet symbols. " +
        "Be concise and direct.";

    /// <summary>
    /// Constructor requires the OpenAI API key to be provided.
    /// Set via app startup using SecureStorage.
    /// </summary>
    public ApiHandler(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));

        _chatClient       = new ChatClient(ChatModel, apiKey);
        _completionClient = new ResponsesClient(CompletionModel, apiKey);
    }

    /// <summary>
    /// Streams a multi-turn chat response token by token.
    /// <paramref name="history"/> contains all turns so far (including the new user turn).
    /// <paramref name="editorContext"/> is optional selected text injected as a system note.
    /// </summary>
    public async IAsyncEnumerable<string> StreamChatAsync(
        IReadOnlyList<(string role, string content)> history,
        string? editorContext,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(ChatSystemPrompt)
        };

        if (!string.IsNullOrWhiteSpace(editorContext))
            messages.Add(ChatMessage.CreateSystemMessage(
                $"The user currently has the following text selected in the editor:\n{editorContext}"));

        foreach (var (role, content) in history)
        {
            messages.Add(role == "user"
                ? ChatMessage.CreateUserMessage(content)
                : ChatMessage.CreateAssistantMessage(content));
        }

        var streaming = _chatClient.CompleteChatStreamingAsync(messages, cancellationToken: ct);

        await foreach (var update in streaming.WithCancellation(ct))
        {
            foreach (var part in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(part.Text))
                    yield return part.Text;
            }
        }
    }

    /// <summary>
    /// Inline text completion — takes text written so far, returns a short continuation.
    /// </summary>
    public async Task<string> GetCompletionResponse(string context)
    {
        if (string.IsNullOrWhiteSpace(context)) return string.Empty;

        CreateResponseOptions options = new()
        {
            Model = CompletionModel,
            Instructions =
                "You are an inline text completion engine. " +
                "The user sends you the text they have written so far. " +
                "Your ONLY job is to output the most natural continuation — " +
                "do NOT repeat any of the existing text, add no explanations, " +
                "no markdown, no commentary. Output only the raw continuation text. " +
                "Keep it concise (1–3 sentences or code lines)."
        };
        options.InputItems.Add(ResponseItem.CreateUserMessageItem(context));
        ResponseResult response = await _completionClient.CreateResponseAsync(options);
        foreach (ResponseItem item in response.OutputItems)
            if (item is MessageResponseItem msg)
                return msg.Content.FirstOrDefault()?.Text?.Trim() ?? string.Empty;
        return string.Empty;
    }
}
