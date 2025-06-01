using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManagement.Services
{
    public class RiskService : IRiskService
    {
        private readonly ApplicationDbContext _context;

        public RiskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RiskProfile> GetRiskByUserIdAsync(int userId)
        {
            return await _context.RiskProfiles.FirstOrDefaultAsync(r => r.UserId == userId);
        }

        public async Task AssessRiskAsync(RiskProfile profile)
        {
            var existing = await GetRiskByUserIdAsync(profile.UserId);
            if (existing != null)
            {
                existing.RiskLevel = profile.RiskLevel;
                existing.Description = profile.Description;
                existing.AssessedOn = DateTime.UtcNow;
            }
            else
            {
                await _context.RiskProfiles.AddAsync(profile);
            }

            await _context.SaveChangesAsync();
        }
    }
}
