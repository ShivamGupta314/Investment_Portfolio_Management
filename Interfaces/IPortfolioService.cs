using InvestmentPortfolioManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentPortfolioManagement.Interfaces
{
    public interface IPortfolioService
    {
        Task<IEnumerable<Portfolio>> GetAllPortfoliosAsync(int userId);
        Task<Portfolio> GetPortfolioByIdAsync(int id);
        Task AddPortfolioAsync(Portfolio portfolio);
        Task UpdatePortfolioAsync(Portfolio portfolio);
        Task DeletePortfolioAsync(int id);
    }
}
