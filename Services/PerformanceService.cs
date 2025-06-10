
using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;

namespace InvestmentPortfolioManagement.Services
{
    public class PerformanceService :IPerformanceService
    {
        private readonly ApplicationDbContext _context;

        public PerformanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Performance> CalculatePerformanceAsync(Guid portfolioId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, decimal> GetAllocationByType(Guid portfolioId)
        {
            var assets = _context.Assets.Where(a => a.PortfolioId == portfolioId).ToList();
            var total = assets.Sum(a => (decimal)a.TotalValue);
            return assets
                .GroupBy(a => a.AssetType)
                .ToDictionary(g => g.Key, g => total == 0 ? 0 : Math.Round(g.Sum(a => (decimal)a.TotalValue) * 100 / total, 2));
        }

        public Dictionary<string, decimal> GetGainLossTrend(Guid portfolioId)
        {
            var assets = _context.Assets.Where(a => a.PortfolioId == portfolioId).ToList();
            return assets.ToDictionary(
                a => a.Name,
                a => Math.Round((a.CurrentPrice - a.BasePrice) * a.Quantity, 2)
            );
        }
    }
}
