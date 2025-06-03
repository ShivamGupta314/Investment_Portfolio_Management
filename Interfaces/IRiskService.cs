using InvestmentPortfolioManagement.Models;

namespace InvestmentPortfolioManagement.Interfaces
{
    public interface IRiskService
    {
        Task<RiskProfile> GetRiskByUserIdAsync(Guid userId);
        Task AssessRiskAsync(RiskProfile profile);
    }
}
