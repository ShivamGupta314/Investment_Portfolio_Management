using Microsoft.EntityFrameworkCore;
using InvestmentPortfolioManagement.Models;

namespace InvestmentPortfolioManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        public DbSet<InvestmentPortfolioManagement.Models.Investment> Investments { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<RiskProfile> Risks { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<Asset> Assets { get; set; } // ✅ Add this line
        public DbSet<Performance> Performances { get; set; }
        public DbSet<RiskProfile> RiskProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationship causing the multiple cascade paths.
            // We are telling EF that when a User is deleted, it should not
            // try to directly delete the Investment records associated with that User.
            // This is safe because the Investments will be deleted when their parent Portfolio
            // is deleted (which happens when the User is deleted).
            modelBuilder.Entity<Investment>()
                .HasOne(i => i.User)
                .WithMany() // The User model does not have a navigation property back to Investment, which is fine.
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.NoAction); // This is the key change!

            // EF can usually figure out the other relationships, but you could
            // define them here for clarity if you wanted to. For example, the
            // Portfolio -> Investment relationship will default to Cascade, which is correct.
            modelBuilder.Entity<Investment>()
                .HasOne(i => i.Portfolio)
                 .WithMany(p => p.Investments) // The Portfolio model also doesn't have an ICollection<Investment>
                .HasForeignKey(i => i.PortfolioId)
                .OnDelete(DeleteBehavior.Cascade); // Explicitly set to cascade (this is the default anyway)


        }
    }
}
