using Microsoft.EntityFrameworkCore;
using Models;

namespace ORM.Services;

public class DbManager : DbContext
{
    private const string ConnectionString = "Server=localhost;Database=vision_edit;User=root;Password=root;";

    public DbSet<UserModel> Users { get; set; }
    public DbSet<DocumentModel> Documents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString));
    }
}