namespace Vision_Edit.Views;

public partial class EditorPage : ContentPage
{
    private const double PreferredMinEditorWidth = 280;
    private const double PreferredMinChatWidth = 240;
    private const double DefaultChatWidth = 360;

    private double _dragStartEditorWidth;
    private double _lastEditorWidth;
    private double _lastAvailableWidth;

    public EditorPage()
    {
        InitializeComponent();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0 || ContentGrid.ColumnDefinitions.Count < 3) return;

        double dividerW = GetDividerWidth();
        double available = Math.Max(0, width - dividerW);
        if (available <= 0) return;

        GetAdaptiveMinimums(available, out double minEditor, out double minChat);

        double editorTarget;

        if (_lastEditorWidth <= 0 || _lastAvailableWidth <= 0)
        {
            double chatDefault = Math.Clamp(DefaultChatWidth, minChat, Math.Max(minChat, available - minEditor));
            editorTarget = available - chatDefault;
        }
        else
        {
            double ratio = _lastEditorWidth / _lastAvailableWidth;
            editorTarget = available * ratio;
        }

        editorTarget = Math.Clamp(editorTarget, minEditor, Math.Max(minEditor, available - minChat));
        ApplySplit(editorTarget, available, minChat);
    }

    private void OnDividerPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        double dividerW = GetDividerWidth();

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _dragStartEditorWidth = ContentGrid.ColumnDefinitions[0].Width.IsAbsolute
                    ? ContentGrid.ColumnDefinitions[0].Width.Value
                    : EditorPanel.Width;
                break;

            case GestureStatus.Running:
                double available = Math.Max(0, ContentGrid.Width - dividerW);
                if (available <= 0) return;

                GetAdaptiveMinimums(available, out double minEditor, out double minChat);
                double maxEditor = Math.Max(minEditor, available - minChat);
                double newEditor = Math.Clamp(_dragStartEditorWidth + e.TotalX, minEditor, maxEditor);
                ApplySplit(newEditor, available, minChat);
                break;
        }
    }

    private static void GetAdaptiveMinimums(double available, out double minEditor, out double minChat)
    {
        minEditor = Math.Min(PreferredMinEditorWidth, Math.Max(170, available * 0.35));
        minChat = Math.Min(PreferredMinChatWidth, Math.Max(160, available * 0.28));

        if (minEditor + minChat <= available) return;

        minEditor = Math.Max(150, available * 0.5);
        minChat = Math.Max(140, available - minEditor);
    }

    private double GetDividerWidth()
    {
        if (ContentGrid.ColumnDefinitions[1].Width.IsAbsolute)
            return ContentGrid.ColumnDefinitions[1].Width.Value;
        return 8;
    }

    private void ApplySplit(double editorWidth, double availableWidth, double minChat)
    {
        double chatWidth = Math.Max(minChat, availableWidth - editorWidth);
        ContentGrid.ColumnDefinitions[0].Width = new GridLength(editorWidth, GridUnitType.Absolute);
        ContentGrid.ColumnDefinitions[2].Width = new GridLength(chatWidth, GridUnitType.Absolute);
        _lastEditorWidth = editorWidth;
        _lastAvailableWidth = availableWidth;
    }
}
