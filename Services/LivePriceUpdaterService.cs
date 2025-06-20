
//using InvestmentPortfolioManagement.Data;
//using InvestmentPortfolioManagement.Models;
//using Microsoft.Extensions.DependencyInjection;

//namespace InvestmentPortfolioManagement.Services
//{
//    public class LivePriceUpdaterService : BackgroundService
//    {
//        private readonly IServiceProvider _serviceProvider;
//        private readonly Random _random = new();

//        public LivePriceUpdaterService(IServiceProvider serviceProvider)
//        {
//            _serviceProvider = serviceProvider;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                using (var scope = _serviceProvider.CreateScope())
//                {
//                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                    var adminAssets = context.Assets
//                                             .Where(a => a.PortfolioId == null)
//                                             .ToList();

//                    foreach (var asset in adminAssets)
//                    {
//                        // Calculate new price
//                        var percentChange = _random.Next(-20, 21); // -20% to +20%
//                        var changeFactor = 1 + (percentChange / 100.0);
//                        var newPrice = Math.Round(asset.BasePrice * (decimal)changeFactor, 2);

//                        // Update asset price
//                        asset.CurrentPrice = newPrice;

//                        // Update investments tied to this asset
//                        var relatedInvestments = context.Investments
//                                                        .Where(inv => inv.AssetId == asset.AssetId)
//                                                        .ToList();

//                        foreach (var SANKET in relatedInvestments)
//                        {
//                            SANKET.CurrentPrice = (double)newPrice;
//                        }
//                    }

//                    await context.SaveChangesAsync();
//                }

//                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Update every 10 seconds
//            }
//        }

//    }

//}


using InvestmentPortfolioManagement.Data;
using InvestmentPortfolioManagement.Models;
using InvestmentPortfolioManagement.Interfaces; // Add this using directive
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting; // Ensure BackgroundService is from this namespace
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Needed for .ToList() and .Where()

namespace InvestmentPortfolioManagement.Services
{
    public class LivePriceUpdaterService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Random _random = new();

        public LivePriceUpdaterService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Get the AssetService from the service provider
                    var assetService = scope.ServiceProvider.GetRequiredService<IAssetService>();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); // Still need context to query assets

                    // Fetch assets that are not assigned to a specific portfolio (or modify as per your logic)
                    // You might want to get all unique assets in the system if they are the source of truth for prices.
                    var assetsToUpdate = await context.Assets
                                                      .Where(a => a.PortfolioId == null) // Or .Where(a => true) to update all assets
                                                      .ToListAsync(stoppingToken);

                    foreach (var asset in assetsToUpdate)
                    {
                        // Calculate new price
                        var percentChange = _random.Next(-20, 21); // -20% to +20%
                        var changeFactor = 1 + (percentChange / 100.0);
                        // Ensure newPrice is decimal, matching Asset.CurrentPrice type
                        var newPrice = Math.Round(asset.BasePrice * (decimal)changeFactor, 2);

                        try
                        {
                            // **CRUCIAL CHANGE:** Use the AssetService to update the price
                            // This will trigger the cascade to Investment and then Portfolio TotalValue
                            await assetService.UpdateAssetCurrentPriceAsync(asset.AssetId, newPrice);
                        }
                        catch (Exception ex)
                        {
                            // Log the error but don't stop the service
                            Console.WriteLine($"Error updating price for asset {asset.Name} (ID: {asset.AssetId}): {ex.Message}");
                        }
                    }
                    // No need for context.SaveChangesAsync() here, as AssetService handles it.
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken); // Update every 5 seconds
            }
        }
    }
}
