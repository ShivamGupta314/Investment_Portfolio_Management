
using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;

namespace InvestmentPortfolioManagement.Services
{
    public class RiskService :IRiskService
    {
        private readonly ApplicationDbContext _context;

        public RiskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task AssessRiskAsync(RiskProfile profile)
        {
            throw new NotImplementedException();
        }

        public Task<RiskProfile> GetRiskByUserIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public string GetRiskLevel(Guid portfolioId)
        {
            var assets = _context.Assets.Where(a => a.PortfolioId == portfolioId).ToList();
            var total = assets.Sum(a => a.TotalValue);
            if (total == 0) return "Low";

            var stockTotal = assets.Where(a => a.AssetType.ToLower() == "stock").Sum(a => a.TotalValue);
            var stockPercent = (stockTotal * 100) / total;

            if (stockPercent < 40)
                return "Low";
            else if (stockPercent < 70)
                return "Medium";
            else
                return "High";
        }
    }
}
