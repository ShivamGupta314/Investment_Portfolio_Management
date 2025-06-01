using InvestmentPortfolioManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentPortfolioManagement.Interfaces
{
    public interface IAssetService
    {
        Task<IEnumerable<Asset>> GetAssetsByPortfolioIdAsync(int portfolioId);
        Task<Asset> GetAssetByIdAsync(int id);
        Task AddAssetAsync(Asset asset);
        Task UpdateAssetAsync(Asset asset);
        Task DeleteAssetAsync(int id);
    }
}
