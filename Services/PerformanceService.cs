using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolioManagement.Services
{
    public class PerformanceService : IPerformanceService
    {
        private readonly ApplicationDbContext _context;

        public PerformanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Performance> CalculatePerformanceAsync(int portfolioId)
        {
            var assets = await _context.Assets
                .Where(a => a.PortfolioId == portfolioId)
                .ToListAsync();

            decimal totalInvestment = assets.Sum(a => a.PurchasePrice * a.Quantity);
            decimal currentValue = totalInvestment * 1.1M; // Mock: 10% gain assumption

            var performance = new Performance
            {
                PortfolioId = portfolioId,
                TotalInvestment = totalInvestment,
                CurrentValue = currentValue,
                CalculatedOn = DateTime.UtcNow
            };

            _context.Performances.Add(performance);    // 🔁 Add this line
            await _context.SaveChangesAsync();         // 🔁 Add this too


            return performance;
        }
    }
}
