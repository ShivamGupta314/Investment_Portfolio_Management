using InvestmentPortfolioManagement.Models;
using System.Threading.Tasks;

namespace InvestmentPortfolioManagement.Interfaces
{
    public interface IPerformanceService
    {
        Task<Performance> CalculatePerformanceAsync(Guid portfolioId);
        Dictionary<string, decimal> GetAllocationByType(Guid portfolioId);
        Dictionary<string, decimal> GetGainLossTrend(Guid portfolioId);
    }
}
