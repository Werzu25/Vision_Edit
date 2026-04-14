using Microsoft.Maui.Storage;

namespace Vision_Edit.Views;

public partial class ApiKeyPopupPage : ContentPage
{
    public const string StorageKey = "openai_api_key";

    public ApiKeyPopupPage()
    {
        InitializeComponent();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var key = ApiKeyEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(key))
        {
            ErrorLabel.Text = "API key is required.";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (!key.StartsWith("sk-"))
        {
            ErrorLabel.Text = "API key should start with 'sk-'.";
            ErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            await SecureStorage.Default.SetAsync(StorageKey, key);
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Error saving key: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
    }
}
