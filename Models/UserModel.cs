using Microsoft.EntityFrameworkCore;

namespace Models;

[Index(nameof(Username), IsUnique = true)]
public class UserModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public List<DocumentModel> Documents { get; set; } = [];
}
