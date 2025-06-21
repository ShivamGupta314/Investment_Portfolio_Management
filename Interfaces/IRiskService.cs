using InvestmentPortfolioManagement.Common;
using InvestmentPortfolioManagement.Models;

namespace InvestmentPortfolioManagement.Interfaces
{
    public interface IRiskService
    {
        Task<RiskProfile?> GetUserRiskProfile(Guid userId);
        Task<RiskProfile> CreateOrUpdateUserRiskProfile(Guid userId, RiskLevel riskLevel, string description);
        Task<List<PortfolioRiskAnalysis>> GetPortfolioRiskHistory(Guid portfolioId, int days = 30);
        Task<PortfolioRiskAnalysis?> GetPortfolioRiskAnalysis(Guid portfolioId);
        Task<PortfolioRiskAnalysis> CalculateAndStorePortfolioRisk(Guid portfolioId);

    }
}
