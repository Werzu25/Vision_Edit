namespace Vision_Edit.Views;

public partial class OpenDocumentPage : ContentPage
{
    public event EventHandler<string?>? DocumentChosen;

    public OpenDocumentPage(IReadOnlyList<string> documentNames)
    {
        InitializeComponent();
        DocList.ItemsSource = documentNames;
    }

    private async void OnDocumentSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0) return;
        if (e.CurrentSelection[0] is string chosen)
        {
            DocumentChosen?.Invoke(this, chosen);
            await Navigation.PopModalAsync();
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        DocumentChosen?.Invoke(this, null);
        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        DocumentChosen?.Invoke(this, null);
        _ = Navigation.PopModalAsync();
        return true;
    }
}
