using OpenAI;
using OpenAI.Responses;

#pragma warning disable OPENAI001

namespace Tools;

public class ApiHandler
{
    private readonly ResponsesClient _chatClient;
    private readonly ResponsesClient _completionClient;

    private const string ChatModel       = "gpt-4o";
    private const string CompletionModel = "gpt-4o-mini";

    public ApiHandler()
    {
        string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
        _chatClient       = new ResponsesClient(ChatModel, apiKey);
        _completionClient = new ResponsesClient(CompletionModel, apiKey);
    }

    /// <summary>Chat assistant response for the AI panel.</summary>
    public async Task<string> getApiResponse(string inputMessage)
    {
        CreateResponseOptions options = new()
        {
            Model        = ChatModel,
            Instructions = "You are a helpful assistant inside a smart AI text editor. " +
                           "Appended context is for reference only — never echo it back. " +
                           "Reply in plain text only, never use markdown."
        };
        options.InputItems.Add(ResponseItem.CreateUserMessageItem(inputMessage));
        ResponseResult response = await _chatClient.CreateResponseAsync(options);
        foreach (ResponseItem item in response.OutputItems)
            if (item is MessageResponseItem msg)
                return msg.Content.FirstOrDefault()?.Text ?? string.Empty;
        return "Could not get a response";
    }

    /// <summary>
    /// Inline text completion using the Completions API approach:
    /// takes the text written so far and returns a natural continuation.
    /// Uses gpt-4o-mini with a strict completion-style system prompt
    /// (mirrors the /v1/completions endpoint behaviour).
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
