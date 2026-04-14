namespace Models;

public class DocumentModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public UserModel User { get; set; } = null!;   // required navigation property
}
