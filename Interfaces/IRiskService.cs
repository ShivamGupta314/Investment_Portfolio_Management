using InvestmentPortfolioManagement.Models;

namespace InvestmentPortfolioManagement.Interfaces
{
    public interface IRiskService
    {
        Task<RiskProfile> GetRiskByUserIdAsync(int userId);
        Task AssessRiskAsync(RiskProfile profile);
    }
}
