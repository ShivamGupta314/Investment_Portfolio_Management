using InvestmentPortfolioManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentPortfolioManagement.Interfaces
{
    public interface IAssetService
    {
        Task<IEnumerable<Asset>> GetAssetsByPortfolioIdAsync(Guid portfolioId);
        Task<Asset> GetAssetByIdAsync(Guid id);
        Task AddAssetAsync(Asset asset);
        Task UpdateAssetAsync(Asset asset);
        Task DeleteAssetAsync(Guid id);
    }
}
