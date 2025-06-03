using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Models;
using Microsoft.AspNetCore.Identity; // For password hashing
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentPortfolioManagement.Data
{
    public static class DbInitializer
    {
        public static async Task EnsureAdminUser(ApplicationDbContext context)
        {
            // Ensure the database is created
            context.Database.EnsureCreated();

            // Check if an admin user already exists
            if (context.Users.Any(u => u.Role == "Admin"))
            {
                return; // Admin user already exists, no need to seed
            }

            // Create a default admin user
            var adminUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = "admin",
                Email = "admin@gmail.com",
                Role = "Admin"
            };

            // Use ASP.NET Core Identity's PasswordHasher for secure password hashing
            var passwordHasher = new PasswordHasher<User>();
            adminUser.Password = passwordHasher.HashPassword(adminUser, "Admin@123");

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }
    }
}
