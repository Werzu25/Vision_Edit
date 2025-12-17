using Microsoft.EntityFrameworkCore;
using Models;

namespace ORM.Services;

public class DbManager : DbContext
{
    public DbSet<UserModel> Users { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = "Server=localhost;Database=vision_edit;User=root;Password=root;";
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
}