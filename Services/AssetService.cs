using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Interfaces;
using InvestmentPortfolioManagement.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace InvestmentPortfolioManagement.Services
{
    public class AssetService : IAssetService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPortfolioService _portfolioService;

        public AssetService(ApplicationDbContext context, IPortfolioService portfolioService)
        {
            _context = context;
            _portfolioService = portfolioService;
        }

        public async Task<IEnumerable<Asset>> GetAssetsByPortfolioIdAsync(Guid portfolioId)
        {
            return await _context.Assets.Where(a => a.PortfolioId == portfolioId).ToListAsync();
        }

        public async Task<Asset> GetAssetByIdAsync(Guid id)
        {
            return await _context.Assets.FindAsync(id);
        }

        public async Task AddAssetAsync(Asset asset)
        {
            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();
        }

        public async Task<Asset?> UpdateAssetAsync(Asset asset)
        {
            var existingAsset = await _context.Assets.FindAsync(asset.AssetId);
            if (existingAsset == null)
            {
                return null;
            }

            bool priceChanged = existingAsset.CurrentPrice != asset.CurrentPrice;

            // ... update other asset properties ...
            existingAsset.CurrentPrice = asset.CurrentPrice; // This line is crucial for price update

            await _context.SaveChangesAsync();

            if (priceChanged)
            {
                await RecalculatePortfoliosForAssetChange(existingAsset.AssetId);
            }

            return existingAsset;
        }

        // Specific method for updating only the current price of an asset (used by LivePriceUpdaterService)
        public async Task UpdateAssetCurrentPriceAsync(Guid assetId, decimal newCurrentPrice)
        {
            var asset = await _context.Assets.FindAsync(assetId);
            if (asset == null)
            {
                throw new ArgumentException($"Asset with ID {assetId} not found.");
            }

            if (asset.CurrentPrice != newCurrentPrice) // Only proceed if price actually changed
            {
                asset.CurrentPrice = newCurrentPrice;
                await _context.SaveChangesAsync();

                // **Trigger the cascade update:**
                await RecalculatePortfoliosForAssetChange(assetId);
            }
        }

        // Helper method to find affected portfolios and trigger recalculation
        private async Task RecalculatePortfoliosForAssetChange(Guid changedAssetId)
        {
            // Find all investments linked to this asset
            var investments = await _context.Investments
                                            .Where(i => i.AssetId == changedAssetId)
                                            .ToListAsync();

            // Collect unique PortfolioIds from these investments
            var uniquePortfolioIds = investments
                                        .Where(i => i.PortfolioId.HasValue)
                                        .Select(i => i.PortfolioId.Value)
                                        .Distinct()
                                        .ToList();

            // Update Investment.CurrentPrice for these investments
            var asset = await GetAssetByIdAsync(changedAssetId); // Re-fetch to get latest price
            if (asset != null)
            {
                foreach (var investment in investments)
                {
                    // **IMPORTANT: Ensure Investment.CurrentPrice is also decimal to avoid precision issues**
                    // If Investment.CurrentPrice is still `double`, you'll need the cast:
                    if ((decimal)investment.CurrentPrice != asset.CurrentPrice)
                    {
                        investment.CurrentPrice = (double)asset.CurrentPrice; // Cast if Investment.CurrentPrice is double
                        _context.Entry(investment).State = EntityState.Modified;
                    }
                }
                await _context.SaveChangesAsync(); // Save changes to investments
            }

            // Trigger portfolio recalculation for each affected portfolio
            foreach (var portfolioId in uniquePortfolioIds)
            {
                await _portfolioService.CalculateAndSetPortfolioTotalValueAsync(portfolioId);
            }
        }

        public async Task DeleteAssetAsync(Guid id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset != null)
            {
                _context.Assets.Remove(asset);
                await _context.SaveChangesAsync();
            }
        }
    }
}
