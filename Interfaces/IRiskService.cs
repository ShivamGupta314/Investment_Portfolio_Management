using InvestmentPortfolioManagement.Models;
using InvestmentPortfolioManagement.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentPortfolioManagement.Interfaces
{
    public interface IRiskService
    {
        Task<PortfolioRiskAnalysis> CalculateAndStorePortfolioRisk(Guid portfolioId);
        Task<PortfolioRiskAnalysis?> GetPortfolioRiskAnalysis(Guid portfolioId);
        Task<List<PortfolioRiskAnalysis>> GetPortfolioRiskHistory(Guid portfolioId, int days = 30);

        // --- NEW METHOD SIGNATURE ---
        // For fetching history from a specific point in time (e.g., last 5 minutes)
        Task<List<PortfolioRiskAnalysis>> GetPortfolioRiskHistory(Guid portfolioId, DateTime since);

        Task<RiskProfile> CreateOrUpdateUserRiskProfile(Guid userId, RiskLevel riskLevel, string description);
        Task<RiskProfile?> GetUserRiskProfile(Guid userId);
    }
}