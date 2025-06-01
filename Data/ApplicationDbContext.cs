using Microsoft.EntityFrameworkCore;
using InvestmentPortfolioManagement.Models;

namespace InvestmentPortfolioManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        public DbSet<User> Users { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Risk> Risks { get; set; }
        public DbSet<Performance> Performances { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<Asset> Assets { get; set; } // ✅ Add this line
        public DbSet<Performance> Performances { get; set; }
        public DbSet<RiskProfile> RiskProfiles { get; set; }


    }
}
