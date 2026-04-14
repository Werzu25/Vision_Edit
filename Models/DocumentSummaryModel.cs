namespace Models;

/// <summary>Lightweight projection returned by GET /api/Document/list/{username}.</summary>
public class DocumentSummaryModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
}
