using Microsoft.EntityFrameworkCore;

namespace Models;

[Index(nameof(Username), IsUnique = true)]
public class UserModel
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}