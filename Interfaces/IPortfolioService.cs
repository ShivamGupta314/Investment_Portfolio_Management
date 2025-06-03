using InvestmentPortfolioManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentPortfolioManagement.Interfaces
{
    public interface IPortfolioService
    {
        Task<IEnumerable<Portfolio>> GetAllPortfoliosAsync(Guid userId);
        Task<Portfolio> GetPortfolioByIdAsync(Guid id);
        Task AddPortfolioAsync(Portfolio portfolio);
        Task UpdatePortfolioAsync(Portfolio portfolio);
        Task DeletePortfolioAsync(Guid id);
    }
}
