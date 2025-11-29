using GraphQLShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GraphQLShop.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Вызываем базовый метод
        base.OnModelCreating(modelBuilder);

        // --- НАЧАЛЬНЫЕ ДАННЫЕ (SEEDING) ---
        // Создаем хешер паролей (стандартный из ASP.NET Identity)
        var hasher = new PasswordHasher<User>();

        // Создаем пользователя "admin" с паролем "admin123"
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Username = "admin",
            Role = "Admin",
            // Эта функция сама сгенерирует безопасный хеш с "солью"
            PasswordHash = hasher.HashPassword(null!, "admin123")
        });
    }
}